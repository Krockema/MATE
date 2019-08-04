using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
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
                    new DbContextOptionsBuilder<MasterDBContext>().UseLoggerFactory(MyLoggerFactory)
                        .UseSqlServer(
                            // Constants.DbConnectionZppLocalDb)
                            Constants.DbConnectionZppSqlServer()).Options);
                Constants.IsLocalDb = false;
            }
            else
            {
                // With Sql Server for Mac/Linux
                productionDomainContext = new ProductionDomainContext(
                    new DbContextOptionsBuilder<MasterDBContext>().UseLoggerFactory(MyLoggerFactory)
                        .UseSqlServer(Constants.DbConnectionZppSqlServer()).Options);

                // sqlite
                // _productionDomainContext = InMemoryContext.CreateInMemoryContext();
                // inMemory
                /*_productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                    .Options);*/
            }

            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            productionDomainContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return productionDomainContext;
        }

        public static bool CanConnect(string connectionString)
        {
            bool canConnect = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    Thread.Sleep(5000);
                    canConnect = con.State == ConnectionState.Open;
                }
                catch (SqlException e)
                {
                    canConnect = false;

                }
                
                
            }
            Thread.Sleep(5000);
            return canConnect;
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
                try
                {
                    result = sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    return false;
                }
            }

            // For UPDATE, INSERT, and DELETE statements, the return value is the number of rows
            // affected by the command. For all other types of statements, the return value is -1
            // source: https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand.executenonquery?view=netframework-4.8
            return result == -1;
        }
    }
}