using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    public interface IDemandToProviders
    {
        bool IsSatisfied(Demand demand);

        void SatisfyDemandWithProviders(Demand demand, Providers providers);

        Provider FindNonExhaustedProvider(Demand demand);
    }
}