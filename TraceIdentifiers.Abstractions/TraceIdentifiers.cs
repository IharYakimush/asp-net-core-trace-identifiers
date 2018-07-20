using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TraceIdentifiers.Abstractions
{
    public class TraceIdentifiers
    {
        private readonly HashSet<string> all;

        public TraceIdentifiers(string current, IEnumerable<string> all)
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
            this.all = new HashSet<string>(all);           
        }

        public string Current { get; }

        public IEnumerable<string> All => this.all;
    }
}
