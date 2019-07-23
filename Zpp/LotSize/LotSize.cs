using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.LotSize
{
    public class LotSize : ILotSize
    {
        private readonly Quantity _lotSize = new Quantity(1);
        private readonly Quantity _neededQuantity;
        private readonly Id _articleId;

        public LotSize(Quantity neededQuantity, Id articleId)
        {
            _neededQuantity = neededQuantity;
            _articleId = articleId;
        }

        public List<Quantity> GetCalculatedQuantity()
        {
            List<Quantity> lotSizes = new List<Quantity>();
            
            // you work on a copy here
            Quantity calculatedQuantity = new Quantity(_lotSize);
            lotSizes.Add(calculatedQuantity);
            while (calculatedQuantity.IsSmallerThan(_neededQuantity))
            {
                lotSizes.Add(_lotSize);
                calculatedQuantity.IncrementBy(_lotSize);
            }
            
            return lotSizes;
        }
    }
}