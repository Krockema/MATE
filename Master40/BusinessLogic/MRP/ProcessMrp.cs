using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Models;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Models;
using Master40.DB.Data.Context;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task CreateAndProcessOrderDemand(MrpTask task);
        void ProcessDemand(IDemandToProvider demand, MrpTask task);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            _context = context;
        }

        async Task IProcessMrp.CreateAndProcessOrderDemand(MrpTask task)
        {
            await Task.Run(() =>
            {
                var orderParts = _context.OrderParts.Where(a => a.IsPlanned == false).Include(a => a.Article).ToList();
                if (task != MrpTask.GifflerThompson)
                {
                    foreach (var orderPart in orderParts.ToList())
                    {
                        var demandOrderParts =
                            _context.Demands.OfType<DemandOrderPart>().Where(a => a.OrderPartId == orderPart.Id);
                        IDemandToProvider demand;
                        if (demandOrderParts.Any())
                        {
                            demand = demandOrderParts.First();
                        }
                        else
                        {
                            demand = CreateDemandOrderPart(orderPart);
                            _context.SaveChanges();
                        }
                        RunMrp(demand, task);
                        orderPart.IsPlanned = true;
                    }
                }
                if (task == MrpTask.All || task == MrpTask.GifflerThompson)
                {
                    var capacity = new CapacityScheduling(_context);
                    var machineList = capacity.CapacityPlanning();
                    if (capacity.CapacityLevelingNeeded(machineList))
                        capacity.GifflerThompsonScheduling();
                    else
                    {
                        foreach (var orderPart in orderParts)
                        {
                            SetStartEndFromTermination(orderPart);
                        }
                    }
                }
                   
                
            });
        }

        private void SetStartEndFromTermination(OrderPart orderPart)
        {
            var demand = _context.Demands.OfType<DemandOrderPart>().Single(a => a.OrderPartId == orderPart.Id);
            var schedules = _context.ProductionOrderWorkSchedule
                .Include(a => a.ProductionOrder)
                .ThenInclude(a => a.DemandProviderProductionOrders)
                .ThenInclude(a => a.DemandRequester)
                .Where(a => a.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequesterId == demand.Id)
                .ToList();
            foreach (var schedule in schedules)
            {
                if (schedule.EndForward - schedule.StartForward > 0)
                {
                    schedule.End = schedule.EndForward;
                    schedule.Start = schedule.StartForward;
                }
                else
                {
                    schedule.End = schedule.EndBackward;
                    schedule.Start = schedule.StartBackward;
                }
                _context.ProductionOrderWorkSchedule.Update(schedule);
                _context.SaveChanges();
            }
        }

        public void RunMrp(IDemandToProvider demand, MrpTask task)
        {
            if (demand.State == State.Created)
                ExecutePlanning(demand, null, task);
            ProcessDemand(demand, task);
    
           

            
            _context.SaveChanges();
        }

        public void ProcessDemand(IDemandToProvider demand, MrpTask task)
        { 
            var schedule = new Scheduling(_context);
            if (task == MrpTask.All || task == MrpTask.Backward)
            {
                schedule.BackwardScheduling(demand);
                demand.State = State.BackwardScheduleExists;
            }
            
            if ((task == MrpTask.All && CheckNeedForward(demand)) || task == MrpTask.Forward)
            {
                schedule.ForwardScheduling(demand);
                demand.State = State.ForwardScheduleExists;
            }
                
            _context.Demands.Update((DemandToProvider)demand);
            _context.SaveChanges();
        }

        private bool CheckNeedForward(IDemandToProvider demand)
        {
            var forwardNecessary = false;
            var demandProviderProductionOrders = _context.Demands.OfType<DemandProviderProductionOrder>()
                .Where(a => a.DemandRequesterId == demand.DemandRequesterId);
            foreach (var demandProviderProductionOrder in demandProviderProductionOrders)
            {
                var schedules = _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder)
                    .Where(a => a.ProductionOrderId == demandProviderProductionOrder.ProductionOrderId);
                foreach (var schedule in schedules)
                {
                    if (schedule.StartBackward < 0) forwardNecessary = true;
                }
            }
            return forwardNecessary;
        }

        private void ExecutePlanning(IDemandToProvider demand, 
                                     IDemandToProvider parent, MrpTask task)
        {
            IDemandForecast demandForecast = new DemandForecast(_context, this);
            IScheduling schedule = new Scheduling(_context);

            var productionOrder = demandForecast.NetRequirement(demand, parent, task);
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
                    ArticleId = child.ArticleChildId,
                    Article = child.ArticleChild,
                    Quantity = productionOrder.Quantity * (int)child.Quantity,
                    DemandRequesterId = demand.DemandRequesterId,
                    DemandProvider = new List<DemandToProvider>(),
                    State = State.Created
                }, demand, task);
            }
        }

        private IDemandToProvider CreateDemandOrderPart(OrderPart orderPart)
        {
            var demand = new DemandOrderPart()
            {
                OrderPartId = orderPart.Id,
                Quantity = orderPart.Quantity,
                Article = orderPart.Article,
                ArticleId = orderPart.ArticleId,
                OrderPart = orderPart,
                DemandProvider = new List<DemandToProvider>(),
                State = State.Created
            };
            _context.Demands.Add(demand);
            _context.SaveChanges();
            demand.DemandRequesterId = demand.Id;
            _context.Update(demand);
            _context.SaveChanges();
            return demand;
        }


    }

    public enum MrpTask
    {
        All,
        Forward,
        Backward,
        GifflerThompson
    }


}