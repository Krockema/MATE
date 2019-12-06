using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;

namespace Zpp.Mrp2.impl.Mrp1.impl.Production.impl.ProductionTypes
{
    public interface IProductionOrderBomCreator
    {
        Demands CreateProductionOrderBomsForArticleBom(
            M_ArticleBom articleBom, Quantity quantity,
            ProductionOrder parentProductionOrder);
    }
}