namespace TraceIdentifiers.Tests
{
    using System;
    using System.Linq;

    using TraceIdentifiers.AspNetCore;

    using Xunit;

    public class TraceIdentifiersContextTests
    {
        [Fact]
        public void CreateAndDisposeSingle()
        {
            TraceIdentifiersContext c1 = new TraceIdentifiersContext(true, "qwe");
            TraceIdentifiersContext c2 = new TraceIdentifiersContext(false, "qwe");
            using (c1)
            using (c2)
            {
                Assert.Equal("qwe", Assert.Single(c1.Local));
                Assert.Equal("qwe", Assert.Single(c1.LocalShared));

                Assert.Equal("qwe", Assert.Single(c2.Local));
                Assert.Empty(c2.LocalShared);
            }
        }

        [Fact]
        public void CreateAndDisposeChilds()
        {
            TraceIdentifiersContext c = new TraceIdentifiersContext(true, "qwe");

            using (var c1 = c.CreateChild(false, "c1"))
            {
                using (var c11 = c1.CreateChild(false, "c11"))
                {
                    Assert.Equal("c11", c11.Local.ElementAt(0));
                    Assert.Equal("c1", c11.Local.ElementAt(1));
                    Assert.Equal("qwe", c11.Local.ElementAt(2));
                }

                using (var c12 = c1.CreateChild(false, "c12"))
                {
                    Assert.Equal("c12", c12.Local.ElementAt(0));
                    Assert.Equal("c1", c12.Local.ElementAt(1));
                    Assert.Equal("qwe", c12.Local.ElementAt(2));
                }
            }

            using (var c2 = c.CreateChild(false, "c2"))
            {
                Assert.Equal("c2", c2.Local.ElementAt(0));
                Assert.Equal("qwe", c2.Local.ElementAt(1));
            }
        }

        [Fact]
        public void CreateOnSameLevel()
        {
            TraceIdentifiersContext c = new TraceIdentifiersContext(true, "qwe");
            c.CreateChild(false, "1");
            Assert.Throws<InvalidOperationException>(() => c.CreateChild(false, "2"));
        }

        [Fact]
        public void AcceptRemotes()
        {
            TraceIdentifiersContext c = new TraceIdentifiersContext(true, "qwe");
            using (var c1 = c.CreateChild(false, "c1"))
            {
                var r1 = c1.AcceptRemote(new[] { "r1", "r2" });
                using (r1)
                {
                    Assert.Equal("c1", c1.Local.ElementAt(0));
                    Assert.Equal("qwe", c1.Local.ElementAt(1));

                    // Local saved if accept remote
                    Assert.Equal("c1", r1.Local.ElementAt(0));
                    Assert.Equal("qwe", r1.Local.ElementAt(1));

                    Assert.Equal("r1", r1.RemoteShared.ElementAt(0));
                    Assert.Equal("r2", r1.RemoteShared.ElementAt(1));
                    
                    Assert.Empty(c.RemoteShared);
                    Assert.Empty(c1.RemoteShared);
                }

                Assert.Empty(r1.RemoteShared);

                Assert.Equal("c1", c1.Local.ElementAt(0));
                Assert.Equal("qwe", c1.Local.ElementAt(1));
                Assert.Empty(c.RemoteShared);
                Assert.Empty(c1.RemoteShared);
            }
        }
    }
}