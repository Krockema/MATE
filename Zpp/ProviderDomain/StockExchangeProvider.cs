using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
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

        public override Demands CreateNeededDemands(M_Article article,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProvider, Quantity quantity)
        {
            _demands = new Demands();
            _demands.Add(StockExchangeDemand.CreateStockExchangeStockDemand(article, GetDueTime(), quantity, _dbMasterDataCache));
            return _demands;
        }

        /**
         * returns a provider, which can be a stockExchangeProvider, if article can be fulfilled by stock, else
         * a productionOrder/purchaseOrderPart
         */
        public static Providers CreateStockProvider(M_Article article, DueTime dueTime, Quantity demandedQuantity,
            IDbMasterDataCache dbMasterDataCache, IDbTransactionData dbTransactionData)
        {
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(article.GetId());
            Quantity currentStockQuantity = new Quantity(stock.Current);
            Quantity providedQuantityByStock =
                CalcQuantityProvidedByProvider(currentStockQuantity, demandedQuantity);
            if (providedQuantityByStock != null)
            {
                T_StockExchange stockExchange = new T_StockExchange();
                stockExchange.Provider = new T_Provider();
                stockExchange.ProviderId = stockExchange.Provider.Id;
                stockExchange.Quantity = providedQuantityByStock.GetValue();
                stockExchange.State = State.Created;

                stockExchange.Stock = stock;
                stockExchange.StockId = stock.Id;
                stockExchange.RequiredOnTime = dueTime.GetValue();
                stockExchange.ExchangeType = ExchangeType.Withdrawal;
                StockExchangeProvider stockExchangeProvider =
                    new StockExchangeProvider(stockExchange, dbMasterDataCache);
                
                // Update stock
                stock.Current = currentStockQuantity.Minus(providedQuantityByStock).GetValue();
                if (stock.Current < stock.Min)
                {
                    stockExchangeProvider.CreateNeededDemands(article,
                        dbTransactionData, dbMasterDataCache, stockExchangeProvider, new Quantity(stock.Max - stock.Current));
                }
                
                if (stockProvider != null)
                {
                    unsatisfiedQuantity.Minus(stockProvider.GetQuantity());
                    providers.Add(stockProvider);
                    if (stockProvider.AnyDependingDemands())
                    {
                        // TODO: This should do the caller, but then the caller must get providers and nextDemands...
                        nextDemands.AddAll(stockProvider.GetAllDependingDemands());
                    }

                    if (unsatisfiedQuantity.Equals(nullQuantity))
                    {
                        return providers;
                    }
                }

                return stockExchangeProvider;
            }

            return null;
        }
        
    }
}