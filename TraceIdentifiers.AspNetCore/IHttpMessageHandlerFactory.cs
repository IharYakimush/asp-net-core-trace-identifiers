using System.Net.Http;
using TraceIdentifiers.Abstractions;

namespace TraceIdentifiers.AspNetCore
{
    public interface IHttpMessageHandlerFactory
    {
        HttpMessageHandler Create(TraceIdentifiersSendOptions options = null);
    }
}