using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IDemandForecast
    {
        ProductionOrder NetRequirement(IDemandToProvider demand, IDemandToProvider parent, MrpTask task);
    }


    public class DemandForecast : IDemandForecast
    {
        private readonly ProductionDomainContext _context;
        private readonly IProcessMrp _processMrp;

        public DemandForecast(ProductionDomainContext context, IProcessMrp processMrp)
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
            var plannedStock = _context.GetPlannedStock(stock,demand);
            ProductionOrder productionOrder = null;
            _context.TryCreateStockReservation(stock, demand);
            //if the plannedStock is below zero, articles have to be produced for its negative amount 
            if (plannedStock < 0)
            {
                //if the article has no children it has to be purchased
                var children = _context.ArticleBoms.Where(a => a.ArticleParentId == demand.ArticleId).ToList();
                if (children.Any())
                {
                    productionOrder = _context.CreateChildProductionOrder(demand, parent, -plannedStock);
                    
                }
                else
                    _context.CreatePurchaseDemand(demand, -plannedStock);
            }
            //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
            if (stock.Min <= 0 || plannedStock >= stock.Min || demand.GetType() == typeof(DemandStock))
                return productionOrder;
            
            if (_context.Demands.OfType<DemandStock>()
                .Any(a => a.ArticleId == demand.ArticleId
                            && a.State != State.Produced
                            && a.State != State.Purchased))
                return productionOrder;

            var demandStock = plannedStock < 0 ? _context.CreateStockDemand(demand, stock, stock.Min) : _context.CreateStockDemand(demand, stock, stock.Min - plannedStock);
            //call processMrp to plan and schedule the stockDemand
            _processMrp.RunRequirementsAndTermination(demandStock, task);
            return productionOrder;
        }

        

        
        
    }
    
}
