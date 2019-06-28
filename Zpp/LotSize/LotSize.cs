using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.LotSize
{
    public class LotSize : ILotSize
    {
        private readonly Quantity _lotSize = new Quantity(1); // TODO for Marvin: This is not dependant from given article
        private readonly Quantity _neededQuantity;
        private readonly Id _articleId;

        public LotSize(Quantity neededQuantity, Id articleId)
        {
            _neededQuantity = neededQuantity;
            _articleId = articleId;
        }

        public Quantity GetCalculatedQuantity()
        {
            // you work on a copy here
            Quantity calculatedQuantity = new Quantity(_lotSize);
            while (calculatedQuantity.IsSmallerThan(_neededQuantity))
            {
                calculatedQuantity.IncrementBy(_lotSize);
            }
            
            return calculatedQuantity;
        }
    }
}