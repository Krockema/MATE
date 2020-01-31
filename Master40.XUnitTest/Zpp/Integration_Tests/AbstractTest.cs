using System;
using Master40.DB.Data.Context;
using Master40.XUnitTest.Zpp.Configuration;
using Zpp;
using Zpp.DataLayer;
using Zpp.Utils;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    /**
     * A test can be initialized via base() constructor on three ways:
     * - no dbInit: use base(false)
     * - dbInit: default db (truck scenario) use base(true) else use base(TestConfigurationFileNames.X)
     * - dbInit + CO/COP: use base(false) and call InitTestScenario(TestConfigurationFileNames.X)
     */
    public abstract class AbstractTest : IDisposable
    {
        protected ProductionDomainContext ProductionDomainContext;

        protected static ICentralPlanningConfiguration TestConfiguration;

        protected static readonly string DefaultTestScenario =
            TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2;

        private IDbMasterDataCache _dbMasterDataCache = global::Zpp.ZppConfiguration.CacheManager.GetMasterDataCache();

        public AbstractTest() : this(true)
        {
            
        }

        /**
         * This constructor must be always called (else ProductionDomainContext is null)
         * --> seems a bit strange, but is needed to enable parameterized tests:
         * the default db should not be initialized in this case, but the testConfig is not available as constructor parameter
         */
        public AbstractTest(bool initDefaultTestConfig)
        {
            if (initDefaultTestConfig)
            {
                InitTestScenario(DefaultTestScenario);
            }
        }

        // @before
        public AbstractTest(string testConfiguration) : this(false)
        {
            InitTestScenario(testConfiguration);
        }

        // @after
        public void Dispose()
        {
            global::Zpp.ZppConfiguration.CacheManager.Dispose();
        }

        /**
         * init db
         */
        protected void InitTestScenario(string testConfiguration)
        {
            global::Zpp.ZppConfiguration.CacheManager.InitByReadingFromDatabase(testConfiguration, true);
            TestConfiguration = global::Zpp.ZppConfiguration.CacheManager.GetTestConfiguration();
            ProductionDomainContext = global::Zpp.ZppConfiguration.CacheManager.GetProductionDomainContext();
        }
    }
}