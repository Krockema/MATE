using System;
using System.Runtime.InteropServices;

namespace Master40.XUnitTest.Preparations
{
        public static class ConnectionsStrings
        {
            public static readonly bool IsWindows = RuntimeInformation
                .IsOSPlatform(osPlatform: OSPlatform.Windows);

            public static readonly String DbConnectionZppWindows =
                "Server=(localdb)\\mssqllocaldb;Database=UnitTestDB;Trusted_Connection=True;MultipleActiveResultSets=true";

            public static readonly String DbConnectionZppUnix = "Server=localhost,1433;Database=UnitTestDB;MultipleActiveResultSets=true;User ID=SA;Password=123*Start#";
        }
}
