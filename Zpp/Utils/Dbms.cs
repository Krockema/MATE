using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NLog.Extensions.Logging;

namespace Zpp.Utils
{
    public static class Dbms
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(
            /*new[]
        {
             new ConsoleLoggerProvider((category, level) => category == DbLoggerCategory.Database.Command.Name &&
                                        level == LogLevel.Information, true)
        }*/
        );

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
                        .UseLoggerFactory(MyLoggerFactory).UseSqlServer(
                            // Constants.DbConnectionZppLocalDb)
                            Constants.DbConnectionZppSqlServer())
                        .Options);
                Constants.IsLocalDb = false;
            }
            else
            {
                
                
                // With Sql Server for Mac/Linux
                productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseLoggerFactory(MyLoggerFactory).UseSqlServer(
                        Constants.DbConnectionZppSqlServer())
                    .Options);

                // sqlite
                // _productionDomainContext = InMemoryContext.CreateInMemoryContext();
                // inMemory
                /*_productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                    .Options);*/
            }
            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            productionDomainContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            return productionDomainContext;
        }
    }
}