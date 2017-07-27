using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using Master40.BusinessLogicCentral.HelperCapacityPlanning;
using Master40.DB.Enums;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IProcessMrp
    {
        Task CreateAndProcessOrderDemand(MrpTask task);
        void RunRequirementsAndTermination(IDemandToProvider demand, MrpTask task);
        void PlanCapacities(MrpTask task);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly IMessageHub _messageHub;
        private readonly ProductionDomainContext _context;
        private readonly IScheduling _scheduling;
        private readonly IDemandForecast _demandForecast;
        private readonly ICapacityScheduling _capacityScheduling;
        public ProcessMrp(ProductionDomainContext context, IScheduling scheduling, ICapacityScheduling capacityScheduling, IMessageHub messageHub)
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
                _messageHub.SendToAllClients("Start full MRP cycle...", MessageType.info);

                //get all unplanned orderparts and iterate through them for MRP
                var maxAllowedTime = _context.SimulationConfigurations.Last().Time +
                                     _context.SimulationConfigurations.Last().MaxCalculationTime;
                var orderParts = _context.OrderParts.Where(a => a.IsPlanned == false && a.Order.DueTime < maxAllowedTime).Include(a => a.Article).ToList();
                foreach (var orderPart in orderParts.ToList())
                {
                    var demand = GetDemand(orderPart);
                    //run the requirements planning and backward/forward termination algorithm
                    RunRequirementsAndTermination(demand, task);
                }
                
                if (task == MrpTask.All || task == MrpTask.GifflerThompson || task == MrpTask.Capacity)
                {
                    //run the capacity algorithm
                    PlanCapacities(task);
                    _messageHub.SendToAllClients("Capacities are planned");
                }
                //set all orderparts to be planned
                foreach (var orderPart in orderParts)
                {
                    if (task == MrpTask.All || task == MrpTask.GifflerThompson)
                        orderPart.IsPlanned = true;
                }
                _context.SaveChanges();
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
        /// Plans capacities for all demands which are either already planned or just finished with MRP.
        /// </summary>
        /// <param name="task"></param>
        public void PlanCapacities(MrpTask task)
        {
            var timer = _context.SimulationConfigurations.Last().Time;
            var demands = _context.Demands.Where(a =>
                               a.State == State.BackwardScheduleExists || 
                               a.State == State.ForwardScheduleExists ||
                               a.State == State.ExistsInCapacityPlan ||
                               (timer > 0 && a.State == State.Injected)).ToList();
           
            List<MachineGroupProductionOrderWorkSchedule> machineList = null;

            if (timer > 0)
                _capacityScheduling.RebuildNets(timer);

            if (timer == 0 && (task == MrpTask.All || task == MrpTask.Capacity))
                //creates a list with the needed capacities to follow the terminated schedules
                machineList = _capacityScheduling.CapacityRequirementsPlanning(timer);
            
            if (timer != 0 || (task == MrpTask.GifflerThompson || (_capacityScheduling.CapacityLevelingCheck(machineList) && task == MrpTask.All)))
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
            var schedules = _context.ProductionOrderWorkSchedules
                .Include(a => a.ProductionOrder)
                .ThenInclude(a => a.DemandProviderProductionOrders)
                .ThenInclude(a => a.DemandRequester)
                .Where(a => a.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequesterId == demand.Id 
                    || a.ProductionOrder.DemandProviderProductionOrders.First().DemandRequesterId == demand.Id)
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
                _context.ProductionOrderWorkSchedules.Update(schedule);
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
                ExecutePlanning(demand, task);
                demand.State = State.ProviderExist;
                _context.Update(demand);
                _context.SaveChanges();
                _messageHub.SendToAllClients("Requirements planning completed.");
            }
            
            if (task == MrpTask.All || task == MrpTask.Backward)
            {
                _scheduling.BackwardScheduling(demand);
                demand.State = State.BackwardScheduleExists;
                _context.Update(demand);
                _context.SaveChanges();
                _messageHub.SendToAllClients("Backward schedule exists.");

            }

            if ((task == MrpTask.All && CheckNeedForward(demand)) || task == MrpTask.Forward)
            {
                //schedules forward and then backward again with the finish of the forward algorithm
                _scheduling.ForwardScheduling(demand);
                demand.State = State.ForwardScheduleExists;
                _context.Update(demand);
                _context.SaveChanges();
                _scheduling.BackwardScheduling(demand);
                _messageHub.SendToAllClients("Forward schedule exists.");
            }
            _context.SaveChanges();
        }

        private bool CheckNeedForward(IDemandToProvider demand)
        {
            return demand.GetType() == typeof(DemandStock) || _context.GetProductionOrderWorkSchedules(demand).Any(a => a.StartBackward < 0);
        }

        private void ExecutePlanning(IDemandToProvider demand, MrpTask task)
        {
            //creates Provider for the needs
            var productionOrders = _demandForecast.NetRequirement(demand, task);
            demand.State = State.ProviderExist;
            
            //If there was enough in stock this does not have to be produced
            if (!productionOrders.Any()) return;
            foreach (var productionOrder in productionOrders)
            {
                //if the ProductionOrder was just created, initialize concrete WorkSchedules for the ProductionOrders
                if (productionOrder.ProductionOrderWorkSchedule == null ||
                    !productionOrder.ProductionOrderWorkSchedule.Any())
                    _context.CreateProductionOrderWorkSchedules(productionOrder);
                
                var children = _context.ArticleBoms
                                    .Include(a => a.ArticleChild)
                                    .ThenInclude(a => a.ArticleBoms)
                                    .Where(a => a.ArticleParentId == productionOrder.ArticleId)
                                    .ToList();
                
                if (!children.Any()) return;
                foreach (var child in children)
                {
                    //create Production-BOM
                    var pob = new ProductionOrderBom()
                    {
                        ProductionOrderParentId = productionOrder.Id,
                        Quantity = child.Quantity * productionOrder.Quantity
                    };
                    _context.ProductionOrderBoms.Add(pob);
                    _context.SaveChanges();

                    //create Requester
                    var dpob = new DemandProductionOrderBom
                    {
                        ArticleId = child.ArticleChildId,
                        Article = child.ArticleChild,
                        Quantity = productionOrder.Quantity * (int)child.Quantity,
                        DemandProvider = new List<DemandToProvider>(),
                        State = State.Created,
                        DemandRequesterId = null,
                        ProductionOrderBomId = pob.Id
                    };
                    _context.Add(dpob);
                    _context.SaveChanges();
                    //call this method recursively for a depth-first search
                    ExecutePlanning(dpob, task);
                }
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