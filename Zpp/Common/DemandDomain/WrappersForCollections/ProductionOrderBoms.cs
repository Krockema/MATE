using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.Common.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all productionOrderBoms
     */
    public class ProductionOrderBoms : Demands
    {
        
        public ProductionOrderBoms(List<T_ProductionOrderBom> iDemands, IDbMasterDataCache
            dbMasterDataCache) : base(ToDemands(iDemands, dbMasterDataCache))
        {
        }
        
        public ProductionOrderBoms(List<Demand> demands) : base(demands)
        {
        }

        private static List<Demand> ToDemands(List<T_ProductionOrderBom> iDemands,IDbMasterDataCache
            dbMasterDataCache )
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new ProductionOrderBom(iDemand, dbMasterDataCache));
            }

            return demands;
        }

        public List<T_ProductionOrderBom> GetAllAsT_ProductionOrderBom()
        {
            List<T_ProductionOrderBom> productionOrderBoms = new List<T_ProductionOrderBom>();
            foreach (var demand in List)
            {
                productionOrderBoms.Add((T_ProductionOrderBom)demand.ToIDemand());
            }
            return productionOrderBoms;
        }
    }
}