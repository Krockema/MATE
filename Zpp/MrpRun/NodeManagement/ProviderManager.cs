using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.Utils;
using Zpp.WrappersForCollections;

namespace Zpp.MrpRun.NodeManagement
{
    
    public class ProviderManager : IProviderManager
    {
        private readonly IDemandToProviderTable _demandToProviderTable;
        private readonly IProviderToDemandTable _providerToDemandTable;
        private readonly IProviders _providers;
        private readonly List<Demand> _nextDemands = new List<Demand>();
        private readonly IDbTransactionData _dbTransactionData;
        
        private readonly OpenNodes<Provider> _openProviders = new OpenNodes<Provider>();

        public ProviderManager(IDbTransactionData dbTransactionData)
        {
            _providers = new Providers();
            _demandToProviderTable = new DemandToProviderTable();
            _providerToDemandTable = new ProviderToDemandTable();
            _dbTransactionData = dbTransactionData;
        }

        public ProviderManager(IDemandToProviderTable demandToProviderTable, IProviderToDemandTable providerToDemandTable, IProviders providers)
        {
            _demandToProviderTable = demandToProviderTable;
            _providerToDemandTable = providerToDemandTable;
            _providers = providers;
        }

        /**
         * aka ReserveQuantityOfExistingProvider or satisfyByAlreadyExistingProvider
         */
        public ResponseWithProviders SatisfyDemand(Demand demand, Quantity demandedQuantity, IDbTransactionData dbTransactionData)
        {
            if (_openProviders.AnyOpenProvider(demand.GetArticle()))
            {
                OpenNode<Provider> openNode = _openProviders.GetOpenProvider(demand.GetArticle());
                Quantity remainingQuantity = demandedQuantity.Minus(openNode.GetOpenQuantity());
                openNode.GetOpenQuantity().DecrementBy(demandedQuantity);
                
                if (openNode.GetOpenQuantity().IsNegative() || openNode.GetOpenQuantity().IsNull())
                {
                    _openProviders.Remove(openNode);
                }
                if (remainingQuantity.IsNegative())
                {
                    remainingQuantity = Quantity.Null();
                }

                T_DemandToProvider demandToProvider = new T_DemandToProvider()
                {
                    DemandId = demand.GetId().GetValue(),
                    ProviderId = openNode.GetOpenNode().GetId().GetValue(),
                    Quantity = demandedQuantity.Minus(remainingQuantity).GetValue()
                };

                return new ResponseWithProviders(null, demandToProvider, demandedQuantity);
            }
            else
            {
                return new ResponseWithProviders((Provider)null, null, demandedQuantity);
            }
        }

        public void AddDemandToProvider(T_DemandToProvider demandToProvider)
        {
            _demandToProviderTable.Add(demandToProvider);
        }

        public void AddProvider(Id demandId, Provider oneProvider, Quantity reservedQuantity)
        {
            if (_providers.GetProviderById(oneProvider.GetId()) != null)
            {
                throw new MrpRunException("You cannot add an already added provider.");
            }
            
            
            // if it has quantity that is not reserved, remember it for later reserving
            if (oneProvider.GetType() == typeof(StockExchangeProvider) && reservedQuantity.IsSmallerThan(oneProvider.GetQuantity()))
            {
                _openProviders.Add(oneProvider.GetArticle(), new OpenNode<Provider>(oneProvider, oneProvider.GetQuantity().Minus(reservedQuantity), oneProvider.GetArticle()));
            }

            // save provider
            _providers.Add(oneProvider);

            // TODO: this should be done in separate method and be controlled by MrpRun
            // save depending demands
            Demands dependingDemands = oneProvider.GetAllDependingDemands();
            if (dependingDemands != null)
            {
                _nextDemands.AddRange(dependingDemands);
                if (oneProvider.GetType() == typeof(StockExchangeProvider))
                {
                    _providerToDemandTable.AddAll(oneProvider.GetProviderToDemandTable());    
                }
                else
                {
                    foreach (var dependingDemand in dependingDemands)
                    {
                        _providerToDemandTable.Add(oneProvider, dependingDemand.GetId(), dependingDemand.GetQuantity());        
                    }
                }
                
            }
        }

        public Quantity GetSatisfiedQuantityOfDemand(Id demandId)
        {
            Quantity sum = Quantity.Null();
            foreach (var demandToProvider in _demandToProviderTable.GetAll())
            {
                if (demandToProvider.DemandId.Equals(demandId.GetValue()))
                {
                    sum.IncrementBy(new Quantity(demandToProvider.Quantity));
                }
            }

            return sum;
        }

        public Quantity GetReservedQuantityOfProvider(Id providerId)
        {
            Quantity sum = Quantity.Null();
            foreach (var demandToProvider in _demandToProviderTable.GetAll())
            {
                if (demandToProvider.ProviderId.Equals(providerId.GetValue()))
                {
                    sum.IncrementBy(new Quantity(demandToProvider.Quantity));
                }
            }

            return sum;
        }

        public Demands GetNextDemands()
        {
            if (_nextDemands.Any() == false)
            {
                return null;
            }
            Demands nextDemands = new Demands(_nextDemands);
            _nextDemands.Clear();
            return nextDemands;
        }

        public IDemandToProviderTable GetDemandToProviderTable()
        {
            return _demandToProviderTable;
        }

        public IProviderToDemandTable GetProviderToDemandTable()
        {
            return _providerToDemandTable;
        }

        public bool IsSatisfied(Demand demand)
        {
            return GetSatisfiedQuantityOfDemand(demand.GetId()).IsGreaterThanOrEqualTo(demand.GetQuantity());
        }

        public IProviders GetProviders()
        {
            return _providers;
        }
    }
}