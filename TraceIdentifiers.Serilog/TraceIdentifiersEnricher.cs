using System;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace TraceIdentifiers.Serilog
{
    public class TraceIdentifiersEnricher : ILogEventEnricher, IDisposable
    {
        public TraceIdentifiersContext Context { get; }
        public LogContextBuilder Builder { get; }

        private ILogEventEnricher[] Enrichers = new ILogEventEnricher[0];

        public TraceIdentifiersEnricher(TraceIdentifiersContext context, LogContextBuilder builder)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.UpdateEnrichers();
            this.Context.OnChildCreated += ContextOnChildCreated;
        }

        private void ContextOnChildCreated(object sender, EventArgs e)
        {
            this.UpdateEnrichers();
            TraceIdentifiersContext child = (TraceIdentifiersContext) sender;
            child.Link(this); // Update enrichers after disposing child
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (ILogEventEnricher enricher in this.Enrichers)
            {
                enricher.Enrich(logEvent, propertyFactory);
            }
        }

        private void UpdateEnrichers()
        {
            this.Enrichers = this.Builder.Factories.Select(
                func =>
                {
                    try
                    {
                        return func(this.Context);
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(e => e != null).ToArray();
        }

        public void Dispose()
        {
            this.UpdateEnrichers();
        }
    }
}