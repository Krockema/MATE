using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp
{
    public interface IDbCacheMasterData
    {
        M_Article M_ArticleGetById(Id id);
        
        M_ArticleBom M_ArticleBomGetById(Id id);
        
        List<M_BusinessPartner> M_BusinessPartnerGetAll();
        
        M_ArticleToBusinessPartner M_ArticleToBusinessPartnerGetById(Id id);
        
            M_ArticleType M_ArticleTypeGetById(Id id);
            
        M_Machine M_MachineGetById(Id id);
        
        M_MachineGroup M_MachineGroupGetById(Id id);
        
        M_MachineTool M_MachineToolGetById(Id id);
        
        M_Operation M_OperationGetById(Id id);
        
        M_Stock M_StockGetById(Id id);
        
        M_Unit M_UnitGetById(Id id);

    }
}