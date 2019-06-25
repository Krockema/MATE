using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class CustomerOrderPart : Demand, IDemandLogic 
    {

        public CustomerOrderPart(IDemand demand) : base(demand)
        {
            
        }

        public override IDemand ToIDemand()
        {
            return (T_CustomerOrderPart)_demand;
        }
    }
}