using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util;
using Zpp.Util.Graph.impl;

namespace Zpp.DataLayer.impl.WrapperForEntities
{
    public class EntityCollector
    {
        private readonly LinkDemandAndProviderTable _demandToProviderTable = new LinkDemandAndProviderTable();
        private readonly LinkDemandAndProviderTable _providerToDemandTable = new LinkDemandAndProviderTable();
        private readonly Demands _demands = new Demands();
        private readonly Providers _providers = new Providers();

        public void AddAll(EntityCollector otherEntityCollector)
        {
            if (otherEntityCollector == null)
            {
                return;
            }
            if (otherEntityCollector._demands.Any())
            {
                _demands.AddAll(otherEntityCollector._demands);
            }

            if (otherEntityCollector._providers.Any())
            {
                _providers.AddAll(otherEntityCollector._providers);
            }

            if (otherEntityCollector._demandToProviderTable.Any())
            {
                _demandToProviderTable.AddAll(otherEntityCollector._demandToProviderTable);
            }

            if (otherEntityCollector._providerToDemandTable.Any())
            {
                _providerToDemandTable.AddAll(otherEntityCollector._providerToDemandTable);
            }
        }

        public void AddAll(LinkDemandAndProviderTable linkDemandAndProviderTable)
        {
            if (linkDemandAndProviderTable.Any())
            {
                ILinkDemandAndProvider linkDemandAndProvider = linkDemandAndProviderTable.GetAny();
                if (linkDemandAndProvider.GetType() == typeof(T_DemandToProvider))
                {
                    _demandToProviderTable.AddAll(linkDemandAndProviderTable);        
                }
                else if (linkDemandAndProvider.GetType() == typeof(T_ProviderToDemand))
                {
                    _providerToDemandTable.AddAll(linkDemandAndProviderTable);     
                }
                else
                {
                    throw new MrpRunException("Unexpected type.");
                }
            }
            
        }

        public void AddAll(Demands demands)
        {
            _demands.AddAll(demands);
        }

        public void AddAll(Providers providers)
        {
            _providers.AddAll(providers);
        }

        public void Add(Demand demand)
        {
            _demands.Add(demand);
        }

        public void Add(Provider provider)
        {
            _providers.Add(provider);
        }

        public void Add(T_DemandToProvider demandToProvider)
        {
            _demandToProviderTable.Add(demandToProvider);
        }

        public void Add(T_ProviderToDemand providerToDemand)
        {
            _providerToDemandTable.Add(providerToDemand);
        }

        public LinkDemandAndProviderTable GetDemandToProviderTable()
        {
            return _demandToProviderTable;
        }

        public LinkDemandAndProviderTable GetLinkDemandAndProviderTable()
        {
            return _providerToDemandTable;
        }

        public Demands GetDemands()
        {
            return _demands;
        }

        public Providers GetProviders()
        {
            return _providers;
        }

        public bool IsSatisfied(IDemandOrProvider demandOrProvider)
        {
            return GetRemainingQuantity(demandOrProvider).Equals(Quantity.Zero());
        }

        public Quantity GetRemainingQuantity(IDemandOrProvider demandOrProvider)
        {
            Quantity reservedQuantity = demandOrProvider.GetQuantity()
                .Minus(SumReservedQuantity(demandOrProvider));
            if (reservedQuantity.IsNegative())
            {
                return Quantity.Zero();
            }
            else
            {
                return reservedQuantity;
            }
        }

        private Quantity SumReservedQuantity(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider.GetNodeType().Equals(NodeType.Demand))
            {
                return SumReservedQuantity(Demand.AsDemand(demandOrProvider));
            }
            else
            {
                return SumReservedQuantity(Provider.AsProvider(demandOrProvider));
            }
        }

        public Quantity SumReservedQuantity(Demand demand)
        {
            return SumReservedQuantity(
                _demandToProviderTable.Where(x => x.GetDemandId().Equals(demand.GetId())));
        }

        public Quantity SumReservedQuantity(Provider provider)
        {
            return SumReservedQuantity(
                _providerToDemandTable.Where(x => x.GetProviderId().Equals(provider.GetId())));
        }

        /// <summary>
        /// Sums the reserved quantity
        /// </summary>
        /// <param name="demandAndProviderLinks">ATTENTION: filter this for demand/provider id before,
        /// else the sum can could be higher than expected</param>
        /// <returns></returns>
        public static Quantity SumReservedQuantity(
            IEnumerable<ILinkDemandAndProvider> demandAndProviderLinks)
        {
            Quantity reservedQuantity = Quantity.Zero();
            foreach (var demandAndProviderLink in demandAndProviderLinks)
            {
                reservedQuantity.IncrementBy(demandAndProviderLink.GetQuantity());
            }

            return reservedQuantity;
        }
    }
}