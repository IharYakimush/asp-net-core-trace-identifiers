namespace TraceIdentifiers.Tests
{
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

            using (var c1 =c.CreateChild(false,"c1"))
            {
                using (var c11 = c.CreateChild(false, "c11"))
                {

                }

                using (var c12 = c.CreateChild(false, "c11"))
                {

                }
            }

            using (var c2 = c.CreateChild(false, "c2"))
            {
                
            }
        }
    }
}