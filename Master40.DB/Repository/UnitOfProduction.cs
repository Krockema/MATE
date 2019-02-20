using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.DB.Repository
{
    public class UnitOfProduction : UnitOfWork
    {
        public UnitOfProduction(MasterDBContext dbContext, UnitOfWork origin) : base(dbContext, origin)
        {

        }

        #region ProcessMRP

        public List<ProductionOrderWorkSchedule> GetParents(ProductionOrderWorkSchedule schedule)
        {
            var parents = new List<ProductionOrderWorkSchedule>();
            if (schedule == null) return parents;
            var parent = GetHierarchyParent(schedule);
            if (parent != null)
            {
                parents.Add(parent);
                return parents;
            }
            var bomParents =  GetBomParents(schedule);
            return bomParents ?? parents;
        }
        public static ProductionOrderWorkSchedule GetHierarchyParent(ProductionOrderWorkSchedule pows)
        {

            ProductionOrderWorkSchedule hierarchyParent = null;
            var hierarchyParentNumber = int.MaxValue;
            //find next higher element
            foreach (var mainSchedule in pows.ProductionOrder.ProductionOrderWorkSchedule)
            {
                //if (mainSchedule.ProductionOrderId != pows.ProductionOrderId) continue;
                if (mainSchedule.HierarchyNumber <= pows.HierarchyNumber ||
                    mainSchedule.HierarchyNumber >= hierarchyParentNumber) continue;
                hierarchyParent = mainSchedule;
                hierarchyParentNumber = mainSchedule.HierarchyNumber;
            }
            return hierarchyParent;
        }

        public List<ProductionOrderWorkSchedule> GetBomParents(ProductionOrderWorkSchedule plannedSchedule)
        {
            var provider = plannedSchedule.ProductionOrder.DemandProviderProductionOrders;
            if (provider == null || provider.Any(dppo => dppo.DemandRequester == null))
                return new List<ProductionOrderWorkSchedule>();
            var requester = (from demandProviderProductionOrder in provider
                             select demandProviderProductionOrder.DemandRequester into req
                             select req).ToList();


            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleRequester in requester)
            {
                if (singleRequester.GetType() == typeof(DemandOrderPart) || singleRequester.GetType() == typeof(DemandStock)) return null;
                var demand = this.Demands.OfType<DemandProductionOrderBom>()
                    // .Include(a => a.ProductionOrderBom)
                    // .ThenInclude(b => b.ProductionOrderParent).ThenInclude(c => c.ProductionOrderWorkSchedule)
                    .Single(a => a.Id == singleRequester.Id);
                var schedules = demand.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule;
                pows.Add(schedules.Single(a => a.HierarchyNumber == schedules.Min(b => b.HierarchyNumber)));
            }
            return pows;

        }

        #endregion

        #region DemandForecast

        public decimal GetPlannedStock(Stock stock, IDemandToProvider demand)
        {
            var amountReserved = GetReserved(demand.ArticleId);
            var amountBought = 0;
            var articlesBought = PurchaseParts.Where(a => a.ArticleId == demand.ArticleId && a.State != State.Finished);
            foreach (var articleBought in articlesBought)
            {
                amountBought += articleBought.Quantity;
            }
            //just produced articles have a reason and parents they got produced for so they cannot be reserved by another requester
            var amountJustProduced = Demands.OfType<DemandProductionOrderBom>()
                .Where(a => (a.State != State.Finished || a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.All(b => b.ProducingState == ProducingState.Created || b.ProducingState == ProducingState.Waiting))
                            && a.ArticleId == demand.ArticleId
                            && a.DemandProvider.Any()
                            && a.DemandProvider.All(b => b.State == State.Finished)).Sum(a => a.Quantity);
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current + amountBought - demand.Quantity - amountReserved - amountJustProduced;

            return plannedStock;
        }

        public decimal GetReserved(int articleId)
        {
            var demands = Demands.OfType<DemandProviderStock>()
                .Where(a => a.State != State.Finished && a.ArticleId == articleId).Sum(a => a.Quantity);
            return demands;
        }

        public DemandProviderStock TryCreateStockReservation(IDemandToProvider demand)
        {
            var stock = Stocks.ToList().Single(a => a.ArticleForeignKey == demand.ArticleId);
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetAmountBought(demand.ArticleId);
            var justProduced = Demands.OfType<DemandProductionOrderBom>()
                .Where(a => (a.State != State.Finished || a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.All(b => b.ProducingState == ProducingState.Created || b.ProducingState == ProducingState.Waiting))
                            && a.ArticleId == demand.ArticleId
                            && a.DemandProvider.Any()
                            && a.DemandProvider.All(b => b.State == State.Finished)).Sum(a => a.Quantity);

            //get the current amount of free available articles
            var current = stock.Current + bought - stockReservations - justProduced;
            decimal quantity;
            //either reserve all that are in stock or the amount needed
            quantity = demand.Quantity > current ? current : demand.Quantity;

            return quantity <= 0 ? null : CreateDemandProviderStock(demand, quantity);
        }

        public decimal GetAmountBought(int articleId)
        {
            var purchaseParts = PurchaseParts.Where(a => a.ArticleId == articleId && a.State != State.Finished);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
                purchasedAmount += purchasePart.Quantity;
            return purchasedAmount;
        }

        public DemandProviderStock CreateDemandProviderStock(IDemandToProvider demand, decimal amount)
        {
            var stock = Stocks.ToList().Single(a => a.ArticleForeignKey == demand.ArticleId);
            var article = Articles.ToList().Single(a => a.Id == demand.ArticleId);
            var dps = new DemandProviderStock()
            {
                ArticleId = demand.ArticleId,
                Article = article,
                Quantity = amount,
                StockId = Stocks.ToList().Single(a => a.ArticleForeignKey == demand.ArticleId).Id,
                Stock = stock,
                DemandRequesterId = demand.Id,
                State = State.Created
            };
            demand.DemandRequester = dps;
            article.DemandToProviders.Add(dps);
            stock.DemandProviderStocks.Add(dps);
            dps.DemandRequester = demand as DemandToProvider;

            Save();
            return dps;
        }
        #endregion
    }
}
