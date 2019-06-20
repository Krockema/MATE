using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class WT_CustomerOrderPart : Demand, WIDemand 
    {

        public WT_CustomerOrderPart(IDemand demand) : base(demand)
        {
            
        }
    }
}