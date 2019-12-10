using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IProvider: IBaseEntityCodeGeneratedId
    {
       
        M_Article GetArticle();

        Quantity GetQuantity();
    }
}