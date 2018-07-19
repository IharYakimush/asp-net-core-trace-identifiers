using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using TraceIdentifiers.AspNetCore.Serilog;

namespace TraceIdentifiers.AspNetCore.Integration
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

            loggerConfiguration.Enrich.FromLogContext();
            loggerConfiguration.WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {EventId} {Message:lj} {Properties}{NewLine}{Exception}{NewLine}");

            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddLogging(builder => builder.ClearProviders().AddSerilog(dispose: true));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseTraceIdentifiers(new TraceIdentifiersMiddlewareOptions
                {
                    RequestIdentifiersHeaderName = "Accept-Encoding",
                    RequestIdentifiersSeparator = ','
                })
                .SetToSerilogContext();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World! ");
                TraceIdentifiersFeature ti = context.Features.Get<TraceIdentifiersFeature>();

                if (ti != null)
                {
                    await context.Response.WriteAsync($"Current: {ti.Current} ");

                    foreach (var item in ti.All)
                    {
                        await context.Response.WriteAsync(item + "|");
                    }
                }

                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Demo").LogInformation("Hellow World Log. TraceCurrent: {TraceIdentifier} TraceAll: {TraceIdentifiers}");
            });
        }
    }
}
