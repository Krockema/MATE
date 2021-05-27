using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.StorageAgent.Types
{
    public class StockItem
    {
        public T_StockExchange StockExchange { get; set; }
        public Quantity QuantityLeft { get; set; }
    }
}
