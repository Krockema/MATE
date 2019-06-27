using System.Collections.Generic;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_StockExchange for T_StockExchange providers
     */
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        public StockExchangeProvider(IProvider provider, Demands demands) : base(provider, demands)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_StockExchange)_provider;
        }
    }
}