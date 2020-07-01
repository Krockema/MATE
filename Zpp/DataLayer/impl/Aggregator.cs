using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util;

namespace Zpp.DataLayer.impl
{
    public class Aggregator : IAggregator
    {
        private readonly IDbMasterDataCache _dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();

        private readonly IDbTransactionData _dbTransactionData;

        public Aggregator(IDbTransactionData dbTransactionData)
        {
            _dbTransactionData = dbTransactionData;
        }

        public ProductionOrderBoms GetProductionOrderBomsOfProductionOrder(
            ProductionOrder productionOrder)
        {
            throw new System.NotImplementedException();
        }

        public List<Resource> GetResourcesByResourceCapabilityId(Id resourceCapabilityId)
        {
            var setupIds = _dbMasterDataCache.M_ResourceSetupGetAll()
                .Where(x => x.ResourceCapabilityProviderId.Equals(resourceCapabilityId.GetValue()))
                .Select(i => i.ResourceId);
            var resources = _dbMasterDataCache.ResourceGetAll()
                .Where(x => setupIds.Contains(x.GetValue().Id)).ToList();
            return resources;
        }

        public List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(
            ProductionOrder productionOrder)
        {
            return GetProductionOrderOperationsOfProductionOrder(productionOrder.GetId());
        }

        public List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(
            Id productionOrderId)
        {
            List<ProductionOrderOperation> productionOrderOperations =
                new List<ProductionOrderOperation>();
            Ids ids = _dbTransactionData.ProductionOrderOperationGetAll()
                .GetProductionOrderOperationsBy(productionOrderId);
            foreach (var id in ids)
            {
                productionOrderOperations.Add(
                    _dbTransactionData.ProductionOrderOperationGetById(id));
            }

            if (productionOrderOperations.Any() == false)
            {
                return null;
            }

            return productionOrderOperations;
        }

        public ProductionOrderBom GetAnyProductionOrderBomByProductionOrderOperation(
            ProductionOrderOperation productionOrderOperation)
        {
            T_ProductionOrderBom productionOrderBom = _dbTransactionData.ProductionOrderBomGetAll()
                .GetAllAs<T_ProductionOrderBom>().Find(x =>
                    x.ProductionOrderOperationId.Equals(productionOrderOperation.GetId()
                        .GetValue()));
            if (productionOrderBom == null)
            {
                throw new MrpRunException(
                    "How could an productionOrderOperation without an T_ProductionOrderBom exists?");
            }

            return new ProductionOrderBom(productionOrderBom);
        }

        public ProductionOrderBoms GetAllProductionOrderBomsBy(
            ProductionOrderOperation productionOrderOperation)
        {
            List<T_ProductionOrderBom> productionOrderBoms = _dbTransactionData
                .ProductionOrderBomGetAll().GetAllAs<T_ProductionOrderBom>().FindAll(x =>
                    x.ProductionOrderOperationId.Equals(productionOrderOperation.GetId()
                        .GetValue()));
            if (productionOrderBoms == null || productionOrderBoms.Any() == false)
            {
                throw new MrpRunException(
                    $"How could an productionOrderOperation({productionOrderOperation}) without an T_ProductionOrderBom exists?");
            }

            return new ProductionOrderBoms(productionOrderBoms);
        }

        /**
         * DemandToProvider
         */
        public Providers GetAllChildProvidersOf(Demand demand)
        {
            Providers providers = new Providers();
            Ids ids = _dbTransactionData.DemandToProviderGetAll().GetByDemandId(demand.GetId());
            if (ids == null)
            {
                return null;
            }

            foreach (var id in ids)
            {
                T_DemandToProvider demandToProvider =
                    _dbTransactionData.DemandToProviderGetById(id);
                providers.Add(
                    _dbTransactionData.ProvidersGetById(demandToProvider.GetProviderId()));
            }

            return providers;
        }

