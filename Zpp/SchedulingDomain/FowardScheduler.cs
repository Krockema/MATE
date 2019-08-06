using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.SchedulingDomain
{
    public class ForwardScheduler
    {
        public static DueTime FindMinDueTime(IDemands demands, IProviders providers, IDbTransactionData dbTransactionData)
        {
            DueTime minDueTime = null;
            
            // find min dueTime
            foreach (var provider in providers.GetAll())
            {
                DueTime currentDueTime = provider.GetDueTime(dbTransactionData);
                if (minDueTime == null)
                {
                    minDueTime = currentDueTime;
                }

                if (minDueTime.GetValue() > currentDueTime.GetValue())
                {
                    minDueTime = currentDueTime;
                }
            }

            return minDueTime;
        }
    }
}