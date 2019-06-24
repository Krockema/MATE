using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class StockExchangeDemand : Demand, IDemandLogic
    {

        public StockExchangeDemand(IDemand demand) : base(demand)
        {
        }
    }
}