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
        protected readonly ITestOutputHelper TestOutputHelper;
        private readonly bool resetDb = true; // TODO: At the end this must be set to true

        // @before
        public AbstractTest(ITestOutputHelper testOutputHelper)
        {
            // for console outputs in the debug view, usage:  TestOutputHelper.WriteLine("Bla")
            TestOutputHelper = testOutputHelper;

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