﻿using System;

namespace TraceIdentifiers.Serilog
{
    using System.Collections.Generic;
    using System.Linq;

    using global::Serilog.Context;
    using global::Serilog.Core;

    using TraceIdentifiers.AspNetCore;

    public static class SerilogExtensions
    {
        public static TraceIdentifiersContext LinkToSerilogLogContext(
            this TraceIdentifiersContext traceIdentifiersContext, Action<LogContextBuilder> settings = null)
        {
            LogContextBuilder builder = new LogContextBuilder();
            settings?.Invoke(builder);

            LinkEnrichersToContext(traceIdentifiersContext, builder);

            traceIdentifiersContext.OnChildCreated += (sender, args) =>
                {
                    TraceIdentifiersContext context = (TraceIdentifiersContext)sender;

                    LinkEnrichersToContext(context, builder);
                };

            return traceIdentifiersContext;
        }

        private static void LinkEnrichersToContext(TraceIdentifiersContext context, LogContextBuilder builder)
        {
            ILogEventEnricher[] enrichers = builder.Factories.Select(
                func =>
                    {
                        try
                        {
                            return func(context);
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(e => e != null).ToArray();

            if (enrichers.Any())
            {
                IDisposable disposable = LogContext.Push(enrichers);
                context.Link(disposable);
            }
        }
    }
}