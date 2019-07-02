using System;

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
        
        public bool IsGreaterThan(Quantity quantity)
        {
            return _quantity > quantity._quantity;
        }

        public bool IsSmallerThan(Quantity quantity)
        {
            return _quantity < quantity.GetValue();
        }

        /**
         * Consider using AbsoluteValue() after this function
         */
        public Quantity Minus(Quantity quantity)
        {
            return new Quantity(quantity.GetValue() - quantity.GetValue());
        }

        public Quantity AbsoluteValue()
        {
            return new Quantity(Math.Abs(_quantity));
        }

        public override bool Equals(object obj)
        {
            Quantity otherQuantity = (Quantity) obj;
            return _quantity.Equals(otherQuantity.GetValue());
        }

        public override int GetHashCode()
        {
            return _quantity.GetHashCode();
        }
    }
}