using Microsoft.Extensions.Options;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersMiddlewareOptions : IOptions<TraceIdentifiersMiddlewareOptions>
    {
        private const string DefaultHeaderName = TraceIdentifiersSendOptions.DefaultHeaderName;

        public TraceIdentifiersMiddlewareOptions Value => this;

        public bool WriteLocal { get; set; } = true;
        public bool ShareLocal { get; set; } = true;

        public string WriteLocalHeaderName { get; set; } = DefaultHeaderName;

        public bool ReadRemoteShared { get; set; } = true;

        public string RemoteSharedHeaderName { get; set; } = DefaultHeaderName;

        public char RemoteSharedSeparator { get; set; } = TraceIdentifiersSendOptions.DefaultSeparator;

        public int RemoteSharedMaxCount { get; set; } = TraceIdentifiersSendOptions.DefaultIdentifiersMaxCount;

        public int RemoteSharedMaxLength { get; set; } = TraceIdentifiersSendOptions.DefaultIdentifierMaxLength;
    }
}
