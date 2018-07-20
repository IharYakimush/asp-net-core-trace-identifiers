using System;
using System.Collections.Generic;

namespace TraceIdentifiers
{
    public class TraceIdentifiersCollection
    {
        private readonly HashSet<string> all;

        public TraceIdentifiersCollection(string current, IEnumerable<string> all)
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
            this.all = new HashSet<string>(all);           
        }

        public string Current { get; }

        public IEnumerable<string> All => this.all;
    }
}
