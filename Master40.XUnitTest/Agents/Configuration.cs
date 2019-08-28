using Xunit;
using Master40.SimulationCore.Environment.Options;

namespace Master40.XUnitTest.Agents
{
    public class Configuration
    {
        [Fact]
        public void CreateAndRead()
        {
            var config = SimulationCore.Environment.Configuration.Create(new object[] { new Seed(2) });
            var seed = config.GetOption<Seed>();
            Assert.Equal(2, actual: seed.Value);
        }
    }
}
