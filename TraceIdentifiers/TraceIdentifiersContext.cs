﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceIdentifiers
{
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

        public event EventHandler<EventArgs> OnClonedForThread;

        public static string StartupId { get; set; } = GetNonSecureRandomString(4);

        public static TraceIdentifiersContext Startup { get; } = new TraceIdentifiersContext(
            new Stack<KeyValuePair<string, bool>>(),
            new LinkedList<IEnumerable<string>>());

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

        private TraceIdentifiersContext(Stack<KeyValuePair<string, bool>> local, LinkedList<IEnumerable<string>> remoteShared)
        {
            this.local = local;

            this.remoteShared = remoteShared;
        }

        private TraceIdentifiersContext(
            Stack<KeyValuePair<string, bool>> local,
            LinkedList<IEnumerable<string>> remoteShared,
            string traceIdentifier,
            bool shared)
            : this(local, remoteShared)
        {
            this.AddIdentifierToLocal(shared, traceIdentifier);
        }

        public TraceIdentifiersContext CreateChildWithLocal(bool shared = true, string local = null)
        {
            if (!this.local.Any() || this.localBookmark == this.local.Peek().Key)
            {
                TraceIdentifiersContext result = new TraceIdentifiersContext(this.local, this.remoteShared, local, shared);

                result.OnChildCreated = this.OnChildCreated;
                result.OnClonedForThread = this.OnClonedForThread;
                this.OnChildCreated?.Invoke(result, EventArgs.Empty);
                return result;
            }
            
            throw new InvalidOperationException("Unable to create child context, because previous context with same nested level not disposed");
        }

        public TraceIdentifiersContext CreateChildWithRemote(IEnumerable<string> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            TraceIdentifiersContext result = new TraceIdentifiersContext(this.local, this.remoteShared);
            result.remoteBookmark = result.remoteShared.AddLast(values);
            result.localBookmark = this.localBookmark;
            result.OnChildCreated = this.OnChildCreated;
            result.OnClonedForThread = this.OnClonedForThread;
            this.OnChildCreated?.Invoke(result, EventArgs.Empty);
            return result;
        }

        public TraceIdentifiersContext CloneForThread()
        {
            Stack<KeyValuePair<string, bool>> stack = new Stack<KeyValuePair<string, bool>>(this.local.Reverse());
            LinkedList<IEnumerable<string>> list = new LinkedList<IEnumerable<string>>(this.remoteShared.Select(enm => enm.ToArray()).ToArray());

            TraceIdentifiersContext result = new TraceIdentifiersContext(stack, list);

            result.localBookmark = this.localBookmark;
            result.OnClonedForThread = this.OnClonedForThread;
            
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

            this.OnClonedForThread?.Invoke(result, EventArgs.Empty);

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