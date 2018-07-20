using System.Net.Http;

namespace TraceIdentifiers.AspNetCore
{
    public interface IHttpMessageHandlerFactory
    {
        HttpMessageHandler Create(TraceIdentifiersSendOptions options = null);
    }
}