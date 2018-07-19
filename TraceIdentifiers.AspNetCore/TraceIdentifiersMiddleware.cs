using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TraceIdentifiers.AspNetCore
{
    class TraceIdentifiersMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdentifiersMiddleware(RequestDelegate next, IOptions<TraceIdentifiersMiddlewareOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this._next = next;
            Options = options.Value;
        }

        public TraceIdentifiersMiddlewareOptions Options { get; }

        public async Task InvokeAsync(HttpContext context)
        {
            TryToWriteTraceIdentifier(context);
            IEnumerable<string> all = TryToReadTraceIdentifier(context);

            TraceIdentifiersFeature feature = new TraceIdentifiersFeature(context.TraceIdentifier, all);
            context.Features.Set(feature);
            await _next(context);
            TryToWriteTraceIdentifier(context);
        }

        private void TryToWriteTraceIdentifier(HttpContext context)
        {
            if (this.Options.AppendCurrentToResponse)
            {
                if (!context.Response.HasStarted)
                {
                    if (!context.Response.Headers.ContainsKey(this.Options.AppendCurrentHeaderName))
                    {
                        context.Response.Headers[this.Options.AppendCurrentHeaderName] = context.TraceIdentifier;
                    }
                }
            }
        }

        private IEnumerable<string> TryToReadTraceIdentifier(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(this.Options.RequestIdentifiersHeaderName, out var vals))
            {                
                IEnumerable<string> allValues = vals.SelectMany(str => 
                str.Contains(this.Options.RequestIdentifiersSeparator) 
                    ? str.Split(this.Options.RequestIdentifiersSeparator) 
                    : Enumerable.Repeat(str, 1));

                return allValues.Where(v => !string.IsNullOrWhiteSpace(v))
                    .Take(this.Options.RequestIdentifiersMaxCount);
            }

            return Enumerable.Empty<string>();
        }
    }
}
