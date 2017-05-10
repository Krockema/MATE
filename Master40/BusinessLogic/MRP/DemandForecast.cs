using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using Master40.Models;

namespace Master40.BusinessLogic.MRP
{
    public interface IDemandForecast
    {
        Task<List<ArticleBomItem>> GrossRequirement(int orderId);
        Task<List<ProductionOrder>> NetRequirement(List<ArticleBomItem> articles);
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
        /// Resolves Bom to return a list of every material needed and the correct amount
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>List</returns>
        async Task<List<ArticleBomItem>> IDemandForecast.GrossRequirement(int orderId)
        {
            var needs = new List<ArticleBomItem>();
            await Task.Run(() => {
                //get Orderparts from Order
                var parts = _context.OrderParts.AsNoTracking()
                    .Include(a => a.Article)
                    .Where(a => a.OrderId == orderId);
                //for every orderPart get its bom
                foreach (var part in parts)
                {
                    var msg = "Articles ordered: " +
                              _context.Articles.AsNoTracking().Single(a => a.ArticleId == part.ArticleId).Name + " " +
                              part.Quantity;
                    Logger.Add(new LogMessage() {MessageType = MessageType.success, Message = msg});

                    //get bom for every orderpart
                    var bomItems = _context.ArticleBoms.AsNoTracking()
                        .Include(a => a.ArticleBomItems)
                        .Single(a => a.ArticleId == part.ArticleId)
                        .ArticleBomItems
                        .ToList();

                    //manually add ordered Item, because its head of the bom
                    //needs.Add(_context.ArticleBomItems.AsNoTracking().Include(a => a.ArticleBom).Include(a => a.Article).Single(a => a.ArticleId == part.ArticleId));
                    var article = _context.Articles.AsNoTracking().Single(a => a.ArticleId == part.ArticleId);
                    needs.Add(new ArticleBomItem()
                    {
                        ArticleId = article.ArticleId,
                        Article = article,
                        ArticleBom = null,
                        Quantity = part.Quantity,
                        Name = article.Name

                    });
                    //recursively going through bom to list every attached article
                    GetNeeds(ref needs, bomItems, part.Quantity);
                }
               
            });
            return needs;
        }

        private void GetNeeds(ref List<ArticleBomItem> needs, List<ArticleBomItem> bomItems,  decimal multiplier)
        {
            //Iterate through boms to add them to the list "needs"
            foreach (var bomItem in bomItems)
            {
                //multiply Quantity with the multiplier which consists of the Quantity of the parent article
                var tempBomItem = bomItem;
                tempBomItem.Quantity *= multiplier;
                needs.Add(tempBomItem);

                var bom = _context.ArticleBoms.AsNoTracking().Include(a => a.ArticleBomItems).Where(a => a.ArticleId == bomItem.ArticleId);


                if (bom.Any()) 
                {
                    //If the article has a child recursively call this method to resolve its bom
                    
                    GetNeeds(ref needs, bom.First().ArticleBomItems.ToList(), bomItem.Quantity);
                }
            }
        }
  
        /// <summary>
        /// Uses List from NetRequirements to start productionorders if there are not enough materials in stock
        /// </summary>
        /// <param name="needs">List</param>
        async Task<List<ProductionOrder>> IDemandForecast.NetRequirement(List<ArticleBomItem> needs)
        {
            List<ProductionOrder> productionOrders = new List<ProductionOrder>();
            await Task.Run(() => {
             
                //Iterate through needs-List from the method NetRequirements
                foreach (var need in needs)
                {
                    //get the actual item from db
                    var article = _context.Articles.AsNoTracking().Include(a => a.Stock).Single(a => a.ArticleId == need.ArticleId);
                
                    //plannedStock is the amount of this article in stock after taking out the amount needed
                    var plannedStock = article.Stock.Current - need.Quantity;

                    //if there is at least one or more of this article in stock
                    if (article.Stock.Current > 0)
                    {
                        var amount = need.Quantity - article.Stock.Current;
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
                        productionOrders.Add(new ProductionOrder()
                        {
                            Article = need.Article,
                            ArticleId = need.ArticleId,
                            Quantity = need.Quantity


                        });
                        //Set PlannedStock to zero because the rest will already be produced
                        plannedStock = 0;
                    }

                    //if the plannedStock goes below the Minimum for this article, start a productionOrder for this article until max is reached
                    if (plannedStock < article.Stock.Min)
                        //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)
                        ;
                }
                /*foreach (var need in needs)
                {
                    if (need.Quantity > 0)
                    {
                        var msg = "Articles in the needs-list: " + need.Name + " " + need.Quantity.ToString("#.##");
                        Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
                    }
                }*/

            });
            return productionOrders;
        }

        private void DeleteChildren(ref List<ArticleBomItem> needs, ArticleBomItem bomNeed, decimal amount)
        {
            foreach (var need in needs)
            {
                if (need.ArticleBomId == bomNeed.ArticleBomId)
                {
                    if (bomNeed.ArticleBom != null)
                    {
                        //recursively call this method for the children
                        DeleteChildren(ref needs, need, need.Quantity);
                        //Change Quantity for how many articles are in stock
                        //substract the amount of not needed items * the amount of items needed for one head-article
                    }
                    if (need.ArticleBom != null)
                        needs[needs.IndexOf(need)].Quantity -= amount *
                                                               _context.ArticleBomItems.AsNoTracking()
                                                                   .Single(a => a.ArticleId == need.ArticleId)
                                                                   .Quantity;
                    else
                        needs[needs.IndexOf(need)].Quantity -= amount;
                }   
            }
        }
    }
    
}
