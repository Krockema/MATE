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

        /**
         * returns the DbMasterDataCache after the run which is+will not persisted
         * (only for validation purpose)
         */
        IDbMasterDataCache GetNotPersistedDbMasterDataCache();
    }
}