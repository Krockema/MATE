using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.LotSize
{
    public class LotSize : ILotSize
    {
        private static Quantity _defaultFixedLotSize = new Quantity(1);
        private readonly Quantity _neededQuantity;
        private readonly Id _articleId;
        private static LotSizeType _lotSizeType = LotSizeType.FixedLotSize;

        public LotSize(Quantity neededQuantity, Id articleId)
        {
            _neededQuantity = neededQuantity;
            _articleId = articleId;
        }

        public List<Quantity> GetLotSizes()
        {
            if (_lotSizeType.Equals(LotSizeType.LotForLotOrderQuantity))
            {
                return GetLotForLotOrderQuantity();
            }
            else
            {
                return GetFixedLotSize();
            }
        }

        private List<Quantity> GetLotForLotOrderQuantity()
        {
            return new List<Quantity>(){_neededQuantity};
        }

        private List<Quantity> GetFixedLotSize()
        {
            List<Quantity> lotSizes = new List<Quantity>();
            
            // you work on a copy here
            Quantity currentQuantity = new Quantity(_neededQuantity);
            while (currentQuantity.IsGreaterThan(Quantity.Null()))
            {
                lotSizes.Add(_defaultFixedLotSize);
                currentQuantity.DecrementBy(_defaultFixedLotSize);
            }
            
            return lotSizes;
        }
        
        

        /**
         * should be used by tests only
         */
        public static void SetDefaultLotSize(Quantity defaultLotSize)
        {
            _defaultFixedLotSize = defaultLotSize;
        }
        
        /**
         * should be used by tests only
         */
        public static void SetLotSizeType(LotSizeType lotSizeType)
        {
            _lotSizeType = lotSizeType;
        }
    }
}