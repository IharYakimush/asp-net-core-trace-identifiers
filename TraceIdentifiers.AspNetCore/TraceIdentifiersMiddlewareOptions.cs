using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersMiddlewareOptions : IOptions<TraceIdentifiersMiddlewareOptions>
    {
        public TraceIdentifiersMiddlewareOptions Value => this;

        public bool AppendCurrentToResponse { get; set; } = true;

        public string AppendCurrentHeaderName { get; set; } = "X-TraceIdentifier";
    }
}
