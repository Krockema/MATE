using System.Collections.Generic;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    public sealed class DemandOrProviders: CollectionWrapperWithStackSet<IDemandOrProvider>
    {
        public DemandOrProviders()
        {
        }

        public DemandOrProviders(IEnumerable<IDemandOrProvider> list)
        {
            AddAll(list);
        }

        public DemandOrProviders(IDemandOrProvider item)
        {
            Add(item);
        }
    }
}