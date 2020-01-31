using Master40.SimulationCore.Environment.Options;
using Xunit;

namespace Master40.XUnitTest.Online.Simulation
{
    public class Configuration
    {
        [Fact]
        public void CreateAndRead()
        {
            var config = SimulationCore.Environment.Configuration.Create(args: new object[] { new Seed(value: 2) });
            var seed = config.GetOption<Seed>();
            Assert.Equal(expected: 2, actual: seed.Value);
        }
    }
}
