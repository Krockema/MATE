using System.IO;
using Master40.DB.Data.Helper;
using Master40.SimulationMrp;
using Xunit;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestObjectTracking : AbstractTest
    {
        
        public TestObjectTracking() : base(initDefaultTestConfig: false)
        {
            Master40.DB.Configuration.TrackObjects = true;
            InitTestScenario(DefaultTestScenario);
        }
    
        [Fact]
        public void TestTrackingOfObjects()
        {

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();
            
            string usedIdsFileName = IdGenerator.WriteToFile();
            
            Assert.True( File.Exists(usedIdsFileName),
                $"Tracking created object hasn't worked: File '{usedIdsFileName}' was not created.");
        }
    }
}