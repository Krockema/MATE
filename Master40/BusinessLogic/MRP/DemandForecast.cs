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
        ProductionOrder NetRequirement(DemandOrderPart demand, int orderId);
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
        
  
        /// <summary>
        /// Uses List from NetRequirements to start productionorders if there are not enough materials in stock
        /// </summary>
        /// <param name="needs">List</param>
        ProductionOrder IDemandForecast.NetRequirement(DemandOrderPart demand, int orderId)
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
                var msg = "Articles ordered to produce: " + demand.Article.Name + " " + (-plannedStock);
                Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
                //TODO: implement productionOrder with orderId for -PlannedStock
                productionOrder = new ProductionOrder()
                {
                    Article = demand.Article,
                    ArticleId = demand.Article.ArticleId,
                    Quantity = -plannedStock,
                    //TODO: enable copying over for ProductionOrderBoms = demand.Article.ArticleBoms,

                    
                };

                //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                //if (plannedStock < article.Stock.Min)
                    //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)
                    
            }
            return productionOrder;
        }
       
    }
    
}
