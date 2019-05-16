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

        public Func<HttpContext, string> WriteLocalHeaderName { get; set; } = (c) => TraceIdentifiersSendOptions.DefaultHeaderName;

        public Func<HttpContext, bool> ReadRemote { get; set; } = (c) => true;

        public Func<HttpContext, string> ReadRemoteHeaderName { get; set; } = (c) => TraceIdentifiersSendOptions.DefaultHeaderName;

        public Func<HttpContext, char> ReadRemoteSeparator { get; set; } = (c) => TraceIdentifiersSendOptions.DefaultSeparator;

        public Func<HttpContext, int> ReadRemoteMaxCount { get; set; } = (c) => TraceIdentifiersSendOptions.DefaultIdentifiersMaxCount;

        public Func<HttpContext, int> ReadRemoteMaxLength { get; set; } = (c) => TraceIdentifiersSendOptions.DefaultIdentifierMaxLength;
    }
}
