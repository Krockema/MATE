using System;
using System.Data;
using System.Data.SqlClient;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Helper.Types;
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

        private static DbConnectionString GetConnectionString(DataBaseName dataBaseName)
        {
            var connectionString = String.Empty;
            if (UseLocalDb() && Constants.IsWindows)
            {
                connectionString = Constants.CreateLocalConnectionString(dataBaseName);
                Constants.IsLocalDb = true;
            }
            else if (Constants.IsWindows)
            {
                Constants.IsLocalDb = false;
                connectionString = Constants.CreateServerConnectionString(dataBaseName);
            }
            else
            {
                connectionString = Constants.CreateServerConnectionString(dataBaseName);
            }
            return new DbConnectionString(connectionString);
        }

        private static DbConnectionString GetResultConnectionString(DataBaseName dataBaseName)
        {
            var connectionString = String.Empty;
            if (UseLocalDb() && Constants.IsWindows)
            {
                connectionString = Constants.CreateLocalConnectionString(dataBaseName);
            } else { 
                connectionString = Constants.CreateServerConnectionString(dataBaseName);
            }
            return new DbConnectionString(connectionString);
        }

        public static DataBase<ProductionDomainContext> GetNewDataBase()
        {
            DataBase<ProductionDomainContext> dbInfo = 
                new DataBase<ProductionDomainContext>(Constants.DbWithSuffixMaster());
            if (UseLocalDb() && Constants.IsWindows)
            {
                    Constants.IsLocalDb = true;
            }
            else if (Constants.IsWindows)
            {
                    Constants.IsLocalDb = false;
            }
            // else Linux
            dbInfo.ConnectionString = GetConnectionString(dbInfo.DataBaseName);
            dbInfo.DbContext = new ProductionDomainContext(
                new DbContextOptionsBuilder<MasterDBContext>().UseLoggerFactory(MyLoggerFactory)
                    .UseSqlServer(dbInfo.ConnectionString.Value).Options);

            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            dbInfo.DbContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return dbInfo;
        }

        public static DataBase<ResultContext> GetResultDataBase()
        {
            DataBase<ResultContext> dbInfo = new DataBase<ResultContext>(Constants.DbWithSuffixResults());
            dbInfo.ConnectionString = GetResultConnectionString(dbInfo.DataBaseName);
            dbInfo.DbContext = new ResultContext(
                new DbContextOptionsBuilder<ResultContext>().UseLoggerFactory(MyLoggerFactory)
                    .UseSqlServer(dbInfo.ConnectionString.Value).Options);

            MyLoggerFactory.AddNLog();

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            dbInfo.DbContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return dbInfo;
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