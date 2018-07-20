using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            services.AddTraceIdentifiersClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseTraceIdentifiers(new TraceIdentifiersMiddlewareOptions
                {
                    RequestIdentifiersHeaderName = "Accept-Encoding",
                    RequestIdentifiersSeparator = ','
                })
                .PushToSerilogContext();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World! ");
                TraceIdentifiersCollection ti = context.Features.Get<TraceIdentifiersCollection>();

                if (ti != null)
                {
                    await context.Response.WriteAsync($"Current: {ti.Current} ");

                    foreach (var item in ti.All)
                    {
                        await context.Response.WriteAsync(item + "|");
                    }

                    if (!context.Request.Headers.ContainsKey(TraceIdentifiersSendOptions.DefaultHeaderName))
                    {
                        var handler = context.RequestServices.GetRequiredService<IHttpMessageHandlerFactory>();
                        using (HttpClient client = new HttpClient(handler.Create()))
                        {
                            var text = await client.GetAsync("http://localhost:65239");
                            await context.Response.WriteAsync(await text.Content.ReadAsStringAsync());
                        }
                    }                    
                }

                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Demo").LogInformation("Hellow World Log. TraceCurrent: {TraceIdentifier} TraceAll: {TraceIdentifiersCollection}");
            });
        }
    }
}
