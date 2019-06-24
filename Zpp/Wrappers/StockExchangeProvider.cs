using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        public StockExchangeProvider(IProvider provider, List<Demand> demands) : base(provider, demands)
        {
        }
    }
}