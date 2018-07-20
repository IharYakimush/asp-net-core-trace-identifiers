using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public static class TraceIdentifiersExtensions
    {
        public static IAppWithTraceIdentifiersBuilder UseTraceIdentifiers(this IApplicationBuilder app, TraceIdentifiersMiddlewareOptions options = null)
        {
            options = options ?? new TraceIdentifiersMiddlewareOptions();
            IApplicationBuilder builder = app.UseMiddleware<TraceIdentifiersMiddleware>(Options.Create(options));

            return new AppBuilderWithTraceIdentifiers(builder);
        }

        public static IServiceCollection AddTraceIdentifiersClient(this IServiceCollection services)
        {
            services.TryAddScoped<IHttpMessageHandlerFactory, HttpClientHandlerWithTraceIdentifiersFactory>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }
}
