using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IDemandForecast
    {
        ProductionOrder NetRequirement(IDemandToProvider demand, IDemandToProvider parent, MrpTask task);
    }


    public class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;
        private readonly IProcessMrp _processMrp;

        public DemandForecast(MasterDBContext context, IProcessMrp processMrp)
        {
            _context = context;
            _processMrp = processMrp;
        }
        
        /// <summary>
        /// Creates providers for the demands through stock, productionOrders or purchases
        /// </summary>
        /// <param name="demand"></param>
        /// <param name="parent"></param>
        /// <param name="task"></param>
        /// <returns>ProductionOrder to fulfill the demand, ProductionOrder is null if there was enough in stock</returns>
        public ProductionOrder NetRequirement(IDemandToProvider demand, IDemandToProvider parent, MrpTask task)
        {
            var stock = _context.Stocks.Include(a => a.DemandStocks)
                .Single(a => a.ArticleForeignKey == demand.ArticleId);
            var plannedStock = GetPlannedStock(stock,demand);
            ProductionOrder productionOrder = null;
            TryCreateStockReservation(stock, demand);
            //if the plannedStock is below zero, articles have to be produced for its negative amount 
            if (plannedStock < 0)
            {
                //if the article has no children it has to be purchased
                var children = _context.ArticleBoms.Where(a => a.ArticleParentId == demand.ArticleId).ToList();
                if (children.Any())
                    productionOrder = CreateChildProductionOrder(demand, parent, -plannedStock);
                else
                    CreatePurchaseDemand(demand, -plannedStock);
            }
            //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
            if (stock.Min <= 0 || plannedStock >= stock.Min || demand.GetType() == typeof(DemandStock))
                return productionOrder;
            
            if (_context.Demands.OfType<DemandStock>()
                .Any(a => a.ArticleId == demand.ArticleId
                            && a.State != State.Produced
                            && a.State != State.Purchased))
                return productionOrder;
            if (plannedStock < 0)
                CreateStockDemand(demand, stock, stock.Min, task);
            else CreateStockDemand(demand, stock, stock.Min - plannedStock, task);

            return productionOrder;
        }

        private ProductionOrder CreateChildProductionOrder(IDemandToProvider demand, IDemandToProvider parent, decimal amount)
        {

            var productionOrder = CreateProductionOrder(demand, amount);
            var demandProviderProductionOrder = CreateDemandProviderProductionOrder(demand, productionOrder, amount);
            demand.DemandProvider.Add(demandProviderProductionOrder);
            //if the article has a parent create a relationship
            if (parent != null)
            {
                demandProviderProductionOrder.DemandRequesterId = parent.DemandRequesterId;
                _context.Demands.Update(demandProviderProductionOrder);
                CreateProductionOrderBom(demand, parent, productionOrder);
            }
            _context.SaveChanges();
            return productionOrder;
        }

        private void CreateStockDemand(IDemandToProvider demand, Stock stock, decimal amount, MrpTask task)
        {
            var demandStock = new DemandStock()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                State = State.Created,
                DemandProvider = new List<DemandToProvider>(),
                StockId = stock.Id,
            };
            _context.Demands.Add(demandStock);
            _context.SaveChanges();
            demandStock.DemandRequesterId = demandStock.Id;

            _context.Update(demandStock);
            _context.SaveChanges();
            
            //call processMrp to plan and schedule the stockDemand
            _processMrp.RunRequirementsAndTermination(demandStock, task);
        }

        private ProductionOrder CreateProductionOrder(IDemandToProvider demand, decimal amount)
        {
            var productionOrder = new ProductionOrder()
            {
                Article = demand.Article,
                ArticleId = demand.Article.Id,
                Quantity = amount,
            };

            _context.ProductionOrders.Add(productionOrder);
            _context.SaveChanges();
            return productionOrder;
        }

        private decimal GetPlannedStock(Stock stock,IDemandToProvider demand)
        {
            var amountReserved = GetReserved(demand.ArticleId);
            var amountBought=0;
            var articlesBought = _context.PurchaseParts.Where(a => a.ArticleId == demand.ArticleId);
            foreach (var articleBought in articlesBought)
            {
                amountBought += articleBought.Quantity;
            }
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current + amountBought - demand.Quantity - amountReserved;
            
            return plannedStock;
        }

        /// <summary>
        /// Check in stock for reservations for the article
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        private decimal GetReserved(int articleId)
        {
            decimal amountReserved = 0;
            IQueryable<IDemandToProvider> reservations = _context.Demands.OfType<DemandProviderStock>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            //Todo check logic
            reservations = _context.Demands.OfType<DemandProviderPurchasePart>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            return amountReserved;
        }

        private DemandProviderProductionOrder CreateDemandProviderProductionOrder(IDemandToProvider demand, ProductionOrder productionOrder,decimal amount)
        {
            var demandProviderProductionOrder = new DemandProviderProductionOrder()
            {
                DemandRequesterId = null,
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                ProductionOrderId = productionOrder.Id,
            };
            _context.Demands.Add(demandProviderProductionOrder);
            _context.SaveChanges();
            return demandProviderProductionOrder;
        }

        public void CreateProductionOrderBom(IDemandToProvider demand, IDemandToProvider parent, ProductionOrder productionOrder)
        {
            var bom = _context.ArticleBoms.AsNoTracking()
                        .Single(a => a.ArticleChildId == demand.ArticleId && a.ArticleParentId == parent.ArticleId);

            //find parent
            ProductionOrder productionOrderParent = null;
            foreach (var provider in parent.DemandProvider)
            {
                if (provider.GetType() == typeof(DemandProviderProductionOrder))
                    productionOrderParent = ((DemandProviderProductionOrder)provider).ProductionOrder;
            }
            //create relationship to parent
            var productionOrderBom = new ProductionOrderBom()
            {
                Quantity = bom.Quantity * productionOrder.Quantity,
                ProductionOrderChildId = productionOrder.Id,
                ProductionOrderParent = productionOrderParent,
            };
            _context.ProductionOrderBoms.Add(productionOrderBom);
            _context.SaveChanges();
            
            if (demand.GetType() == typeof(DemandProductionOrderBom))
                ((DemandProductionOrderBom)demand).ProductionOrderBomId = productionOrderBom.Id;
            _context.Update(demand);
            _context.SaveChanges();
        }

        //Todo: check logic
        private void CreatePurchaseDemand(IDemandToProvider demand, decimal amount)
        {
            if (NeedToRefill(demand, amount))
            {
                var providerPurchasePart = new DemandProviderPurchasePart()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.DemandRequesterId,
                    State = State.Created,
                };
                _context.Demands.Add(providerPurchasePart);

                CreatePurchase(demand, amount, providerPurchasePart);
                _context.Demands.Update(providerPurchasePart);
            }
            else
            {
                var providerStock = new DemandProviderStock()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.DemandRequesterId,
                    State = State.Created,
                    StockId = _context.Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Id
                };
                _context.Demands.Add(providerStock);
            }
            _context.SaveChanges();
        }

        private void CreatePurchase(IDemandToProvider demand, decimal amount, DemandProviderPurchasePart demandProviderPurchasePart)
        {
            var articleToPurchase = _context.ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new Purchase()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Ceiling(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new PurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<DemandProviderPurchasePart>() { demandProviderPurchasePart },
                PurchaseId = purchase.Id
            };
            purchase.PurchaseParts = new List<PurchasePart>()
            {
                purchasePart
            };
            
            _context.Purchases.Add(purchase);
            _context.PurchaseParts.Add(purchasePart);
            _context.SaveChanges();
        }

        private bool NeedToRefill(IDemandToProvider demand, decimal amount)
        {
            var purchasedAmount = GetAmountBought(demand.ArticleId);
            var neededAmount = GetReserved(demand.ArticleId);
            var stockMin = _context.Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Min;
            return (purchasedAmount - neededAmount - amount < stockMin);
        }

        /// <summary>
        /// Creates stock reservation if possible
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="demand"></param>
        public void TryCreateStockReservation(Stock stock, IDemandToProvider demand)
        {
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetAmountBought(demand.ArticleId);
            //get the current amount of free available articles
            var current = stock.Current + bought - stockReservations;
            decimal quantity;
            //either reserve all that are in stock or the amount needed
            quantity = demand.Quantity > current ? current : demand.Quantity;
            if (quantity == 0) return;
            var demandProviderStock = new DemandProviderStock()
            {
                ArticleId = stock.ArticleForeignKey,
                Quantity = quantity,
                DemandRequesterId = demand.DemandRequesterId,
                StockId = stock.Id
            };
            _context.Demands.Add(demandProviderStock);
            _context.SaveChanges();
        }

        private decimal GetAmountBought(int articleId)
        {
            var purchaseParts = _context.PurchaseParts.Where(a => a.ArticleId == articleId);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
                purchasedAmount += purchasePart.Quantity;
            return purchasedAmount;
        }
    }
    
}
