using System;
using System.Data;
using System.Data.SqlClient;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Master40.DB
{
    public static class Dbms
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory();

        /// <summary>
        /// If localDb shall be used - set it via command line with :
        /// setx UseLocalDb true
        /// </summary>
        /// <returns></returns>
        public static bool UseLocalDb()
        {
            var environmentUseLocalDb = Environment.GetEnvironmentVariable("UseLocalDb", EnvironmentVariableTarget.User);
            if (environmentUseLocalDb != null)
                return environmentUseLocalDb.Equals("true");
            return false;
        }

        public static string GetConnectionString()
        {
            if (UseLocalDb() && Constants.IsWindows)
            {
                return Constants.DbConnectionLocalDb;
            }
            return  Constants.DbConnectionSqlServerMaster;
        }

        public static ProductionDomainContext GetDbContext()
        {
            ProductionDomainContext productionDomainContext;

            if (UseLocalDb() && Constants.IsWindows)
            {
                    Constants.IsLocalDb = true;
            } else if (Constants.IsWindows)
            {
                Constants.IsLocalDb = false;
            }
            // else Linux
            productionDomainContext = new ProductionDomainContext(
                new DbContextOptionsBuilder<MasterDBContext>().UseLoggerFactory(MyLoggerFactory)
                    .UseSqlServer(GetConnectionString()).Options);

            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            productionDomainContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return productionDomainContext;
        }

        public static ResultContext GetDbResultContext()
        {
            ResultContext resultContext;

            if (UseLocalDb() && Constants.IsWindows)
            {
                resultContext = new ResultContext(
                    new DbContextOptionsBuilder<ResultContext>().UseLoggerFactory(MyLoggerFactory)
                        .UseSqlServer(Constants.DbConnectionResultSqlServerLocal).Options);
            }
            else if (Constants.IsWindows)
            {
                Constants.IsLocalDb = false;
            }
            resultContext = new ResultContext(
                    new DbContextOptionsBuilder<ResultContext>().UseLoggerFactory(MyLoggerFactory)
                        .UseSqlServer(Constants.DbConnectionResultSqlServer).Options);
            
            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            resultContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return resultContext;
        }


        public static bool CanConnect(string connectionString)
        {
            bool canConnect = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    canConnect = con.State == ConnectionState.Open;
                }
                catch (SqlException e)
                {
                    canConnect = false;
                }
            }
            return canConnect;
        }

        /**
         * @return: true, if db was succesfully dropped
         */
        public static bool DropDatabase(string dbName, string connectionString)
        {
            if (CanConnect(connectionString) == false)
            {
                return false;
            }

            int result = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
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