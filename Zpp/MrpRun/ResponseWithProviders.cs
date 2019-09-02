using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.WrappersForCollections;

namespace Zpp.MrpRun
{
    public class ResponseWithProviders
    {
        private readonly IProviders _providers = new Providers();

        private readonly List<T_DemandToProvider> _demandToProviders =
            new List<T_DemandToProvider>();

        private readonly Quantity _demandedQuantity;

        public ResponseWithProviders(Provider provider, T_DemandToProvider demandToProvider,
            Quantity demandedQuantity)
        {
            if (provider != null)
            {
                _providers.Add(provider);    
            }

            if (demandToProvider != null)
            {
                _demandToProviders.Add(demandToProvider);    
            }
            _demandedQuantity = demandedQuantity;
        }

        public ResponseWithProviders(IProviders providers, List<T_DemandToProvider> demandToProviders,
            Quantity demandedQuantity)
        {
            _providers = providers;
            _demandToProviders = demandToProviders;
            _demandedQuantity = demandedQuantity;
        }

        public IProviders GetProviders()
        {
            return _providers;
        }

        public List<T_DemandToProvider> GetDemandToProviders()
        {
            return _demandToProviders;
        }

        public Id GetDemandId()
        {
            return new Id(_demandToProviders[0].DemandId);
        }

        public bool IsSatisfied()
        {
            return _demandedQuantity.Minus(CalculateReservedQuantity()).Equals(Quantity.Null());
        }

        public Quantity GetRemainingQuantity()
        {
            return _demandedQuantity.Minus(CalculateReservedQuantity());
        }

        private Quantity CalculateReservedQuantity()
        {
            Quantity reservedQuantity = Quantity.Null();
            foreach (var demandToProvider in _demandToProviders)
            {
                reservedQuantity.IncrementBy(demandToProvider.GetQuantity());
            }

            return reservedQuantity;
        }
    }
}