using System;
using System.Data.SqlClient;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Zpp.Utils;

namespace Zpp.Test
{
    public abstract class AbstractTest : IDisposable
    {
        protected readonly ProductionDomainContext ProductionDomainContext;
        private readonly bool resetDb = true;

        // @before
        public AbstractTest()
        {
            ProductionDomainContext = Dbms.getDbContext();

            if (resetDb)
            {
                ProductionDomainContext.Database.EnsureDeleted();
                MasterDBInitializerSmall.DbInitialize(ProductionDomainContext);
            }
        }

        // @after
        public void Dispose()
        {
            ProductionDomainContext.Database.CloseConnection();
        }
    }
}