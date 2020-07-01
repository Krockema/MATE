using Master40.SimulationMrp;

namespace Master40.XUnitTest.Zpp.Integration_Tests.Verification
{
    public class AbstractVerification: AbstractTest
    {
        protected AbstractVerification() : base(initDefaultTestConfig: false)
        {
        }

        protected void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            // TODO: set to true once dbPersist() has an acceptable time and and enable ReloadTransactionData
            zppSimulator.StartPerformanceStudy(false);
            // IDbTransactionData dbTransactionData =
            //    ZppConfiguration.CacheManager.ReloadTransactionData();
        }
    
        
    }
}