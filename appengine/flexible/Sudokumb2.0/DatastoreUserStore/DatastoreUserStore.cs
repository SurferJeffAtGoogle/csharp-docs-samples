﻿using Google.Cloud.Datastore.V1;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Sudokumb
{
    public class DatastoreOptions 
    {
        public string ProjectId { get; set; }
        public string Namespace { get; set; }
    }

    public class DatastoreUserStore : IUserStore<IdentityUser>
    {
        DatastoreDb _datastore;
        KeyFactory _userKeyFactory;

        static string
            KIND = "webuser",
            NORMALIZED_EMAIL = "normalized-email",
            NORMALIZED_NAME = "normalized-name",
            USER_NAME = "user-name",
            CONCURRENCY_STAMP = "concurrency-stamp";

        public DatastoreUserStore(DatastoreDb datastore, IOptions<DatastoreOptions> options)
        {
            _datastore = datastore;
            var opts = options.Value;
            _userKeyFactory = new KeyFactory(opts.ProjectId, opts.Namespace, KIND);
        }

        Key KeyFromUserId(string userId) => _userKeyFactory.CreateKey(userId);
        async Task<IdentityResult> WrapRpcExceptionsAsync(Func<Task> f)
        {
            try
            {
                await f();
                return IdentityResult.Success;
            }
            catch (Grpc.Core.RpcException e)
            {
                return IdentityResult.Failed(new IdentityError() {
                    Code = e.Status.Detail,
                    Description = e.Message
                });
            }
        }

        Entity UserToEntity(IdentityUser user) {
            var entity = new Entity() 
            {
                [NORMALIZED_EMAIL] = user.NormalizedEmail,
                [NORMALIZED_NAME] = user.NormalizedUserName,
                [USER_NAME] = user.UserName,
                [CONCURRENCY_STAMP] = user.ConcurrencyStamp,
                Key = KeyFromUserId(user.Id)
            };
            entity[CONCURRENCY_STAMP].ExcludeFromIndexes = true;
            return entity;
        }

        IdentityUser EntityToUser(Entity entity)
        {
            if (null == entity)
            {
                return null;
            }
            IdentityUser user = new IdentityUser()
            {
                NormalizedUserName = (string)entity[NORMALIZED_NAME],
                NormalizedEmail = (string)entity[NORMALIZED_EMAIL],
                UserName = (string)entity[USER_NAME],
                ConcurrencyStamp = (string)entity[CONCURRENCY_STAMP]
            };
            return user;
        }


        public async Task<IdentityResult> CreateAsync(IdentityUser user,
            CancellationToken cancellationToken)
        {                        
            return await WrapRpcExceptionsAsync(() => 
                _datastore.InsertAsync(UserToEntity(user), CallSettings.FromCancellationToken(cancellationToken)));
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user,
            CancellationToken cancellationToken)
        {
            return await WrapRpcExceptionsAsync(() => 
                _datastore.DeleteAsync(KeyFromUserId(user.Id), CallSettings.FromCancellationToken(cancellationToken)));                        
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return EntityToUser(await _datastore.LookupAsync(KeyFromUserId(userId),
                callSettings:CallSettings.FromCancellationToken(cancellationToken)));            
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var result = await _datastore.RunQueryAsync(new Query(KIND) {
                Filter = Filter.Equal(NORMALIZED_NAME, normalizedUserName)
            });
            return EntityToUser(result.Entities.FirstOrDefault());
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }
        void IDisposable.Dispose()
        {
        }
        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return await WrapRpcExceptionsAsync(() => 
                _datastore.UpsertAsync(UserToEntity(user), CallSettings.FromCancellationToken(cancellationToken)));
        }
    }
}