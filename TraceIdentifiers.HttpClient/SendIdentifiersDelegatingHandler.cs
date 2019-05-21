using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Internal;

namespace TraceIdentifiers.HttpClient
{
    public class SendIdentifiersDelegatingHandler : DelegatingHandler
    {
        public Action<HttpRequestMessage> RequestAction { get; }

        public SendIdentifiersDelegatingHandler(Action<HttpRequestMessage> requestAction)
        {
            RequestAction = requestAction ?? throw new ArgumentNullException(nameof(requestAction));
        }
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            this.RequestAction.Invoke(request);

            return base.SendAsync(request, cancellationToken);
        }
    }
}