using System;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
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
                ProductionDomainContext.Database.EnsureCreated();
                MasterDBInitializerSmall.DbInitialize(ProductionDomainContext);
            }
        }

        // @after
        public void Dispose()
        {
        }
    }
}