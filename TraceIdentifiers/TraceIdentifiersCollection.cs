using System;
using System.Collections.Generic;
using System.Linq;

namespace TraceIdentifiers
{
    public class TraceIdentifiersCollection
    {
        private readonly HashSet<string> all;

        public TraceIdentifiersCollection(string current, IEnumerable<string> all)
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
            this.all = new HashSet<string>(all.Concat(Enumerable.Repeat(current, 1)));
        }

        public string Current { get; }

        public IEnumerable<string> All => this.all;
    }
}
