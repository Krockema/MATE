using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Helper.Types;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using System;
using System.Data;

namespace Master40.DB
{
    public static class Dbms
    {
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

        public static DataBase<ProductionDomainContext> GetNewMasterDataBase(bool archive = false)
        {
            var archiveSuffix = "";
            if (archive) archiveSuffix = "_archive"; 
            DataBase<ProductionDomainContext> dataBase = 
                new DataBase<ProductionDomainContext>(Constants.DbWithSuffixMaster(archiveSuffix));
            if (UseLocalDb() && Constants.IsWindows)
            {
                    Constants.IsLocalDb = true;
            }
            else if (Constants.IsWindows)
            {
                    Constants.IsLocalDb = false;
            }
            // else Linux
            dataBase.ConnectionString = GetConnectionString(dataBase.DataBaseName);
            dataBase.DbContext = new ProductionDomainContext(
                new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);
            
            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            dataBase.DbContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return dataBase;
        }

        public static DataBase<ResultContext> GetNewResultDataBase()
        {
            DataBase<ResultContext> dataBase = new DataBase<ResultContext>(Constants.DbWithSuffixResults());
            dataBase.ConnectionString = GetResultConnectionString(dataBase.DataBaseName);
            dataBase.DbContext = new ResultContext(
                new DbContextOptionsBuilder<ResultContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            dataBase.DbContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return dataBase;
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
                catch (SqlException)
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
                catch (SqlException)
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