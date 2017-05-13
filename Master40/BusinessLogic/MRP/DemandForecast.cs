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
            //get the actual item from db
            //ToDo: test if one is enough
            var stock = _context.Stocks.Include(a => a.DemandStocks)
                .Single(a => a.ArticleForeignKey == demand.ArticleId);
                
            decimal amountReserved=0;
            foreach (var demandStock in stock.DemandStocks)
            {
                if (demandStock.StockId == stock.StockId)
                    amountReserved =  demandStock.Quantity;


            }
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current - demand.Quantity - amountReserved;
            //if there is at least one or more of this article in stock
            if (stock.Current > 0)
            {
                var msg = "Articles in stock: " + demand.Article.Name + " " + stock.Current;
                Logger.Add(new LogMessage() { MessageType = MessageType.success,Message = msg });
            }
            ProductionOrder productionOrder = null;
            //if the plannedStock is below zero articles have to be produced    
            if (plannedStock < 0)
            {
                var children = _context.ArticleBoms.Where(a => a.ArticleParentId == demand.ArticleId).ToList();
                if (children.Any())
                {
                    var msg = "Articles ordered to produce: " + demand.Article.Name + " " + (-plannedStock);
                    Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });

                    var demandProviderProductionOrder = new DemandProviderProductionOrder()
                    {
                        DemandRequesterId = null,
                        Quantity = -plannedStock,
                        Article = demand.Article,
                        ArticleId = demand.ArticleId,

                    };

                    productionOrder = new ProductionOrder()
                    {
                        Article = demand.Article,
                        ArticleId = demand.Article.ArticleId,
                        Quantity = -plannedStock,
                        DemandProviderProductionOrders = new List<DemandProviderProductionOrder>()
                        {
                            demandProviderProductionOrder
                        },
                        
                    };
                    demand.DemandProvider.Add(demandProviderProductionOrder);
                    demandProviderProductionOrder.ProductionOrder = productionOrder;
                    demandProviderProductionOrder.ProductionOrderId = productionOrder.ProductionOrderId;
                    _context.Demands.Add(demandProviderProductionOrder);
                    _context.ProductionOrders.Add(productionOrder);
                    
                    if (parent != null)
                    {
                        demandProviderProductionOrder.DemandRequester = parent.DemandRequester;
                        demandProviderProductionOrder.DemandRequesterId = parent.DemandRequesterId;
                        var bom =
                            _context.ArticleBoms.Single(
                                a => (a.ArticleChildId == demand.ArticleId) && (a.ArticleParentId == parent.ArticleId));

                        ProductionOrder productionOrderParent = null;
                        foreach (var provider in parent.DemandProvider)
                        {
                            if (provider.GetType() == typeof(DemandProviderProductionOrder))
                                productionOrderParent = ((DemandProviderProductionOrder)provider).ProductionOrder;
                        }
                        
                        var productionOrderBom = new ProductionOrderBom()
                        {
                            Quantity = bom.Quantity *productionOrder.Quantity,
                            ProductionOrderChildId = productionOrder.ProductionOrderId,
                            ProductionOrderChild = productionOrder,
                            ProductionOrderParent = productionOrderParent

                        };
                        productionOrder.ProductionOrderBoms = new List<ProductionOrderBom>()
                        {
                            productionOrderBom
                        };
                        _context.ProductionOrderBoms.Add(productionOrderBom);
                    }
                    
                }
                else
                {
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



                //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                //if (plannedStock < article.Stock.Min)
                //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)

            }
            else
            {
                //create ProviderStock entry
                //create demandstock entry
            }
            return productionOrder;
        }
       
    }
    
}
