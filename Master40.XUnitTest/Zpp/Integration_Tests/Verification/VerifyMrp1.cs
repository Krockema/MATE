using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;

namespace Master40.XUnitTest.Zpp.Integration_Tests.Verification
{
    public class VerifyMrp1 : AbstractVerification
    {
        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestMrp1(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionData();
            IDbTransactionData dbTransactionDataArchive =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionDataArchive();
            
            VerifyQuantities(dbTransactionData, false);
            VerifyQuantities(dbTransactionDataArchive, true);
            VerifyEdgeTypes(dbTransactionData, false);
            VerifyEdgeTypes(dbTransactionDataArchive, true);
        }

        private void VerifyQuantities(IDbTransactionData dbTransactionData, bool isArchive)
        {
            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(demandToProvider.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(demandToProvider.GetProviderId());

                // every quantity > 0
                Assert.True(demand.GetQuantity().IsGreaterThan(Quantity.Zero()));
                Assert.True(provider.GetQuantity().IsGreaterThan(Quantity.Zero()));
                Assert.True(demandToProvider.GetQuantity().IsGreaterThan(Quantity.Zero()));

                // demand's quantity == provider's quantity
                Assert.True(demand.GetQuantity().Equals(provider.GetQuantity()));
                // demand's quantity == demandToProvider's quantity
                Assert.True(demand.GetQuantity().Equals(demandToProvider.GetQuantity()));
                // provider's quantity == demandToProvider's quantity
                Assert.True(provider.GetQuantity().Equals(demandToProvider.GetQuantity()));
            }
            
            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(providerToDemand.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(providerToDemand.GetProviderId());

                // this is a special case, doc can be found at TODO
                if (provider ==null && demand.GetType() == typeof(StockExchangeDemand) && isArchive )
                {
                    continue;
                }
                
                // every quantity > 0
                Assert.True(demand.GetQuantity().IsGreaterThan(Quantity.Zero()));
                Assert.True(provider.GetQuantity().IsGreaterThan(Quantity.Zero()));
                Assert.True(providerToDemand.GetQuantity().IsGreaterThan(Quantity.Zero()));

                if (provider.GetType() == typeof(StockExchangeProvider))
                {
                    // stockExchangeProvider's quantity >= providerToDemand's quantity
                    Assert.True(
                        provider.GetQuantity().IsGreaterThanOrEqualTo(providerToDemand.GetQuantity()));
                }
                else if (provider.GetType() == typeof(ProductionOrder))
                {
                    // no condition
                }
                else if (provider.GetType() == typeof(PurchaseOrderPart))
                {
                    Assert.True(false, "This arrow is not allowed.");
                }
                else
                {
                    Assert.True(false, "Unexpected type.");
                }
            }
        }
        
        /**
         * Assumptions:
         * - IDemand:   T_CustomerOrderPart (COP), T_ProductionOrderBom (PrOB), T_StockExchange (SE:I)
         * - IProvider: T_PurchaseOrderPart (PuOP), T_ProductionOrder (PrO),    T_StockExchange (SE:W)
         *
         * Verifies that,
         * for demand (parent) --> provider (child) direction following takes effect:
         * - COP  --> SE:W
         * - PrOB --> SE:W
         * - SE:I --> PuOP | PrO
         *
         * for provider (parent) --> demand (child) direction following takes effect:
         * - PuOP --> NONE
         * - PrO  --> PrOB
         * - SE:W --> SE:I
         *
         * where SE:I = StockExchangeDemand
         * and SE:W = StockExchangeProvider
         * TODO: remove StockExchangeType from T_StockExchange since it's exactly specified by Withdrawal/Insert
         *
         * TODO: add a new Quality to test: check that NONE is only if it's defined in upper connections
         * (e.g. after a PrO MUST come another Demand )
         */
        private void VerifyEdgeTypes(IDbTransactionData dbTransactionData, bool isArchive)
        {
           
            IDictionary<Type, Type[]> allowedEdges = new Dictionary<Type, Type[]>()
            {
                // demand --> provider
                {
                    typeof(CustomerOrderPart),
                    new Type[]
                    {
                        typeof(StockExchangeProvider)
                    }
                },
                {
                    typeof(ProductionOrderBom), new Type[]
                    {
                        typeof(StockExchangeProvider)
                    }
                },
                {
                    typeof(StockExchangeDemand),
                    new Type[] {typeof(PurchaseOrderPart), typeof(ProductionOrder)}
                },
                // provider --> demand
                {
                    typeof(PurchaseOrderPart),
                    new Type[] { }
                },
                {
                    typeof(ProductionOrder),
                    new Type[] {typeof(ProductionOrderBom)}
                },
                {
                    typeof(StockExchangeProvider),
                    new Type[] {typeof(StockExchangeDemand)}
                }
            };
            
            // verify edgeTypes
            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(demandToProvider.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(demandToProvider.GetProviderId());
                    Assert.True(allowedEdges[demand.GetType()].Contains(provider.GetType()),
                        $"This is no valid edge: {demand.GetType()} --> {provider.GetType()}");
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(providerToDemand.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(providerToDemand.GetProviderId());
                
                // this is a special case, doc can be found at TODO
                if (provider ==null && demand.GetType() == typeof(StockExchangeDemand) && isArchive )
                {
                    continue;
                }
                
                Assert.True(allowedEdges[provider.GetType()].Contains(demand.GetType()),
                    $"This is no valid edge: {provider.GetType()} --> {demand.GetType()}");
            }
        }
        
    }

}