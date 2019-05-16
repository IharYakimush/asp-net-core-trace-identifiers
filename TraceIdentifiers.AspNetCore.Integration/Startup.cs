using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TraceIdentifiers.Serilog;

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
            TraceIdentifiersContext.Startup.LinkToSerilogLogContext();

            app.UseTraceIdentifiers(new TraceIdentifiersMiddlewareOptions
            {
                ReadRemoteHeaderName = c => "Accept-Encoding",
                ReadRemoteSeparator = c => ','
            });
                

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World! ");
                TraceIdentifiersContext ti = context.Features.Get<TraceIdentifiersContext>();

                if (ti != null)
                {
                    await context.Response.WriteAsync($"Startup: {TraceIdentifiersContext.StartupId}\n");
                    await context.Response.WriteAsync($"Local: \n");
                    foreach (string s in ti.Local)
                    {
                        await context.Response.WriteAsync($"{s}\n");
                    }

                    await context.Response.WriteAsync($"RemoteShared: \n");
                    foreach (string s in ti.RemoteShared)
                    {
                        await context.Response.WriteAsync($"{s}\n");
                    }
                }

                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Demo").LogInformation("Hello World Log. correlationStartup: {correlationStartup} correlationLocal: {correlationLocal} correlationAll: {correlationAll}");
            });
        }
    }
}
