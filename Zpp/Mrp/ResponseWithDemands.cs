using System.Collections.Generic;
using System.Net;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.WrappersForCollections;

namespace Zpp.Mrp
{
    /**
     * This is an adapted copy of ResponseWithProviders,
     * but a generic class for both would be more ugly, since it would have three generic types
     */
    public class ResponseWithDemands 
    {
        private readonly IDemands _demands = new Demands();
        private readonly HttpStatusCode _statusCode;

        private readonly List<T_ProviderToDemand> _providerToDemands =
            new List<T_ProviderToDemand>();

        private readonly Quantity _demandedQuantity;

        public ResponseWithDemands(Demand demand, T_ProviderToDemand providerToDemand,
            Quantity demandedQuantity)
        {
            if (demand != null)
            {
                _demands.Add(demand);
                // _statusCode = HttpStatusCode.OK;
            }
            else
            {
                // _statusCode = HttpStatusCode.NotFound;
            }

            if (providerToDemand != null)
            {
                _providerToDemands.Add(providerToDemand);    
            }
            _demandedQuantity = demandedQuantity;
        }

        public ResponseWithDemands(IDemands demands, List<T_ProviderToDemand> providerToDemands,
            Quantity demandedQuantity)
        {
            _demands = demands;
            _providerToDemands = providerToDemands;
            _demandedQuantity = demandedQuantity;
            /*if (demands.Any())
            {
                _statusCode = HttpStatusCode.OK;
            }
            else
            {
                _statusCode = HttpStatusCode.NotFound;
            }*/
        }

        public IDemands GetDemands()
        {
            return _demands;
        }

        public List<T_ProviderToDemand> GetProviderToDemands()
        {
            return _providerToDemands;
        }

        public Id GetDemandId()
        {
            return new Id(_providerToDemands[0].DemandId);
        }

        public bool IsSatisfied()
        {
            return _demandedQuantity.Minus(CalculateReservedQuantity()).Equals(Quantity.Null());
        }

        public Quantity GetRemainingQuantity()
        {
            return _demandedQuantity.Minus(CalculateReservedQuantity());
        }

        // Todo: performance, cache it
        public Quantity CalculateReservedQuantity()
        {
            Quantity reservedQuantity = Quantity.Null();
            foreach (var providerToDemand in _providerToDemands)
            {
                reservedQuantity.IncrementBy(providerToDemand.GetQuantity());
            }

            return reservedQuantity;
        }

        /*// TODO: use both also for providerResponse 
        public bool IsStatusOk()
        {
            return _statusCode.Equals(HttpStatusCode.OK);
        }
        
        public bool IsStatusNotFound()
        {
            return _statusCode.Equals(HttpStatusCode.NotFound);
        }*/
    }
}