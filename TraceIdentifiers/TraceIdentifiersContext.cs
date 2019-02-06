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

        private LinkedList<IEnumerable<string>> remoteShared { get; set; } = new LinkedList<IEnumerable<string>>();

        private LinkedListNode<IEnumerable<string>> remoteBookmark = null;

        private string localBookmark;

        private const int MaxNestedLevel = 1000;

        private bool disposed = false;

        private LinkedList<IDisposable> linkedDisposable = new LinkedList<IDisposable>();

        public event EventHandler<EventArgs> OnChildCreated;

        public static TraceIdentifiersContext StartupEmpty { get; } = new TraceIdentifiersContext(
            new Stack<KeyValuePair<string, bool>>(),
            new LinkedList<IEnumerable<string>>(), 
            null);

        public static string StartupId { get; set; } = GetNonSecureRandomString(4);

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

        public IEnumerable<string> Local => this.local.Select(p => p.Key).SkipWhile(s => s != this.localBookmark);

        public IEnumerable<string> LocalShared => this.local.Where(p => p.Value).Select(p => p.Key).SkipWhile(s => s != this.localBookmark);

        public string Remote { get; set; }

        public IEnumerable<string> RemoteShared => this.remoteShared.SelectMany(enumerable => enumerable);

        public TraceIdentifiersContext(
            bool shared = true,
            string traceIdentifier = null)
        {
            this.local = new Stack<KeyValuePair<string, bool>>();

            this.AddIdentifierToLocal(shared, traceIdentifier);
        }

        public TraceIdentifiersContext Link(IDisposable disposable)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));

            this.linkedDisposable.AddLast(disposable);

            return this;
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

        private TraceIdentifiersContext(Stack<KeyValuePair<string, bool>> local, LinkedList<IEnumerable<string>> remoteShared, string remote)
        {
            this.local = local;

            this.remoteShared = remoteShared;

            this.Remote = remote;
        }

        private TraceIdentifiersContext(
            Stack<KeyValuePair<string, bool>> local,
            LinkedList<IEnumerable<string>> remoteShared,
            string remote,
            string traceIdentifier,
            bool shared)
            : this(local, remoteShared, remote)
        {
            this.AddIdentifierToLocal(shared, traceIdentifier);
        }

        public TraceIdentifiersContext CreateChildWithLocal(bool shared = true, string local = null)
        {
            if (this.localBookmark == this.local.Peek().Key)
            {
                TraceIdentifiersContext result = new TraceIdentifiersContext(this.local, this.remoteShared, this.Remote, local, shared);

                result.OnChildCreated = this.OnChildCreated;
                this.OnChildCreated?.Invoke(result, EventArgs.Empty);
                return result;
            }
            
            throw new InvalidOperationException("Unable to create child context, because previous context with same nested level not disposed");
        }

        public TraceIdentifiersContext CreateChildWithRemote(IEnumerable<string> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            TraceIdentifiersContext result = new TraceIdentifiersContext(this.local, this.remoteShared, this.Remote);
            result.remoteBookmark = result.remoteShared.AddLast(values);
            result.localBookmark = this.localBookmark;
            result.OnChildCreated = this.OnChildCreated;
            this.OnChildCreated?.Invoke(result, EventArgs.Empty);
            return result;
        }

        public TraceIdentifiersContext CloneForThread()
        {
            Stack<KeyValuePair<string, bool>> stack = new Stack<KeyValuePair<string, bool>>(this.local.Reverse());
            LinkedList<IEnumerable<string>> list = new LinkedList<IEnumerable<string>>(this.remoteShared.Select(enm => enm.ToArray()).ToArray());

            TraceIdentifiersContext result = new TraceIdentifiersContext(stack, list, this.Remote);

            result.OnChildCreated = this.OnChildCreated;
            result.localBookmark = this.localBookmark;
            
            if (this.remoteBookmark != null)
            {
                LinkedListNode<IEnumerable<string>> nthis = this.remoteShared.First;
                LinkedListNode<IEnumerable<string>> nresult = result.remoteShared.First;

                while (this.remoteBookmark != nthis)
                {
                    nthis = nthis.Next;
                    nresult = nresult.Next;
                }

                result.remoteBookmark = nresult;
            }

            return result;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                throw new InvalidOperationException("Already disposed");
            }

            this.disposed = true;

            if (this.remoteBookmark != null)
            {
                this.remoteShared.Remove(this.remoteBookmark);
            }
            else
            {
                if (this.localBookmark == this.local.Peek().Key)
                {
                    this.local.Pop();
                }
            }

            if (this.linkedDisposable.Any())
            {
                List<Exception> inner = new List<Exception>(this.linkedDisposable.Count);

                foreach (IDisposable disposable in this.linkedDisposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        inner.Add(exception);
                    }
                }

                if (inner.Any())
                {
                    throw new AggregateException("Exceptions in linked disposables", inner);
                }
            }
        }
    }
}