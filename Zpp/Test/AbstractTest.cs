using System;
using System.Data.SqlClient;
using System.IO;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Zpp.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Zpp.Test.Configurations;

namespace Zpp.Test
{
    public abstract class AbstractTest : IDisposable
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected readonly ProductionDomainContext ProductionDomainContext;
        private readonly bool resetDb = true;

        protected static TestConfiguration TestConfiguration =
            ReadTestConfiguration(TestConfigurationFileNames.DUMP_TRUCK_COP_6_LOTSIZE_2);

        public AbstractTest() : this(TestConfigurationFileNames.DUMP_TRUCK_COP_6_LOTSIZE_2)
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, TestConfiguration.Quantity);
            LotSize.LotSize.SetDefaultLotSize(new Quantity(TestConfiguration.LotSize));
        }

        // @before
        public AbstractTest(string testConfiguration)
        {
            TestConfiguration = ReadTestConfiguration(testConfiguration);
            ProductionDomainContext = Dbms.getDbContext();

            if (resetDb)
            {
                bool isDeleted = ProductionDomainContext.Database.EnsureDeleted();
                if (!isDeleted)
                {
                    LOGGER.Error("Database could not be deleted.");
                }

                Type dbSetInitializer = Type.GetType(TestConfiguration.DbSetInitializer);
                dbSetInitializer.GetMethod("DbInitialize")
                    .Invoke(null, new[] {ProductionDomainContext});
            }
        }

        // @after
        public void Dispose()
        {
            ProductionDomainContext.Database.CloseConnection();
        }

        private static TestConfiguration ReadTestConfiguration(string testConfigurationFileNames)
        {
            return JsonConvert.DeserializeObject<TestConfiguration>(
                File.ReadAllText(testConfigurationFileNames));
        }
    }
}