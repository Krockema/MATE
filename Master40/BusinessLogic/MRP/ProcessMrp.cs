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
                    //MRP I
                    ExecutePlanning(null,null,null,orderPart);
                    _context.SaveChanges();
                    //MRP II 
                    var schedule = new Scheduling(_context);
                    schedule.BackwardScheduling(orderPart);
                    schedule.ForwardScheduling(orderPart);
                    //schedule.CapacityScheduling();
               }
           });
        }
        
        private void ExecutePlanning(IDemandToProvider demand, 
                                     IDemandToProvider parent,
                                     IDemandToProvider demandRequester,
                                     OrderPart orderPart)
        {
            //checks if the method is called for the first time
            if (demand == null)
            {
                demand = CreateDemandOrderPart(orderPart);
                demandRequester = demand;
            }
            IDemandForecast demandForecast = new DemandForecast(_context);
            IScheduling schedule = new Scheduling(_context);

            var productionOrder = demandForecast.NetRequirement(demand, parent, orderPart.OrderPartId);
            foreach (var log in demandForecast.Logger)
            {
                Logger.Add(log);
            }

            if (productionOrder == null)
            {
                //there was enough in stock, so this does not have to be produced
                return;
            }
            schedule.CreateSchedule(orderPart.OrderPartId, productionOrder);
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
                    DemandRequesterId = demandRequester.DemandId,
                    DemandRequester = (DemandToProvider)demandRequester,
                    DemandProvider = new List<DemandToProvider>()
                }, demand, demandRequester,orderPart);
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
                IsProvided = false,
                DemandProvider = new List<DemandToProvider>(),

            };
            _context.Demands.Add(demand);
            _context.SaveChanges();
            demand.DemandRequesterId = demand.DemandId;
            _context.Update(demand);
            return demand;
        }


    }


}