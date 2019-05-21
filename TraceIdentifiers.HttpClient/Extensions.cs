using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TraceIdentifiers.HttpClient
{
    public static class Extensions
    {
        public static IHttpClientBuilder SendTraceIdentifiersFromHttpContext(this IHttpClientBuilder builder, Action<HttpRequestMessage,TraceIdentifiersContext> setupIdentifiers = null)
        {
            builder.AddHttpMessageHandler(configureHandler: provider =>
            {
                IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                TraceIdentifiersContext context = httpContextAccessor.HttpContext.Features.Get<TraceIdentifiersContext>();

                if (setupIdentifiers == null)
                {
                    return new SendIdentifiersDelegatingHandler(request =>
                        request.TryAddLocalSharedAndRemoteShared(context, SendIdentifiersOptions.Default));
                }

                return new SendIdentifiersDelegatingHandler(request =>
                    setupIdentifiers.Invoke(request, context));
            });

            return builder;
        }
    }
}