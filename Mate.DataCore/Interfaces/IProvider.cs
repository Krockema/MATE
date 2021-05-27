using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Interfaces
{
    public interface IProvider: IBaseEntityCodeGeneratedId
    {
       
        M_Article GetArticle();

        Quantity GetQuantity();
    }
}