        /**
         * ProviderToDemand
         */
        public Providers GetAllParentProvidersOf(Demand demand)
        {
            Providers providers = new Providers();
            Ids ids = _dbTransactionData.ProviderToDemandGetAll().GetByDemandId(demand.GetId());
            foreach (var id in ids)
            {
                T_ProviderToDemand providerToDemand =
                    _dbTransactionData.ProviderToDemandGetById(id);
                providers.Add(
                    _dbTransactionData.ProvidersGetById(providerToDemand.GetProviderId()));
            }

            return providers;
        }

        public List<Provider> GetProvidersForInterval(DueTime from, DueTime to)
        {
            var providers = _dbTransactionData.StockExchangeProvidersGetAll();
            var currentProviders = providers.GetAll().FindAll(x =>
                x.GetStartTimeBackward().GetValue() >= from.GetValue() &&
                x.GetStartTimeBackward().GetValue() <= to.GetValue());
            return currentProviders;
        }

        /**
         * DemandToProvider
         * D --> P
         */
        public Demands GetAllParentDemandsOf(Provider provider)
        {
            Demands demands = new Demands();
            Ids ids = _dbTransactionData.DemandToProviderGetAll().GetByProviderId(provider.GetId());
            if (ids == null)
            {
                return null;
            }
            foreach (var id in ids)
            {
                T_DemandToProvider demandToProvider =
                    _dbTransactionData.DemandToProviderGetById(id);
                demands.Add(_dbTransactionData.DemandsGetById(demandToProvider.GetDemandId()));
            }

            return demands;
        }

        /**
         * ProviderToDemand
         */
        public Demands GetAllChildDemandsOf(Provider provider)
        {
            Demands demands = new Demands();
            Ids ids = _dbTransactionData.ProviderToDemandGetAll().GetByProviderId(provider.GetId());
            foreach (var id in ids)
            {
                T_ProviderToDemand providerToDemand =
                    _dbTransactionData.ProviderToDemandGetById(id);
                demands.Add(_dbTransactionData.DemandsGetById(providerToDemand.GetDemandId()));
            }

            return demands;
        }

        public DueTime GetEarliestPossibleStartTimeOf(ProductionOrderBom productionOrderBom)
        {
            DueTime earliestStartTime = productionOrderBom.GetStartTimeBackward();
            Providers providers = ZppConfiguration.CacheManager.GetAggregator()
                .GetAllChildProvidersOf(productionOrderBom);
            if (providers.Count() > 1)
            {
                throw new MrpRunException("A productionOrderBom can only have one provider !");
            }


            Provider stockExchangeProvider = providers.GetAny();
            if (earliestStartTime.IsGreaterThanOrEqualTo(
                stockExchangeProvider.GetStartTimeBackward()))
            {
                earliestStartTime = stockExchangeProvider.GetStartTimeBackward();
            }
            else
            {
                throw new MrpRunException("A provider of a demand cannot have a later dueTime.");
            }

            Demands stockExchangeDemands = ZppConfiguration.CacheManager.GetAggregator()
                .GetAllChildDemandsOf(stockExchangeProvider);
            if (stockExchangeDemands.Any() == false)
                // StockExchangeProvider has no childs (stockExchangeDemands),
                // take that from stockExchangeProvider
            {
                DueTime childDueTime = stockExchangeProvider.GetStartTimeBackward();
                if (childDueTime.IsGreaterThan(earliestStartTime))
                {
                    earliestStartTime = childDueTime;
                }
            }
            else
                // StockExchangeProvider has childs (stockExchangeDemands)
            {
                foreach (var stockExchangeDemand in stockExchangeDemands)
                {
                    DueTime stockExchangeDemandDueTime = stockExchangeDemand.GetStartTimeBackward();
                    if (stockExchangeDemandDueTime.IsGreaterThan(earliestStartTime))
                    {
                        earliestStartTime = stockExchangeDemandDueTime;
                    }
                }
            }

            return earliestStartTime;
        }

