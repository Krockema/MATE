namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Quantity
    {
        private decimal _quantity;

        public Quantity()
        {
        }

        public Quantity(decimal quantity)
        {
            _quantity = quantity;
        }

        public decimal GetValue()
        {
            return _quantity;
        }

        public void IncrementBy(Quantity quantity)
        {
            _quantity += quantity._quantity;
        }

        public bool IsGreaterThanOrEqualTo(Quantity quantity)
        {
            return _quantity >= quantity._quantity;
        }
    }
}