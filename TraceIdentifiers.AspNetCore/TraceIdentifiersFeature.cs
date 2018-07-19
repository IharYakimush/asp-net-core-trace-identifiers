﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersFeature
    {
        private readonly HashSet<string> all;

        public TraceIdentifiersFeature(string current, IEnumerable<string> all)
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
            this.all = new HashSet<string>(all);
            this.all.Add(current);
        }

        public string Current { get; }

        public IEnumerable<string> All => this.all;
    }
}
