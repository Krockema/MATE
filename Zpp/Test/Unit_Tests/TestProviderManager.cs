using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Xunit;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.MrpRun;
using Zpp.MrpRun.NodeManagement;
using Zpp.MrpRun.StockManagement;

namespace Zpp.Test.Unit_Tests
{
    public class TestProviderManager : AbstractTest
    {
        public TestProviderManager()
        {
        }

        /**
         * Verifies, that a demand (COP, PrOBom) is fulfilled by an existing provider, if such exists
         * - 
         */
        [Fact(Skip="Must be completely rewritten")]
        // TODO: Must be completely rewritten
        public void TestSatisfyByExistingDemand()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            StockManager stockManager =
                new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);
            
            IOpenDemandManager openDemandManager = new OpenDemandManager();

            IProviderManager providerManager = new ProviderManager(dbTransactionData);
            IProvidingManager providingManager = (IProvidingManager) providerManager;
            CustomerOrderPart customerOrderPart1 =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache, 4);
            CustomerOrderPart customerOrderPart2 =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache, 5);
            ProductionOrder productionOrder = EntityFactory.CreateT_ProductionOrder(
                dbMasterDataCache, dbTransactionData, customerOrderPart1,
                customerOrderPart1.GetQuantity().Plus(customerOrderPart2.GetQuantity()));
            providerManager.AddProvider(customerOrderPart1.GetId(), productionOrder,
                customerOrderPart1.GetQuantity());

            ResponseWithProviders responseWithProviders = providingManager.Satisfy(customerOrderPart2,
                customerOrderPart2.GetQuantity(), dbTransactionData);
            MrpRun.MrpRun.ProcessProvidingResponse(responseWithProviders, providerManager, stockManager,
                dbTransactionData, customerOrderPart2, openDemandManager);

            bool isSatisfied = responseWithProviders.IsSatisfied() && responseWithProviders.GetProviders().Any() == false &&
                               responseWithProviders.GetDemandToProviders().Count == 1 &&
                               IsValidDemandToProvider(responseWithProviders.GetDemandToProviders()[0],
                                   customerOrderPart2, productionOrder,
                                   customerOrderPart2.GetQuantity());
            Assert.True(isSatisfied, "Demand was not satisfied by existing provider.");
        }

        private bool IsValidDemandToProvider(T_DemandToProvider demandToProvider,
            Common.DemandDomain.Demand expectedDemand, Common.ProviderDomain.Provider expectedProvider, Quantity expectedQuantity)
        {
            return demandToProvider.DemandId.Equals(expectedDemand.GetId().GetValue()) &&
                   demandToProvider.ProviderId.Equals(expectedProvider.GetId().GetValue()) &&
                   demandToProvider.Quantity.Equals(expectedQuantity.GetValue());
        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestAddProvider()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // TODO
            Assert.True(false);
        }
    }
}