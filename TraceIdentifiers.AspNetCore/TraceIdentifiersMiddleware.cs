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
            this.TryToWriteLocal(context);
            ICollection<string> all = this.TryToReadRemote(context).ToArray();

            TraceIdentifiersContext feature = TraceIdentifiersContext.StartupEmpty
                .CloneForThread();

            string remote = this.TryToReadRemoteSingle(context);

            feature = feature.CreateChildWithLocal(this.Options.ShareLocal, context.TraceIdentifier);

            using (feature)
            {
                if (all.Any() || remote != null)
                {
                    using (var withRemote = feature.CreateChildWithRemote(all, remote))
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
                       
            this.TryToWriteLocal(context);
        }

        private string TryToReadRemoteSingle(HttpContext context)
        {
            string result = null;

            return result;
        }

        private void TryToWriteLocal(HttpContext context)
        {
            if (this.Options.WriteLocal)
            {
                if (!context.Response.HasStarted)
                {
                    if (!context.Response.Headers.ContainsKey(this.Options.WriteLocalHeaderName))
                    {
                        context.Response.Headers[this.Options.WriteLocalHeaderName] = context.TraceIdentifier;
                    }
                }
            }
        }

        private IEnumerable<string> TryToReadRemote(HttpContext context)
        {
            if (this.Options.ReadRemote(context) && context.Request.Headers.TryGetValue(this.Options.ReadRemoteHeaderName(context), out var vals))
            {
                char separator = this.Options.ReadRemoteSeparator(context);
                int maxLength = this.Options.ReadRemoteMaxLength(context);
                int maxCount = this.Options.ReadRemoteMaxCount(context);

                IEnumerable<string> allValues = vals.SelectMany(str =>
                    str.Contains(separator)
                        ? str.Split(separator)
                        : Enumerable.Repeat(str, 1));

                return allValues.Where(v => !string.IsNullOrWhiteSpace(v) && v.Length <= maxLength).Take(maxCount);
            }

            return Enumerable.Empty<string>();
        }
    }
}
