using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Data;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using Master40.Models;

namespace Master40.BusinessLogic.MRP
{
    public interface IDemandForecast
    {
        ProductionOrder NetRequirement(IDemandToProvider demand, IDemandToProvider parent, int orderId);
        List<LogMessage> Logger { get; set; }
    }


    public class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;
        //public DemandForecast(MasterDBContext context)
        public List<LogMessage> Logger { get; set; }

        public DemandForecast(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
            _context = context;
        }
        
        ProductionOrder IDemandForecast.NetRequirement(IDemandToProvider demand, IDemandToProvider parent, int orderId)
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
                    CreatePurchase(demand, -plannedStock);
                }
                
                //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                //if (plannedStock < stock.Min)
                //CreateProductionOrder()
                //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)

            }
            return productionOrder;
        }

        private ProductionOrder CreateProductionOrder(IDemandToProvider demand, decimal amount)
        {
            var productionOrder = new ProductionOrder()
            {
                Article = demand.Article,
                ArticleId = demand.Article.ArticleId,
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
            decimal amountReserved = 0;
            //check for reservations for the article
            foreach (var demandStock in stock.DemandStocks)
            {
                if (demandStock.StockId == stock.StockId)
                    amountReserved += demandStock.Quantity;
            }
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current - demand.Quantity - amountReserved;

            if (stock.Current > 0)
            {
                var msg = "Articles in stock: " + demand.Article.Name + " " + stock.Current;
                Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
            }
            return plannedStock;
        }

        private DemandProviderProductionOrder CreateDemandProviderProductionOrder(IDemandToProvider demand,ProductionOrder productionOrder,decimal amount)
        {
            var demandProviderProductionOrder = new DemandProviderProductionOrder()
            {
                DemandRequesterId = null,
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                ProductionOrderId = productionOrder.ProductionOrderId,
            };
            _context.Demands.Add(demandProviderProductionOrder);
            _context.SaveChanges();
            return demandProviderProductionOrder;
        }

        private void CreateProductionOrderBom(IDemandToProvider demand, IDemandToProvider parent, ProductionOrder productionOrder)
        {
            var bom =
                _context.ArticleBoms.Single(
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
                ProductionOrderChildId = productionOrder.ProductionOrderId,
                ProductionOrderParent = productionOrderParent

            };

            _context.ProductionOrderBoms.Add(productionOrderBom);
        }

        private void CreatePurchase(IDemandToProvider demand, decimal amount)
        {
            var providerPurchasePart = new DemandProviderPurchasePart()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                DemandRequesterId = demand.DemandRequesterId,
                DemandRequester = demand.DemandRequester,
                
            };
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
                Quantity = (int) amount,
                DemandProviderPurchaseParts = new List<DemandProviderPurchasePart>() {providerPurchasePart},
                PurchaseId = purchase.PurchaseId
            };
            purchase.PurchaseParts = new List<PurchasePart>()
            {
                purchasePart
            };
            _context.Demands.Add(providerPurchasePart);
            _context.Purchases.Add(purchase);
            _context.PurchaseParts.Add(purchasePart);
            _context.SaveChanges();

            var msg = "Articles ordered to purchase: " + demand.Article.Name + " " + (amount);
            Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
           
            
        }

        private void CreateStockReservation(Stock stock, IDemandToProvider demand)
        {
            if (stock.Current == 0)
                return;
            //get all reservations for this article
            var stockReservations = _context.Demands.OfType<DemandProviderStock>().Where(a => a.ArticleId == demand.ArticleId);
            var current = stock.Current;
            foreach (var stockReservation in stockReservations)
            {
                current -= stockReservation.Quantity;
            }
            decimal quantity;
            quantity = demand.Quantity > current ? current : demand.Quantity;
            var demandProviderStock = new DemandProviderStock()
            {
                ArticleId = stock.ArticleForeignKey,
                Quantity = quantity,
                DemandRequesterId = demand.DemandRequesterId,
                IsProvided = true,
                StockId = stock.StockId
            };
           _context.Demands.Add(demandProviderStock);
            _context.SaveChanges();
        }

    }
    
}
