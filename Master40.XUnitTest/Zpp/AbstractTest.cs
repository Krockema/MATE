using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.XUnitTest.Zpp.Configuration;
using Master40.XUnitTest.Zpp.Configuration.Scenarios;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using Zpp.DbCache;
using Zpp.LotSize;

namespace Master40.XUnitTest.Zpp
{
    /**
     * A test can be initialized via base() constructor on three ways:
     * - no dbInit: use base(false)
     * - dbInit: default db (truck scenario) use base(true) else use base(TestConfigurationFileNames.X)
     * - dbInit + CO/COP: use base(false) and call InitTestScenario(TestConfigurationFileNames.X)
     */
    public abstract class AbstractTest : IDisposable
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private DataBase<ProductionDomainContext> ProductionDataBase;
        protected readonly ProductionDomainContext ProductionDomainContext;

        protected static TestConfiguration TestConfiguration;

        private static readonly string _defaultTestScenario =
            TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2;

        private IDbMasterDataCache _dbMasterDataCache;

        public AbstractTest() : this(initDefaultTestConfig: true)
        {
            _dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
        }

        /**
         * This constructor must be always called (else ProductionDomainContext is null)
         * --> seems a bit strange, but is needed to enable parameterized tests:
         * the default db should not be initialized in this case, but the testConfig is not available as constructor parameter
         */
        public AbstractTest(bool initDefaultTestConfig)
        {
            ProductionDataBase = Dbms.GetNewMasterDataBase();
            ProductionDomainContext = ProductionDataBase.DbContext;
            if (initDefaultTestConfig)
            {
                InitTestScenario(_defaultTestScenario);
            }
        }

        // @before
        public AbstractTest(string testConfiguration) : this(false)
        {
            InitDb(testConfiguration);
        }

        // @after
        public void Dispose()
        {
            ProductionDomainContext.Database.CloseConnection();
        }

        /**
         * Initialize the db:
         * - deletes current
         * - creates db according to given configuration
         */
        protected void InitDb(string testConfiguration)
        {
            TestConfiguration = ReadTestConfiguration(testConfiguration);
            if (Constants.IsLocalDb)
            {
                bool isDeleted = ProductionDomainContext.Database.EnsureDeleted();
                if (!isDeleted)
                {
                    LOGGER.Error("Database could not be deleted.");
                }
            }

            else
            {
                bool wasDropped = Dbms.DropDatabase(ProductionDataBase.DataBaseName.Value,
                                                    ProductionDataBase.ConnectionString.Value);
                if (wasDropped == false)
                {
                    LOGGER.Warn($"Database {ProductionDataBase.DataBaseName.Value} could not be dropped.");
                }
            }

            Type dbSetInitializer = Type.GetType(TestConfiguration.DbSetInitializer);
            dbSetInitializer.GetMethod("DbInitialize")
                .Invoke(null, new[]
                {
                    ProductionDomainContext
                });

            LotSize.SetDefaultLotSize(new Quantity(TestConfiguration.LotSize));
            LotSize.SetLotSizeType(TestConfiguration.LotSizeType);
        }

        /**
         * init db and customerOrders
         */
        protected void InitTestScenario(string testConfiguration)
        {
            InitDb(testConfiguration);
            _dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            Type testScenarioType = Type.GetType(TestConfiguration.TestScenario);
            TestScenario testScenario = (TestScenario) Activator.CreateInstance(testScenarioType, _dbMasterDataCache);
            testScenario.CreateCustomerOrders(
                new Quantity(TestConfiguration.CustomerOrderPartQuantity), ProductionDomainContext);
        }

        private static TestConfiguration ReadTestConfiguration(string testConfigurationFileNames)
        {
            return JsonConvert.DeserializeObject<TestConfiguration>(
                File.ReadAllText(testConfigurationFileNames));
        }
    }
}