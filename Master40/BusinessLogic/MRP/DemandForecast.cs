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
        List<ArticleBom> GrossRequirement(int orderId);
        void NetRequirement(List<ArticleBom> articles);
        List<LogMessage> Logger { get; set; }
    }

    /*
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
        /// Resolves Bom to return a list of every material needed and the correct amount
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>List</returns>
        List<ArticleBom> IDemandForecast.GrossRequirement(int orderId)
        {
            var needs = new List<ArticleBom>();
            //get Order from Database
            var orders = _context.Orders.Include(a => a.OrderParts).Where(a => a.OrderId == orderId);
            
            //for every orderPart get its bom
            foreach (var order in orders)
            {
                System.Diagnostics.Debug.WriteLine(order.Name, order.OrderId);

                //get Orderparts from Order
                var parts = _context.OrderParts.AsNoTracking()
                    .Include(a => a.Article)
                    .Where(a => a.OrderId == orderId);

                foreach (var part in parts)
                {
                    var msg = "Articles ordered: " +_context.Articles.AsNoTracking().Single(a=>a.ArticleId==part.ArticleId).Name + " " + part.Quantity;
                    Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
                    
                    //get bom for every orderpart
                    var boms = _context.ArticleBoms.AsNoTracking()
                        .Include(a => a.ArticleBomItems)
                        .ThenInclude(a => a.Article)
                        .Where(a => a.ArticleId == part.ArticleId)
                        .ToList();
                    
                    //manually add ordered Item, because its head of the bom
                    needs.Add(_context.ArticleBom.AsNoTracking().Single(a => a.ArticleBomId == 1));
                    //multiply Quantity of the material with the amount ordered
                    needs[0].Quantity *= part.Quantity;
                    //recursively going through bom to list every attached article
                    GetNeeds(ref needs, boms, part.Quantity);
                    
                }
            }
            return needs;
        }

        private void GetNeeds(ref List<ArticleBom> needs, IEnumerable<ArticleBom> boms,  decimal multiplier)
        {
            //Iterate through boms to add them to the list "needs"
            foreach (var bom in boms)
            {
                //multiply Quantity with the multiplier which consists of the Quantity of the parent article
                var tempBom = bom;
                tempBom.Quantity *= multiplier;
                needs.Add(tempBom);

                if (bom.ArticleChildId != null)
                {
                    //If the article has a child recursively call this method to resolve its bom
                    GetNeeds(ref needs, _context.ArticleBoms.AsNoTracking().Where(a => a.ArticleParentId == bom.ArticleChildId), bom.Quantity);
                }
            }
        }
  
        /// <summary>
        /// Uses List from NetRequirements to start productionorders if there are not enough materials in stock
        /// </summary>
        /// <param name="needs">List</param>
        void IDemandForecast.NetRequirement(List<ArticleBom> needs)
        {
            //Iterate through needs-List from the method NetRequirements
            foreach (var need in needs)
            {
                //get the actual item from db
                var tempNeed = _context.ArticleBoms.AsNoTracking()
                    .Include(a => a.ArticleChild)
                    .Include(a => a.ArticleChild.Stock)
                    .Single(a => a.ArticleBomId == need.ArticleBomId);
                
                //plannedStock is the amount of this article in stock after taking out the amount needed
                var plannedStock = tempNeed.ArticleChild.Stock.Current - need.Quantity;

                //if there is at least one or more of this article in stock
                if (tempNeed.ArticleChild.Stock.Current > 0)
                {
                    var amount = need.Quantity - tempNeed.ArticleChild.Stock.Current;
                    if (amount < 0) amount = need.Quantity;

                    //Delete/update the children of this article
                    //Example: if 2 "Kipper" are in stock and 5 are required, 3 have to be produced
                    //so the needs-List needs to be updated for the "Kipper" and all its children
                    DeleteChildren(ref needs,need, amount);
                }

                //if the plannedStock is below zero articles have to be produced    
                if (plannedStock < 0)
                {
                    var msg = "Articles ordered to produce: " + need.Name + " " + need.Quantity.ToString("#.##");
                    Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
                    //TODO: implement productionOrder with orderId for -PlannedStock
                    //Set PlannedStock to zero because the rest will already be produced
                    plannedStock = 0;
                }

                //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                if (plannedStock < tempNeed.ArticleChild.Stock.Min)
                //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)
                { }
            }
            foreach (var need in needs)
            {
                if (need.Quantity > 0)
                {
                    var msg = "Articles in the needs-list: " + need.Name + " " + need.Quantity.ToString("#.##");
                    Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
                }
            }
        }

        private void DeleteChildren(ref List<ArticleBom> needs, ArticleBom bomNeed, decimal amount)
        {
            foreach (var need in needs)
            {
                //ParentId == null means its the head-article
                if (need.ArticleParentId != null && need.ArticleParentId == bomNeed.ArticleChildId)
                {
                    if (need.ArticleChildId != null)
                        //recursively call this method for the children
                        DeleteChildren(ref needs, need, need.Quantity);
                    //Change Quantity for how many articles are in stock
                    needs[needs.IndexOf(need)].Quantity-=amount*_context.ArticleBoms.AsNoTracking().Single(a => a.ArticleChildId == need.ArticleChildId).Quantity;
                }
            }
        }
    }
    */
}
