namespace TraceIdentifiers.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TraceIdentifiersContext : IDisposable
    {
        private static readonly char[] Chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        private Stack<KeyValuePair<string,bool>> local;

        private string localBookmark;

        private const int MaxNestedLevel = 1000;

        public static string GetNonSecureRandomString(int length = 8)
        {
            if (length < 1 || length > 16)
            {
                throw new ArgumentException();
            }

            StringBuilder result = new StringBuilder(length);
            foreach (byte b in Guid.NewGuid().ToByteArray().Take(length))
            {
                result.Append(Chars[b % Chars.Length]);
            }

            return result.ToString();
        }

        public IEnumerable<string> Local => this.local.Select(p => p.Key);

        public IEnumerable<string> LocalShared => this.local.Where(p => p.Value).Select(p => p.Key);

        public string Remote { get; set; }

        public LinkedList<Stack<string>> RemoteShared { get; set; } = new LinkedList<Stack<string>>();

        public TraceIdentifiersContext(
            bool shared = true,
            string traceIdentifier = null)
        {
            this.local = new Stack<KeyValuePair<string, bool>>();

            this.AddIdentifierToLocal(shared, traceIdentifier);
        }

        private void AddIdentifierToLocal(bool shared, string traceIdentifier)
        {
            if (traceIdentifier != null)
            {
                if (this.local.Any(p => p.Key == traceIdentifier))
                {
                    throw new ArgumentException($"Identifier with same name {traceIdentifier} already used.");
                }
            }

            if (this.local.Count >= MaxNestedLevel)
            {
                throw new InvalidOperationException($"Limit of {MaxNestedLevel} for nested child elements exceeded");
            }

            if (traceIdentifier == null)
            {
                do
                {
                    traceIdentifier = GetNonSecureRandomString();
                }
                while (this.local.Any(p => p.Key == traceIdentifier));
            }

            this.local.Push(new KeyValuePair<string, bool>(traceIdentifier, shared));
            this.localBookmark = traceIdentifier;
        }

        private TraceIdentifiersContext(Stack<KeyValuePair<string, bool>> local, string traceIdentifier, bool shared)
        {
            this.local = local;

            this.AddIdentifierToLocal(shared, traceIdentifier);
        }

        public TraceIdentifiersContext CreateChild(bool shared = true, string local = null)
        {
            if (this.localBookmark == this.local.Peek().Key)
            {
                return new TraceIdentifiersContext(this.local, local, shared);
            }
            
            throw new InvalidOperationException("Unable to create child context, because previous context with same nested level not disposed");
        }

        public void Dispose()
        {
            if (this.localBookmark == this.local.Peek().Key)
            {
                this.local.Pop();
            }            
        }
    }
}