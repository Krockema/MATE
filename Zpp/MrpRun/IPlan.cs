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

        /**
         * returns the dbTransactionData after the run which should be completed persisted
         *
         * NOTE: only for validation purpose --> use this instead: (reLoad from database)
         * IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
         * IDbTransactionData dbTransactionData = new DbTransactionData(ProductionDomainContext, dbMasterDataCache)
         */
        IDbTransactionData GetDbTransactionData();

        /**
         * returns the dbMasterDataCache after the run which is+will not persisted
         *
         * NOTE:
         * only for validation purpose --> use this instead: (reLoad from database)
         * IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
         */
        IDbMasterDataCache GetNotPersistedDbMasterDataCache();
    }
}