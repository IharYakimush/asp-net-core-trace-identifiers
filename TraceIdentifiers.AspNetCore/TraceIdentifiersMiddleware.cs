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
            ICollection<string> all = this.TryToReadRemoteShared(context).ToArray();

            TraceIdentifiersContext feature = TraceIdentifiersContext.StartupEmpty
                .CloneForThread();

            feature.Remote = this.TryToReadRemoteSingle(context);

            feature = feature.CreateChildWithLocal(this.Options.ShareLocal, context.TraceIdentifier);

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

        private IEnumerable<string> TryToReadRemoteShared(HttpContext context)
        {
            if (this.Options.ReadRemoteShared && context.Request.Headers.TryGetValue(this.Options.RemoteSharedHeaderName, out var vals))
            {                
                IEnumerable<string> allValues = vals.SelectMany(str => 
                str.Contains(this.Options.RemoteSharedSeparator) 
                    ? str.Split(this.Options.RemoteSharedSeparator) 
                    : Enumerable.Repeat(str, 1));

                return allValues.Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(s => s.Length > this.Options.RemoteSharedMaxLength ? s.Substring(0, this.Options.RemoteSharedMaxLength) : s)
                    .Take(this.Options.RemoteSharedMaxCount);
            }

            return Enumerable.Empty<string>();
        }
    }
}
