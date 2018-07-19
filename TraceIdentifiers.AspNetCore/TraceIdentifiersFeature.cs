using System;
using System.Collections.Generic;
using System.Text;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersFeature : List<string>
    {
        public TraceIdentifiersFeature(string current)
        {
            Current = current ?? throw new ArgumentNullException(nameof(current));
        }

        public string Current { get; }
    }
}
