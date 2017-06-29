using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;
using Master40.BusinessLogicCentral.HelperCapacityPlanning;
using Master40.DB.Data.Helper;
using Master40.MessageSystem.SignalR;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IProcessMrp
    {
        Task CreateAndProcessOrderDemand(MrpTask task);
        void RunRequirementsAndTermination(IDemandToProvider demand, MrpTask task);
        void PlanCapacities(MrpTask task, int timer);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MessageHub _messageHub;
        private readonly MasterDBContext _context;
        private readonly IScheduling _scheduling;
        private readonly IDemandForecast _demandForecast;
        private readonly ICapacityScheduling _capacityScheduling;
        public ProcessMrp(MasterDBContext context, IScheduling scheduling, ICapacityScheduling capacityScheduling, MessageHub messageHub)
        {
            _messageHub = messageHub;
            _context = context;
            _scheduling = scheduling;
            _capacityScheduling = capacityScheduling;
            _demandForecast = new DemandForecast(context,this);
        }

        /// <summary>
        /// Plans all unplanned Orders with MRP I + II
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task CreateAndProcessOrderDemand(MrpTask task)
        {
            
            await Task.Run(() =>
            {
                _messageHub.SendToAllClients("Start full cycle...", MessageType.info);
                //get all unplanned orderparts and iterate through them for MRP
                var orderParts = _context.OrderParts.Where(a => a.IsPlanned == false).Include(a => a.Article).ToList();
                foreach (var orderPart in orderParts.ToList())
                {
                    var demand = GetDemand(orderPart);
                    //run the requirements planning and backward/forward termination algorithm
                    RunRequirementsAndTermination(demand, task);
                }
                
                if (task == MrpTask.All || task == MrpTask.GifflerThompson || task == MrpTask.Capacity)
                {
                    //run the capacity algorithm
                    PlanCapacities(task, 0);
                }
                //set all orderparts to be planned
                foreach (var orderPart in orderParts)
                {
                    if (task == MrpTask.All || task == MrpTask.GifflerThompson)
                        orderPart.IsPlanned = true;
                }
                _context.SaveChanges();
                _messageHub.EndScheduler();
            });
            
        }

        /// <summary>
        /// Check if a demand exists for the orderpart, else a new one is created.
        /// </summary>
        /// <param name="orderPart"></param>
        /// <returns></returns>
        private IDemandToProvider GetDemand(OrderPart orderPart)
        {
            var demandOrderParts =
                        _context.Demands.OfType<DemandOrderPart>().Include(a => a.DemandProvider).Where(a => a.OrderPartId == orderPart.Id).ToList();
            IDemandToProvider demand;
            if (demandOrderParts.Any())
                demand = demandOrderParts.First();
            else
            {
                demand = CreateDemandOrderPart(orderPart);
                _context.SaveChanges();
            }
            return demand;
        }

        /// <summary>
        /// Plans capacities for all demands which are either already planned or just finished with MRP. Timer != 0 is used in a Simulation.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timer"></param>
        public void PlanCapacities(MrpTask task, int timer)
        {
            var demands = _context.Demands.Where(a =>
                               a.State == State.BackwardScheduleExists || 
                               a.State == State.ForwardScheduleExists ||
                               a.State == State.ExistsInCapacityPlan).ToList();
           
            List<MachineGroupProductionOrderWorkSchedule> machineList = null;

            if (timer > 0)
                _capacityScheduling.RebuildNets(timer);

            if (task == MrpTask.All || task == MrpTask.Capacity)
                //creates a list with the needed capacities to follow the terminated schedules
                machineList = _capacityScheduling.CapacityRequirementsPlanning(timer);
            
            if (task == MrpTask.GifflerThompson || (_capacityScheduling.CapacityLevelingCheck(machineList) && task == MrpTask.All))
                _capacityScheduling.GifflerThompsonScheduling(timer);
            else
            {
                foreach (var demand in demands)
                    SetStartEndFromTermination(demand);
                
                _capacityScheduling.SetMachines(timer);
            }
            foreach (var demand in demands)
            {
                demand.State = State.ExistsInCapacityPlan;
            }
            _context.Demands.UpdateRange(demands);
            _context.SaveChanges();
        }
        
        private void SetStartEndFromTermination(IDemandToProvider demand)
        { //Todo: replace provider.first()
            var schedules = _context.ProductionOrderWorkSchedule
                .Include(a => a.ProductionOrder)
                .ThenInclude(a => a.DemandProviderProductionOrders)
                .ThenInclude(a => a.DemandRequester)
                .Where(a => a.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequesterId == demand.Id)
                .ToList();
            foreach (var schedule in schedules)
            {
                //if forward was calculated take forward, else take from backward termination
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

        /// <summary>
        /// Run requirements planning and backward/forward termination.
        /// </summary>
        /// <param name="demand"></param>
        /// <param name="task"></param>
        public void RunRequirementsAndTermination(IDemandToProvider demand, MrpTask task)
        {
            if (demand.State == State.Created)
            {
                ExecutePlanning(demand, null, task);
                demand.State = State.ProviderExist;
            }
            
            if (task == MrpTask.All || task == MrpTask.Backward)
            {
                _scheduling.BackwardScheduling(demand);
                demand.State = State.BackwardScheduleExists;
            }

            if ((task == MrpTask.All && CheckNeedForward(demand)) || task == MrpTask.Forward)
            {
                //schedules forward and then backward again with the finish of the forward algorithm
                _scheduling.ForwardScheduling(demand);
                demand.State = State.ForwardScheduleExists;
                _context.Update(demand);
                _context.SaveChanges();
                _scheduling.BackwardScheduling(demand);
            }
            _context.SaveChanges();
        }

        private bool CheckNeedForward(IDemandToProvider demand)
        {
            var demandProviderProductionOrders = _context.Demands.OfType<DemandProviderProductionOrder>()
                .Where(a => a.DemandRequesterId == demand.DemandRequesterId).ToList();
            if (demand.GetType() == typeof(DemandStock))
                return true;
            return demandProviderProductionOrders
                        .Select(demandProviderProductionOrder => _context.ProductionOrderWorkSchedule
                        .Include(a => a.ProductionOrder)
                        .Where(a => a.ProductionOrderId == demandProviderProductionOrder.ProductionOrderId)
                        .ToList())
                    .Any(schedules => schedules.Any(schedule => schedule.StartBackward < 0));
        }

        private void ExecutePlanning(IDemandToProvider demand, IDemandToProvider parent, MrpTask task)
        {
            //creates Provider for the needs
            var productionOrder = _demandForecast.NetRequirement(demand, parent, task);
            demand.State = State.ProviderExist;
            
            //If there was enough in stock this does not have to be produced
            if (productionOrder == null) return;

            //create concrete WorkSchedules for the ProductionOrders
            _scheduling.CreateSchedule(demand, productionOrder);

            var children = _context.ArticleBoms
                                .Include(a => a.ArticleChild)
                                .ThenInclude(a => a.ArticleBoms)
                                .Where(a => a.ArticleParentId == demand.ArticleId)
                                .ToList();

            if (!children.Any()) return;
            foreach (var child in children)
            {
                //call this method recursively for a depth-first search
                var dpob = new DemandProductionOrderBom()
                {
                    ArticleId = child.ArticleChildId,
                    Article = child.ArticleChild,
                    Quantity = productionOrder.Quantity * (int) child.Quantity,
                    DemandRequesterId = demand.DemandRequesterId,
                    DemandProvider = new List<DemandToProvider>(),
                    State = State.Created,
                    
                };
                _context.Add(dpob);
                _context.SaveChanges();
                ExecutePlanning(dpob, demand, task);
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