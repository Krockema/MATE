using System;
using System.Runtime.InteropServices;
using Mate.DataCore.Data.Helper.Types;

namespace Mate.DataCore.Data.Helper
{
    public static class Constants
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static string DbWithSuffixMaster(string appendix = "") => GetDbName("Test" + appendix);
        public static string DbWithSuffixResults() => GetDbName("TestResults");
        public static bool IsLocalDb = false;
        // TODO: the random/dateTime is a workaround, remove this if drop database query in Dispose() in TestClasses is added
        // private static readonly string random = $"{new Random().Next(1, 1000000)}";

        public static string GetDbName(string dbSuffix)
        {
            return (IsWindows)
                ? /* then */  $"{dbSuffix}{GetDateString()}"
                : /* else */  $"{dbSuffix}{GetDateString()}";

        }

        private static string GetDateString()
        {
            var date = new DateTime(DateTime.Now.Ticks);
            var ticks = date.Ticks.ToString();
            // the last 9 decimal places of ticks are enough
            return date.ToString("MM-dd_HH:mm")
                   + $"__{ticks.Substring(10, ticks.Length - 10)}";
        }

        public static String CreateLocalConnectionString(DataBaseName dataBaseName)
        {
            return $"Server=(localdb)\\mssqllocaldb;Database={dataBaseName.Value};Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public static String CreateServerConnectionString(DataBaseName dataBaseName)
        {
            return $"Server={DataBaseConfiguration.SQLServer};Database={dataBaseName.Value}" +
                   $"{DataBaseConfiguration.DefaultLocalHostConnection}";
        }

        public static string EnumToString<T>(T enumValue, Type enumType)
        {
            return Enum.GetName(enumType, enumValue);
        }
    }
}