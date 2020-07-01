using System.Collections.Generic;
using Master40.DB.Interfaces;
using Master40.SimulationMrp;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestDemandToProvider : AbstractTest
    {
        public TestDemandToProvider()
        {
        }

        [Fact]
        public void TestAllDemandsAreInDemandToProviderTable()
        {
            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.ReloadTransactionData();

            Demands allDbDemands = dbTransactionData.DemandsGetAll();
            LinkDemandAndProviderTable demandToProviderTable =
                dbTransactionData.DemandToProviderGetAll();

            foreach (var demand in allDbDemands)
            {
                bool isInDemandToProviderTable = demandToProviderTable.Contains(demand);
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
            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.ReloadTransactionData();

            Demands demands = dbTransactionData.DemandsGetAll();
            Providers providers = dbTransactionData.ProvidersGetAll();
            Demands unsatisfiedDemands = providers.CalculateUnsatisfiedDemands(demands);
            foreach (var unsatisfiedDemand in unsatisfiedDemands)
            {
                Assert.True(false,
                    $"The demand {unsatisfiedDemand} should be satisfied, but it is NOT.");
            }
        }
        
        [Fact]
        public void TestEveryQuantityOnArrowIsPositive()
        {
            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.ReloadTransactionData();

            List<ILinkDemandAndProvider>
                demandAndProviderLinks = new List<ILinkDemandAndProvider>();
            demandAndProviderLinks.AddRange(dbTransactionData.DemandToProviderGetAll());
            demandAndProviderLinks.AddRange(dbTransactionData.ProviderToDemandGetAll());

            foreach (var demandAndProviderLink in demandAndProviderLinks)
            {
                Assert.False(
                    demandAndProviderLink.GetQuantity().IsNegative() ||
                    demandAndProviderLink.GetQuantity().IsNull(),
                    $"A quantity on arrow ({demandAndProviderLink}) cannot be negative or null.");
            }
        }
    }
}