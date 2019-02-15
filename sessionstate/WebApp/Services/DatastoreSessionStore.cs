using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Datastore.V1;
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

        public DatastoreDistributedCache()
        {
            _projectId = GetProjectId();
            _namespaceId = "";
            _datastore = DatastoreDb.Create(_projectId, _namespaceId);
            _keyFactory = new KeyFactory(_projectId, _namespaceId, "WebSession");
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

        public byte[] Get(string cacheKey) => 
            UnpackEntities(_datastore.Lookup(CreateBothKeys(cacheKey)));

        public async Task<byte[]> GetAsync(string cacheKey, CancellationToken token = default(CancellationToken)) =>
            UnpackEntities(await _datastore.LookupAsync(
                CreateBothKeys(cacheKey),  null,
                CallSettings.FromCancellationToken(token) ));    

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
            if (entity.Properties.TryGetValue("expires", out expires))
            {
                if (expires.TimestampValue.ToDateTime() < now)
                {
                    return null;  // Expired.
                }
            }
            Google.Cloud.Datastore.V1.Value slidingExpirationSeconds;
            Google.Cloud.Datastore.V1.Value atimeValue;
            if (entity.Properties.TryGetValue("slidingExpirationSeconds",
                out slidingExpirationSeconds) &&
                atime.Properties.TryGetValue("atime", out atimeValue))
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
        public void Refresh(string cacheKey) =>
            _datastore.Upsert(CreateAtime(cacheKey));

        public Task RefreshAsync(string cacheKey, CancellationToken token = default(CancellationToken)) =>
            _datastore.UpsertAsync(CreateAtime(cacheKey),
                CallSettings.FromCancellationToken(token));

        public void Remove(string cacheKey) =>
            _datastore.Delete(CreateEntityKey(cacheKey));

        public Task RemoveAsync(string cacheKey, CancellationToken token = default(CancellationToken)) =>
            _datastore.DeleteAsync(CreateEntityKey(cacheKey), 
                CallSettings.FromCancellationToken(token));

        public void Set(string cacheKey, byte[] value, DistributedCacheEntryOptions options)
        {
            var entities = new [] { CreateEntity(cacheKey, value, options),
                CreateAtime(cacheKey) };
            _datastore.Upsert(entities);
        }

        public Task SetAsync(string cacheKey, byte[] value, 
            DistributedCacheEntryOptions options, 
            CancellationToken token = default(CancellationToken))
        {
            var entities = new [] { CreateEntity(cacheKey, value, options),
                CreateAtime(cacheKey) };
            return _datastore.UpsertAsync(
                entities, CallSettings.FromCancellationToken(token));
        }

        Entity CreateEntity(string cacheKey, byte[] value, 
            DistributedCacheEntryOptions options)
        {
            var now = DateTime.UtcNow;
            var entity = new Entity() {
                Key = CreateEntityKey(cacheKey),
                ["payload"] = ByteString.CopyFrom(value),
            };
            if (options.AbsoluteExpiration.HasValue) 
            {
                entity["expires"] = Timestamp.FromDateTimeOffset(
                    options.AbsoluteExpiration.Value);
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                entity["expires"] = Timestamp.FromDateTime(
                    now.Add(options.AbsoluteExpirationRelativeToNow.Value));
            }
            if (options.SlidingExpiration.HasValue)
            {
                entity["slidingExpirationSeconds"] = 
                    options.SlidingExpiration.Value.TotalSeconds;
            }
            return entity;
        }

        Entity CreateAtime(string cacheKey) 
        {
            var atime = new Entity() {
                Key = CreateAtimeKey(cacheKey),
                ["atime"] = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            return atime;
        }

        Key CreateEntityKey(string cacheKey) =>
            _keyFactory.CreateKey(cacheKey);

        Key CreateAtimeKey(Key entityKey) =>
            new KeyFactory(entityKey, "atime").CreateKey(2);

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

        public DatastoreSessionStore(ILoggerFactory loggerFactory)
        {
            _distributedSessionStore = new DistributedSessionStore(
                new DatastoreDistributedCache(), loggerFactory);
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