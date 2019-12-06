using System.Collections.Generic;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper.DistributionProvider;
using Zpp;
using Zpp.DataLayer;
using Zpp.Utils;

namespace Master40.SimulationMrp.impl.CustomerOrder.impl
{
    public class CustomerOrderCreator : ICustomerOrderCreator
    {
        private IOrderGenerator _orderGenerator = null;
        private readonly Quantity _defaultCustomerOrderQuantityPerCycle = new Quantity(10);
        private int _createdCustomerOrdersCount = 0;

        public CustomerOrderCreator(Quantity defaultCustomerOrderQuantityPerCycle)
        {
            if (defaultCustomerOrderQuantityPerCycle != null &&
                defaultCustomerOrderQuantityPerCycle.IsSmallerThan(
                    _defaultCustomerOrderQuantityPerCycle))
            {
                _defaultCustomerOrderQuantityPerCycle = defaultCustomerOrderQuantityPerCycle;
            }

            var orderArrivalRate = new OrderArrivalRate(0.025);
            IDbMasterDataCache masterDataCache = ZppConfiguration.CacheManager.GetMasterDataCache();
            _orderGenerator = TestScenario.GetOrderGenerator(new MinDeliveryTime(200),
                new MaxDeliveryTime(1430), orderArrivalRate, masterDataCache.M_ArticleGetAll(),
                masterDataCache.M_BusinessPartnerGetAll());
        }

        public void CreateCustomerOrders(SimulationInterval interval,
            Quantity customerOrderQuantity)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            ICentralPlanningConfiguration testConfiguration =
                ZppConfiguration.CacheManager.GetTestConfiguration();
            // skip creating COs, if goal has reached
            if (_createdCustomerOrdersCount >= testConfiguration.CustomerOrderPartQuantity)
            {
                return;
            }
            
            int cycles =
                testConfiguration.SimulationMaximumDuration /
                testConfiguration.SimulationInterval;
            int customerOrdersToCreate;
            int totalCustomerOrdersToCreate =
                (int) customerOrderQuantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();
            if (totalCustomerOrdersToCreate < cycles)
            {
                customerOrdersToCreate = totalCustomerOrdersToCreate;
            }
            else
            {
                customerOrdersToCreate = totalCustomerOrdersToCreate / (cycles+1);// round up
            }

            List<T_CustomerOrder> createdCustomerOrders = new List<T_CustomerOrder>();
            List<T_CustomerOrderPart> createdCustomerOrderParts = new List<T_CustomerOrderPart>();

            while (createdCustomerOrders.Count < customerOrdersToCreate)
            {
                var creationTime = interval.StartAt;
                var endOrderCreation = interval.EndAt;
                
                while (creationTime < endOrderCreation)
                {
                    var order = _orderGenerator.GetNewRandomOrder(time: creationTime);
                    foreach (var orderPart in order.CustomerOrderParts)
                    {
                        orderPart.CustomerOrder = order;
                        orderPart.CustomerOrderId = order.Id;
                        createdCustomerOrderParts.Add(orderPart);
                    }

                    createdCustomerOrders.Add(order);

                    // TODO : Handle this another way
                    creationTime = order.CreationTime;

                    if (createdCustomerOrders.Count >= customerOrdersToCreate)
                    {
                        break;
                    }
                }
            }
            dbTransactionData.CustomerOrderPartAddAll(createdCustomerOrderParts);
            dbTransactionData.CustomerOrderAddAll(createdCustomerOrders);
            _createdCustomerOrdersCount += createdCustomerOrders.Count;
        }
    }
}