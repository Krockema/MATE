using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.SqlServer;

namespace Master40.Simulation.HangfireConfiguration
{
    public static class StorageOptions
    {
        public static SqlServerStorageOptions Default => new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(15),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(15),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            UsePageLocksOnDequeue = true,
            DisableGlobalLocks = true,

        };
    }
}
