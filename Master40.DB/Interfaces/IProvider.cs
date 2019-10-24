using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IProvider 
    {
        int Id { get; set; }
        
        M_Article GetArticle();

        Quantity GetQuantity();
    }
}