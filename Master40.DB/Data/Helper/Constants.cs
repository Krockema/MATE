using System;
using System.Runtime.InteropServices;
using Master40.DB.Data.Helper.Types;

namespace Master40.DB.Data.Helper
{
    public static class Constants
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static DataBaseName DbSuffixMaster = new DataBaseName(GetDbName("Test")) ;
        public static DataBaseName DbSuffixResults = new DataBaseName(GetDbName("TestResults"));

        public static bool IsLocalDb = false;
        // TODO: the random/dateTime is a workaround, remove this if drop database query in Dispose() in TestClasses is added
        private static readonly string random = $"{new Random().Next(1, 1000000)}";

        public static string GetDbName(string dbSuffix)
        {
            if (IsWindows)
            {
                // use always the same databaseName and drop db before the next test
                return $"{dbSuffix}{GetDateString()}";
            }
            else
            {
                // never got this feature working: use always the same databaseName and drop db before the next test
                return $"{dbSuffix}{GetDateString()}";
            }
        }

        private static string GetDateString()
        {
            string ticks = DateTime.UtcNow.Ticks.ToString();
            // the last 9 decimal places of ticks are enough
            return DateTime.Now.ToString("MM-dd_HH:mm") + $"__{ticks.Substring(10, ticks.Length - 10)}";
        }

        public static String CreateLocalConnectionString(DataBaseName dataBaseName)
        { 
            return $"Server=(localdb)\\mssqllocaldb;Database={dataBaseName.Value}" + 
            ";Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public static String CreateServerConnectionString(DataBaseName dataBaseName) { 
            return $"Server=localhost,1433;Database={dataBaseName.Value};" +
            $"MultipleActiveResultSets=true;User ID=SA;Password=123*Start#";
        }

        public static string EnumToString<T>(T enumValue, Type enumType)
        {
            return Enum.GetName(enumType, enumValue);
        }
    }
}