using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Zpp.Utils
{
    public static class Dbms
    {
        public static ProductionDomainContext getDbContext()
        {
            ProductionDomainContext productionDomainContext;

            // EF inMemory
            // MasterDBContext _inMemmoryContext = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
            /*_productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDB")
                .Options);*/


            if (Constants.IsWindows)
            {
                // Windows
                productionDomainContext = new ProductionDomainContext(
                    new DbContextOptionsBuilder<MasterDBContext>()
                        .UseSqlServer(
                            Constants.DbConnectionZppWindows)
                        .Options);
            }
            else
            {
                // With Sql Server for Mac/Linux
                productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        Constants.DbConnectionZppUnix())
                    .Options);

                // sqlite
                // _productionDomainContext = InMemoryContext.CreateInMemoryContext();
                // inMemory
                /*_productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                    .Options);*/
            }

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            productionDomainContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            return productionDomainContext;
        }
    }
}