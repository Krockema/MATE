using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    /**
     * wraps T_StockExchange for T_StockExchange demands
     */
    public class StockExchangeDemand : Demand, IDemandLogic
    {

        public StockExchangeDemand(IDemand demand) : base(demand)
        {
        }

        public override IDemand ToIDemand()
        {
            return (T_StockExchange)_demand;
        }
    }
}