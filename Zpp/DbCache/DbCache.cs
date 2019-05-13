using Master40.DB.Data.Context;

namespace Zpp
{
    public class DbCache : IDbCache
    {
        protected readonly ProductionDomainContext _productionDomainContext;
        
        public DbCache(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
        }

        public void DemandToProvidersRemoveAll()
        {
            _productionDomainContext.DemandToProviders.RemoveRange(_productionDomainContext
                .DemandToProviders);
        }
    }
}