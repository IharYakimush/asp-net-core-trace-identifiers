using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

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
            ICollection<string> all = this.TryToReadRemote(context).ToArray();

            TraceIdentifiersContext feature = TraceIdentifiersContext.Startup
                .CloneForThread();

            string local = this.Options.LocalValueFactory(context);

            feature = feature.CreateChildWithLocal(this.Options.LocalIsShared(context), local);

            try
            {
                using (feature)
                {
                    if (all.Any())
                    {
                        using (var withRemote = feature.CreateChildWithRemote(all))
                        {
                            context.Features.Set(withRemote);
                            await _next(context);
                        }
                    }
                    else
                    {
                        context.Features.Set(feature);
                        await _next(context);
                    }
                }
            }
            finally
            {
                this.TryToWriteLocal(context, local);
            }
        }

        private void TryToWriteLocal(HttpContext context, string value)
        {
            if (this.Options.LocalIsWrite(context))
            {
                if (!context.Response.HasStarted)
                {
                    string writeLocalHeaderName = this.Options.WriteLocalHeaderName(context);
                    if (!context.Response.Headers.ContainsKey(writeLocalHeaderName))
                    {
                        context.Response.Headers[writeLocalHeaderName] = value;
                    }
                }
            }
        }

        private IEnumerable<string> TryToReadRemote(HttpContext context)
        {
            if (this.Options.ReadRemote(context) && context.Request.Headers.TryGetValue(this.Options.ReadRemoteHeaderName(context), out StringValues stringValues))
            {
                char separator = this.Options.ReadRemoteSeparator(context);
                int maxLength = this.Options.ReadRemoteMaxLength(context);
                int maxCount = this.Options.ReadRemoteMaxCount(context);

                IEnumerable<string> allValues = stringValues.SelectMany(str =>
                    str.Contains(separator)
                        ? str.Split(separator)
                        : Enumerable.Repeat(str, 1)).Select(s => s.Trim());

                return allValues.Where(v => !string.IsNullOrWhiteSpace(v) && v.Length <= maxLength).Take(maxCount);
            }

            return Enumerable.Empty<string>();
        }
    }
}
