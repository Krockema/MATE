using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp
{
    public static class PurchaseManagerUtils
    {
        public static int calculateQuantity(M_ArticleToBusinessPartner articleToBusinessPartner,
            Quantity demandQuantity)
        {
            // force round up the decimal demandQuantity
            int demandQuantityInt = (int) decimal.Truncate(demandQuantity.GetValue()) + 1;
            int purchaseQuantity = 0;
            
            for (int quantity = 0;
                quantity < demandQuantityInt;
                quantity += articleToBusinessPartner.PackSize)
            {
                purchaseQuantity++;
            }

            return purchaseQuantity;
        }
    }
}