using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Quantity: DecimalPrimitive<Quantity>
    {
        // TODO: T_PurchaseOrderPart, T_CustomerOrderPart uses int, ensure correct rounding.

        public Quantity(Quantity quantity):base(quantity.GetValue())
        {
        }

        public Quantity(decimal quantity):base(quantity)
        {
        }

        public Quantity()
        {
        }
    }
}