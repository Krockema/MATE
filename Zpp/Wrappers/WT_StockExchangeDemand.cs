using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class WT_StockExchangeDemand : Demand, WIDemand
    {

        public WT_StockExchangeDemand(IDemand demand) : base(demand)
        {
        }
    }
}