using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class WT_StockExchangeProvider : Provider, WIProvider
    {
        public WT_StockExchangeProvider(IProvider provider, List<WIDemand> demands) : base(provider, demands)
        {
        }
    }
}