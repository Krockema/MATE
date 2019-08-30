using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.StorageAgent.Types
{
    public class StockExchanges : List<T_StockExchange>
    {
        public List<Guid> ToGuidList()
        {
            return this.Select(x => x.ProductionArticleKey).ToList();
        }
    }
}
