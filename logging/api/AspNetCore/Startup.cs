// [START logging_setup_aspnetcore_using]
using Google.Cloud.Diagnostics.AspNetCore;
// [END logging_setup_aspnetcore_using]
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
// [START logging_setup_aspnetcore_using]
using Microsoft.Extensions.Logging;
// [END logging_setup_aspnetcore_using]

namespace WebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // [START logging_setup_aspnetcore_logger_factory]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            // Log to Google.
            loggerFactory.AddGoogle("YOUR-PROJECT-ID");
            // [END logging_setup_aspnetcore_logger_factory]

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
