using System;
using Serilog.Core.Enrichers;

namespace TraceIdentifiers.Serilog
{
    using System.Collections.Generic;
    using System.Linq;

    using global::Serilog.Context;
    using global::Serilog.Core;

    public static class SerilogExtensions
    {
        public static TraceIdentifiersContext LinkToSerilogLogContext(
            this TraceIdentifiersContext context, Action<LogContextBuilder> settings = null)
        {
            LogContextBuilder builder = new LogContextBuilder();

            if (settings != null)
            {
                settings.Invoke(builder);
            }
            else
            {
                builder.WithDefaults();
            }
            
            LinkEnrichersToContext(context, builder);

            context.OnClonedForThread += (sender, args) =>
            {
                TraceIdentifiersContext newContext = (TraceIdentifiersContext) sender;
                LinkEnrichersToContext(newContext, builder);
            };

            return context;
        }

        

        private static void LinkEnrichersToContext(TraceIdentifiersContext context, LogContextBuilder builder)
        {
            TraceIdentifiersEnricher traceIdentifiersEnricher = new TraceIdentifiersEnricher(context, builder);
            IDisposable disposable = LogContext.Push(traceIdentifiersEnricher);
            context.Link(disposable);            
        }
    }
}
