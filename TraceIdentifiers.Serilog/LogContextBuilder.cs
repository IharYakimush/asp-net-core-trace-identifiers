namespace TraceIdentifiers.Serilog
{
    using System;
    using System.Collections.Generic;

    using global::Serilog.Core;

    public class LogContextBuilder
    {
        public bool EscapeRemote { get; set; } = true;

        public List<Func<TraceIdentifiersContext, ILogEventEnricher>> Factories { get; } =
            new List<Func<TraceIdentifiersContext, ILogEventEnricher>>();
    }
}