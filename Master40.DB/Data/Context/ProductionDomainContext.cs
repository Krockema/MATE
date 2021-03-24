﻿using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Helper;

namespace Master40.DB.Data.Context
{
    public class ProductionDomainContext : MasterDBContext
    {
        public ProductionDomainContext(DbContextOptions<MasterDBContext> options) : base(options: options) { }

        public new static ProductionDomainContext GetContext(string connectionString)
        {
            return new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(connectionString: connectionString)
                .Options);
        }

        public void ClearCustomerOrders()
        {
            this.Database.ExecuteSqlRaw("Delete from T_CustomerOrderPart");
            this.Database.ExecuteSqlRaw("Delete from T_CustomerOrder");
        }

        public T_CustomerOrder OrderById(int id)
        {
            return CustomerOrders.FirstOrDefault(predicate: x => x.Id == id);
        }

        public List<M_Article> GetProducts()
        {
            return this.Articles.Include(x => x.ArticleType)
                .Where(predicate: b => b.ArticleType.Name == "Product")
                .ToList();
        }

        public Task<List<Job>> GetFollowerProductionOrderWorkSchedules(Job simulationWorkSchedule, SimulationType type, List<Job> relevantItems)
        {
            var rs = Task.Run(function: () =>
            {
                var priorItems = new List<Job>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = relevantItems.Where(predicate: x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId)
                    .Max(selector: x => x.HierarchyNumber);

                if (maxHierarchy == simulationWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article
                    priorItems.AddRange(collection: relevantItems
                        .Where(predicate: x => x.ProductionOrderId == simulationWorkSchedule.ParentId
                                && x.HierarchyNumber == relevantItems
                                    .Where(predicate: w => w.ProductionOrderId == simulationWorkSchedule.ParentId)
                                    .Min(selector: m => m.HierarchyNumber)));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        relevantItems.Where(
                                predicate: x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > simulationWorkSchedule.HierarchyNumber)
                            .OrderBy(keySelector: x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(item: previousPows);
                }
                return priorItems;
            });
            return rs;
        }

        public List<M_ArticleBom> GetProductBoms()
        {
            return this.ArticleBoms
                        .Where(predicate: b => b.ArticleParentId == null)
                            .Include(c => c.ArticleChild)
                        .ToList();
        }

        /// <summary>
        /// To Try with DB Context
        /// FOR THE GANTTPLAN!!!111
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="amount"></param>
        /// <param name="creationTime"></param>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        public T_CustomerOrder CreateNewOrder(int orderid, int orderpartid, int articleId, int amount, long creationTime, long dueTime)
        {
            var olist = new List<T_CustomerOrderPart>();
            olist.Add(item: new T_CustomerOrderPart
            {
                Id = orderpartid,
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
            });

            var bp = BusinessPartners.First(predicate: x => x.Debitor);
            var order = new T_CustomerOrder()
            {
                Id = orderid,
                BusinessPartnerId = bp.Id,
                DueTime = (int)dueTime,
                CreationTime = (int)creationTime,
                Name = Articles.Single(predicate: x => x.Id == articleId).Name,
                CustomerOrderParts = olist
            };

            this.CustomerOrders.Add(entity: order);
            return order;
        }

        public T_CustomerOrder CreateNewOrder(int articleId, int amount, long creationTime, long dueTime)
        {
            var article = Articles.Single(x => x.Id == articleId);
            var olist = new List<T_CustomerOrderPart>();
            var orderPart = new T_CustomerOrderPart
            {
                Article = article,
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
            };
            olist.Add(item: orderPart);

            var bp = BusinessPartners.First(predicate: x => x.Debitor);
            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = bp.Id,
                DueTime = (int)dueTime,
                CreationTime = (int)creationTime,
                DueDateTime = dueTime.ToDateTime(),
                Name = Articles.Single(predicate: x => x.Id == articleId).Name,
                CustomerOrderParts = olist
            };
            return order;
        }

        public async Task<M_Article> GetArticleBomRecursive(M_Article article, int articleId)
        {
            article.ArticleChilds = ArticleBoms.Include(navigationPropertyPath: a => a.ArticleChild)
                .ThenInclude(navigationPropertyPath: w => w.Operations)
                .Where(predicate: a => a.ArticleParentId == articleId).ToList();

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(article: item.ArticleParent, articleId: item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }

        public long GetEarliestStart(ResultContext kpiContext, Job simulationWorkschedule, SimulationType simulationType, int simulationId,  List<Job> schedules = null)
        {
            if (simulationType == SimulationType.Central)
            {
                var orderId = simulationWorkschedule.OrderId.Replace(oldValue: "[", newValue: "").Replace(oldValue: "]", newValue: "");
                var start = kpiContext.SimulationJobs
                    .Where(predicate: x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                    .Where(predicate: a =>
                    a.OrderId.Equals("[" + orderId.ToString() + ",")
                    || a.OrderId.Equals("," + orderId.ToString() + "]")
                    || a.OrderId.Equals("[" + orderId.ToString() + "]")
                    || a.OrderId.Equals("," + orderId.ToString() + ",")).Min(selector: b => b.Start);
                return start;
            }

            var children = new List<Job>();
            children = schedules.Where(predicate: x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                                .Where(predicate: a => a.ParentId.Equals(value: simulationWorkschedule.ProductionOrderId.ToString())).ToList();
            
            if (!children.Any()) return simulationWorkschedule.Start;
            var startTimes = children.Select(selector: child => GetEarliestStart(kpiContext: kpiContext, simulationWorkschedule: child, simulationType: simulationType, simulationId: simulationId, schedules: schedules)).ToList();
            return startTimes.Min();
        }
    }
}
