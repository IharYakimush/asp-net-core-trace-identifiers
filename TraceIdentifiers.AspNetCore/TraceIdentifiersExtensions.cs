using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public static IServiceCollection AddTraceIdentifiers(this IServiceCollection services)
        {
            services.TryAddScoped<ITraceIdentifiersAccessor, TraceIdentifiersFromHttpContextAccessor>();

            return services;
        }
    }
}
