using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TraceIdentifiers
{
    public static class HttpRequestTraceIdentifiersExtensions
    {
        public static HttpRequestMessage AppendTraceIdentifiers(
            this HttpRequestMessage request, 
            TraceIdentifiersCollection traceIdentifiers,
            TraceIdentifiersSendOptions options)
        {
            if (!request.Headers.Contains(options.HeaderHame))
            {
                IEnumerable<string> values = traceIdentifiers.All
                    .Select(s => s.Length > options.IdentifierMaxLength ? s.Substring(0, options.IdentifierMaxLength) : s)
                    .Take(options.IdentifiersMaxCount);
                
                if (options.AllwoMultipleHeaders)
                {
                    request.Headers.Add(options.HeaderHame, values);
                }
                else
                {
                    request.Headers.Add(options.HeaderHame, string.Join(options.SingleHeaderSeparator, values));
                }                
            }

            return request;
        }
    }
}