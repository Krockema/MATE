using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    /**
     * This is returned by MrpRun for further processing e.g. for the confirmations
     */
    public interface IPlan
    {
        IDemands GetDemands();

        IProviders GetProviders();

        IDemandToProviderTable GetDemandToProviders();

        IDbTransactionData GetDbTransactionData();
    }
}