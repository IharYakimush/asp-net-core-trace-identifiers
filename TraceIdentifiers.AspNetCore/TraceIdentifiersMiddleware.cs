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
            TryToReadTraceIdentifier(context);
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

        private void TryToReadTraceIdentifier(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(this.Options.RequestIdentifiersHeaderName, out var vals))
            {                
                IEnumerable<string> allValues = vals.SelectMany(str => 
                str.Contains(this.Options.RequestIdentifiersSeparator) 
                    ? str.Split(this.Options.RequestIdentifiersSeparator) 
                    : Enumerable.Repeat(str, 1));

                TraceIdentifiersFeature result = new TraceIdentifiersFeature(context.TraceIdentifier);
                result.AddRange(allValues.Where(v => !string.IsNullOrWhiteSpace(v)).Take(this.Options.RequestIdentifiersMaxCount));

                context.Features.Set(result);
            }
        }
    }
}
