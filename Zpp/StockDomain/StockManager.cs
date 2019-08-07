using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp.StockDomain
{
    public class StockManager
    {
        public static void CalculateCurrent(M_Stock stock, IDbTransactionData dbTransactionData, Quantity startQuantity)
        {
            Quantity currentQuantity = new Quantity(startQuantity);
            // TODO
        }
    }
}