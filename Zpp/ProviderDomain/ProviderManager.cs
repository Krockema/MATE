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
using Zpp.Utils;

namespace Zpp.ProviderDomain
{
    public class ProviderManager : IProviderManager, IProvidingManager
    {
        private readonly IDemandToProviderTable _demandToProviderTable;
        private readonly IProviderToDemandTable _providerToDemandTable;
        private readonly IProviders _providers;
        private readonly List<Demand> _nextDemands = new List<Demand>();
        private readonly IDbTransactionData _dbTransactionData;
        
        private readonly OpenProviders _openProviders = new OpenProviders();

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
        public Response Satisfy(Demand demand, Quantity demandedQuantity, IDbTransactionData dbTransactionData)
        {
            if (_openProviders.AnyOpenProvider(demand.GetArticle()))
            {
                OpenProvider openProvider = _openProviders.GetOpenProvider(demand.GetArticle());
                Quantity remainingQuantity = demandedQuantity.Minus(openProvider.GetOpenQuantity());
                openProvider.GetOpenQuantity().DecrementBy(demandedQuantity);
                
                if (openProvider.GetOpenQuantity().IsNegative() || openProvider.GetOpenQuantity().IsNull())
                {
                    _openProviders.Remove(openProvider);
                }
                if (remainingQuantity.IsNegative())
                {
                    remainingQuantity = Quantity.Null();
                }

                T_DemandToProvider demandToProvider = new T_DemandToProvider()
                {
                    DemandId = demand.GetId().GetValue(),
                    ProviderId = openProvider.GetOpenProvider().GetId().GetValue(),
                    Quantity = demandedQuantity.Minus(remainingQuantity).GetValue()
                };

                return new Response(null, demandToProvider, demandedQuantity);
            }
            else
            {
                return new Response((Provider)null, null, demandedQuantity);
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
                _openProviders.Add(oneProvider.GetArticle(), new OpenProvider(oneProvider, oneProvider.GetQuantity().Minus(reservedQuantity), oneProvider.GetArticle()));
            }

            // save provider
            _providers.Add(oneProvider);

            // TODO: this should be done in separate method and be controlled by MrpRun
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