        public Demands GetUnsatisfiedCustomerOrderParts()
        {
            Demands unsatisfiedCustomerOrderParts = new Demands();

            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IAggregator aggregator = ZppConfiguration.CacheManager.GetAggregator();

            Demands customerOrderParts = dbTransactionData.CustomerOrderPartGetAll();

            foreach (var customerOrderPart in customerOrderParts)
            {
                Providers customerOrderPartChilds =
                    aggregator.GetAllChildProvidersOf(customerOrderPart);
                if (customerOrderPartChilds == null || customerOrderPartChilds.Any() == false)
                {
                    unsatisfiedCustomerOrderParts.Add(customerOrderPart);
                }
            }

            return unsatisfiedCustomerOrderParts;
        }

        public DemandOrProviders GetDemandsOrProvidersWhereStartTimeIsWithinInterval(
            SimulationInterval simulationInterval, DemandOrProviders demandOrProviders)
        {
            // startTime within interval
            return new DemandOrProviders(demandOrProviders.GetAll().Where(x =>
                simulationInterval.IsWithinInterval(x.GetStartTimeBackward())));
        }

        public DemandOrProviders GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
            SimulationInterval simulationInterval, DemandOrProviders demandOrProviders)
        {
            // endTime within interval
            return new DemandOrProviders(demandOrProviders.GetAll().Where(x =>
            {
                DueTime endTime = x.GetEndTimeBackward();
                return simulationInterval.IsWithinInterval(endTime) ||
                       simulationInterval.IsBeforeInterval(endTime);
            }));
        }

        /**
         * DemandToProvider
         */
        public List<ILinkDemandAndProvider> GetArrowsTo(Provider provider)
        {
            if (_dbTransactionData.DemandToProviderGetAll().Contains(provider) == false)
            {
                return null;
            }

            List<ILinkDemandAndProvider> demandToProviders = new List<ILinkDemandAndProvider>();
            Ids ids = _dbTransactionData.DemandToProviderGetAll().GetByProviderId(provider.GetId());
            foreach (var id in ids)
            {
                demandToProviders.Add(_dbTransactionData.DemandToProviderGetById(id));
            }

            return demandToProviders;
        }

        /**
         * ProviderToDemand
         */
        public List<ILinkDemandAndProvider> GetArrowsFrom(Provider provider)
        {
            if (_dbTransactionData.ProviderToDemandGetAll().Contains(provider) == false)
            {
                return null;
            }

            List<ILinkDemandAndProvider> providerToDemands = new List<ILinkDemandAndProvider>();
            Ids ids = _dbTransactionData.ProviderToDemandGetAll().GetByProviderId(provider.GetId());
            foreach (var id in ids)
            {
                providerToDemands.Add(_dbTransactionData.ProviderToDemandGetById(id));
            }

            return providerToDemands;
        }

        /**
         * ProviderToDemand
         */
        public List<ILinkDemandAndProvider> GetArrowsTo(Demand demand)
        {
            if (_dbTransactionData.ProviderToDemandGetAll().Contains(demand) == false)
            {
                return null;
            }

            List<ILinkDemandAndProvider> providerToDemands = new List<ILinkDemandAndProvider>();
            Ids ids = _dbTransactionData.ProviderToDemandGetAll().GetByDemandId(demand.GetId());
            foreach (var id in ids)
            {
                providerToDemands.Add(_dbTransactionData.ProviderToDemandGetById(id));
            }

            return providerToDemands;
        }

        /**
         * DemandToProvider
         */
        public List<ILinkDemandAndProvider> GetArrowsFrom(Demand demand)
        {
            if (_dbTransactionData.DemandToProviderGetAll().Contains(demand) == false)
            {
                return null;
            }

            List<ILinkDemandAndProvider> demandToProviders = new List<ILinkDemandAndProvider>();
            Ids ids = _dbTransactionData.DemandToProviderGetAll().GetByDemandId(demand.GetId());
            foreach (var id in ids)
            {
                demandToProviders.Add(_dbTransactionData.DemandToProviderGetById(id));
            }

            return demandToProviders;
        }

