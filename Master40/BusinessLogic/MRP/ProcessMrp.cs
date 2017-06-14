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
        void RunMrp(IDemandToProvider demand, MrpTask task);
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
                foreach (var orderPart in orderParts.ToList())
                {
                    var demandOrderParts =
                        _context.Demands.OfType<DemandOrderPart>().AsNoTracking().Include(a => a.DemandProvider).Where(a => a.OrderPartId == orderPart.Id).ToList();
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
                }
                
                if (task == MrpTask.All || task == MrpTask.GifflerThompson || task == MrpTask.Capacity)
                {
                    PlanCapacities(task);
                }
                foreach (var orderPart in orderParts)
                {
                    if (task == MrpTask.All || task == MrpTask.GifflerThompson)
                        orderPart.IsPlanned = true;
                }
                _context.SaveChanges();
            });
        }

        private void PlanCapacities(MrpTask task)
        {
            var demands = _context.Demands.Where(a =>
                               a.State == State.BackwardScheduleExists || a.State == State.ForwardScheduleExists ||
                               a.State == State.ExistsInCapacityPlan).ToList();
           
            var capacity = new CapacityScheduling(_context);
            List<MachineGroupProductionOrderWorkSchedule> machineList = null;
            //Todo: single only works when machineList gets written into DB, when its done take last part out
            if (task == MrpTask.All || task == MrpTask.Capacity || task == MrpTask.GifflerThompson)
            {
                machineList = capacity.CapacityRequirementsPlanning();
            }

            if ((capacity.CapacityLevelingCheck(machineList) && task == MrpTask.All) || task == MrpTask.GifflerThompson)
            {
                Logger.Add(new LogMessage()
                {
                    Message = "GT executed"
                });
                capacity.GifflerThompsonScheduling();
            }
            else

            {
                capacity.GifflerThompsonScheduling();/*
                foreach (var demand in demands)
                {
                    SetStartEndFromTermination(demand);
                }
                capacity.SetMachines();*/
            }
            foreach (var demand in demands)
            {
                demand.State = State.ExistsInCapacityPlan;
            }
            _context.Demands.UpdateRange(demands);
            _context.SaveChanges();
        }

        private void SetStartEndFromTermination(IDemandToProvider demand)
        {
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
            {
                ExecutePlanning(demand, null, task);
                demand.State = State.ProviderExist;
            }

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
                //Todo: backward-Scheduling mit fixem Endpunkt
            }
            _context.SaveChanges();
        }

        private bool CheckNeedForward(IDemandToProvider demand)
        {
            var demandProviderProductionOrders = _context.Demands.OfType<DemandProviderProductionOrder>()
                .Where(a => a.DemandRequesterId == demand.DemandRequesterId);
            if (demand.GetType() == typeof(DemandStock))
                return true;
            foreach (var demandProviderProductionOrder in demandProviderProductionOrders)
            {
                var schedules = _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder)
                    .Where(a => a.ProductionOrderId == demandProviderProductionOrder.ProductionOrderId);
                foreach (var schedule in schedules)
                {
                    if (schedule.StartBackward < 0) return true;
                }
            }
            return false;
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
        Capacity,
        GifflerThompson
    }


}