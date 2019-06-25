namespace ZppForPrimitives
{
    public class Quantity
    {
        private decimal _quantity;

        public Quantity(decimal quantity)
        {
            _quantity = quantity;
        }

        public decimal GetQuantity()
        {
            return _quantity;
        }
    }
}