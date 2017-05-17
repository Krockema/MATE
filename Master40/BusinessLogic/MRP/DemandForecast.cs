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
        //List<ArticleBom> GrossRequirement(int orderId);
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
                    CreatePurchase(demand, plannedStock);
                }
                
                //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                //if (plannedStock < stock.Min)
                //CreateProductionOrder()
                //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)

            }
            else
            {
                CreateStockReservation();
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

        private void CreatePurchase(IDemandToProvider demand, decimal plannedStock)
        {
            //Todo: implement
            /*var purchase = new Purchase()
                    {
                        
                    };
                    var purchasePart = new DemandProviderPurchasePart()
                    {
                        Quantity = -plannedStock,
                        Article = demand.Article,
                        ArticleId = demand.ArticleId,
                        DemandRequesterId = demand.DemandRequesterId,
                        DemandRequester = demand.DemandRequester,
                        
                    };
                    _context.PurchaseParts.Add(purchase);*/

            var msg = "Articles ordered to purchase: " + demand.Article.Name + " " + (-plannedStock);
            Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
        }

        private void CreateStockReservation()
        {
            //Todo: implement
            //create ProviderStock entry
            //create demandstock entry
        }

    }
    
}
