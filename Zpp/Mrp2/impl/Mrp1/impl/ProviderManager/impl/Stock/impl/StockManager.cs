using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.Util;

namespace Zpp.Mrp2.impl.Mrp1.impl.Stock.impl
{
    public class StockManager
    {
        

        private readonly HashSet<Provider> _alreadyConsideredProviders = new HashSet<Provider>();

        private readonly IDbMasterDataCache _dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();

        private readonly ICacheManager _cacheManager = ZppConfiguration.CacheManager;

        private readonly IOpenDemandManager _openDemandManager =
            ZppConfiguration.CacheManager.GetOpenDemandManager();

        public StockManager()
        {
        }

        /**
         * COP or PrOB --> satisfy by SE:W
         */
        public EntityCollector Satisfy(Demand demand, Quantity demandedQuantity)
        {
            EntityCollector entityCollector = new EntityCollector();

            Provider stockProvider = CreateStockExchangeProvider(demand.GetArticle(),
                demand.GetStartTimeBackward(), demandedQuantity);
            entityCollector.Add(stockProvider);

            T_DemandToProvider demandToProvider =
                new T_DemandToProvider(demand.GetId(), stockProvider.GetId(), demandedQuantity);
            entityCollector.Add(demandToProvider);


            return entityCollector;
        }

        /**
         * returns a provider, which can be a stockExchangeProvider, if article can be fulfilled by stock, else
         * a productionOrder/purchaseOrderPart
         */
        private Provider CreateStockExchangeProvider(M_Article article, DueTime dueTime,
            Quantity demandedQuantity)
        {
            if (demandedQuantity == null || demandedQuantity.GetValue() == 0)
            {
                throw new MrpRunException("Quantity is not set.");
            }
            M_Stock stock = _dbMasterDataCache.M_StockGetByArticleId(article.GetId());
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockExchangeType = StockExchangeType.Provider;
            stockExchange.Quantity = demandedQuantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();
            stockExchange.State = State.Created;

            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Withdrawal;
            StockExchangeProvider stockExchangeProvider = new StockExchangeProvider(stockExchange);

            return stockExchangeProvider;
        }

        public HashSet<Provider> GetAlreadyConsideredProviders()
        {
            return _alreadyConsideredProviders;
        }

        public EntityCollector CreateDependingDemands(Provider provider)
        {
            if (provider.GetQuantity().IsNull())
            {
                return null;
            }

            if (provider.GetType() != typeof(StockExchangeProvider))
            {
                throw new MrpRunException("This can only be called for StockExchangeProviders");
            }

            // try to provide by existing demand

            // collects stockExchangeDemands, providerToDemands
            EntityCollector entityCollector =
                _openDemandManager.SatisfyProviderByOpenDemand(provider, provider.GetQuantity());
            if (entityCollector == null)
            {
                entityCollector = new EntityCollector();
            }

            Quantity remainingQuantity = entityCollector.GetRemainingQuantity(provider);

            if (remainingQuantity.IsGreaterThan(Quantity.Zero()))
            {
                LotSize.Impl.LotSize lotSizes =
                    new LotSize.Impl.LotSize(remainingQuantity, provider.GetArticleId());
                bool isLastIteration = false;
                foreach (var lotSize in lotSizes.GetLotSizes())
                {
                    if (isLastIteration || remainingQuantity.IsNegative() ||
                        remainingQuantity.IsNull())
                    {
                        throw new MrpRunException("This is one iteration too many.");
                    }

                    Demand stockExchangeDemand =
                        StockExchangeDemand.CreateStockExchangeStockDemand(provider.GetArticleId(),
                            provider.GetStartTimeBackward(), lotSize);
                    entityCollector.Add(stockExchangeDemand);

                    // 3 cases
                    Quantity quantityOfNewCreatedDemandToReserve;
                    if (remainingQuantity.IsGreaterThan(lotSize))
                    {
                        quantityOfNewCreatedDemandToReserve = lotSize;
                    }
                    else if (remainingQuantity.Equals(lotSize))
                    {
                        // last iteration
                        isLastIteration = true;
                        quantityOfNewCreatedDemandToReserve = lotSize;
                    }
                    else
                    {
                        // last iteration, remaining < lotsize
                        isLastIteration = true;
                        quantityOfNewCreatedDemandToReserve = new Quantity(remainingQuantity);
                        // remember created demand as openDemand
                        _openDemandManager.AddDemand(stockExchangeDemand,
                            quantityOfNewCreatedDemandToReserve);
                    }

                    remainingQuantity.DecrementBy(lotSize);
                    if (quantityOfNewCreatedDemandToReserve.IsNegative() ||
                        quantityOfNewCreatedDemandToReserve.IsNull())
                    {
                        throw new MrpRunException(
                            $"quantityOfNewCreatedDemandToReserve cannot be negative or null: " +
                            $"{quantityOfNewCreatedDemandToReserve}");
                    }

                    T_ProviderToDemand providerToDemand = new T_ProviderToDemand(provider.GetId(),
                        stockExchangeDemand.GetId(), quantityOfNewCreatedDemandToReserve);
                    entityCollector.Add(providerToDemand);
                }
            }

            return entityCollector;
        }
    }
}