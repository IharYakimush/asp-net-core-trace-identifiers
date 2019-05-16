using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TraceIdentifiers.HttpClient;
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
            IHttpClientBuilder httpClientBuilder = services.AddHttpClient<System.Net.Http.HttpClient>(client => { });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // All identifiers contexts derived from this will have link with Serilog LogContext
            TraceIdentifiersContext.Startup.LinkToSerilogLogContext();

            app.UseTraceIdentifiers();
                
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World! ");
                TraceIdentifiersContext ti = context.Features.Get<TraceIdentifiersContext>();

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

                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Demo").LogInformation("Hello World Log. correlationStartup: {correlationStartup} correlationLocal: {correlationLocal} correlationAll: {correlationAll}");

                if (!context.Request.Headers.ContainsKey(TraceIdentifiersDefaults.DefaultSharedHeaderName))
                {
                    System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

                    string localhost = "http://localhost:65239/";

                    using (var c1 = ti.CreateChildWithLocal(true, "clientManual1"))
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, localhost);
                        request.TryAddLocalSharedAndRemoteShared(c1, SendIdentifiersOptions.Default);
                        HttpResponseMessage response = await httpClient.SendAsync(request);

                        await context.Response.WriteAsync($"\nResponse1: \n");
                        await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    }

                    using (var c2 = ti.CreateChildWithLocal(true, "clientManual2"))
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, localhost);
                        request.TryAddLocalSharedAndRemoteShared(c2, new SendIdentifiersOptions {UseSeparator = true});
                        HttpResponseMessage response = await httpClient.SendAsync(request);

                        await context.Response.WriteAsync($"\nResponse2: \n");
                        await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    }

                    //httpClient =
                    //    context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();

                    //using (ti.CreateChildWithLocal(true, "clientFromFactory"))
                    //{
                    //    HttpResponseMessage response = await httpClient.GetAsync(localhost);

                    //    await context.Response.WriteAsync($"Response2: \n");
                    //    await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    //}
                }

            });
        }
    }
}
