using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrapperForEntities;

namespace Zpp.Mrp2.impl.Mrp1.impl
{
    /**
     * Central interface for the mainModules
     */
    public interface IProviderManager
    {
        EntityCollector Satisfy(Demand demand, Quantity demandedQuantity);

        EntityCollector CreateDependingDemands(Providers providers);
        
    }
}