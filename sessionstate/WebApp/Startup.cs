using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Diagnostics.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            if (false)
            {
                // Use KMS.
                services.AddSingleton<IDataProtectionProvider>(provider =>
                    new TracingDataProtectionProvider(
                        new KmsDataProtectionProvider("surferjeff-test2", 
                        "us-central1", "sessions"), 
                        provider.GetService<IManagedTracer>()));
            }
            else
            {
                // Trace the local file implementation of the keystore.
                var builder = services.AddDataProtection();   
                builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(provider =>
                {
                    var loggerFactory = provider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
                    return new ConfigureOptions<KeyManagementOptions>(options =>
                    {
                        if (false) 
                        {
                            options.XmlRepository = new TracingXmlRepository(
                                new FileSystemXmlRepository(new System.IO.DirectoryInfo("/tmp/sessions"), loggerFactory),
                                provider.GetService<IManagedTracer>(),
                                loggerFactory);
                        }
                        else
                        {
                            var datastoreOptions = new DataStoreXmlRepository.Options() {
                                ProjectId = "surferjeff-test2"
                            };
                            options.XmlRepository = new TracingXmlRepository(
                                new DataStoreXmlRepository(
                                    Options.Create(datastoreOptions),
                                    loggerFactory.CreateLogger<DataStoreXmlRepository>()),
                                provider.GetService<IManagedTracer>(),
                                loggerFactory);
                        }
                        var kmsOptions = new KmsXmlEncryptorOptions {
                            ProjectId = "surferjeff-test2",
                            LocationId = "us-central1",
                            KeyringId = "sessions",
                            KeyId = "master"
                        };
                        options.XmlEncryptor = new KmsXmlEncryptor(Options.Create(kmsOptions));
                    });
                });
            }
            var redisOptions = new RedisCacheOptions()
            {
                Configuration = "10.240.0.89,password=zG6WkNaCkhun"
            };
            services.AddSingleton<IDistributedCache>(provider =>
                new TracingDistributedCache(
                    new RedisCache(Options.Create(redisOptions)),
                    provider.GetService<IManagedTracer>()));
            services.AddSession();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
