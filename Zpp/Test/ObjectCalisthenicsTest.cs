using System.IO;
using Xunit;

namespace Zpp.Test
{
    public class ObjectCalisthenicsTest
    {
        [Fact]
        public void testNoElse()
        {
            Assert.False(true,Directory.GetCurrentDirectory() + "");
        }
    }
}