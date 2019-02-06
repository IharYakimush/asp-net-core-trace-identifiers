using System;
using Serilog.Core.Enrichers;
using Serilog.Enrichers.AspNetCore.HttpContext;

namespace TraceIdentifiers.AspNetCore.Serilog
{
    public static class TraceIdentifiersExtensions
    {
        public static IAppWithTraceIdentifiersBuilder PushToSerilogContext(this IAppWithTraceIdentifiersBuilder app, Action<SerilogTraceIdentifiersOptions> settings = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var options = new SerilogTraceIdentifiersOptions();

            settings?.Invoke(options);

            app.UseSerilogLogContext(e =>
            {
                e.EnrichersForContextFactory = context =>
                {
                    TraceIdentifiersCollection feature = context.Features.Get<TraceIdentifiersCollection>();
                    if (options.SingleProperty)
                    {
                        return new[]
                        {
                            new PropertyEnricher(options.TraceIdentifiersSinglePropertyName, feature, true),
                        };
                    }

                    return new[]
                    {
                        new PropertyEnricher(options.TraceIdentifiersCurrentPropertyName, feature?.Current, false),
                        new PropertyEnricher(options.TraceIdentifiersAllPropertyName, feature?.All, true)
                    };
                };
            });

            return app;
        }
    }
}
