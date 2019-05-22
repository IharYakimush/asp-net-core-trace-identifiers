# asp-net-core-trace-identifiers
Track related activities across multiple services

# Nuget
- https://www.nuget.org/packages/TraceIdentifiers
Base classes e.g. `TraceIdentifiersContext`. Also allow to send trace identifiers in HttpRequestMessage as headers.
- https://www.nuget.org/packages/TraceIdentifiers.AspNetCore
Middelware to read external trace identifiers from http request and push local trace identifiers to response. Make `TraceIdentifiersContext` available in `HttpContext.Features` and implement `ITraceIdentifiersAccessor` to access `TraceIdentifiersContext` from `HttpContext.Features`.
- https://www.nuget.org/packages/TraceIdentifiers.Serilog
Integration of `TraceIdentifiersContext` with Serilog `LogContext`. Allows to enrich log entries with trace identifiers information
- https://www.nuget.org/packages/TraceIdentifiers.HttpClient
Integration with `Microsoft.Extensions.Http`. Configure `IHttpClientBuilder` to append trace identifiers to requests executed from HttpClient created from `IHttpClientFactory` in ASP.NET Core applications

# Get Started
## Setup in ASP.NET Core
```
public void ConfigureServices(IServiceCollection services)
{
    services.AddTraceIdentifiers();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // Should be configured as eraly as possible in middlewares
    app.UseTraceIdentifiers();
}
```
## Serilog Integration
```
// Before app.UseTraceIdentifiers();
TraceIdentifiersContext.Startup.LinkToSerilogLogContext();

// in Serilog configuration ensure that `FromLogContext` enricher used.
LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
loggerConfiguration.Enrich.FromLogContext();
```
## Add Identifiers to HttpRequestMessage
```
TraceIdentifiersContext context;
HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "some address");

// add both local shared and remote shared identifiers
request.TryAddLocalSharedAndRemoteShared(context, SendIdentifiersOptions.Default);

// add local shared identifiers
request.TryAddLocalShared(context, SendIdentifiersOptions.Default);

// add remote shared identifiers
request.TryAddLocalShared(context, SendIdentifiersOptions.Default);
```
## Read Identifier from HttpResponseMessage
```
HttpResponseMessage response = await httpClient.SendAsync(request);
string remote = response.ReadRemoteIdentifier();
```
## Access TraceIdentifiersContext from MVC Controller
```
    public class HomeController : Controller
    {
        public ITraceIdentifiersAccessor TraceIdentifiersAccessor { get; }

        public HomeController(ITraceIdentifiersAccessor traceIdentifiersAccessor)
        {
            TraceIdentifiersAccessor = traceIdentifiersAccessor ?? throw new ArgumentNullException(nameof(traceIdentifiersAccessor));
        }

        [HttpGet]
        [Route("/mvc")]
        public IActionResult Index()
        {
            return this.Ok(new
            {
                this.TraceIdentifiersAccessor.TraceIdentifiersContext.Local,
                this.TraceIdentifiersAccessor.TraceIdentifiersContext.Remote
            });
        }
    }
```


