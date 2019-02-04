namespace TraceIdentifiers.Serilog
{
    using System;
    using System.Collections.Generic;

    using global::Serilog.Core;

    using TraceIdentifiers.AspNetCore;

    public class LogContextBuilder
    {
        public List<Func<TraceIdentifiersContext, ILogEventEnricher>> Factories { get; } =
            new List<Func<TraceIdentifiersContext, ILogEventEnricher>>();
    }
}