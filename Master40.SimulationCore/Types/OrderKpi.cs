using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.SimulationCore.Types
{
    public class OrderKpi: Dictionary<OrderKpi.OrderState, Quantity>
    {
        public enum OrderState
        {
            New,
            Finished,
            Open,
            Total,
            InDue,
            OverDue
            
        }

        public OrderKpi()
        {
            this.Add(OrderState.New, new Quantity(0));
            this.Add(OrderState.Finished, new Quantity(0));
            this.Add(OrderState.Open, new Quantity(0));
            this.Add(OrderState.Total, new Quantity(0));
            this.Add(OrderState.InDue, new Quantity(0));
            this.Add(OrderState.OverDue, new Quantity(0));
        }
        
    }
}