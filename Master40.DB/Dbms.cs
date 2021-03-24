using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Helper.Types;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private static bool UseLocalDb()
        {
            var environmentUseLocalDb = Environment.GetEnvironmentVariable("UseLocalDb", EnvironmentVariableTarget.User);
            if (environmentUseLocalDb != null)
                return environmentUseLocalDb.Equals("true");
            return false;
        }

        public static DbConnectionString GetConnectionString(DataBaseName dataBaseName)
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
            }
            else
            {
                connectionString = Constants.CreateServerConnectionString(dataBaseName);
            }
            return new DbConnectionString(connectionString);
        }
        public static DataBase<ProductionDomainContext> GetNewMasterDataBase(bool archive = false, string dbName = "")
        {
            if (dbName.Equals(""))
            {
                var archiveSuffix = "";
                if (archive) archiveSuffix = "_archive";
                dbName = Constants.DbWithSuffixMaster(archiveSuffix);
            }
            return GetMasterDataBase(archive, dbName);
        }

        public static DataBase<ProductionDomainContext> GetMasterDataBase(bool archive = false, string dbName = "", bool noTracking = true)
        {
            DataBase<ProductionDomainContext> dataBase = new DataBase<ProductionDomainContext>(dbName);
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

            if (noTracking)
            {
                // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
                dataBase.DbContext.ChangeTracker.QueryTrackingBehavior =
                    QueryTrackingBehavior.NoTracking;
            }
            return dataBase;
        }


        public static DataBase<ResultContext> GetNewResultDataBase()
        {
            var dbName = Constants.DbWithSuffixResults();
            return GetResultDataBase(dbName);
        }

        public static DataBase<ResultContext> GetResultDataBase(string dbName)
        {
            DataBase<ResultContext> dataBase = new DataBase<ResultContext>(dbName);

            if (UseLocalDb() && Constants.IsWindows)
            {
                Constants.IsLocalDb = true;
            }
            else if (Constants.IsWindows)
            {
                Constants.IsLocalDb = false;
            }

            dataBase.ConnectionString = GetResultConnectionString(dataBase.DataBaseName);
            dataBase.DbContext = new ResultContext(
                new DbContextOptionsBuilder<ResultContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);

            // disable tracking (https://docs.microsoft.com/en-us/ef/core/querying/tracking)
            dataBase.DbContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;

            return dataBase;
        }
        public static DataBase<GanttPlanDBContext> GetGanttDataBase(string dbName)
        {
            DataBase<GanttPlanDBContext> dataBase = new DataBase<GanttPlanDBContext>(dbName);
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
            dataBase.DbContext = new GanttPlanDBContext(
                new DbContextOptionsBuilder<GanttPlanDBContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);
            return dataBase;
        }

        public static DataBase<HangfireDBContext> GetHangfireDataBase(string dbName)
        {
            DataBase<HangfireDBContext> dataBase = new DataBase<HangfireDBContext>(dbName);
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
            dataBase.DbContext = new HangfireDBContext(
                new DbContextOptionsBuilder<HangfireDBContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);
            return dataBase;
        }

        public static DataBase<DataGeneratorContext> GetGeneratorDataBase(string dbName)
        {
            DataBase<DataGeneratorContext> dataBase = new DataBase<DataGeneratorContext>(dbName);
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
            dataBase.DbContext = new DataGeneratorContext(
                new DbContextOptionsBuilder<DataGeneratorContext>()
                    .UseSqlServer(dataBase.ConnectionString.Value).Options);
            return dataBase;
        }

        private static bool CanConnect(string connectionString)
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