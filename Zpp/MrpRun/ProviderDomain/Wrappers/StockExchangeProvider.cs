using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_StockExchange for T_StockExchange providers
     */
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        public StockExchangeProvider(IProvider provider, IDbMasterDataCache dbMasterDataCache) :
            base(provider, dbMasterDataCache)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_StockExchange) _provider;
        }

        public override Id GetArticleId()
        {
            Id stockId = new Id(((T_StockExchange) _provider).StockId);
            M_Stock stock = _dbMasterDataCache.M_StockGetById(stockId);
            return new Id(stock.ArticleForeignKey);
        }

        public override void CreateDependingDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity quantity)
        {
            if (quantity.IsNull())
            {
                return;
            }

            _dependingDemands = new Demands();
            Demand stockExchangeDemand =
                StockExchangeDemand.CreateStockExchangeStockDemand(article, GetDueTime(dbTransactionData), quantity,
                    _dbMasterDataCache);
            if (stockExchangeDemand.GetQuantity().IsSmallerThan(quantity))
            {
                throw new MrpRunException($"Created demand should have not a smaller " +
                                          $"quantity ({stockExchangeDemand.GetQuantity()}) " +
                                          $"than the needed quantity ({quantity}).");
            }

            _dependingDemands.Add(stockExchangeDemand);
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string exchangeType =
                Constants.EnumToString(((T_StockExchange) _provider).ExchangeType, typeof(ExchangeType));
            string graphizString = $"P(SE:{exchangeType[0]});{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_StockExchange stockExchange = (T_StockExchange) _provider;
            return new DueTime(stockExchange.RequiredOnTime);
        }
        
        public Id GetStockId()
        {
            return new Id(((T_StockExchange) _provider).StockId);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            return GetDueTime(dbTransactionData);
        }
    }
}