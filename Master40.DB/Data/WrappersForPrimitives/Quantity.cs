namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Quantity
    {
        // TODO: T_PurchaseOrderPart, T_CustomerOrderPart uses int, ensure correct rounding.
        private decimal _quantity;

        public Quantity()
        {
        }

        public Quantity(Quantity quantity)
        {
            _quantity = quantity.GetValue();
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

        public bool IsSmallerThan(Quantity quantity)
        {
            return _quantity < quantity.GetValue();
        }

        public Quantity Minus(Quantity quantity)
        {
            return new Quantity(quantity.GetValue() - quantity.GetValue());
        }
    }
}