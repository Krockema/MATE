using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.LotSize
{
    public class LotSize : ILotSize
    {
        private static Quantity _defaultLotSize = new Quantity(1);
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
            Quantity calculatedQuantity = new Quantity(_defaultLotSize);
            lotSizes.Add(calculatedQuantity);
            while (calculatedQuantity.IsSmallerThan(_neededQuantity))
            {
                lotSizes.Add(_defaultLotSize);
                calculatedQuantity.IncrementBy(_defaultLotSize);
            }
            
            return lotSizes;
        }

        /**
         * should be used by tests only
         */
        public static void SetDefaultLotSize(Quantity defaultLotSize)
        {
            _defaultLotSize = defaultLotSize;
        }
    }
}