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
using System;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Master40.BusinessLogicCentral.MRP
{
    public interface IProcessMrp
    {
        Task CreateAndProcessOrderDemand(MrpTask task, ProductionDomainContext context, int simulationId, ProductionDomainContext c);
        void RunRequirementsAndTermination(IDemandToProvider demand, MrpTask task, int simulationId);
        void PlanCapacities(MrpTask task, bool newOrdersAdded, int simulationId, ProductionDomainContext evaluationContext);
        void UpdateDemandsAndOrders(int simulationId);
        void ExecutePlanning(IDemandToProvider demand, MrpTask task, int simulationId);
        IDemandToProvider GetDemand(OrderPart orderPart);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly IRebuildNets _rebuildNets;
        private readonly IMessageHub _messageHub;
        private ProductionDomainContext _context;
        private readonly IScheduling _scheduling;
        private readonly IDemandForecast _demandForecast;
        private readonly ICapacityScheduling _capacityScheduling;
        private int itemcount = 0;
        public ProcessMrp(ProductionDomainContext context, IScheduling scheduling, ICapacityScheduling capacityScheduling, IMessageHub messageHub, IRebuildNets rebuildNets)
        {
            _messageHub = messageHub;
            _context = context;
            _scheduling = scheduling;
            _capacityScheduling = capacityScheduling;
            _demandForecast = new DemandForecast(context,this);
            _rebuildNets = rebuildNets;
        }

        /// <summary>
        /// Plans all unplanned Orders with MRP I + II
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task CreateAndProcessOrderDemand(MrpTask task, ProductionDomainContext context, int simulationId, ProductionDomainContext evaluationContext)
        {
            await Task.Run(() =>
            {
                if (context != null) _context = context;
                var newOrdersAdded = false;
                _messageHub.SendToAllClients("Start full MRP cycle...", MessageType.info);

                //get all unplanned orderparts and iterate through them for MRP
                var simConfig = _context.SimulationConfigurations.Single(a => a.Id == simulationId);
                var maxAllowedTime = simConfig.Time + simConfig.MaxCalculationTime;
                //Todo: put together if problem is solved
                var orderParts3 = _context.OrderParts.Include(a => a.Order).Where(a => a.IsPlanned == false);
                var orderParts2 = orderParts3.Where(a => a.Order.CreationTime <= simConfig.Time);
                var orderParts = orderParts2.Where(a => a.Order.DueTime < maxAllowedTime).Include(a => a.Article).ToList();

                if (orderParts.Any()) newOrdersAdded = true;
                foreach (var orderPart in orderParts.ToList())
                {
                    var demand = GetDemand(orderPart);
                    _messageHub.SendToAllClients("running requirements for orderpart "+orderPart.Id);
                    //run the requirements planning and backward/forward termination algorithm
                    RunRequirementsAndTermination(demand, task, simulationId);
                }
                
                if (task == MrpTask.All || task == MrpTask.GifflerThompson || task == MrpTask.Capacity)
                {
                    //run the capacity algorithm
                    PlanCapacities(task, newOrdersAdded, simulationId, evaluationContext);
                    
                    _messageHub.SendToAllClients("Capacities are planned");
                }
                //set all orderparts to be planned
                foreach (var orderPart in orderParts)
                {
                    if (task == MrpTask.All || task == MrpTask.GifflerThompson)
                        orderPart.IsPlanned = true;
                }
                _context.SaveChanges();
                _messageHub.SendToAllClients("End of the latest calculated order: "+ _context.ProductionOrderWorkSchedules?.Max(a => a.End));
            });
            
        }

        /// <summary>
        /// Check if a demand exists for the orderpart, else a new one is created.
        /// </summary>
        /// <param name="orderPart"></param>
        /// <returns></returns>
        public IDemandToProvider GetDemand(OrderPart orderPart)
        {
            var demandOrderParts = _context.Demands.OfType<DemandOrderPart>()
                                           .Include(a => a.DemandProvider)
                                           .Where(a => a.OrderPartId == orderPart.Id)
                                           .ToList();
            IDemandToProvider demand;
            if (demandOrderParts.Any())
                demand = demandOrderParts.First();
            else
            {
                demand = _context.CreateDemandOrderPart(orderPart);
                _context.SaveChanges();
            }
            return demand;
        }

        /// <summary>
        /// Plans capacities for all demands which are either already planned or just finished with MRP.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="newOrdersAdded"></param>
        public void PlanCapacities(MrpTask task, bool newOrdersAdded, int simulationId,ProductionDomainContext evaluationContext)
        {
            var timer = _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time;
            var demands = _context.Demands.Where(a =>
                               a.State == State.BackwardScheduleExists || 
                               a.State == State.ForwardScheduleExists ||
                               a.State == State.ExistsInCapacityPlan ||
                               (timer > 0 && a.State == State.Injected)).ToList();
           
            List<MachineGroupProductionOrderWorkSchedule> machineList = null;
            
            /*if (newOrdersAdded)
            {
                _rebuildNets.Rebuild(simulationId, evaluationContext);
                _messageHub.SendToAllClients("RebuildNets completed");
            }*/

            if (timer == 0 && (task == MrpTask.All || task == MrpTask.Capacity))
                //creates a list with the needed capacities to follow the terminated schedules
                machineList = _capacityScheduling.CapacityRequirementsPlanning(simulationId);
            
            if (timer != 0 || (task == MrpTask.GifflerThompson || (_capacityScheduling.CapacityLevelingCheck(machineList) && task == MrpTask.All)))
                _capacityScheduling.GifflerThompsonScheduling(simulationId);
            else
            {
                foreach (var demand in demands)
                    SetStartEndFromTermination(demand);
                
                _capacityScheduling.SetMachines(simulationId);
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
        /// <param name="simulationId"></param>
        public void RunRequirementsAndTermination(IDemandToProvider demand, MrpTask task, int simulationId)
        {
            if (demand.State == State.Created)
            {
                ExecutePlanning(demand, task, simulationId);
                if (demand.State != State.Finished)
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

            if (task == MrpTask.All && CheckNeedForward(demand, simulationId) || task == MrpTask.Forward)
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

        private bool CheckNeedForward(IDemandToProvider demand, int simulationId)
        {
            var pows = new List<ProductionOrderWorkSchedule>();
            return demand.GetType() == typeof(DemandStock) 
                                    || _context.GetWorkSchedulesFromDemand(demand, ref pows)
                                                                    .Any(a => a.StartBackward < _context.SimulationConfigurations.Single(b => b.Id == simulationId).Time);
        }

        public void ExecutePlanning(IDemandToProvider demand, MrpTask task, int simulationId)
        {
            //creates Provider for the needs
            var productionOrders = _demandForecast.NetRequirement(demand, task, simulationId);
            if (demand.State != State.Finished)
                demand.State = State.ProviderExist;

            var articleBoms = _context.ArticleBoms
                .Include(a => a.ArticleChild)
                .ThenInclude(a => a.ArticleBoms)
                .ToList();

            //If there was enough in stock this does not have to be produced
            if (!productionOrders.Any()) return;
            foreach (var productionOrder in productionOrders)
            {
                //if the ProductionOrder was just created, initialize concrete WorkSchedules for the ProductionOrders
                if (productionOrder.ProductionOrderWorkSchedule == null ||
                    !productionOrder.ProductionOrderWorkSchedule.Any())
                    _context.CreateProductionOrderWorkSchedules(productionOrder);
                
                var children = articleBoms.Where(a => a.ArticleParentId == productionOrder.ArticleId)
                                    .ToList();
                
                if (!children.Any()) return;
                foreach (var child in children)
                {
                    //create Requester
                    var dpob = _context.CreateDemandProductionOrderBom(child.ArticleChildId,productionOrder.Quantity * (int) child.Quantity);
                    //create Production-BOM
                    _context.TryCreateProductionOrderBoms(dpob, productionOrder, simulationId);
                    //call this method recursively for a depth-first search
                    ExecutePlanning(dpob, task, simulationId);
                }
            }
        }

        public void UpdateDemandsAndOrders(int simulationId)
        {
            var requester = UpdateDemandStates();
            if (requester == null) return;
            var orderParts = (from singleRequester in requester
                              where singleRequester.GetType() == typeof(DemandOrderPart)
                              select ((DemandOrderPart) singleRequester).OrderPart).ToList();
            foreach (var orderPart in orderParts)
            {
                var finishedOrderPart =
                        _context.OrderParts.Include(a => a.Order)
                            .ThenInclude(b => b.OrderParts)
                            .ThenInclude(c => c.DemandOrderParts)
                            .ThenInclude(d => d.DemandProvider)
                            .Single(a => a.Id == orderPart.Id);
                if (finishedOrderPart.Order.OrderParts.Any(a => a.State != State.Finished))
                {
                    if (!TryFinishDemandsAndOrderParts(finishedOrderPart.Order))
                        continue;
                }
                
                FinishOrder(finishedOrderPart.OrderId, simulationId);
                _context.SaveChanges();
            }
        }

        private bool TryFinishDemandsAndOrderParts(Order order)
        {
            var unfinishedOrderParts = order.OrderParts.Where(a => a.State != State.Finished).ToList();
            /*var unfinishedRequester = (from uop in unfinishedOrderParts
                                      from dop in uop.DemandOrderParts
                                      where dop.State != State.Finished
                                      select dop).ToList();
            if (unfinishedOrderParts.Any(a => a.DemandOrderParts.Any(b => b.State != State.Finished)))
            {
                if (unfinishedRequester.Any() && unfinishedRequester.Any(a => a.State != State.Finished))
                    return false;
            }*/
            foreach (var uop in unfinishedOrderParts)
            {
                foreach (var dop in uop.DemandOrderParts.Where(a => a.State != State.Finished))
                {
                    if (_context.TryUpdateStockProvider(dop))
                    {
                        dop.State = State.Finished;
                        _context.Update(dop);
                    }
                    else
                    {
                        _context.SaveChanges();
                        return false;
                    }
                }
                uop.State = State.Finished;
                _context.Update(uop);
            }
            _context.SaveChanges();
            return true;
        }


        private void FinishOrder(int orderId, int simulationId)
        {
            var order = _context.Orders.Single(a => a.Id == orderId);
            var simConfig = _context.SimulationConfigurations.Single(a => a.Id == simulationId);
            if (order.State == State.Finished) return;
            order.State = State.Finished;
            order.FinishingTime = simConfig.Time;
            _context.Update(order);
            _context.SaveChanges();
            _messageHub.ProcessingUpdate(simulationId, ++itemcount, SimulationType.Central, simConfig.OrderQuantity);
            _messageHub.SendToAllClients("Order with Id " + order.Id + " finished!");
            foreach (var singleOrderPart in order.OrderParts)
            {
                var stock = _context.Stocks.Single(a => a.ArticleForeignKey == singleOrderPart.ArticleId);
                stock.Current -= singleOrderPart.Quantity;
                _context.StockExchanges.Add(new StockExchange()
                {
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = singleOrderPart.Quantity,
                    StockId = stock.Id,
                    RequiredOnTime = order.DueTime,
                    Time = _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time
                });
                _context.Update(stock);
            }
            _context.SaveChanges();
        }

        private List<IDemandToProvider> UpdateDemandStates()
        {
            var changedDemands = new List<IDemandToProvider>();
            changedDemands.AddRange(UpdateStateDemandProviderProductionOrders());
            changedDemands.AddRange(_context.UpdateStateDemandProviderPurchaseParts());
            changedDemands.AddRange(UpdateDemandProviderStock());

            return !changedDemands.Any() ? null : _context.UpdateStateDemandRequester(changedDemands);
        }

        private IEnumerable<IDemandToProvider> UpdateDemandProviderStock()
        {
            var changedDemands = new List<IDemandToProvider>();
            var provider = _context.Demands.OfType<DemandProviderStock>().Include(a => a.DemandRequester).ThenInclude(b => b.DemandProvider).Where(a => a.State != State.Finished).ToList();
            foreach (var singleProvider in provider)
            {
                if (singleProvider.DemandRequester != null)
                { 
                    var unfinishedProvider = singleProvider.DemandRequester.DemandProvider.Where(a => a.State != State.Finished);
                    if (unfinishedProvider.Any(a => a.GetType() != typeof(DemandProviderStock)))
                        continue;
                    if (singleProvider.DemandRequester.GetType() == typeof(DemandOrderPart))
                    {
                        var singledop = _context.Demands.OfType<DemandOrderPart>()
                            .Include(a => a.OrderPart)
                            .ThenInclude(b => b.Order)
                            .ThenInclude(c => c.OrderParts)
                            .ThenInclude(d => d.DemandOrderParts)
                            .ThenInclude(e => e.DemandProvider)
                            .Single(a => a.Id == singleProvider.DemandRequester.Id);
                        var orderParts = singledop.OrderPart.Order.OrderParts;
                        var orderPartProvider = from op in orderParts
                            from dop in op.DemandOrderParts
                            where dop.State != State.Finished
                            from dp in dop.DemandProvider
                            select dp;
                        if (orderPartProvider.Any(a => a.GetType() != typeof(DemandProviderStock))) continue;
                    }
                }
                singleProvider.State = State.Finished;
                _context.Update(singleProvider);
                _context.SaveChanges();
                changedDemands.Add(singleProvider);
            }
            return changedDemands;
        }

        private IEnumerable<IDemandToProvider> UpdateStateDemandProviderProductionOrders()
        {
            var changedDemands = new List<IDemandToProvider>();
            var provider = _context.Demands.OfType<DemandProviderProductionOrder>().Include(a => a.ProductionOrder).ThenInclude(b => b.ProductionOrderWorkSchedule).Where(a => a.State != State.Finished).ToList();
            foreach (var singleProvider in provider)
            {
                if (singleProvider.ProductionOrder.ProductionOrderWorkSchedule.Any(a => a.ProducingState != ProducingState.Finished)) continue;
                singleProvider.State = State.Finished;
                _context.Update(singleProvider);
                _context.SaveChanges();
                changedDemands.Add(singleProvider);
            }
            return changedDemands;
        }
    }

    public enum MrpTask
    {
        None,
        All,
        Forward,
        Backward,
        Capacity,
        GifflerThompson
    }


}