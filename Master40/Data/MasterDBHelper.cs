using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Data
{
    public class MasterDbHelper
    {
        public async static Task<Article> GetArticleBomRecursive(MasterDBContext context, Article article, int ArticleId)
        {
            article.ArticleChilds = context.ArticleBoms.Include(a => a.ArticleChild)
                                                        .ThenInclude(w => w.WorkSchedules)
                                                        .Where(a => a.ArticleParentId == ArticleId).ToList();

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(context, item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }


        public async static Task<ProductionOrder> GetProductionOrderBomRecursive(MasterDBContext context, ProductionOrder prodOrder, int productionOrderId)
        {
            prodOrder.ProdProductionOrderBomChilds = context.ProductionOrderBoms
                                                            .Include(a => a.ProductionOrderChild)
                                                            .ThenInclude(w => w.ProductionOrderWorkSchedule)
                                                            .Where(a => a.ProductionOrderParentId == productionOrderId).ToList();

            foreach (var item in prodOrder.ProdProductionOrderBomChilds)
            {
                await GetProductionOrderBomRecursive(context, item.ProductionOrderParent, item.ProductionOrderChildId);
            }
            await Task.Yield();
            return prodOrder;

        }

        /// <summary>
        /// copies am Article and his Childs to ProductionOrder
        /// Creates Demand Provider for Production oder and DemandRequests for childs
        /// </summary>
        /// <returns></returns>
        public static ProductionOrder CopyArticleToProductionOrder(MasterDBContext context, int articleId, decimal quantity, int demandRequesterId)
        {
            var article = context.Articles.Include(a => a.ArticleBoms).ThenInclude(c => c.ArticleChild).Single(a=> a.ArticleId == articleId);
            var mainProductionOrder = new ProductionOrder {
                ArticleId = article.ArticleId,
                Name = "Prod. Auftrag: " + article.Name,
                Quantity = quantity,
            };
            context.ProductionOrders.Add(mainProductionOrder);

            var demandProvider = new DemandProviderProductionOrder()
            {
                ProductionOrderId = mainProductionOrder.ProductionOrderId,
                Quantity = quantity,
                ArticleId = article.ArticleId,
                DemandRequesterId = demandRequesterId,
            };
            context.Demands.Add(demandProvider);

            foreach (var item in article.ArticleBoms)
            {
                var prodOrder = new ProductionOrder
                {
                    ArticleId = item.ArticleChildId,
                    Name = "Prod. Auftrag: " + article.Name,
                    Quantity = quantity * item.Quantity,
                };
                context.ProductionOrders.Add(prodOrder);



                var prodOrderBom = new ProductionOrderBom
                {
                    Quantity = quantity * item.Quantity,
                    ProductionOrderParentId = mainProductionOrder.ProductionOrderId,
                    ProductionOrderChildId = prodOrder.ProductionOrderId
                };
                context.ProductionOrderBoms.Add(prodOrderBom);

                var demandRequester = new DemandProductionOrderBom
                {
                    ProductionOrderBomId = prodOrderBom.ProductionOrderBomId,
                    Quantity = quantity,
                    ArticleId = item.ArticleChildId,
                    DemandRequesterId = demandProvider.DemandId, // nicht sicher
                };
                context.Demands.Add(demandRequester);

            }
            context.SaveChanges();

            return mainProductionOrder;
        }
    }
}
