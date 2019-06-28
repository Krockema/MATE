using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    /**
     * Maps one provider to demands. A provider can satisfy possibly n demands
     */
    public interface IProviderToDemands
    {
        /// <summary>
        /// demand is added to the provider
        /// </summary>
        /// <param name="provider">that gets a new demand</param>
        /// <param name="demand">that is added to a provider</param>
        void AddDemandForProvider(Provider provider, Demand demand);

        Provider FindNonExhaustedProvider(Demand demand);
    }
}