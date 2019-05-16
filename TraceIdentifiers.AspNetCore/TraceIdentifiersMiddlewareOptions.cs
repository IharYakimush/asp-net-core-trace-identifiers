using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersMiddlewareOptions : IOptions<TraceIdentifiersMiddlewareOptions>
    {
        public TraceIdentifiersMiddlewareOptions Value => this;

        public Func<HttpContext, bool> LocalIsWrite { get; set; } = (c) => true;

        public Func<HttpContext, bool> LocalIsShared { get; set; } = (c) => true;
        public Func<HttpContext, string> LocalValueFactory { get; set; } = (c) => c.TraceIdentifier;

        public Func<HttpContext, string> WriteLocalHeaderName { get; set; } = (c) => TraceIdentifiersDefaults.DefaultHeaderName;

        public Func<HttpContext, bool> ReadRemote { get; set; } = (c) => true;

        public Func<HttpContext, string> ReadRemoteHeaderName { get; set; } = (c) => TraceIdentifiersDefaults.DefaultSharedHeaderName;

        public Func<HttpContext, char> ReadRemoteSeparator { get; set; } = (c) => TraceIdentifiersDefaults.DefaultSeparator;

        public Func<HttpContext, int> ReadRemoteMaxCount { get; set; } = (c) => TraceIdentifiersDefaults.DefaultIdentifiersMaxCount;

        public Func<HttpContext, int> ReadRemoteMaxLength { get; set; } = (c) => TraceIdentifiersDefaults.DefaultIdentifierMaxLength;
    }
}
