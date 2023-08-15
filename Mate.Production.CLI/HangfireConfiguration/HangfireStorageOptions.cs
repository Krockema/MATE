using System;
using Hangfire.SqlServer;

namespace Mate.Production.CLI.HangfireConfiguration
{
    public static class StorageOptions
    {
        public static SqlServerStorageOptions Default => new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(15),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(15),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,

        };
    }
}
