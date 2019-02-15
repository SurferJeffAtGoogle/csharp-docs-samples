using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Datastore.V1;
using Google.Cloud.Diagnostics.Common;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace WebApp
{

    class DatastoreDistributedCache : IDistributedCache
    {
        readonly DatastoreDb _datastore;
        readonly string _projectId;
        readonly string _namespaceId;
        readonly KeyFactory _keyFactory;
        readonly ILogger<DatastoreDistributedCache> _logger;
        private readonly IManagedTracer _tracer;
        const string CACHE_ENTRY = "CacheEntry",
            EXPIRES = "expires",
            ATIME = "atime",
            SLIDING_EXPIRATION_SECONDS = "seconds";

        public DatastoreDistributedCache(ILoggerFactory loggerFactory,
            IManagedTracer tracer)
        {
            _logger = loggerFactory.CreateLogger<DatastoreDistributedCache>();
            _projectId = GetProjectId();
            _namespaceId = "";
            _datastore = DatastoreDb.Create(_projectId, _namespaceId);
            _keyFactory = new KeyFactory(_projectId, _namespaceId, CACHE_ENTRY);
            this._tracer = tracer;
        }

        private static string GetProjectId()
        {
            GoogleCredential googleCredential = Google.Apis.Auth.OAuth2
                .GoogleCredential.GetApplicationDefault();
            if (googleCredential != null)
            {
                ICredential credential = googleCredential.UnderlyingCredential;
                ServiceAccountCredential serviceAccountCredential =
                    credential as ServiceAccountCredential;
                if (serviceAccountCredential != null)
                {
                    return serviceAccountCredential.ProjectId;
                }
            }
            return Google.Api.Gax.Platform.Instance().ProjectId;
        }

        public byte[] Get(string cacheKey) 
        {
            using (_tracer.StartSpan($"Get({cacheKey})"))
            {
                return UnpackEntities(_datastore.Lookup(CreateBothKeys(cacheKey)));
            }
        }

        public async Task<byte[]> GetAsync(string cacheKey, CancellationToken token = default(CancellationToken))
        {
            using (_tracer.StartSpan($"GetAsync({cacheKey})"))
            {
                return
                UnpackEntities(await _datastore.LookupAsync(
                    CreateBothKeys(cacheKey),  null,
                    CallSettings.FromCancellationToken(token) ));
            }
        }

        byte[] UnpackEntities(IEnumerable<Entity> entities) 
        {
            if (entities.Count() != 2) 
            {
                return null;  // Doesn't exist.
            }
            var entity = entities.First();
            var atime = entities.Last();
            if (entity == null || atime == null)
            {
                return null;
            }
            Google.Cloud.Datastore.V1.Value expires;
            var now = DateTime.UtcNow;
            if (entity.Properties.TryGetValue(EXPIRES, out expires))
            {
                if (expires.TimestampValue.ToDateTime() < now)
                {
                    return null;  // Expired.
                }
            }
            Google.Cloud.Datastore.V1.Value slidingExpirationSeconds;
            Google.Cloud.Datastore.V1.Value atimeValue;
            if (entity.Properties.TryGetValue(SLIDING_EXPIRATION_SECONDS,
                out slidingExpirationSeconds) &&
                atime.Properties.TryGetValue(ATIME, out atimeValue))
            {
                if (atimeValue.TimestampValue.ToDateTime().Add(
                    TimeSpan.FromSeconds((double)slidingExpirationSeconds))
                    < now)
                {
                    return null;  // Expired.
                }
            }
            return entity["payload"].BlobValue.ToByteArray();
        }

        // Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
        public void Refresh(string cacheKey) 
        {
            _logger.LogTrace($"Refresh({cacheKey})");
            using (_tracer.StartSpan($"Refresh({cacheKey})"))
            {
                _datastore.Upsert(CreateAtime(cacheKey));
            }
        }

        public Task RefreshAsync(string cacheKey, CancellationToken token = default(CancellationToken))
        {
            _logger.LogTrace($"RefreshAsync({cacheKey})");
            using (_tracer.StartSpan($"RefreshAsync({cacheKey})"))
            {
                return _datastore.UpsertAsync(CreateAtime(cacheKey),
                    CallSettings.FromCancellationToken(token));
            }
        }

        public void Remove(string cacheKey) =>
            _datastore.Delete(CreateBothKeys(cacheKey));

        public Task RemoveAsync(string cacheKey, CancellationToken token = default(CancellationToken)) =>
            _datastore.DeleteAsync(CreateBothKeys(cacheKey), 
                CallSettings.FromCancellationToken(token));

        public void Set(string cacheKey, byte[] value, DistributedCacheEntryOptions options)
        {
            _logger.LogTrace($"Set({cacheKey})");
            using (_tracer.StartSpan($"Set({cacheKey})"))
            {
                var entities = new [] { CreateEntity(cacheKey, value, options),
                    CreateAtime(cacheKey) };
                _datastore.Upsert(entities);
            }
        }

        public Task SetAsync(string cacheKey, byte[] value, 
            DistributedCacheEntryOptions options, 
            CancellationToken token = default(CancellationToken))
        {
            _logger.LogTrace($"SetAsync({cacheKey})");
            using (_tracer.StartSpan($"SetAsync({cacheKey})"))
            {
                var entities = new [] { CreateEntity(cacheKey, value, options),
                    CreateAtime(cacheKey) };
                return _datastore.UpsertAsync(
                    entities, CallSettings.FromCancellationToken(token));
            }
        }

        Entity CreateEntity(string cacheKey, byte[] value, 
            DistributedCacheEntryOptions options)
        {
            var now = DateTime.UtcNow;
            var entity = new Entity() {
                Key = CreateEntityKey(cacheKey),
                ["payload"] = ByteString.CopyFrom(value),
            };
            entity["payload"].ExcludeFromIndexes = true;
            if (options.AbsoluteExpiration.HasValue) 
            {
                entity[EXPIRES] = Timestamp.FromDateTimeOffset(
                    options.AbsoluteExpiration.Value);
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                entity[EXPIRES] = Timestamp.FromDateTime(
                    now.Add(options.AbsoluteExpirationRelativeToNow.Value));
            }
            if (options.SlidingExpiration.HasValue)
            {
                entity[SLIDING_EXPIRATION_SECONDS] = 
                    options.SlidingExpiration.Value.TotalSeconds;
                entity[SLIDING_EXPIRATION_SECONDS].ExcludeFromIndexes = true;
            }
            return entity;
        }

        Entity CreateAtime(string cacheKey) 
        {
            var atime = new Entity() {
                Key = CreateAtimeKey(cacheKey),
                [ATIME] = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            return atime;
        }

        Key CreateEntityKey(string cacheKey) =>
            _keyFactory.CreateKey(cacheKey);

        Key CreateAtimeKey(Key entityKey) =>
            new KeyFactory(entityKey, ATIME).CreateKey(1);

        Key CreateAtimeKey(string cacheKey) =>
            CreateAtimeKey(CreateEntityKey(cacheKey));

        Key[] CreateBothKeys(string cacheKey)
        {
            var entityKey = CreateEntityKey(cacheKey);
            return new Key[] { entityKey, CreateAtimeKey(entityKey) };
        }
    }

    public class DatastoreSessionStore : ISessionStore
    {
        readonly DistributedSessionStore _distributedSessionStore;

        public DatastoreSessionStore(ILoggerFactory loggerFactory,
            IManagedTracer tracer)
        {
            _distributedSessionStore = new DistributedSessionStore(
                new DatastoreDistributedCache(loggerFactory, tracer), loggerFactory);
        }

        public ISession Create(string sessionKey, TimeSpan idleTimeout, 
            TimeSpan ioTimeout, Func<bool> tryEstablishSession, 
            bool isNewSessionKey)
        {
            return _distributedSessionStore.Create(sessionKey, idleTimeout,
                ioTimeout, tryEstablishSession, isNewSessionKey);
        }
    }
}