        public List<ILinkDemandAndProvider> GetArrowsTo(Providers providers)
        {
            List<ILinkDemandAndProvider> list = new List<ILinkDemandAndProvider>();
            foreach (var provider in providers)
            {
                list.AddRange(GetArrowsTo(provider));
            }

            return list;
        }

        public List<ILinkDemandAndProvider> GetArrowsFrom(Providers providers)
        {
            List<ILinkDemandAndProvider> list = new List<ILinkDemandAndProvider>();
            foreach (var provider in providers)
            {
                list.AddRange(GetArrowsFrom(provider));
            }

            return list;
        }

        public List<ILinkDemandAndProvider> GetArrowsTo(Demands demands)
        {
            List<ILinkDemandAndProvider> list = new List<ILinkDemandAndProvider>();
            foreach (var demand in demands)
            {
                list.AddRange(GetArrowsTo(demand));
            }

            return list;
        }

        public List<ILinkDemandAndProvider> GetArrowsFrom(Demands demands)
        {
            List<ILinkDemandAndProvider> list = new List<ILinkDemandAndProvider>();
            foreach (var demand in demands)
            {
                list.AddRange(GetArrowsFrom(demand));
            }

            return list;
        }


        /**
         * Arrow equals DemandToProvider and ProviderToDemand
         */
        public List<ILinkDemandAndProvider> GetArrowsToAndFrom(Provider provider)
        {
            List<ILinkDemandAndProvider>
                demandAndProviderLinks = new List<ILinkDemandAndProvider>();

            List<ILinkDemandAndProvider> demandToProviders = GetArrowsTo(provider);
            List<ILinkDemandAndProvider> providerToDemands = GetArrowsFrom(provider);

            if (demandToProviders != null)
            {
                demandAndProviderLinks.AddRange(demandToProviders);
            }

            if (providerToDemands != null)
            {
                demandAndProviderLinks.AddRange(providerToDemands);
            }

            return demandAndProviderLinks;
        }

        /**
         * Arrow equals DemandToProvider and ProviderToDemand
         */
        public List<ILinkDemandAndProvider> GetArrowsToAndFrom(Demand demand)
        {
            List<ILinkDemandAndProvider>
                demandAndProviderLinks = new List<ILinkDemandAndProvider>();

            List<ILinkDemandAndProvider> demandToProviders = GetArrowsTo(demand);
            List<ILinkDemandAndProvider> providerToDemands = GetArrowsFrom(demand);
            if (demandToProviders != null)
            {
                demandAndProviderLinks.AddRange(demandToProviders);
            }

            if (providerToDemands != null)
            {
                demandAndProviderLinks.AddRange(providerToDemands);
            }

            return demandAndProviderLinks;
        }

        public List<ILinkDemandAndProvider> GetArrowsToAndFrom(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider is Demand)
            {
                return GetArrowsToAndFrom((Demand) demandOrProvider);
            }
            else if (demandOrProvider is Provider)
            {
                return GetArrowsToAndFrom((Provider) demandOrProvider);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }
        
        public List<ILinkDemandAndProvider> GetArrowsTo(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider is Demand)
            {
                return GetArrowsTo((Demand) demandOrProvider);
            }
            else if (demandOrProvider is Provider)
            {
                return GetArrowsTo((Provider) demandOrProvider);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }

        public List<ILinkDemandAndProvider> GetArrowsFrom(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider is Demand)
            {
                return GetArrowsFrom((Demand) demandOrProvider);
            }
            else if (demandOrProvider is Provider)
            {
                return GetArrowsFrom((Provider) demandOrProvider);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }

