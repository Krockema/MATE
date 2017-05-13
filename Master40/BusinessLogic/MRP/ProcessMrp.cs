using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.Helper;
using Master40.Data;
using Master40.Models.DB;
using Master40.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process(int orderId);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            _context = context;
        }

        async Task IProcessMrp.Process(int orderId)
        {
           await Task.Run(() => {
                
                var order = _context.OrderParts.Where(a => a.OrderId == orderId);
                
                foreach (var orderPart in order.ToList())
                {
                    var manufacturingSchedule = new ManufacturingSchedule();
                    ExecutePlanning(null,null,null,order.First().OrderPartId);
                    //schedule.ForwardScheduling(manufacturingScheduleItem);
                    //schedule.CapacityScheduling();
                    _context.SaveChanges();
                }
           });
        }

        // gross, net requirement, create schedule, backward, forward, call children
        private void ExecutePlanning(IDemandToProvider demand, IDemandToProvider parent,IDemandToProvider demandRequester,int orderPartId)
        {
            var orderPart = _context.OrderParts.Include(a => a.Article).Single(a => a.OrderPartId == orderPartId);
            if (demand == null)
            {
                demand = new DemandOrderPart()
                {
                    OrderPartId = orderPartId,
                    Quantity = orderPart.Quantity,
                    Article = orderPart.Article,
                    ArticleId = orderPart.ArticleId,
                    OrderPart = orderPart,
                    IsProvided = false,
                    DemandProvider = new List<DemandToProvider>(),
                    
                };
                demandRequester = demand;

            }
            
            IDemandForecast demandForecast = new DemandForecast(_context);
            IScheduling schedule = new Scheduling(_context);
            var productionOrder = demandForecast.NetRequirement(demand, parent, orderPartId);
            foreach (var log in demandForecast.Logger)
            {
                Logger.Add(log);
            }
            
            if (productionOrder != null)
            {
                schedule.CreateSchedule(orderPartId, productionOrder);

                schedule.BackwardScheduling(productionOrder);
                
                var children =
                    _context.ArticleBoms.Include(a => a.ArticleChild).ThenInclude(a => a.ArticleBoms)
                        .Where(a => a.ArticleParentId == demand.ArticleId).ToList();
                if (children.Any())
                {
                    foreach (var child in children)
                    {
                        ExecutePlanning(new DemandProductionOrderBom()
                        {
                            ProductionOrderBomId = child.ArticleBomId,
                            ArticleId = child.ArticleChildId,
                            Article = child.ArticleChild,
                            Quantity = productionOrder.Quantity * (int) child.Quantity,
                            DemandRequesterId = demandRequester.DemandId,
                            DemandRequester = (DemandToProvider)demandRequester,
                            DemandProvider = new List<DemandToProvider>()
                        }, demand, demandRequester,orderPartId);
                    }
                }
            }
        }


    }


}