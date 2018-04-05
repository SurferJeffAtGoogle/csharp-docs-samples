// Copyright (c) 2018 Google LLC.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using GoogleCloudSamples;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Sudokumb
{
    public class ApplicationUser : IdentityUser<string> {}

    public class DatastoreTestFixture {
        public ServiceProvider ServiceProvider { get; set; } = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        public ILoggerFactory LoggerFactory { get; private set; }        
        
        public ILogger<DatastoreUserStore<ApplicationUser>> Logger { get; private set; }

        public Google.Cloud.Datastore.V1.DatastoreDb Datastore { get; private set; }

        public DatastoreTestFixture()
        {
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();
            Logger = LoggerFactory.CreateLogger<DatastoreUserStore<ApplicationUser>>();
            Datastore = Google.Cloud.Datastore.V1.DatastoreDb.Create(
                Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
                "DatastoreTestFixture-" 
                + GoogleCloudSamples.TestUtil.RandomName());
        }
    }
    public class DatastoreUserStoreTest : IClassFixture<DatastoreTestFixture>
    {
        readonly IUserStore<ApplicationUser> _userStore;
        CancellationToken Timeout 
        { 
            get 
            { 
                var source = new CancellationTokenSource();
                source.CancelAfter(30 * 1000);
                return source.Token;
            }
        }

        public DatastoreUserStoreTest(DatastoreTestFixture fixture)
        {
            _userStore = new DatastoreUserStore<ApplicationUser>(
                fixture.Datastore, fixture.Logger);
            
        }

        [Fact]
        public async Task Test1()
        {
            string userId = TestUtil.RandomName();
            string name = TestUtil.RandomName();
            var found = await _userStore.FindByIdAsync(userId, Timeout);
            Assert.Null(found);
            found = await _userStore.FindByNameAsync(name, Timeout);
            Assert.Null(found);
            var newUser = new ApplicationUser()
            {
                NormalizedUserName = name,
                Email = 
            };
            _userStore.CreateAsync()
        }
    }
}
