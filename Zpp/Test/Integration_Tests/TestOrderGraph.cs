using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.Test
{
    public class TestOrderGraph : AbstractTest
    {
        private const int ORDER_QUANTITY = 6;
        private const int DEFAULT_LOT_SIZE = 2;

        public TestOrderGraph()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
            LotSize.LotSize.SetDefaultLotSize(new Quantity(DEFAULT_LOT_SIZE));

            MrpRun.RunMrp(ProductionDomainContext);
        }

        /**
         * Verifies, that the orderGraph
         * - can be build up from DemandToProvider+ProviderToDemand table
         * - is a top down graph TODO is not done yet<br>
         * - has all demandToProvider and providerToDemand edges
         */
        [Fact]
        public void TestAllEdgesAreInOrderGraph()
        {
            // build orderGraph up
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            foreach (var customerOrderPart in dbMasterDataCache.T_CustomerOrderPartGetAll().GetAll()
            )
            {
                IGraph<INode> orderGraph = new OrderGraph(dbTransactionData,
                    (CustomerOrderPart) customerOrderPart);

                Assert.True(orderGraph.GetAllToNodes().Count > 0,
                    "There are no toNodes in the orderGraph.");

                int sumDemandToProviderAndProviderToDemand =
                    dbTransactionData.DemandToProviderGetAll().Count() +
                    dbTransactionData.ProviderToDemandGetAll().Count();

                Assert.True(sumDemandToProviderAndProviderToDemand == orderGraph.CountEdges(),
                    $"Should be equal size: sumDemandToProviderAndProviderToDemand " +
                    $"{sumDemandToProviderAndProviderToDemand} and  sumValuesOfOrderGraph {orderGraph.CountEdges()}");
            }
        }

        /**
         * Assumptions:
         * - IDemand:   T_CustomerOrderPart (COP), T_ProductionOrderBom (PrOB), T_StockExchange (SE:I)
         * - IProvider: T_PurchaseOrderPart (PuOP), T_ProductionOrder (PrO),    T_StockExchange (SE:W)
         *
         * Verifies that,
         * for demand (parent) --> provider (child) direction following takes effect:
         * - COP  --> PuOP | PrO | SE:W
         * - PrOB --> PuOP | PrO | SE:W
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
         */
        [Fact]
        public void TestEdgeTypes()
        {
            IDictionary<Type, Type[]> allowedEdges = new Dictionary<Type, Type[]>()
            {
                // demand --> provider
                {
                    typeof(CustomerOrderPart),
                    new Type[]
                    {
                        typeof(PurchaseOrderPart), typeof(ProductionOrder),
                        typeof(StockExchangeProvider)
                    }
                },
                {
                    typeof(ProductionOrderBom), new Type[]
                    {
                        typeof(PurchaseOrderPart), typeof(ProductionOrder),
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

            // build orderGraph up
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            foreach (var customerOrderPart in dbMasterDataCache.T_CustomerOrderPartGetAll().GetAll()
            )
            {
                // verify edgeTypes
                IGraph<INode> orderGraph = new OrderGraph(dbTransactionData,
                    (CustomerOrderPart) customerOrderPart);

                orderGraph.TraverseDepthFirst((INode parentNode, List<INode> childNodes) =>
                {
                    if (childNodes != null && childNodes.Any())
                    {
                        Type parentType = parentNode.GetEntity().GetType();
                        foreach (var childNode in childNodes)
                        {
                            Type childType = childNode.GetEntity().GetType();
                            Assert.True(allowedEdges[parentType].Contains(childType),
                                $"This is no valid edge: {parentType} --> {childType}");
                        }
                    }
                });
            }
        }
    }
}