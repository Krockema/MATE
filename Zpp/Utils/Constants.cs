using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Master40.DB.Data.Helper;

namespace Zpp.Utils
{
    public static class Constants
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsLocalDb = false;
        // TODO: the random/dateTime is a workaround, remove this if drop database query in Dispose() in TestClasses is added
        private static readonly string random = $"{new Random().Next(1, 1000000)}";

        private static string GetDateString()
        {
            string ticks = DateTime.UtcNow.Ticks.ToString();
            // the last 9 decimal places of ticks are enough
            return DateTime.Now.ToString("MM-dd_HH:mm") + $"__{ticks.Substring(10, ticks.Length-10)}";
        }
        
        public static readonly String DbConnectionZppLocalDb =
            $"Server=(localdb)\\mssqllocaldb;Database=zpp{GetDateString()};Trusted_Connection=True;MultipleActiveResultSets=true";


        public static String DbConnectionZppSqlServer()
        {
            return $"Server=localhost,1433;Database=zpp{GetDateString()};" +
                   $"MultipleActiveResultSets=true;User ID=SA;Password=123*Start#";
        }

        public static string EnumToString<T>(T enumValue, Type enumType)
        {
            return Enum.GetName(enumType, enumValue);
        }
    }
}