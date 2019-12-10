using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Nominal;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.OpenDemand;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Master40.XUnitTest.Zpp.Integration_Tests.Verification
{
    public class VerifyApplyConfirmations : AbstractVerification
    {
        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestApplyConfirmations(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = global::Zpp.ZppConfiguration.CacheManager.GetAggregator();

            // Ein CustomerOrderParts darf kein Kind haben und darf nicht beendet sein
            Ids customerOrderIds = new Ids();
            foreach (var demand in dbTransactionData.CustomerOrderPartGetAll())
            {
                CustomerOrderPart customerOrderPart = (CustomerOrderPart) demand;
                customerOrderIds.Add(customerOrderPart.GetCustomerOrderId());
                Providers childs = aggregator.GetAllChildProvidersOf(customerOrderPart);
                Assert.False(childs.Any());
                Assert.False(customerOrderPart.IsFinished());
            }

            // Ein PurchaseOrderPart darf kein Elter haben und darf nicht beendet sein.
            Ids purchaseOrderIds = new Ids();
            foreach (var demand in dbTransactionData.PurchaseOrderPartGetAll())
            {
                PurchaseOrderPart purchaseOrderPart = (PurchaseOrderPart) demand;
                purchaseOrderIds.Add(purchaseOrderPart.GetPurchaseOrderId());

                Assert.False(purchaseOrderPart.IsFinished());
                Demands demands = aggregator.GetAllParentDemandsOf(demand);
                Assert.True(demands == null || demands.Any() == false);
            }

            // Für jede CustomerOrder muss es mind. noch ein CustomerOrderPart geben.
            foreach (var customerOrder in dbTransactionData.CustomerOrderGetAll())
            {
                Assert.True(customerOrderIds.Contains(customerOrder.GetId()));
            }

            // Für jede PurchaseOrder muss es mind. noch ein PurchaseOrderPart geben.
            foreach (var purchaseOrder in dbTransactionData.PurchaseOrderGetAll())
            {
                Assert.True(purchaseOrderIds.Contains(purchaseOrder.GetId()));
            }
            
            // Ein StockExchangeProvider muss mind. ein Kind haben.
            foreach (var stockExchangeProvider in dbTransactionData.StockExchangeProvidersGetAll())
            {
                Demands childs = aggregator.GetAllChildDemandsOf(stockExchangeProvider);
                Assert.True(childs.Any());
            }
            
            // Ein StockExchangeDemand darf nicht beendet und geschlossen sein.
            foreach (var stockExchangeDemand in dbTransactionData.StockExchangeDemandsGetAll())
            {
                bool isOpen = OpenDemandManager.IsOpen((StockExchangeDemand) stockExchangeDemand);
                Assert.False(stockExchangeDemand.IsFinished() && isOpen == false);
            }

            // Eine ProductionOrder darf nicht beendet sein und für eine ProductionOrder
            // muss es mind. eine  Operation geben.
            Ids productionOrderIds = new Ids();
            Ids operationIds = new Ids();
            foreach (var operation in dbTransactionData.ProductionOrderOperationGetAll())
            {
                Id productionOrderId = operation.GetProductionOrderId();
                if (productionOrderIds.Contains(productionOrderId) == false)
                {
                    productionOrderIds.Add(productionOrderId);
                }

                operationIds.Add(operation.GetId());
            }
            foreach (var provider in dbTransactionData.ProductionOrderGetAll())
            {
                ProductionOrder productionOrder = (ProductionOrder) provider;
                Assert.False(productionOrder.DetermineProductionOrderState()
                    .Equals(State.Finished));
                Assert.True(productionOrderIds.Contains(productionOrder.GetId()));
            }
            
            // Für jede ProductionOrderBom muss die dazugehörige Operation da sein.
            foreach (var demand in dbTransactionData.ProductionOrderBomGetAll())
            {
                ProductionOrderBom productionOrderBom = (ProductionOrderBom) demand;
                operationIds.Contains(productionOrderBom.GetProductionOrderOperationId());
            }

            // Für jeden DemandToProvider und ProviderToDemand müssen die dazugehörigen
            // Demands und Provider existieren.
            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(demandToProvider.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(demandToProvider.GetProviderId());
                Assert.NotNull(demand);
                Assert.NotNull(provider);
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(providerToDemand.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(providerToDemand.GetProviderId());
                Assert.NotNull(demand);
                Assert.NotNull(provider);
            }
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestApplyConfirmationsArchive(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionDataArchive =
                global::Zpp.ZppConfiguration.CacheManager.GetDbTransactionDataArchive();
            IAggregator aggregator = new Aggregator(dbTransactionDataArchive);

            // ZPP should only use the archive for the rest of the test
            global::Zpp.ZppConfiguration.CacheManager.UseArchiveForGetters();

            // Ein CustomerOrderParts darf kein Kind haben und muss beendet sein
            Ids customerOrderIds = new Ids();
            foreach (var demand in dbTransactionDataArchive.CustomerOrderPartGetAll())
            {
                CustomerOrderPart customerOrderPart = (CustomerOrderPart) demand;
                customerOrderIds.Add(customerOrderPart.GetCustomerOrderId());
                Providers childs = aggregator.GetAllChildProvidersOf(customerOrderPart);
                Assert.False(childs.Any());
                Assert.True(customerOrderPart.IsFinished());
            }

            // Ein PurchaseOrderPart darf kein Elter haben und muss beendet sein.
            Ids purchaseOrderIds = new Ids();
            foreach (var demand in dbTransactionDataArchive.PurchaseOrderPartGetAll())
            {
                PurchaseOrderPart purchaseOrderPart = (PurchaseOrderPart) demand;
                purchaseOrderIds.Add(purchaseOrderPart.GetPurchaseOrderId());

                Assert.True(purchaseOrderPart.IsFinished());
                Demands demands = aggregator.GetAllParentDemandsOf(demand);
                Assert.True(demands == null || demands.Any() == false);
            }

            // Für jede CustomerOrder muss es mind. noch ein CustomerOrderPart geben.
            foreach (var customerOrder in dbTransactionDataArchive.CustomerOrderGetAll())
            {
                Assert.True(customerOrderIds.Contains(customerOrder.GetId()));
            }

            // Für jede PurchaseOrder muss es mind. noch ein PurchaseOrderPart geben.
            foreach (var purchaseOrder in dbTransactionDataArchive.PurchaseOrderGetAll())
            {
                Assert.True(purchaseOrderIds.Contains(purchaseOrder.GetId()));
            }
            
            // Ein StockExchangeProvider muss mind. ein Kind haben.
            foreach (var stockExchangeProvider in dbTransactionDataArchive.StockExchangeProvidersGetAll())
            {
                Demands childs = aggregator.GetAllChildDemandsOf(stockExchangeProvider);
                Assert.True(childs.Any());
            }
            
            // Ein StockExchangeDemand muss beendet und geschlossen sein.
            foreach (var stockExchangeDemand in dbTransactionDataArchive.StockExchangeDemandsGetAll())
            {
                bool isOpen = OpenDemandManager.IsOpen((StockExchangeDemand) stockExchangeDemand);
                Assert.True(stockExchangeDemand.IsFinished() && isOpen == false);
            }

            // Eine ProductionOrder muss beendet sein und für eine ProductionOrder
            // muss es mind. eine  Operation geben.
            Ids productionOrderIds = new Ids();
            Ids operationIds = new Ids();
            foreach (var operation in dbTransactionDataArchive.ProductionOrderOperationGetAll())
            {
                Id productionOrderId = operation.GetProductionOrderId();
                if (productionOrderIds.Contains(productionOrderId) == false)
                {
                    productionOrderIds.Add(productionOrderId);
                }

                operationIds.Add(operation.GetId());
            }
            foreach (var provider in dbTransactionDataArchive.ProductionOrderGetAll())
            {
                ProductionOrder productionOrder = (ProductionOrder) provider;
                Assert.True(productionOrder.DetermineProductionOrderState()
                    .Equals(State.Finished));
                Assert.True(productionOrderIds.Contains(productionOrder.GetId()));
            }
            
            // Für jede ProductionOrderBom muss die dazugehörige Operation da sein.
            foreach (var demand in dbTransactionDataArchive.ProductionOrderBomGetAll())
            {
                ProductionOrderBom productionOrderBom = (ProductionOrderBom) demand;
                operationIds.Contains(productionOrderBom.GetProductionOrderOperationId());
            }

            // Für jeden DemandToProvider und ProviderToDemand müssen die dazugehörigen
            // Demands und Provider existieren.
            foreach (var demandToProvider in dbTransactionDataArchive.DemandToProviderGetAll())
            {
                Demand demand = dbTransactionDataArchive.DemandsGetById(demandToProvider.GetDemandId());
                Provider provider =
                    dbTransactionDataArchive.ProvidersGetById(demandToProvider.GetProviderId());
                Assert.NotNull(demand);
                Assert.NotNull(provider);
            }

            foreach (var providerToDemand in dbTransactionDataArchive.ProviderToDemandGetAll())
            {
                Demand demand = dbTransactionDataArchive.DemandsGetById(providerToDemand.GetDemandId());
                Provider provider =
                    dbTransactionDataArchive.ProvidersGetById(providerToDemand.GetProviderId());
                Assert.NotNull(demand);
                if (demand.GetType() == typeof(StockExchangeDemand) && provider == null)
                {
                    // no check for notNull, because stockExchangeProviders can be inconsistent TODO doc
                }
                else
                {
                    Assert.NotNull(provider);   
                }
            }
        }
    }
}