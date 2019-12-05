using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.StorageAgent.Types
{
    public class StockItem
    {
        public T_StockExchange StockExchange { get; set; }
        public Quantity QuantityLeft { get; set; }
    }
}
