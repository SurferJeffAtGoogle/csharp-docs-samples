using Google.Cloud.Diagnostics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AntiForgery
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseDiagnostics()
                .UseStartup<Startup>();
    }

    internal static class ProgramExtensions 
    {
        internal static IWebHostBuilder UseDiagnostics(
            this IWebHostBuilder builder)
        {
            // App Engine sets the following 3 environment variables.
            string projectId = Environment
                .GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
            if (!string.IsNullOrWhiteSpace(projectId)) {
                string service = Environment
                    .GetEnvironmentVariable("GAE_SERVICE") ?? "unknown";
                string version = Environment
                    .GetEnvironmentVariable("GAE_VERSION") ?? "unknown";
                builder.UseGoogleDiagnostics(projectId, service, version);
            }            
            return builder;
        }
    }
}
