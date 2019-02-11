namespace TraceIdentifiers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using global::Serilog;
    using global::Serilog.Context;
    using global::Serilog.Core;
    using global::Serilog.Events;

    using Moq;

    using TraceIdentifiers.AspNetCore;
    using TraceIdentifiers.Serilog;

    using Xunit;

    public class SerilogExtensionsTest
    {
        private static Tuple<Logger, Mock<ILogEventSink>,ICollection<LogEvent>> CreateLogger()
        {
            LoggerConfiguration configuration = new LoggerConfiguration();
            configuration.Enrich.FromLogContext();
            Mock<ILogEventSink> sink = new Mock<ILogEventSink>();
            configuration.WriteTo.Sink(sink.Object);

            Logger logger = configuration.CreateLogger();
            List<LogEvent> events = new List<LogEvent>();
            sink.Setup(es => es.Emit(It.IsAny<LogEvent>())).Callback<LogEvent>(le => events.Add(le));

            return new Tuple<Logger, Mock<ILogEventSink>, ICollection<LogEvent>>(logger, sink, events);
        }

        [Fact]
        public void PushLocalToContext()
        {
            TraceIdentifiersContext c =
                new TraceIdentifiersContext(true, "qwe").LinkToSerilogLogContext(builder => builder.WithStartup().WithLocalIdentifiers().WithLocalIdentifier());

            using (LogContext.PushProperty("anyOther", "Any1"))
            using (c)
            {
                var logger = CreateLogger();
                logger.Item1.Information("info1");

                LogEvent logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info1");
                Assert.Equal(4, logEvent.Properties.Count);

                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("[\"qwe\"]", logEvent.Properties["correlationLocalAll"].ToString());

                using (var c1 = c.CreateChildWithLocal(false, "c1"))
                {
                    logger.Item1.Information("info2");
                    logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info2");
                    Assert.Equal(4, logEvent.Properties.Count);

                    Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                    Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                    Assert.Equal("c1", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                    Assert.Equal("[\"qwe\", \"c1\"]", logEvent.Properties["correlationLocalAll"].ToString());

                    using (c1.CreateChildWithLocal(false, "c11"))
                    {
                        logger.Item1.Information("info3");
                        logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info3");
                        Assert.Equal(4, logEvent.Properties.Count);

                        Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                        Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                        Assert.Equal("c11", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                        Assert.Equal("[\"qwe\", \"c1\", \"c11\"]", logEvent.Properties["correlationLocalAll"].ToString());
                    }

                    using (c1.CreateChildWithLocal(false, "c12"))
                    {
                        logger.Item1.Information("info4");
                        logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info4");
                        Assert.Equal(4, logEvent.Properties.Count);

                        Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                        Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                        Assert.Equal("c12", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                        Assert.Equal("[\"qwe\", \"c1\", \"c12\"]", logEvent.Properties["correlationLocalAll"].ToString());
                    }
                }

                using (c.CreateChildWithLocal(false, "c2"))
                {
                    logger.Item1.Information("info5");
                    logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info5");
                    Assert.Equal(4, logEvent.Properties.Count);

                    Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                    Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                    Assert.Equal("c2", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                    Assert.Equal("[\"qwe\", \"c2\"]", logEvent.Properties["correlationLocalAll"].ToString());
                }

                logger.Item1.Information("info6");
                logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info6");
                Assert.Equal(4, logEvent.Properties.Count);

                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("[\"qwe\"]", logEvent.Properties["correlationLocalAll"].ToString());
            }
        }

        [Fact]
        public void MultiThreading()
        {
            TraceIdentifiersContext c =
                new TraceIdentifiersContext(true, "qwe") { Remote = "rm" }.LinkToSerilogLogContext(
                    builder => builder.WithDefaults());

            using (LogContext.PushProperty("anyOther", "Any1"))
            using (c)
            {
                var logger = CreateLogger();
                logger.Item1.Information("info1");

                LogEvent logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info1");
                Assert.Equal(5, logEvent.Properties.Count);
                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(
                    TraceIdentifiersContext.StartupId,
                    logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                Assert.Equal("[\"qwe\"]", logEvent.Properties["correlationAll"].ToString());
                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < 10; i++)
                {
                    Thread thread = new Thread(this.RunThread);
                    thread.Start(c);
                    threads.Add(thread);
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
            }            
        }

        private void RunThread(object state)
        {
            var logger = CreateLogger();
            logger.Item1.Information("info1");

            LogEvent logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info1");
            Assert.Equal(5, logEvent.Properties.Count); // LogContext was able to survive thread switch
            TraceIdentifiersContext context = (TraceIdentifiersContext)state;
            string local = Thread.CurrentThread.ManagedThreadId.ToString("D");
            using (context.CloneForThread().CreateChildWithLocal(true, local))
            {
                logger.Item1.Information("info2");
                logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info2");

                Assert.Equal(5, logEvent.Properties.Count);
                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(
                    TraceIdentifiersContext.StartupId,
                    logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal(local, logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                Assert.Equal($"[\"qwe\", \"{local}\"]", logEvent.Properties["correlationAll"].ToString());
            }
        }

        [Fact]
        public void PushDefaultToContext()
        {
            TraceIdentifiersContext c =
                new TraceIdentifiersContext(true, "qwe") { Remote = "rm" }.LinkToSerilogLogContext(
                    builder => builder.WithDefaults());

            using (LogContext.PushProperty("anyOther", "Any1"))
            using (c)
            {
                var logger = CreateLogger();
                logger.Item1.Information("info1");

                LogEvent logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info1");
                Assert.Equal(5, logEvent.Properties.Count);
                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                Assert.Equal("[\"qwe\"]", logEvent.Properties["correlationAll"].ToString());

                using (var r1 = c.CreateChildWithRemote(new []{"r1","r2"}))
                {
                    logger.Item1.Information("info2");
                    logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info2");

                    Assert.Equal(5, logEvent.Properties.Count);
                    Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                    Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                    Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                    Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                    Assert.Equal("[\"r1\", \"r2\", \"qwe\"]", logEvent.Properties["correlationAll"].ToString());

                    using (var c1 =r1.CreateChildWithLocal(false, "c11"))
                    {
                        logger.Item1.Information("info3");
                        logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info3");

                        Assert.Equal(5, logEvent.Properties.Count);
                        Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                        Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                        Assert.Equal("c11", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                        Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                        Assert.Equal("[\"r1\", \"r2\", \"qwe\", \"c11\"]", logEvent.Properties["correlationAll"].ToString());

                        using (var r2 = c1.CreateChildWithRemote(new[] { "r3", "r4" }))
                        {
                            logger.Item1.Information("info4");
                            logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info4");

                            Assert.Equal(5, logEvent.Properties.Count);
                            Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                            Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                            Assert.Equal("c11", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                            Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                            Assert.Equal("[\"r1\", \"r2\", \"r3\", \"r4\", \"qwe\", \"c11\"]", logEvent.Properties["correlationAll"].ToString());
                        }
                    }
                }

                logger.Item1.Information("info6");
                logEvent = logger.Item3.Single(le => le.MessageTemplate.Text == "info6");
                Assert.Equal(5, logEvent.Properties.Count);

                Assert.Equal("Any1", logEvent.Properties["anyOther"].ToString().Trim('"'));
                Assert.Equal(TraceIdentifiersContext.StartupId, logEvent.Properties["correlationStartup"].ToString().Trim('"'));
                Assert.Equal("qwe", logEvent.Properties["correlationLocal"].ToString().Trim('"'));
                Assert.Equal("rm", logEvent.Properties["correlationRemote"].ToString().Trim('"'));
                Assert.Equal("[\"qwe\"]", logEvent.Properties["correlationAll"].ToString());
            }
        }
    }
}