using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IRebuildNets
    {
        void Rebuild(SimulationConfiguration simConfig, ProductionDomainContext evaluationContext);
    }

    public class RebuildNets : IRebuildNets
    {
        private readonly ProductionDomainContext _context;
        public RebuildNets(ProductionDomainContext context)
        {
            _context = context;
        }

        public void Rebuild(SimulationConfiguration simulationConfiguration, ProductionDomainContext evaluationContext)
        {
            //delete all demands but the head-demands
            _context.Demands.RemoveRange(_context.Demands.Where(a => (a.DemandRequesterId != null || a.GetType() == typeof(DemandProductionOrderBom)) && a.State != State.Finished).ToList());
            _context.ProductionOrderBoms.RemoveRange(_context.ProductionOrderBoms.Where(a => a.State != State.Finished));
            _context.SaveChanges();
            var requester = _context.Demands.Where(a => null == a.DemandRequesterId &&
                                                                       a.State != State.Finished).ToList();

            requester = (from req in requester
                         where req.GetType() == typeof(DemandStock) ||
                               _context.GetDueTimeByOrder(req) < simulationConfiguration.Time
                               + simulationConfiguration.MaxCalculationTime
                         select req).ToList();
            
            //rebuild by using activity-slack to order demands
            while (requester.Any())
            {
                var nextRequester = GetNextByActivitySlack(requester, simulationConfiguration);

                SatisfyRequest(nextRequester, simulationConfiguration, evaluationContext);
                requester.Remove(requester.Find(a => a.Id == nextRequester.Id));
            }

        }

        private void SatisfyRequest(IDemandToProvider demand,SimulationConfiguration simulationConfiguration , ProductionDomainContext evaluationContext)
        {
            var amount = demand.Quantity;

            //if anything is in stock, create demand
            amount = TryAssignStockReservation(demand,amount);

            if (amount == 0) return;
            
            //search for purchase   
            amount = TryAssignPurchase(demand, amount);
            
            if (amount == 0) return;
            //find matching productionOrders
            amount = TryFindProductionOrders(demand,amount);

            if (amount != 0)
            {
                SaveContext(evaluationContext);
                amount = TryAssignStockReservation(demand, amount);
                amount = TryAssignPurchase(demand, amount);
                amount = TryFindProductionOrders(demand, amount);
                throw new NotSupportedException("logical error: still unsatisfied Requests!");
            }
            foreach (var dppo in demand.DemandProvider.OfType<DemandProviderProductionOrder>())
            {
                CallChildrenSatisfyRequest(dppo.ProductionOrder, evaluationContext, simulationConfiguration);
            }
        }

        private void SaveContext(ProductionDomainContext _evaluationContext)
        {
            foreach (var stockexchange in _context.StockExchanges)
            {
                stockexchange.SimulationConfigurationId = 1;
                stockexchange.SimulationNumber = _context.GetSimulationNumber(stockexchange.SimulationConfigurationId, SimulationType.Central);
                stockexchange.SimulationType = SimulationType.Central;
                var name = _context.Stocks.Single(b => b.Id == stockexchange.StockId).Name;
                stockexchange.StockId = _evaluationContext.Stocks.Single(a => a.Name.Equals(name)).Id;
                var exchange = new StockExchange();
                stockexchange.CopyDbPropertiesTo(exchange);
                _evaluationContext.StockExchanges.Add(exchange);
            }
            _evaluationContext.SaveChanges();
            
        }

        private decimal TryAssignPurchase(IDemandToProvider demand, decimal amount)
        {
            var purchaseParts = _context.PurchaseParts.Where(a => a.State != State.Finished && a.ArticleId == demand.ArticleId).ToList();
            var amountAlreadyReserved = purchaseParts.Select(a => a.DemandProviderPurchaseParts.Sum(b => b.Quantity)).Sum();
            var amountReservable = purchaseParts.Select(a => a.Quantity).Sum() - amountAlreadyReserved;
            if (amountReservable == 0)
                return amount;
            var amountReserving = amountReservable > amount ? amount : amountReservable;
            _context.CreateDemandProviderStock(demand, amountReserving);
            amount -= amountReserving;
            return amount;
        }

        private decimal TryFindProductionOrders(IDemandToProvider demand, decimal amount)
        {
            var possibleMatchingProductionOrders = _context.ProductionOrders.Where(a => a.ArticleId == demand.ArticleId && a.ProductionOrderWorkSchedule.Any(b => b.ProducingState != ProducingState.Finished)).ToList();

            while (amount > 0 && possibleMatchingProductionOrders.Any())
            {
                var earliestProductionOrder = _context.GetEarliestProductionOrder(possibleMatchingProductionOrders);
                var availableAmountFromProductionOrder = _context.GetAvailableAmountFromProductionOrder(earliestProductionOrder);
                if (availableAmountFromProductionOrder == 0)
                {
                    possibleMatchingProductionOrders.Remove(possibleMatchingProductionOrders.Find(a => a.Id == earliestProductionOrder.Id));
                    continue;
                }
                var provider = _context.CreateProviderProductionOrder(demand, earliestProductionOrder, amount >= availableAmountFromProductionOrder ? availableAmountFromProductionOrder : amount);
                var duetime = _context.GetDueTimeByOrder(demand);
                if (provider.ProductionOrder.DemandProviderProductionOrders.Count == 1)
                    provider.ProductionOrder.Duetime = duetime;
                else if (provider.ProductionOrder.Duetime > duetime)
                    provider.ProductionOrder.Duetime = duetime;
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignProductionOrderToDemandProvider(earliestProductionOrder, provider);
                if (amount > availableAmountFromProductionOrder)
                {
                    amount -= availableAmountFromProductionOrder;
                }
                else
                {
                    amount = 0;
                }
                possibleMatchingProductionOrders.ToList().Remove(earliestProductionOrder);
            }
            return amount;
        }

        private decimal TryAssignStockReservation(IDemandToProvider demand, decimal amount)
        {
            var articleStock = _context.Stocks.AsNoTracking().Single(a => a.ArticleForeignKey == demand.ArticleId).Current;
            if (articleStock <= 0) return amount;
            var provider = _context.TryCreateStockReservation(demand);
            if (provider != null)
            {
                amount -= provider.Quantity;
            }
            return amount;
        }

        private void CallChildrenSatisfyRequest(ProductionOrder po, ProductionDomainContext evaluationContext, SimulationConfiguration simConfig)
        {
            if (po.ProductionOrderBoms != null && po.ProductionOrderBoms.Any()) return;
            //call method for each child
            var childrenArticleBoms = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == po.ArticleId).ToList();
            foreach (var childBom in childrenArticleBoms)
            {
                var neededAmount = childBom.Quantity * po.Quantity;
                var demandBom = _context.CreateDemandProductionOrderBom(childBom.ArticleChildId, neededAmount);
                _context.TryCreateProductionOrderBoms(demandBom, po, simConfig);
                SatisfyRequest(demandBom, simConfig, evaluationContext);
            }
        }

        private IDemandToProvider GetNextByActivitySlack(List<DemandToProvider> demandRequester, SimulationConfiguration simConfig)
        {
            if (!demandRequester.Any()) return null;
            DemandToProvider mostUrgentRequester = null;
            foreach (var demand in demandRequester)
            {
                var dueTime = _context.GetDueTimeByOrder(demand);
                if (mostUrgentRequester == null || _context.GetDueTimeByOrder(mostUrgentRequester) > dueTime)
                    mostUrgentRequester = demand;
            }

            var lowestActivitySlack = _context.GetDueTimeByOrder(mostUrgentRequester);
            foreach (var singleRequester in demandRequester)
            {
                var activitySlack = GetActivitySlack(singleRequester, simConfig);
                if (activitySlack >= lowestActivitySlack) continue;
                lowestActivitySlack = activitySlack;
                mostUrgentRequester = singleRequester;
            }
            return mostUrgentRequester;
        }

        private int GetActivitySlack(IDemandToProvider demandRequester, SimulationConfiguration simulationConfiguration)
        {
            var dueTime = 999999;
            if (demandRequester.DemandProvider != null) dueTime = _context.GetDueTimeByOrder((DemandToProvider)demandRequester);
            return dueTime - simulationConfiguration.Time;
        }
    }
}
