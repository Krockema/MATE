using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IRebuildNets
    {
        void Rebuild();
    }

    public class RebuildNets : IRebuildNets
    {
        private readonly ProductionDomainContext _context;
        public RebuildNets(ProductionDomainContext context)
        {
            _context = context;
        }

        public void Rebuild()
        {
            //delete all demands but the head-demands
            _context.Demands.RemoveRange(_context.Demands.Where(a => (a.DemandRequesterId != null || a.GetType() == typeof(DemandProductionOrderBom)) && a.State != State.Finished).ToList());
            _context.ProductionOrderBoms.RemoveRange(_context.ProductionOrderBoms.Where(a => a.State != State.Finished));
            _context.SaveChanges();
            var requester = _context.Demands.Where(a => null == a.DemandRequesterId &&
                                                                       a.State != State.Finished).ToList();

            requester = (from req in requester
                         where req.GetType() == typeof(DemandStock) ||
                               _context.GetDueTimeByOrder(req) < _context.SimulationConfigurations.Last().Time
                               + _context.SimulationConfigurations.Last().MaxCalculationTime
                         select req).ToList();
            
            //rebuild by using activity-slack to order demands
            while (requester.Any())
            {
                var nextRequester = GetNextByActivitySlack(requester);

                SatisfyRequest(nextRequester, null);
                requester.Remove(requester.Find(a => a.Id == nextRequester.Id));
            }

        }

        private void SatisfyRequest(IDemandToProvider demand, ProductionOrder parentProductionOrder)
        {//Todo: search for purchases

            var amount = demand.Quantity;

            //if anything is in stock, create demand
            var articleStock = _context.Stocks.AsNoTracking().Single(a => a.ArticleForeignKey == demand.ArticleId).Current;
            if (articleStock > 0)
            {
                var provider = _context.TryCreateStockReservation(demand);
                if (provider != null)
                {
                    amount -= provider.Quantity;
                }
            }
            if (amount == 0) return;
            //find matching productionOrders
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
                ProductionOrderBom pob = null;
                if (parentProductionOrder != null)
                    pob = _context.TryCreateProductionOrderBoms(demand, parentProductionOrder);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignProductionOrderToDemandProvider(earliestProductionOrder, provider);
                if (pob != null && demand.GetType() == typeof(DemandProductionOrderBom)) _context.AssignDemandProviderToProductionOrderBom((DemandProductionOrderBom)demand, pob);
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

            if (amount == 0)
            {
                foreach (var dppo in demand.DemandProvider.OfType<DemandProviderProductionOrder>())
                {
                    CallChildrenSatisfyRequest(dppo.ProductionOrder);
                }
                return;
            }

            //must not occur, everything should be enough
            if (_context.HasChildren(demand))
            {
                //possible multiple iterations because of lotsize
                while (amount > 0)
                {
                    //create ProductionOrders
                    var productionOrder = _context.CreateProductionOrder(demand, parentProductionOrder?.Duetime ?? _context.GetDueTimeByOrder(demand));
                    if (parentProductionOrder != null) _context.TryCreateProductionOrderBoms(demand, parentProductionOrder);
                    _context.CreateProductionOrderWorkSchedules(productionOrder);
                    var provider = _context.CreateProviderProductionOrder(demand, productionOrder, amount);
                    _context.AssignProviderToDemand(demand, provider);
                    _context.AssignProductionOrderToDemandProvider(productionOrder, provider);
                    CallChildrenSatisfyRequest(productionOrder);

                    amount -= productionOrder.Quantity;
                }
            }
            else
            {
                //create Purchase
                var purchasePart = _context.CreatePurchase(demand, amount);
                var provider = _context.CreateProviderPurchase(demand, purchasePart, amount);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignPurchaseToDemandProvider(purchasePart, provider, purchasePart.Quantity);
            }


        }


        private void CallChildrenSatisfyRequest(ProductionOrder po)
        {
            if (po.ProductionOrderBoms != null && po.ProductionOrderBoms.Any()) return;
            //call method for each child
            var childrenArticleBoms = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == po.ArticleId).ToList();
            foreach (var childBom in childrenArticleBoms)
            {
                //check for the existence of a DemandProductionOrderBom
                //if (po.ProductionOrderBoms.FirstOrDefault(a => a.DemandProductionOrderBoms.First().ArticleId == childBom.ArticleChildId) == null) continue;

                //if (po.ProductionOrderWorkSchedule.Any(a => a.ProducingState == ProducingState.Producing)
                //    || po.ProductionOrderWorkSchedule.Any(a => a.ProducingState == ProducingState.Finished)) continue;

                var neededAmount = childBom.Quantity * po.Quantity;
                var demandBom = _context.CreateDemandProductionOrderBom(childBom.ArticleChildId, neededAmount);

                SatisfyRequest(demandBom, po);
            }
        }

        private IDemandToProvider GetNextByActivitySlack(List<DemandToProvider> demandRequester)
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
                var activitySlack = GetActivitySlack(singleRequester);
                if (activitySlack >= lowestActivitySlack) continue;
                lowestActivitySlack = activitySlack;
                mostUrgentRequester = singleRequester;
            }
            return mostUrgentRequester;
        }

        private int GetActivitySlack(IDemandToProvider demandRequester)
        {
            var dueTime = 999999;
            if (demandRequester.DemandProvider != null) dueTime = _context.GetDueTimeByOrder((DemandToProvider)demandRequester);
            return dueTime - _context.SimulationConfigurations.Last().Time;
        }
    }
}
