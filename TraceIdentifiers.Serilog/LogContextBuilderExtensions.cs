using System.Collections.Generic;
using System.Security;

namespace TraceIdentifiers.Serilog
{
    using System;
    using System.Linq;

    using global::Serilog.Core.Enrichers;

    using TraceIdentifiers.AspNetCore;

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
                IEnumerable<string> remoteShared = RemoteSharedEscaped(builder, c);

                return new PropertyEnricher(name, remoteShared.ToArray());
            });

            return builder;
        }

        private static IEnumerable<string> RemoteSharedEscaped(LogContextBuilder builder, TraceIdentifiersContext c)
        {
            IEnumerable<string> remoteShared = c.RemoteShared;

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
                new PropertyEnricher(name, c.Local.Reverse().Concat(RemoteSharedEscaped(builder, c)).ToArray()));

            return builder;
        }

        public static LogContextBuilder WithRemoteIdentifier(this LogContextBuilder builder, string name = "correlationRemote")
        {
            builder.Factories.Add(c =>
            {
                return c.Remote == null
                    ? null
                    : new PropertyEnricher(name,
                        builder.EscapeRemote ? SecurityElement.Escape(c.Remote) : c.Remote);
            });

            return builder;
        }

        public static LogContextBuilder WithDefaults(this LogContextBuilder builder)
        {
            return builder.WithStartup().WithLocalIdentifier().WithRemoteIdentifier().WithRemoteAndLocalIdentifiers();
        }
    }
}