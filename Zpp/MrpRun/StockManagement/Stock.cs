using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Utils;

namespace Zpp.MrpRun.StockManagement
{
    public class Stock
    {
        private readonly Quantity _quantity;
        private readonly Id _articleId;
        private readonly Quantity _minStockLevel;

        public Stock(Quantity initialQuantity, Id articleId, Quantity minStockLevel)
        {
            _quantity = new Quantity(initialQuantity);
            _articleId = articleId;
            _minStockLevel = minStockLevel;
        }

        public void IncrementBy(Quantity quantity)
        {
            _quantity.IncrementBy(quantity);
        }
        
        public void DecrementBy(Quantity quantity)
        {
            if (quantity.IsNegative())
            {
                throw new MrpRunException("This is not what you intended to do.");
            }
            _quantity.DecrementBy(quantity);
        }
        
        public Id GetArticleId()
        {
            return _articleId;
        }

        public Quantity GetQuantity()
        {
            return _quantity;
        }

        public override string ToString()
        {
            return $"ArticleId: {_articleId}, Quantity: {_quantity}";
        }

        public Quantity GetMinStockLevel()
        {
            return _minStockLevel;
        }
    }
}