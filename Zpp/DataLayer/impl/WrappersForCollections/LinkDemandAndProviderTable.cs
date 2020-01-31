using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.Util;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    /**
     * wraps ILinkDemandAndProvider
     */
    public sealed class LinkDemandAndProviderTable : CollectionWrapperWithStackSet<ILinkDemandAndProvider>
    {
        private readonly Dictionary<Id, Ids> _indexDemandId = new Dictionary<Id, Ids>();
        private readonly Dictionary<Id, Ids> _indexProviderId = new Dictionary<Id, Ids>();


        public LinkDemandAndProviderTable(IEnumerable<ILinkDemandAndProvider> list)
        {
            AddAll(list);
        }

        public LinkDemandAndProviderTable()
        {
        }

        public bool Contains(Demand demand)
        {
            return _indexDemandId.ContainsKey(demand.GetId());
        }

        public bool Contains(Provider provider)
        {
            return _indexProviderId.ContainsKey(provider.GetId());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="demandId"></param>
        /// <returns>demandToProvider ids</returns>
        public Ids GetByDemandId(Id demandId)
        {
            if (_indexDemandId.ContainsKey(demandId) == false)
            {
                return null;
            }

            Ids ids = _indexDemandId[demandId];
            return ids;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns>demandToProvider ids</returns>
        public Ids GetByProviderId(Id providerId)
        {
            if (_indexProviderId.ContainsKey(providerId) == false)
            {
                return null;
            }

            Ids ids = _indexProviderId[providerId];
            return ids;
        }

        public override void AddAll(IEnumerable<ILinkDemandAndProvider> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public override void Add(ILinkDemandAndProvider item)
        {
            if (item == null)
            {
                return;
            }

            if (item.Quantity == null || item.Quantity <= 0)
            {
                throw new MrpRunException($"Quantity is not correct: {item.Quantity}");
            }

            // a set contains the element only once, else skip adding
            if (StackSet.Contains(item) == false)
            {
                if (_indexDemandId.ContainsKey(item.GetDemandId()) == false)
                {
                    _indexDemandId.Add(item.GetDemandId(), new Ids());
                }

                _indexDemandId[item.GetDemandId()].Add(item.GetId());

                if (_indexProviderId.ContainsKey(item.GetProviderId()) == false)
                {
                    _indexProviderId.Add(item.GetProviderId(), new Ids());
                }

                _indexProviderId[item.GetProviderId()].Add(item.GetId());


                base.Add(item);
            }
        }

        public override void Clear()
        {
            _indexDemandId.Clear();
            _indexProviderId.Clear();
            base.Clear();
        }

        public override void Remove(ILinkDemandAndProvider t)
        {
            if (base.Contains(t) == false)
            {
                return;
            }
            _indexProviderId[t.GetProviderId()].Remove(t.GetId());
            _indexDemandId[t.GetDemandId()].Remove(t.GetId());
            base.Remove(t);
        }
    }
}