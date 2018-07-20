using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersMiddlewareOptions : IOptions<TraceIdentifiersMiddlewareOptions>
    {
        private const string DefaultHeaderName = TraceIdentifiersSendOptions.DefaultHeaderName;

        public TraceIdentifiersMiddlewareOptions Value => this;

        public bool AppendCurrentToResponse { get; set; } = true;

        public string AppendCurrentHeaderName { get; set; } = DefaultHeaderName;

        public bool ReadRequestIdentifiers { get; set; } = true;

        public string RequestIdentifiersHeaderName { get; set; } = DefaultHeaderName;

        public char RequestIdentifiersSeparator { get; set; } = TraceIdentifiersSendOptions.DefaultSeparator;

        public int RequestIdentifiersMaxCount { get; set; } = TraceIdentifiersSendOptions.DefaultIdentifiersMaxCount;

        public int RequestIdentifierMaxLength { get; set; } = TraceIdentifiersSendOptions.DefaultIdentifierMaxLength;
    }
}
