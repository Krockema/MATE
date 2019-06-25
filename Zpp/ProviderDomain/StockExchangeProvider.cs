using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_StockExchange for T_StockExchange providers
     */
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        public StockExchangeProvider(IProvider provider, List<Demand> demands) : base(provider, demands)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_StockExchange)_provider;
        }
    }
}