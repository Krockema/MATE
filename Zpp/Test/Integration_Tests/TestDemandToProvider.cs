using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.ProviderDomain;

namespace Zpp.Test
{
    public class TestDemandToProvider : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;

        public TestDemandToProvider()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }
        
        [Fact]
        public void TestTransactionDataIsCompletelyPersistedAndNoEntityWasLostDuringPersisting()
        {
            // TODO
        }
        
        [Fact]
        public void TestAllDemandsAreInDemandToProviderTable()
        {
            MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // TODO: let T_Demand, T_Provider have an own Id and a foreign as reference

            IDemandToProvidersMap demandToProvidersMap = dbTransactionData.DemandToProviderGetAll()
                .ToDemandToProvidersMap(dbTransactionData);
            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            Demands demandsInDemandToProviderTable = demandToProvidersMap.GetDemands();

            foreach (var demand in allDbDemands.GetAll())
            {
                bool isInDemandToProviderTable =
                    demandsInDemandToProviderTable.GetAll().Contains(demand);
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
            MrpRun.RunMrp(ProductionDomainContext);
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
            MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemandToProvidersMap demandToProvidersMap = dbTransactionData.DemandToProviderGetAll()
                .ToDemandToProvidersMap(dbTransactionData);
            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            foreach (var demand in allDbDemands.GetAll())
            {
                bool isSatisfied = demandToProvidersMap.IsSatisfied(demand);
                Assert.True(isSatisfied, $"Demand {demand} is not satisfied.");
            }
        }
        
        
    }
}