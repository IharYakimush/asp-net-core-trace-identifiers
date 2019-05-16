using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public static class TraceIdentifiersExtensions
    {
        public static IApplicationBuilder UseTraceIdentifiers(this IApplicationBuilder app, TraceIdentifiersMiddlewareOptions options = null)
        {
            options = options ?? new TraceIdentifiersMiddlewareOptions();            
            app.UseMiddleware<TraceIdentifiersMiddleware>(Options.Create(options));

            return app;
        }        
    }
}
