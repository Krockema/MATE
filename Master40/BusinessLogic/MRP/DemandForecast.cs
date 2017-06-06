using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Master40.Models;
using Master40.DB.Models;
using Master40.DB.Data.Context;

namespace Master40.BusinessLogic.MRP
{
    public interface IDemandForecast
    {
        ProductionOrder NetRequirement(IDemandToProvider demand, IDemandToProvider parent, MrpTask task);
        List<LogMessage> Logger { get; set; }
    }


    public class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;
        private readonly ProcessMrp _processMrp;
        //public DemandForecast(MasterDBContext context)
        public List<LogMessage> Logger { get; set; }

        public DemandForecast(MasterDBContext context, ProcessMrp processMrp)
        {
            Logger = new List<LogMessage>();
            _context = context;
            _processMrp = processMrp;
        }
        
        ProductionOrder IDemandForecast.NetRequirement(IDemandToProvider demand, IDemandToProvider parent, MrpTask task)
        {
            var stock = _context.Stocks.Include(a => a.DemandStocks)
                .Single(a => a.ArticleForeignKey == demand.ArticleId);
            var plannedStock = GetPlannedStock(stock,demand);
            ProductionOrder productionOrder = null;
            CreateStockReservation(stock, demand);
            //if the plannedStock is below zero, articles have to be produced for its negative amount 
            if (plannedStock < 0)
            {
                //if the article has no children it has to be purchased
                var children = _context.ArticleBoms.Where(a => a.ArticleParentId == demand.ArticleId).ToList();
                if (children.Any())
                {
                    productionOrder = CreateProductionOrder(demand, -plannedStock);
                    var demandProviderProductionOrder = CreateDemandProviderProductionOrder(demand, productionOrder, -plannedStock);
                    demand.DemandProvider.Add(demandProviderProductionOrder);
                    //if the article has a parent create a relationship
                    if (parent != null)
                    {
                        demandProviderProductionOrder.DemandRequesterId = parent.DemandRequesterId;
                        _context.Demands.Update(demandProviderProductionOrder);
                        CreateProductionOrderBom(demand, parent, productionOrder);
                    }
                    _context.SaveChanges();
                }
                else
                {
                    CreatePurchaseDemand(demand, -plannedStock);
                }
            }
            //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
             if (stock.Min > 0 && plannedStock < stock.Min && demand.GetType() != typeof(DemandStock))
            {

                if (!_context.Demands.OfType<DemandStock>()
                        .Any(a => a.ArticleId == demand.ArticleId 
                            && (a.State != State.Produced 
                                && a.State != State.Purchased)))
                {
                    if (plannedStock < 0)
                        CreateStockDemand(demand, stock, stock.Min, task);
                    else CreateStockDemand(demand, stock, stock.Min - plannedStock, task);
                }
                
            }
                
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

            //var processMrp = new ProcessMrp(_context);
            _processMrp.RunMrp(demandStock, task);
        }

        private ProductionOrder CreateProductionOrder(IDemandToProvider demand, decimal amount)
        {
            var productionOrder = new ProductionOrder()
            {
                Article = demand.Article,
                ArticleId = demand.Article.Id,
                Quantity = amount,
            };
            var msg = "Articles ordered to produce: " + demand.Article.Name + " " + (amount);
            Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });

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

            if (stock.Current > 0)
            {
                var msg = "Articles in stock: " + demand.Article.Name + " " + stock.Current;
                Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
            }
            return plannedStock;
        }

        private decimal GetReserved(int articleId)
        {
            decimal amountReserved = 0;
            //check for reservations for the article
            IQueryable<IDemandToProvider> reservations = _context.Demands.OfType<DemandProviderStock>()
                .Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
            {
                amountReserved += reservation.Quantity;
            }
            reservations =
                _context.Demands.OfType<DemandProviderPurchasePart>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
            {
                amountReserved += reservation.Quantity;
            }
            return amountReserved;
        }

        private DemandProviderProductionOrder CreateDemandProviderProductionOrder(IDemandToProvider demand,ProductionOrder productionOrder,decimal amount)
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

        private void CreateProductionOrderBom(IDemandToProvider demand, IDemandToProvider parent, ProductionOrder productionOrder)
        {
            var bom =
                _context.ArticleBoms.AsNoTracking().Single(
                    a => (a.ArticleChildId == demand.ArticleId) && (a.ArticleParentId == parent.ArticleId));

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
            if (demand.GetType() == typeof(DemandProductionOrderBom))
            {
                ((DemandProductionOrderBom) demand).ProductionOrderBomId = productionOrderBom.Id;
                _context.Add(demand);
            }
            _context.SaveChanges();
        }

        private void CreatePurchaseDemand(IDemandToProvider demand, decimal amount)
        {
            if (NeedToPurchase(demand, amount))
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
                var msg = "Articles ordered to purchase: " + demand.Article.Name + " " + (amount);
                Logger.Add(new LogMessage() {MessageType = MessageType.info, Message = msg});
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

        private bool NeedToPurchase(IDemandToProvider demand, decimal amount)
        {
            var purchasedAmount = GetBought(demand.ArticleId);
            var neededAmount = GetReserved(demand.ArticleId);
            var stockMin = _context.Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Min;
            return (purchasedAmount - neededAmount - amount < stockMin);
        }

        private void CreateStockReservation(Stock stock, IDemandToProvider demand)
        {
            //get all reservations for this article
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetBought(demand.ArticleId);
            var current = stock.Current + bought - stockReservations;
            decimal quantity;
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

        private decimal GetBought(int articleId)
        {
            var purchaseParts = _context.PurchaseParts.Where(a => a.ArticleId == articleId);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
            {
                purchasedAmount += purchasePart.Quantity;
            }
            return purchasedAmount;
        }
    }
    
}
