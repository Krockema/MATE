using System;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.MrpRun;
using Zpp.MrpRun.NodeManagement;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.ProviderDomain.Wrappers
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
            Demands stockExchangeDemands = new Demands();

            // try to provider by existing demand
            ResponseWithDemands responseWithDemands =
                openDemandManager.SatisfyProviderByOpenDemand(this, demandedQuantity, dbTransactionData);

            if (responseWithDemands.GetDemands().Count() > 1)
            {
                throw new MrpRunException("Only one demand should be reservable.");
            }

            Quantity remainingQuantity = new Quantity(demandedQuantity);
            if (responseWithDemands.CalculateReservedQuantity().IsGreaterThan(Quantity.Null()))
            {
                stockExchangeDemands.AddAll(responseWithDemands.GetDemands());
                ProviderToDemandTable.AddAll(responseWithDemands.GetProviderToDemands());
                remainingQuantity = responseWithDemands.GetRemainingQuantity();
            }

            if (responseWithDemands.IsSatisfied() == false)
            {
                LotSize.LotSize lotSizes = new LotSize.LotSize(remainingQuantity, article.GetId(),
                    _dbMasterDataCache);
                Quantity lotSizeSum = Quantity.Null();
                foreach (var lotSize in lotSizes.GetLotSizes())
                {
                    lotSizeSum.IncrementBy(lotSize);


                    Demand stockExchangeDemand =
                        StockExchangeDemand.CreateStockExchangeStockDemand(article,
                            GetDueTime(dbTransactionData), lotSize, _dbMasterDataCache);
                    stockExchangeDemands.Add(stockExchangeDemand);

                    // quantityToReserve can be calculated as following
                    // given demandedQuantity - (sumLotSize - lotSize) - (lotSize - providedByOpen.remaining)
                    Quantity quantityOfNewCreatedDemandToReserve = demandedQuantity
                        .Minus(lotSizeSum.Minus(lotSize))
                        .Minus(demandedQuantity.Minus(responseWithDemands.GetRemainingQuantity()));

                    ProviderToDemandTable.Add(this, stockExchangeDemand.GetId(),
                        quantityOfNewCreatedDemandToReserve);

                    if (lotSizeSum.IsGreaterThan(demandedQuantity))
                        // remember created demand as openDemand
                    {
                        openDemandManager.AddDemand(GetId(), stockExchangeDemand,
                            quantityOfNewCreatedDemandToReserve);
                    }


                    /*if (stockExchangeDemands.GetQuantity().IsSmallerThan(lotSize))
                    {
                        throw new MrpRunException($"Created demand should have not a smaller " +
                                                  $"quantity ({stockExchangeDemand.GetQuantity()}) " +
                                                  $"than the needed quantity ({lotSize}).");
                    }*/
                }
            }

            _dependingDemands.AddAll(stockExchangeDemands);
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

        public override void SetDueTime(DueTime newDueTime, IDbTransactionData dbTransactionData)
        {
            T_StockExchange stockExchange = (T_StockExchange) _provider;
            stockExchange.RequiredOnTime = newDueTime.GetValue();
        }
    }
}