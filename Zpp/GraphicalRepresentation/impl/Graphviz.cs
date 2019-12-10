using Master40.DB.Data.Helper;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Util;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Zpp.GraphicalRepresentation.impl
{
    public class Graphviz : IGraphviz
    {

        private string ToGraphizString(Demand demand)
        {

            return $"\\n{demand.GetId()}: {demand.GetArticle().Name};Anzahl: {demand.GetQuantity()};" 
                    +$"\\nStart/End: {demand.GetStartTimeBackward()}/{demand.GetEndTimeBackward()};"
                ;
        }

        private string ToGraphizString(Provider provider)
        {
            return $"\\n{provider.GetId()}: {provider.GetArticle().Name};Anzahl: {provider.GetQuantity()};" 
                    +$"\\nStart/End: {provider.GetStartTimeBackward()}/{provider.GetEndTimeBackward()};"
                ;
        }

        public string GetGraphizString(CustomerOrderPart customerOrderPart)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"D: CustomerOrderPart;{ToGraphizString(customerOrderPart)}";
            return graphizString;
        }

        public string GetGraphizString(ProductionOrderBom productionOrderBom)
        {
            // Demand(CustomerOrder);20;Truck

            string graphizString;
            ProductionOrderOperation productionOrderOperation =
                productionOrderBom.GetProductionOrderOperation();
            if (productionOrderOperation != null)
            {
                T_ProductionOrderOperation tProductionOrderOperation =
                    productionOrderOperation.GetValue();
                graphizString = $"D: ProductionOrderBom;{ToGraphizString(productionOrderBom)}" 
                                // + $"bs({tProductionOrderOperation.StartBackward});" +
                                // $"be({tProductionOrderOperation.EndBackward});" +
                                // $"\\nOperationName: {tProductionOrderOperation}"
                    ;
            }
            else
            {
                throw new MrpRunException("Every productionOrderBom must have exact one operation.");
            }

            return graphizString;
        }

        public string GetGraphizString(StockExchangeDemand stockExchangeDemand)
        {
            // Demand(CustomerOrder);20;Truck
            string exchangeType = Constants.EnumToString(
                ((T_StockExchange) stockExchangeDemand.ToIDemand()).ExchangeType,
                typeof(ExchangeType));
            string graphizString =
                $"D: StockExchangeDemand;{ToGraphizString(stockExchangeDemand)}";
            return graphizString;
        }

        public string GetGraphizString(ProductionOrderOperation productionOrderOperation)
        {
            return $"Operation;{productionOrderOperation.GetId()};\\n{productionOrderOperation.GetValue().Name};\\n" +
                   $"bs({productionOrderOperation.GetValue().StartBackward});" +
                   $"be({productionOrderOperation.GetValue().EndBackward});";
        }

        public string GetGraphizString(ProductionOrder productionOrder)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P: ProductionOrder;{ToGraphizString(productionOrder)}";
            return graphizString;
        }

        public string GetGraphizString(PurchaseOrderPart purchaseOrderPart)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P: PurchaseOrderPart;{ToGraphizString(purchaseOrderPart)}";
            return graphizString;
        }

        public string GetGraphizString(StockExchangeProvider stockExchangeProvider)
        {
            // Demand(CustomerOrder);20;Truck
            string exchangeType = Constants.EnumToString(
                ((T_StockExchange) stockExchangeProvider.ToIProvider()).ExchangeType,
                typeof(ExchangeType));
            string graphizString =
                $"P: StockExchangeProvider;{ToGraphizString(stockExchangeProvider)}";
            return graphizString;
        }

        public string GetGraphizString(IScheduleNode node)
        {
            switch (node)
            {
                case StockExchangeProvider t1:
                    return GetGraphizString(t1);
                case PurchaseOrderPart t2:
                    return GetGraphizString(t2);
                case ProductionOrder t3:
                    return GetGraphizString(t3);
                case ProductionOrderOperation t4:
                    return GetGraphizString(t4);
                case StockExchangeDemand t5:
                    return GetGraphizString(t5);
                case ProductionOrderBom t6:
                    return GetGraphizString(t6);
                case CustomerOrderPart t7:
                    return GetGraphizString(t7);
                case Node t8:
                    throw new MrpRunException("Call getEntity() before calling this method.");
                default: return node.ToString();
            }
        }
    }
}