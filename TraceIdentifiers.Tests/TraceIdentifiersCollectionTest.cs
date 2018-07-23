using System;
using System.Linq;
using Xunit;

namespace TraceIdentifiers.Tests
{
    public class TraceIdentifiersCollectionTest
    {
        [Fact]
        public void Test1()
        {
            TraceIdentifiersCollection c = new TraceIdentifiersCollection("c", new[] {"a", "b", "c" });
            Assert.Equal(3, c.All.Count());
            Assert.Equal("c", c.Current);
        }
    }
}
