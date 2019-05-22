using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TraceIdentifiers
{
    public static class SendIdentifiersExtensions
    {
        public static bool TryAddLocalSharedAndRemoteShared(
            this HttpRequestMessage message,
            TraceIdentifiersContext context, 
            SendIdentifiersOptions options)
        {
            return message.Headers.TryAddWithoutValidation(options.HeaderName,
                Normalize(context.LocalShared.Reverse().Concat(context.RemoteShared), options));
        }

        public static bool TryAddLocalShared(this HttpRequestMessage message,
            TraceIdentifiersContext context, SendIdentifiersOptions options)
        {
            return message.Headers.TryAddWithoutValidation(options.HeaderName,
                Normalize(context.LocalShared.Reverse(), options));
        }
        public static bool TryAddRemoteShared(this HttpRequestMessage message,
            TraceIdentifiersContext context, SendIdentifiersOptions options)
        {
            return message.Headers.TryAddWithoutValidation(options.HeaderName,
                Normalize(context.RemoteShared, options));
        }

        public static string ReadRemoteIdentifier(this HttpResponseMessage message,
            string headerName = TraceIdentifiersDefaults.DefaultHeaderName)
        {
            if (message.Headers.TryGetValues(headerName, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public static TraceIdentifiersContext CreateChildWithRemote(this TraceIdentifiersContext context, string value, bool shared = true)
        {
            return context.CreateChildWithRemote(Enumerable.Repeat(value, 1), shared);
        }

        private static IEnumerable<string> Normalize(IEnumerable<string> values, SendIdentifiersOptions options)
        {
            if (options.UseSeparator)
            {
                values = Enumerable.Repeat(string.Join(options.Separator, values), 1);
            }

            return values;
        }
    }
}
