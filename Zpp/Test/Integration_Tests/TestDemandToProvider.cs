using Xunit;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.WrappersForCollections;

namespace Zpp.Test.Integration_Tests
{
    public class TestDemandToProvider : AbstractTest
    {

        public TestDemandToProvider()
        {
            
        }
        
        [Fact]
        public void TestTransactionDataIsCompletelyPersistedAndNoEntityWasLostDuringPersisting()
        {
            // TODO
        }
        
        [Fact]
        public void TestAllDemandsAreInDemandToProviderTable()
        {
            MrpRun.MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            IDemandToProviderTable demandToProviderTable = dbTransactionData.GetProviderManager().GetDemandToProviderTable();

            foreach (var demand in allDbDemands.GetAll())
            {
                bool isInDemandToProviderTable =
                    demandToProviderTable.Contains(demand);
                Assert.True(isInDemandToProviderTable,
                    $"Demand {demand} is NOT in demandToProviderTable.");
            }
        }
        
        /**
         * Tests, if the demands are theoretically satisfied by looking for providers in ProviderTable
         * --> success does not mean, that the demands from demandToProvider table are satisfied by providers from demandToProviderTable
         */
        [Fact]
        public void TestAllDemandsAreSatisfiedWithinProviderTable()
        {
            MrpRun.MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemands demands = dbTransactionData.DemandsGetAll();
            IProviders providers = dbTransactionData.ProvidersGetAll();
            IDemands unsatisfiedDemands = providers.CalculateUnsatisfiedDemands(demands);
            foreach (var unsatisfiedDemand in unsatisfiedDemands.GetAll())
            {
                Assert.True(false,
                    $"The demand {unsatisfiedDemand} should be satisfied, but it is NOT.");
            }
        }
        
        [Fact]
        public void TestAllDemandsAreSatisfiedByProvidersOfDemandToProviderTable()
        {
            MrpRun.MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            foreach (var demand in allDbDemands.GetAll())
            {
                bool isSatisfied = dbTransactionData.GetProviderManager().IsSatisfied(demand);
                Assert.True(isSatisfied, $"Demand {demand} is not satisfied.");
            }
        }

    }
}