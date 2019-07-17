using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.DB.Data.Context
{
    public class ProductionDomainContext : MasterDBContext
    {
        public ProductionDomainContext(DbContextOptions<MasterDBContext> options) : base(options) { }
        
        public T_CustomerOrder OrderById(int id)
        {
            return CustomerOrders.FirstOrDefault(x => x.Id == id);
        }
        public Task<List<SimulationWorkschedule>> GetFollowerProductionOrderWorkSchedules(SimulationWorkschedule simulationWorkSchedule, SimulationType type, List<SimulationWorkschedule> relevantItems)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<SimulationWorkschedule>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = relevantItems.Where(x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId)
                    .Max(x => x.HierarchyNumber);

                if (maxHierarchy == simulationWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article
                    priorItems.AddRange(relevantItems
                        .Where(x => x.ProductionOrderId == simulationWorkSchedule.ParentId
                                && x.HierarchyNumber == relevantItems
                                    .Where(w => w.ProductionOrderId == simulationWorkSchedule.ParentId)
                                    .Min(m => m.HierarchyNumber)));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        relevantItems.Where(
                                x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > simulationWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });
            return rs;
        }
        
        public T_CustomerOrder CreateNewOrder(int articleId, int amount, int creationTime, int dueTime)
        {
            var olist = new List<T_CustomerOrderPart>();
            T_CustomerOrderPart tCustomerOrderPart = new T_CustomerOrderPart();
            tCustomerOrderPart.ArticleId = articleId;
            tCustomerOrderPart.IsPlanned = false;
            tCustomerOrderPart.Quantity = amount;

            olist.Add(tCustomerOrderPart);

            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = BusinessPartners.First(x => x.Debitor).Id,
                DueTime = dueTime,
                CreationTime = creationTime,
                Name = Articles.Single(x => x.Id == articleId).Name,
                CustomerOrderParts = olist
            };
            return order;
        }

        public async Task<M_Article> GetArticleBomRecursive(M_Article article, int articleId)
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


        public int GetEarliestStart(ResultContext kpiContext, SimulationWorkschedule simulationWorkschedule, SimulationType simulationType, int simulationId,  List<SimulationWorkschedule> schedules = null)
        {
            if (simulationType == SimulationType.Central)
            {
                var orderId = simulationWorkschedule.OrderId.Replace("[", "").Replace("]", "");
                var start = kpiContext.SimulationOperations
                    .Where(x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                    .Where(a =>
                    a.OrderId.Equals("[" + orderId.ToString() + ",")
                    || a.OrderId.Equals("," + orderId.ToString() + "]")
                    || a.OrderId.Equals("[" + orderId.ToString() + "]")
                    || a.OrderId.Equals("," + orderId.ToString() + ",")).Min(b => b.Start);
                return start;
            }

            var children = new List<SimulationWorkschedule>();
            children = schedules.Where(x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                                .Where(a => a.ParentId.Equals(simulationWorkschedule.ProductionOrderId.ToString())).ToList();
            
            if (!children.Any()) return simulationWorkschedule.Start;
            var startTimes = children.Select(child => GetEarliestStart(kpiContext, child, simulationType, simulationId, schedules)).ToList();
            return startTimes.Min();
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
