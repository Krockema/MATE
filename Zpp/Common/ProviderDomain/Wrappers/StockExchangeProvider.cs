using System;
using System.Collections.Generic;
using System.Linq;
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

        public void CreateDependingDemands(M_Article article, IDbTransactionData dbTransactionData,
            Provider parentProvider, Quantity demandedQuantity,
            IOpenDemandManager openDemandManager)
        {
            if (demandedQuantity.IsNull())
            {
                return;
            }

            _dependingDemands = new Demands();
            LotSize.LotSize lotSizes =
                new LotSize.LotSize(demandedQuantity, article.GetId(), _dbMasterDataCache);
            Quantity lotSizeSum = Quantity.Null();

            foreach (var lotSize in lotSizes.GetLotSizes())
            {
                lotSizeSum.IncrementBy(lotSize);

                Demands stockExchangeDemands = new Demands();

                // try to provider by existing demand
                ResponseWithDemands responseWithDemands =
                    openDemandManager.SatisfyProviderByOpenDemand(this, lotSize, dbTransactionData);

                if (responseWithDemands.GetDemands().Count() > 1)
                {
                    throw new MrpRunException("Only one demand should be reservable.");
                }

                stockExchangeDemands.AddAll(responseWithDemands.GetDemands());
                ProviderToDemandTable.AddAll(responseWithDemands.GetProviderToDemands());

                if (responseWithDemands.IsSatisfied() == false)
                {
                    Demand stockExchangeDemand =
                        StockExchangeDemand.CreateStockExchangeStockDemand(article,
                            GetDueTime(dbTransactionData), lotSize, _dbMasterDataCache);
                    stockExchangeDemands.Add(stockExchangeDemand);

                    // free quantity can be calculated as following
                    // newDemand + providedByOpen - (lotSizeSum - given demandedQuantity)
                    // e.g. 4 + 1 - 2 = 3 if lotsize is 4 and demandedQuantity is 10, providedByOpen is 1

                    // to reserve can be calculated as following
                    // given demandedQuantity - (sumLotSize - lotSize) - (lotSize - providedByOpen.remaining)
                    Quantity quantityOfNewCreatedDemandToReserve = demandedQuantity
                        .Minus(lotSizeSum.Minus(lotSize))
                        .Minus(lotSize.Minus(responseWithDemands.GetRemainingQuantity()));

                    ProviderToDemandTable.Add(this, stockExchangeDemand.GetId(),
                        quantityOfNewCreatedDemandToReserve);

                    if (lotSizeSum.IsGreaterThan(demandedQuantity))
                        // remember created demand as openDemand
                    {
                        openDemandManager.AddDemand(GetId(), stockExchangeDemand,
                            quantityOfNewCreatedDemandToReserve);
                    }
                }

                /*if (stockExchangeDemands.GetQuantity().IsSmallerThan(lotSize))
                {
                    throw new MrpRunException($"Created demand should have not a smaller " +
                                              $"quantity ({stockExchangeDemand.GetQuantity()}) " +
                                              $"than the needed quantity ({lotSize}).");
                }*/

                _dependingDemands.AddAll(stockExchangeDemands);
            }
        }

        public override void CreateDependingDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider,
            Quantity demandedQuantity)
        {
            throw new NotImplementedException();
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string exchangeType = Constants.EnumToString(((T_StockExchange) _provider).ExchangeType,
                typeof(ExchangeType));
            string graphizString =
                $"P(SE:{exchangeType[0]});{base.GetGraphizString(dbTransactionData)}";
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