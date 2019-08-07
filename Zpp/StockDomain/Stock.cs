using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.StockDomain
{
    public class Stock
    {
        private Quantity _quantity;
        private Id _articleId;

        public Stock(Quantity initialQuantity, Id articleId)
        {
            _quantity = new Quantity(initialQuantity);
            _articleId = articleId;
        }

        public void IncrementBy(Quantity quantity)
        {
            _quantity.IncrementBy(quantity);
        }
        
        public void DecrementBy(Quantity quantity)
        {
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
    }
}