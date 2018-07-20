using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TraceIdentifiers
{
    public class HttpClientWithTraceIdentifiersHandler : HttpClientHandler
    {
        private readonly TraceIdentifiersCollection _traceIdentifiers;
        private readonly TraceIdentifiersSendOptions _options;

        public HttpClientWithTraceIdentifiersHandler(TraceIdentifiersCollection traceIdentifiers, TraceIdentifiersSendOptions options)
        {
            _traceIdentifiers = traceIdentifiers ?? throw new ArgumentNullException(nameof(traceIdentifiers));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.AppendTraceIdentifiers(this._traceIdentifiers, this._options);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}