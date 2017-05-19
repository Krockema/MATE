using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models.DB;
using Master40.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process();
        void ProcessDemand(IDemandToProvider demand);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            _context = context;
        }

        async Task IProcessMrp.Process()
        {
           await Task.Run(() => {
                
                var orderParts = _context.OrderParts.Where(a => a.IsPlanned == false).Include(a => a.Article);
                
                foreach (var orderPart in orderParts.ToList())
                {
                   ProcessDemand(CreateDemandOrderPart(orderPart));
                }
                var capacity = new CapacityScheduling(_context);
               capacity.GifflerThompsonScheduling();
               foreach (var orderPart in orderParts.ToList())
               {
                  orderPart.IsPlanned = true; 
               }
               
           });
        }

        public void ProcessDemand(IDemandToProvider demand)
        {
            //MRP I
            ExecutePlanning(demand, null);
            _context.SaveChanges();
            //MRP II 
            var schedule = new Scheduling(_context);
            schedule.BackwardScheduling(demand);
            schedule.ForwardScheduling(demand);
            schedule.SetActivitySlack(demand);
            demand.State = State.SchedulesExist;
            _context.Demands.Update((DemandToProvider)demand);
            _context.SaveChanges();
        }

        private void ExecutePlanning(IDemandToProvider demand, 
                                     IDemandToProvider parent)
        {
            IDemandForecast demandForecast = new DemandForecast(_context);
            IScheduling schedule = new Scheduling(_context);

            var productionOrder = demandForecast.NetRequirement(demand, parent);
            demand.State = State.ProviderExist;

            foreach (var log in demandForecast.Logger)
            {
                Logger.Add(log);
            }

            if (productionOrder == null)
            {
                //there was enough in stock, so this does not have to be produced
                return;
            }
            schedule.CreateSchedule(demand, productionOrder);
            var children = _context.ArticleBoms
                                .Include(a => a.ArticleChild)
                                .ThenInclude(a => a.ArticleBoms)
                                .Where(a => a.ArticleParentId == demand.ArticleId)
                                .ToList();
            if (!children.Any())
            {
                return;
            }
            foreach (var child in children)
            {
                ExecutePlanning(new DemandProductionOrderBom()
                {
                    ProductionOrderBomId = child.ArticleBomId,
                    ArticleId = child.ArticleChildId,
                    Article = child.ArticleChild,
                    Quantity = productionOrder.Quantity * (int) child.Quantity,
                    DemandRequesterId = demand.DemandRequesterId,
                    DemandProvider = new List<DemandToProvider>(),
                    State = State.Created
                }, demand);
            }
        }

        private IDemandToProvider CreateDemandOrderPart(OrderPart orderPart)
        {
            var demand = new DemandOrderPart()
            {
                OrderPartId = orderPart.OrderPartId,
                Quantity = orderPart.Quantity,
                Article = orderPart.Article,
                ArticleId = orderPart.ArticleId,
                OrderPart = orderPart,
                DemandProvider = new List<DemandToProvider>(),
                State = State.Created
            };
            _context.Demands.Add(demand);
            _context.SaveChanges();
            demand.DemandRequesterId = demand.DemandId;
            _context.Update(demand);
            _context.SaveChanges();
            return demand;
        }


    }


}