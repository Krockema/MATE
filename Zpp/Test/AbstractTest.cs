using System;
using System.Data.SqlClient;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Zpp.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Zpp.Test
{
    public abstract class AbstractTest : IDisposable
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected readonly ProductionDomainContext ProductionDomainContext;
        private readonly bool resetDb = true;

        // @before
        public AbstractTest()
        {
            ProductionDomainContext = Dbms.getDbContext();

            if (resetDb)
            {
                bool isDeleted = ProductionDomainContext.Database.EnsureDeleted();
                if (!isDeleted)
                {
                    LOGGER.Error("Database could not be deleted.");    
                }
                // MasterDBInitializerSmall.DbInitialize(ProductionDomainContext);
                MasterDBInitializerLarge.DbInitialize(ProductionDomainContext);
            }
        }

        public AbstractTest(Action<ProductionDomainContext> dbInitializer)
        {
            ProductionDomainContext = Dbms.getDbContext();

            if (resetDb)
            {
                bool isDeleted = ProductionDomainContext.Database.EnsureDeleted();
                if (!isDeleted)
                {
                    LOGGER.Error("Database could not be deleted.");    
                }

                dbInitializer(ProductionDomainContext);
            }
        }

        // @after
        public void Dispose()
        {
            ProductionDomainContext.Database.CloseConnection();
        }
    }
}