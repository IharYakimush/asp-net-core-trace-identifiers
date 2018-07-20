using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public class HttpClientHandlerWithTraceIdentifiersFactory : IHttpMessageHandlerFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientHandlerWithTraceIdentifiersFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public HttpMessageHandler Create(TraceIdentifiersSendOptions options = null)
        {
            HttpContext httpContext = this._httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException($"Unable to get {nameof(HttpContext)} from {nameof(IHttpContextAccessor)}.");
            }

            TraceIdentifiersCollection identifiers = httpContext.Features.Get<TraceIdentifiersCollection>();

            if (identifiers == null)
            {
                throw new InvalidOperationException($"{nameof(HttpContext)} do not have {nameof(TraceIdentifiersCollection)} feature. Ensure that {nameof(TraceIdentifiersMiddleware)} registered before current one.");
            }

            return new HttpClientWithTraceIdentifiersHandler(identifiers, options ?? TraceIdentifiersSendOptions.Default);
        }
    }
}