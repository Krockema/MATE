using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class ProductionDomainContext : MasterDBContext
    {
        public ProductionDomainContext(DbContextOptions<MasterDBContext> options) : base(options) { }


        public Order OrderById(int id)
        {
            return Orders.Where(x => x.Id == id).FirstOrDefault();
        }
        /// <summary>
        /// Recives the prior items to a given ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="productionOrderWorkSchedule"></param>
        /// <returns>List<ProductionOrderWorkSchedule></returns>
        public Task<List<ProductionOrderWorkSchedule>> GetFollowerProductionOrderWorkSchedules(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<ProductionOrderWorkSchedule>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = ProductionOrderWorkSchedule.Where(x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId)
                                                    .Max(x => x.HierarchyNumber);

                if (maxHierarchy == productionOrderWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article
                    var priorBom = ProductionOrderBoms
                                                .Include(x => x.ProductionOrderParent)
                                                    .ThenInclude(x => x.ProductionOrderWorkSchedule)
                                                .Where(x => x.ProductionOrderChildId == productionOrderWorkSchedule.ProductionOrderId).ToList();

                    // out of each Part get Highest Workschedule
                    priorItems.AddRange(priorBom.Select(item => item.ProductionOrderParent.ProductionOrderWorkSchedule
                                                .OrderBy(x => x.HierarchyNumber).FirstOrDefault())
                                                .Where(prior => prior != null));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        ProductionOrderWorkSchedule.Where(
                                x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > productionOrderWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });

            return rs;
        }
        
        public async Task<Article> GetArticleBomRecursive(Article article, int articleId)
        {
            article.ArticleChilds = ArticleBoms.Include(a => a.ArticleChild)
                                                        .ThenInclude(w => w.WorkSchedules)
                                                        .Where(a => a.ArticleParentId == articleId).ToList();

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }


        public async Task<ProductionOrder> GetProductionOrderBomRecursive(ProductionOrder prodOrder, int productionOrderId)
        {
            prodOrder.ProdProductionOrderBomChilds = ProductionOrderBoms
                                                            .Include(a => a.ProductionOrderChild)
                                                            .ThenInclude(w => w.ProductionOrderWorkSchedule)
                                                            .Where(a => a.ProductionOrderParentId == productionOrderId).ToList();

            foreach (var item in prodOrder.ProdProductionOrderBomChilds)
            {
                await GetProductionOrderBomRecursive(item.ProductionOrderParent, item.ProductionOrderChildId);
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
            var article = context.Articles.Include(a => a.ArticleBoms).ThenInclude(c => c.ArticleChild).Single(a => a.Id == articleId);
            var mainProductionOrder = new ProductionOrder
            {
                ArticleId = article.Id,
                Name = "Prod. Auftrag: " + article.Name,
                Quantity = quantity,
            };
            context.ProductionOrders.Add(mainProductionOrder);

            var demandProvider = new DemandProviderProductionOrder()
            {
                ProductionOrderId = mainProductionOrder.Id,
                Quantity = quantity,
                ArticleId = article.Id,
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
                    ProductionOrderParentId = mainProductionOrder.Id,
                    ProductionOrderChildId = prodOrder.Id
                };
                context.ProductionOrderBoms.Add(prodOrderBom);

                var demandRequester = new DemandProductionOrderBom
                {
                    ProductionOrderBomId = prodOrderBom.Id,
                    Quantity = quantity,
                    ArticleId = item.ArticleChildId,
                    DemandRequesterId = demandProvider.Id, // nicht sicher
                };
                context.Demands.Add(demandRequester);

            }
            context.SaveChanges();

            return mainProductionOrder;
        }

        public bool ProductionOrderWorkScheduleIsLowestHierarchy(SimulationProductionOrderWorkSchedule spows)
        {
            var powsList = (from all in ProductionOrderWorkSchedule
                         where (all.ProductionOrderId == spows.ProductionOrderId)
                         select all).AsNoTracking().ToList();
            return powsList.All(t => t.HierarchyNumber >= spows.HierarchyNumber);
        }

        public bool ProductionOrderHasChildren(SimulationProductionOrderWorkSchedule spows)
        {
            return ProductionOrderBoms.Any(a => a.ProductionOrderParentId == spows.ProductionOrderId);
        }
    
        public SimulationProductionOrderWorkSchedule SimulationProductionOrderWorkScheduleGetParent(SimulationProductionOrderWorkSchedule spows, int simulationId)
        {
            return SimulationProductionOrderWorkScheduleGetHierarchyParent(spows, simulationId) ?? SimulationProductionOrderWorkScheduleGetBomParent(spows,simulationId);
        }

        public SimulationProductionOrderWorkSchedule SimulationProductionOrderWorkScheduleGetHierarchyParent(
            SimulationProductionOrderWorkSchedule spows, int simulationId)
        {
            var powsList = (from all in ProductionOrderWorkSchedule
                            where all.ProductionOrderId == spows.ProductionOrderId
                            select all).AsNoTracking().ToList();
            var hierarchyParents =
                (from hierarchyPows in powsList
                where (hierarchyPows.HierarchyNumber > spows.HierarchyNumber)
                select hierarchyPows).ToList();
            if (!hierarchyParents.Any())
                return null;
            var pows = hierarchyParents.OrderBy(i => i.HierarchyNumber).First();
            if (pows == null)
                return null;
            var parentSpows = SimulationProductionOrderWorkSchedules.Where(
                a => a.ProductionOrderId == pows.ProductionOrderId && a.HierarchyNumber == pows.HierarchyNumber && a.SimulationId == simulationId).ToList();
            return parentSpows.Any() ? parentSpows.First() : null;
        }

        public SimulationProductionOrderWorkSchedule SimulationProductionOrderWorkScheduleGetBomParent(SimulationProductionOrderWorkSchedule spows, int simulationId)
        {
            var bom = ProductionOrderBoms.AsNoTracking().Include(a => a.ProductionOrderParent).ThenInclude(a => a.ProductionOrderWorkSchedule).Where(a => a.ProductionOrderChildId == spows.ProductionOrderId);
            if (!bom.Any()) return null;
            var pows = bom.First().ProductionOrderParent.ProductionOrderWorkSchedule.ToList();
            if (!pows.Any()) return null;
            var singlePows = pows.Find(a => a.HierarchyNumber == pows.Min(b => b.HierarchyNumber));
            return SimulationProductionOrderWorkSchedules.Single(a => a.ProductionOrderId == singlePows.ProductionOrderId && a.HierarchyNumber == singlePows.HierarchyNumber && a.SimulationId == simulationId) ?? null;
        }
        
        public IDemandToProvider GetDemand(SimulationProductionOrderWorkSchedule sPows)
        {
            var po = ProductionOrders.Include(a => a.DemandProviderProductionOrders).ThenInclude(a => a.DemandRequester).ThenInclude(a => a.DemandRequester).Single(a => a.Id == sPows.ProductionOrderId);
            return po.DemandProviderProductionOrders.First().DemandRequester.DemandRequester;
        }

        public int GetLatestEndFromChild(SimulationProductionOrderWorkSchedule spows)
        {
            var bom = ProductionOrderBoms.AsNoTracking().Include(a => a.ProductionOrderChild).ThenInclude(a => a.ProductionOrderWorkSchedule).Where(a => a.ProductionOrderParentId == spows.ProductionOrderId);
            if (!bom.Any()) return 0;
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleBom in bom)
            {
                pows.AddRange(singleBom.ProductionOrderChild.ProductionOrderWorkSchedule.ToList());
            }
            return pows.Any() ? (from child in pows select child.End).Concat(new[] { 0 }).Max() : 0;
        }
        /// <summary>
        /// returns the Production Order Work Schedules for a given Order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Task<List<ProductionOrderWorkSchedule>> GetProductionOrderWorkScheduleByOrderId(int orderId)
        {
            return Task.Run(() =>
            {
                // get the corresponding Order Parts to Order
                var demands = Demands.OfType<DemandOrderPart>()
                    .Include(x => x.OrderPart)
                    .Where(o => o.OrderPart.OrderId == orderId)
                    .ToList();

                // ReSharper Linq
                var demandboms = demands.SelectMany(demand => Demands.OfType<DemandProductionOrderBom>()
                    .Where(a => a.DemandRequesterId == demand.Id)).ToList();

                // get Demand Providers for this Order
                var demandProviders = new List<DemandProviderProductionOrder>();
                foreach (var order in (Demands.OfType<DemandProviderProductionOrder>()
                    .Join(demands, c => c.DemandRequesterId, d => ((IDemandToProvider)d).Id, (c, d) => c)))
                {
                    demandProviders.Add(order);
                }

                var demandBomProviders = (Demands.OfType<DemandProviderProductionOrder>()
                    .Join(demandboms, c => c.DemandRequesterId, d => d.Id, (c, d) => c)).ToList();

                // get ProductionOrderWorkSchedule for 
                var pows = ProductionOrderWorkSchedule.Include(x => x.ProductionOrder).Include(x => x.Machine).Include(x => x.MachineGroup).AsNoTracking();
                var powDetails = (pows.Join(demandProviders, p => p.ProductionOrderId, dp => dp.ProductionOrderId,
                    (p, dp) => p)).ToList();

                var powBoms = (from p in pows
                    join dbp in demandBomProviders on p.ProductionOrderId equals dbp.ProductionOrderId
                    select p).ToList();

                powDetails.AddRange(powBoms);
                return powDetails;
            });
        }

        /// <summary>
        /// returns the OrderId for the ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        private int GetOrderIdFromProductionOrderWorkSchedule(ProductionOrderWorkSchedule pow)
        {
            //call requester.requester to make sure that also the DemandProductionOrderBoms find the DemandOrderPart
            var requester = (DemandOrderPart)pow.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequester;
            return OrderParts.Single(a => a.Id == requester.OrderPartId).OrderId;
        }
    }
}
