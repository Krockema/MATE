using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    public class ProviderManager : IProviderManager
    {
        private readonly IDemandToProviderTable _demandToProviderTable;
        private readonly IProviderToDemandTable _providerToDemandTable;
        private readonly IProviders _providers;
        private readonly List<Demand> _nextDemands = new List<Demand>();

        public ProviderManager()
        {
            _providers = new Providers();
            _demandToProviderTable = new DemandToProviderTable();
            _providerToDemandTable = new ProviderToDemandTable();
        }

        public ProviderManager(IDemandToProviderTable demandToProviderTable, IProviderToDemandTable providerToDemandTable, IProviders providers)
        {
            _demandToProviderTable = demandToProviderTable;
            _providerToDemandTable = providerToDemandTable;
            _providers = providers;
        }

        public Quantity ReserveQuantityOfExistingProvider(Id demandId, M_Article demandedArticle, Quantity demandedQuantity)
        {
            List<Provider> providersForDemand = _providers.GetAllByArticleId(demandedArticle.GetId());
            if (providersForDemand == null)
            {
                return demandedQuantity;
            }
            // TODO: performance: naive impl must replaced by one with caching
            foreach (var provider in providersForDemand)
            {
                List<T_DemandToProvider> possibleDemandToProviders = _demandToProviderTable.GetAll()
                    .Where(x => x.ProviderId.Equals(provider.GetId().GetValue())).ToList();
                Quantity alreadyReservedQuantity = Quantity.Null();
                foreach (var possibleDemandToProvider in possibleDemandToProviders)
                {
                    alreadyReservedQuantity.IncrementBy(new Quantity(possibleDemandToProvider.Quantity));
                }
                Quantity freeQuantity = provider.GetQuantity().Minus(alreadyReservedQuantity);
                if (freeQuantity.IsGreaterThan(Quantity.Null()))
                {
                    T_DemandToProvider newDemandToProvider = new T_DemandToProvider();
                    newDemandToProvider.DemandId = demandId.GetValue();
                    newDemandToProvider.ProviderId = provider.GetId().GetValue();
                    _demandToProviderTable.Add(newDemandToProvider);
                    if (freeQuantity.IsGreaterThanOrEqualTo(demandedQuantity))
                    {
                        newDemandToProvider.Quantity = demandedQuantity.GetValue();
                        return Quantity.Null();
                    }
                    Quantity reservedQuantity = demandedQuantity.Minus(freeQuantity);
                    newDemandToProvider.Quantity = reservedQuantity.GetValue();
                    
                    return reservedQuantity;
                }
            }

            return demandedQuantity;
        }

        public Quantity AddProvider(Id demandId, Quantity demandedQuantity, Provider oneProvider)
        {
            _providers.Add(oneProvider);
            T_DemandToProvider demandToProvider = new T_DemandToProvider();
            demandToProvider.DemandId = demandId.GetValue();
            demandToProvider.ProviderId = oneProvider.GetId().GetValue();
            demandToProvider.Quantity = oneProvider.GetQuantity().GetValue();
            _demandToProviderTable.Add(demandToProvider);
            
            // save depending demands
            Demands dependingDemands = oneProvider.GetAllDependingDemands();
            if (dependingDemands != null)
            {
                _nextDemands.AddRange(dependingDemands.GetAll());
                foreach (var dependingDemand in dependingDemands.GetAll())
                {
                    _providerToDemandTable.Add(oneProvider, dependingDemand.GetId());
                }
            }
            
            Quantity unsatisfiedQuantity =
                demandedQuantity.Minus(GetSatisfiedQuantityOfDemand(demandId));
            if (unsatisfiedQuantity.IsNegative())
            {
                return Quantity.Null();
            }
            return unsatisfiedQuantity;
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

        public Quantity AddProvider(Demand demand, Provider provider)
        {
            return AddProvider(demand.GetId(), demand.GetQuantity(), provider);
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