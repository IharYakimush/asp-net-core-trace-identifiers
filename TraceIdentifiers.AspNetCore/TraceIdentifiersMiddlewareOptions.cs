using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersMiddlewareOptions : IOptions<TraceIdentifiersMiddlewareOptions>
    {
        private const string DefaultHeaderName = "X-TraceIdentifier";

        public TraceIdentifiersMiddlewareOptions Value => this;

        public bool AppendCurrentToResponse { get; set; } = true;

        public string AppendCurrentHeaderName { get; set; } = DefaultHeaderName;

        public bool ReadRequestIdentifiers { get; set; } = true;

        public string RequestIdentifiersHeaderName { get; set; } = DefaultHeaderName;

        public char RequestIdentifiersSeparator { get; set; } = '|';

        public int RequestIdentifiersMaxCount { get; set; } = 10;

        public int RequestIdentifiersMaxLength { get; set; } = 32;
    }
}
