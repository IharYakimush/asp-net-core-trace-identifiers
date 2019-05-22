using System.Collections.Generic;
using System.Security;

namespace TraceIdentifiers.Serilog
{
    using System;
    using System.Linq;

    using global::Serilog.Core.Enrichers;

    public static class LogContextBuilderExtensions
    {
        public static LogContextBuilder WithStartup(this LogContextBuilder builder, string name = "correlationStartup")
        {
            builder.Factories.Add(c => new PropertyEnricher(name, TraceIdentifiersContext.StartupId));

            return builder;
        }

        public static LogContextBuilder WithLocalIdentifiers(this LogContextBuilder builder, string name = "correlationLocalAll")
        {
            builder.Factories.Add(c => new PropertyEnricher(name, c.Local.Reverse().ToArray()));

            return builder;
        }

        public static LogContextBuilder WithRemoteIdentifiers(this LogContextBuilder builder, string name = "correlationRemoteAll")
        {
            builder.Factories.Add(c =>
            {
                IEnumerable<string> remoteShared = RemoteAllEscaped(builder, c);

                return new PropertyEnricher(name, remoteShared.ToArray());
            });

            return builder;
        }

        private static IEnumerable<string> RemoteAllEscaped(LogContextBuilder builder, TraceIdentifiersContext c)
        {
            IEnumerable<string> remoteShared = c.Remote;

            if (builder.EscapeRemote)
            {
                remoteShared = remoteShared.Select(SecurityElement.Escape);
            }

            return remoteShared;
        }

        public static LogContextBuilder WithLocalIdentifier(this LogContextBuilder builder, string name = "correlationLocal")
        {
            builder.Factories.Add(c => new PropertyEnricher(name, c.Local.FirstOrDefault()));

            return builder;
        }

        public static LogContextBuilder WithRemoteAndLocalIdentifiers(this LogContextBuilder builder, string name = "correlationAll")
        {
            builder.Factories.Add(c =>
                new PropertyEnricher(name, c.Local.Reverse().Concat(RemoteAllEscaped(builder, c)).ToArray()));

            return builder;
        }

        public static LogContextBuilder WithDefaults(this LogContextBuilder builder)
        {
            return builder.WithStartup().WithLocalIdentifier().WithRemoteAndLocalIdentifiers();
        }
    }
}