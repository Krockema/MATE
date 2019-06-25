using Master40.DB.DataModel;
using Master40.DB.Interfaces;


namespace Zpp.DemandDomain
{
    public class CustomerOrderPart : Demand 
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