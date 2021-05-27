using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Mate.Production.CLI.HangfireConfiguration
{
    public class KeepJobInStore : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(365);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(1);
        }
    }
}
