﻿// Copyright (c) 2018 Google LLC.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GithubHook.Services;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GithubHook
{
    public class Startup
    {
        private readonly Lazy<string> _projectId = new Lazy<string>(() => GetProjectId());

        public string ProjectId
        {
            get { return _projectId.Value; }
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<KmsDataProtectionProviderOptions>(
                          Configuration.GetSection("KmsDataProtection"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<Models.SecretsModel>(
                Configuration.GetSection("Secrets"));

            services.AddSingleton<Models.GoogleProjectModel>(
                (provider) => new Models.GoogleProjectModel()
                {
                    Id = ProjectId
                });
            services.AddSingleton<IDataProtectionProvider,
                KmsDataProtectionProvider>();
            services.AddGoogleExceptionLogging(options =>
            {
                options.ProjectId = ProjectId;
                options.ServiceName = "GithubWebHook";
                options.Version = "0.01";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseGoogleExceptionLogging();
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Install the XForwardedProtoRule so middleware knows requests
            // arrived via https.
            RewriteOptions rewriteOptions = new RewriteOptions();
            rewriteOptions.Add(new XForwardedProtoRule());
            app.UseRewriter(rewriteOptions);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
