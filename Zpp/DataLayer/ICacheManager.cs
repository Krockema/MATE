using Master40.DB.Data.Context;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util.Graph.impl;
using Zpp.Utils;

namespace Zpp.DataLayer
{
    /**
     * Controls data layer objects like dbMasterCache, dbTransactionData to force singleton pattern
     */
    public interface ICacheManager
    {
        void InitByReadingFromDatabase(string testConfiguration, bool addInitialStockLevels);
        
        /**
         * Reloads TransactionData from database.
         * Only reload after DbPersist() Call ! Else the TransactionData is away
         */
        IDbTransactionData ReloadTransactionData();

        IDbMasterDataCache GetMasterDataCache();
        
        /**
         * Don't store the returned reference, since it becomes invalid on ReloadTransactionData() call
         */
        IDbTransactionData GetDbTransactionData();

        /**
         * persists dbTransactionData, dbTransactionDataArchive
         */
        void Persist();
        
        /**
         * Don't store the returned reference, since it becomes invalid on ReloadTransactionData() call
         */
        IDbTransactionData GetDbTransactionDataArchive();

        /**
         * Don't store the returned reference, since it becomes invalid on ReloadTransactionData() call
         */
        IOpenDemandManager GetOpenDemandManager();

        ProductionDomainContext GetProductionDomainContext();

        ICentralPlanningConfiguration GetTestConfiguration();

        /**
         * Free resources like dbConnections
         */
        void Dispose();

        IAggregator GetAggregator();

        void ReadInTestConfiguration(string testConfigurationFileNames);

        /**
         * GetAggregator() and GetDbTransactionData() returns the archive, only for tests
         */
        void UseArchiveForGetters();
        
        /**
         * GetAggregator() and GetDbTransactionData() behaves as before call UseArchiveForGetters()
         */
        void UseArchiveForGettersRevert();
    }
}