using System;
using System.Data;
using System.Data.SqlClient;
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

        public static bool CanConnect(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                return con.State == ConnectionState.Open;
            }
        }
        
        /**
         * @return: true, if db was succesfully dropped
         */
        public static bool DropDatabase(string dbName)
        {
            if (CanConnect(Constants.DbConnectionZppSqlServer()) == false)
            {
                return false;
            }
            String connectionString = Constants.DbConnectionZppSqlServerMaster();
            int result = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                /*String sqlCommandText = $@"
                GO
                    ALTER DATABASE {dbName} 
                SET OFFLINE WITH ROLLBACK IMMEDIATE
                GO
                    ALTER DATABASE {dbName} SET ONLINE
                GO
                    DROP DATABASE {dbName}
                GO";*/
                
                String sqlCommandText = @"
        ALTER DATABASE " + dbName + @" SET OFFLINE WITH ROLLBACK IMMEDIATE;
        ALTER DATABASE " + dbName + @" SET ONLINE;
        DROP DATABASE [" + dbName + "]";
                
                SqlCommand sqlCommand = new SqlCommand(sqlCommandText, con);
                result = sqlCommand.ExecuteNonQuery();
            }

            return result == 1;
        }
    }
}