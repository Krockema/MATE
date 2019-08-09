using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Zpp.DemandDomain;
using Zpp.MachineDomain;
using Zpp.StockDomain;

namespace Zpp.ProviderDomain
{
    public class ProviderManager : IProviderManager
    {
        private readonly IDemandToProviderTable _demandToProviderTable;
        private readonly IProviderToDemandTable _providerToDemandTable;
        private readonly IProviders _providers;
        private readonly List<Demand> _nextDemands = new List<Demand>();
        private readonly StockManager _stockManager;
        private readonly IDbTransactionData _dbTransactionData;
        
        private readonly OpenProviders _openProviders = new OpenProviders();

        public ProviderManager(StockManager stockManager, IDbTransactionData dbTransactionData)
        {
            _providers = new Providers();
            _demandToProviderTable = new DemandToProviderTable();
            _providerToDemandTable = new ProviderToDemandTable();
            _stockManager = stockManager;
            _dbTransactionData = dbTransactionData;
        }

        public ProviderManager(IDemandToProviderTable demandToProviderTable, IProviderToDemandTable providerToDemandTable, IProviders providers)
        {
            _demandToProviderTable = demandToProviderTable;
            _providerToDemandTable = providerToDemandTable;
            _providers = providers;
        }

        public Quantity ReserveQuantityOfExistingProvider(Id demandId, M_Article demandedArticle, Quantity demandedQuantity)
        {
            if (_openProviders.AnyOpenProvider(demandedArticle))
            {
                OpenProvider openProvider = _openProviders.GetOpenProvider(demandedArticle);
                Quantity remainingQuantity = demandedQuantity.Minus(openProvider.GetOpenQuantity());
                openProvider.GetOpenQuantity().DecrementBy(demandedQuantity);
                
                if (openProvider.GetOpenQuantity().IsNegative())
                {
                    _openProviders.Remove(openProvider);
                }
                if (remainingQuantity.IsNegative())
                {
                    remainingQuantity = Quantity.Null();
                }
                ConnectDemandWithProvider(demandId, openProvider.GetOpenProvider(), demandedQuantity.Minus(remainingQuantity));
                return remainingQuantity;
            }
            else
            {
                return demandedQuantity;
            }
        }

        private void ConnectDemandWithProvider(Id demandId, Provider provider, Quantity usedQuantity)
        {
            T_DemandToProvider demandToProvider = new T_DemandToProvider();
            demandToProvider.DemandId = demandId.GetValue();
            demandToProvider.ProviderId = provider.GetId().GetValue();
            demandToProvider.Quantity = usedQuantity.GetValue();
            _demandToProviderTable.Add(demandToProvider);
        }

        public Quantity AddProvider(Id demandId, Quantity demandedQuantity, Provider oneProvider)
        {
            _stockManager.AdaptStock(oneProvider, _dbTransactionData);
            
            // if it has quantity that is not reserved, remember it for later reserving
            if (demandedQuantity.IsSmallerThan(oneProvider.GetQuantity()))
            {
                _openProviders.Add(oneProvider.GetArticle(), new OpenProvider(oneProvider, oneProvider.GetQuantity().Minus(demandedQuantity), oneProvider.GetArticle()));
            }
            
            // save provider
            _providers.Add(oneProvider);
            
            // connect demandToProvider
            ConnectDemandWithProvider(demandId, oneProvider, oneProvider.GetQuantity().Minus(demandedQuantity));
            
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