        public List<ProductionOrderOperation> GetAllOperationsOnResource(M_Resource resource)
        {
            return _dbTransactionData.ProductionOrderOperationGetAll().GetAll()
                .Where(x => x.GetMachineId().GetValue().Equals(resource.Id)).ToList();
        }

        public DueTime GetEarliestPossibleStartTimeOf(
            ProductionOrderOperation productionOrderOperation)
        {
            DueTime maximumOfEarliestStartTimes = null;
            Providers providers = ZppConfiguration.CacheManager.GetAggregator()
                .GetAllChildStockExchangeProvidersOf(productionOrderOperation);

            foreach (var stockExchangeProvider in providers)
            {
                DueTime earliestStartTime = productionOrderOperation.GetStartTimeBackward();
                if (earliestStartTime.IsGreaterThanOrEqualTo(stockExchangeProvider
                    .GetStartTimeBackward()))
                {
                    earliestStartTime = stockExchangeProvider.GetStartTimeBackward();
                }
                else
                {
                    throw new MrpRunException(
                        "A provider of a demand cannot have a later dueTime.");
                }

                Demands stockExchangeDemands = ZppConfiguration.CacheManager.GetAggregator()
                    .GetAllChildDemandsOf(stockExchangeProvider);
                if (stockExchangeDemands.Any() == false)
                    // StockExchangeProvider has no childs (stockExchangeDemands),
                    // take that from stockExchangeProvider
                {
                    DueTime childDueTime = stockExchangeProvider.GetStartTimeBackward();
                    if (childDueTime.IsGreaterThan(earliestStartTime))
                    {
                        earliestStartTime = childDueTime;
                    }
                }
                else
                    // StockExchangeProvider has childs (stockExchangeDemands)
                {
                    foreach (var stockExchangeDemand in stockExchangeDemands)
                    {
                        DueTime stockExchangeDemandDueTime =
                            stockExchangeDemand.GetStartTimeBackward();
                        if (stockExchangeDemandDueTime.IsGreaterThan(earliestStartTime))
                        {
                            earliestStartTime = stockExchangeDemandDueTime;
                        }
                    }
                }

                if (maximumOfEarliestStartTimes == null ||
                    earliestStartTime.IsGreaterThan(maximumOfEarliestStartTimes))
                {
                    maximumOfEarliestStartTimes = earliestStartTime;
                }
            }

            return maximumOfEarliestStartTimes;
        }

        public Providers GetAllChildStockExchangeProvidersOf(ProductionOrderOperation operation)
        {
            Ids productionOrderBomIds = _dbTransactionData.ProductionOrderBomGetAll()
                .GetProductionOrderBomsBy(operation);

            Providers providers = new Providers();
            foreach (var productionOrderBomId in productionOrderBomIds)
            {
                Demand productionOrderBom =
                    _dbTransactionData.ProductionOrderBomGetById(productionOrderBomId);
                foreach (var stockExchangeProvider in GetAllChildProvidersOf(productionOrderBom))
                {
                    if (stockExchangeProvider.GetType() == typeof(StockExchangeProvider))
                    {
                        providers.Add((StockExchangeProvider) stockExchangeProvider);
                    }
                    else
                    {
                        throw new MrpRunException(
                            "A child of an productionOrderBom can only a StockExchangeProvider.");
                    }
                }
            }

            if (providers.Any() == false)
            {
                return null;
            }

            return providers;
        }

        public Demands GetProductionOrderBomsBy(ProductionOrderOperation operation)
        {
            Ids productionOrderBomIds = _dbTransactionData.ProductionOrderBomGetAll()
                .GetProductionOrderBomsBy(operation);

            Demands productionOrderBoms = new Demands();
            foreach (var productionOrderBomId in productionOrderBomIds)
            {
                Demand productionOrderBom =
                    _dbTransactionData.ProductionOrderBomGetById(productionOrderBomId);
                productionOrderBoms.Add(productionOrderBom);
            }

            return productionOrderBoms;
        }
    }
}