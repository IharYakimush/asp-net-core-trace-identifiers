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
            services.AddTraceIdentifiers();
            services.AddMvc();
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

            loggerConfiguration.Enrich.FromLogContext();
            loggerConfiguration.WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {EventId} {Message:lj} {Properties}{NewLine}{Exception}{NewLine}");

            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddLogging(builder => builder.ClearProviders().AddSerilog(dispose: true));

            IHttpClientBuilder httpClientBuilder = services.AddHttpClient("default");           

            httpClientBuilder.SendTraceIdentifiersFromHttpContext((message, context) =>
                message.TryAddLocalSharedAndRemoteShared(context, SendIdentifiersOptions.Default));            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // All identifiers contexts derived from this will have link with Serilog LogContext
            TraceIdentifiersContext.Startup.LinkToSerilogLogContext();

            app.UseTraceIdentifiers();

            app.MapWhen(context =>
                    context.Request.Path.StartsWithSegments(new PathString("/mvc"), StringComparison.OrdinalIgnoreCase),
                builder => builder.UseMvcWithDefaultRoute());
                
            app.Run(async (context) =>
            {
                if (context.Request.Method != "GET")
                {
                    return;
                }
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
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, localhost + "clientManual1");
                        request.TryAddLocalSharedAndRemoteShared(c1, SendIdentifiersOptions.Default);
                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        string remoteSingle = response.ReadRemoteIdentifier();

                        c1.CreateChildWithRemote(remoteSingle, false); //// not disposed but not shared, so available in logs only
                        await context.Response.WriteAsync($"\nResponse1: remote identifier: {remoteSingle}\n");
                        await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    }

                    using (var c2 = ti.CreateChildWithLocal(true, "clientManual2"))
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, localhost + "clientManual2");
                        request.TryAddLocalSharedAndRemoteShared(c2, new SendIdentifiersOptions { UseSeparator = true });
                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        string remoteSingle = response.ReadRemoteIdentifier();
                        c2.CreateChildWithRemote(remoteSingle); // not disposed and shared, so will be propagated to all next requests and available in logs
                        await context.Response.WriteAsync($"\nResponse11: remote identifier: {remoteSingle}\n");
                        await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    }

                    httpClient =
                        context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("default");

                    using (ti.CreateChildWithLocal(true, "clientFromFactory"))
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(localhost + "clientFromFactory");
                        string remoteSingle = response.ReadRemoteIdentifier();

                        await context.Response.WriteAsync($"Response2: remote identifier: {remoteSingle} \n");
                        await context.Response.WriteAsync($"{await response.Content.ReadAsStringAsync()} \n");
                    }
                }
            });
        }
    }
}
