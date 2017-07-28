using System.Collections.Generic;
using System.Linq;
using Master40.BusinessLogicCentral.HelperCapacityPlanning;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{
   public interface ICapacityScheduling
    {
        void GifflerThompsonScheduling(int timer);
        List<MachineGroupProductionOrderWorkSchedule> CapacityRequirementsPlanning(int timer);
        bool CapacityLevelingCheck(List<MachineGroupProductionOrderWorkSchedule> machineList);
        void SetMachines(int timer);
        void RebuildNets(int time);
    }

    public class CapacityScheduling : ICapacityScheduling
    {
        private readonly ProductionDomainContext _context;
        public CapacityScheduling(ProductionDomainContext context)
        {
            _context = context;
        }



        /// <summary>
        /// An algorithm for capacity-leveling. Writes Start/End in ProductionOrderWorkSchedule.
        /// </summary>
        public void GifflerThompsonScheduling(int currentTimer)
        {
            var maxTimer = _context.SimulationConfigurations.Last().MaxCalculationTime;
            var productionOrderWorkSchedules = GetProductionSchedules(currentTimer, maxTimer);
            ResetStartEnd(productionOrderWorkSchedules);
            productionOrderWorkSchedules = CalculateWorkTimeWithParents(productionOrderWorkSchedules);

            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            var plannedSchedules = GetInitialPlannedSchedules(currentTimer) ?? new List<ProductionOrderWorkSchedule>();
            GetInitialPlannables(productionOrderWorkSchedules,plannedSchedules, plannableSchedules,currentTimer);
            while (plannableSchedules.Any())
            {
                //find next element by using the activity slack rule
                CalculateActivitySlack(plannableSchedules);
                var shortest = GetShortest(plannableSchedules);

                plannableSchedules.Remove(shortest);
                //Add a fix spot on a machine with start/end
                AddMachine(plannedSchedules, shortest);
                plannedSchedules.Add(shortest);

                //search for parents and if available and allowed add it to the schedule
                var parents = _context.GetParents(shortest);
                foreach (var parent in parents)
                {
                    if (!plannableSchedules.Contains(parent) && IsTechnologicallyAllowed(parent, plannedSchedules, currentTimer))
                        plannableSchedules.Add(parent);
                    _context.SaveChanges();
                }
                
            }
        }

        private List<ProductionOrderWorkSchedule> GetInitialPlannedSchedules(int timer)
        {
            return timer == 0 ? null : _context.ProductionOrderWorkSchedules.Where(a => a.Start <= timer && a.End - a.Start == a.Duration).ToList();
        }
        
        private void AddMachine(List<ProductionOrderWorkSchedule> plannedSchedules, ProductionOrderWorkSchedule shortest)
        {
            var machines = _context.Machines.Where(a => a.MachineGroupId == shortest.MachineGroupId).ToList();
            if (machines.Count == 1)
            {
                shortest.Start = GetChildEndTime(shortest);
                shortest.End = shortest.Start + shortest.Duration;
                var earliestPlanned = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliestPows = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliest = (earliestPlanned > earliestPows) ? earliestPlanned : earliestPows;
                if (shortest.Start < earliest)
                    shortest.Start = earliest;
                shortest.MachineId = machines.First().Id;
                shortest.End = shortest.Start + shortest.Duration;
            }
               
            else if (machines.Count > 1)
            {
                shortest.Start = GetChildEndTime(shortest);
                shortest.End = shortest.Start + shortest.Duration;
                var earliestPlanned = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliest = earliestPlanned;
                var earliestMachine = machines.First();
                foreach (var machine in machines)
                {
                    var earliestThisMachine = FindStartOnMachine(plannedSchedules, machine.Id, shortest);
                    if (earliest <= earliestThisMachine) continue;
                    earliest = earliestThisMachine;
                    earliestMachine = machine;
                }
                
                
                if (shortest.Start < earliest)
                    shortest.Start = earliest;
                shortest.MachineId = earliestMachine.Id;
                shortest.End = shortest.Start + shortest.Duration;
            }
            _context.Update(shortest);
            _context.SaveChanges();
        }

        private int GetChildEndTime(ProductionOrderWorkSchedule shortest)
        {
            if (shortest.HierarchyNumber != shortest.ProductionOrder.ProductionOrderWorkSchedule.Min(a => a.HierarchyNumber))
            {
                var children = _context.ProductionOrderWorkSchedules.Where(a =>
                    a.ProductionOrderId == shortest.ProductionOrderId &&
                    a.HierarchyNumber < shortest.HierarchyNumber).ToList();
                return children.Max(b => b.End);
            }
            var childrenBoms = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == shortest.ProductionOrderId).ToList();
            var latestEnd = (from bom in childrenBoms
                            from provider in bom.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                            select provider.ProductionOrder.ProductionOrderWorkSchedule.Max(a => a.End)
                            ).Concat(new[] {0}).Max();
            return latestEnd;
        }

        private int FindStartOnMachine(List<ProductionOrderWorkSchedule> plannedSchedules, int machineId, ProductionOrderWorkSchedule shortest)
        {
            for (var i = plannedSchedules.Count-1; i >= 0; i--)
            {
                if (plannedSchedules[i].MachineId == machineId)
                {
                    return DetectCrossing(plannedSchedules[i], shortest) ? plannedSchedules[i].End : (plannedSchedules[i].End>shortest.Start ? plannedSchedules[i].End : shortest.Start);
                }
                    
            }
            return 0;
        }

        private void ResetStartEnd(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                productionOrderWorkSchedule.Start = 0;
                productionOrderWorkSchedule.End = 0;
            }
        }

        /// <summary>
        /// Calculates Capacities needed to use backward/forward termination
        /// </summary>
        /// <returns>capacity-plan</returns>
        public List<MachineGroupProductionOrderWorkSchedule> CapacityRequirementsPlanning(int timer)
        {   
            //Stack for every hour and machinegroup
            var productionOrderWorkSchedules = GetProductionSchedules(timer, _context.SimulationConfigurations.Last().MaxCalculationTime);
            var machineList = new List<MachineGroupProductionOrderWorkSchedule>();

            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                //calculate every pows for the amount of pieces ordered parallel
                for (var i= 0; i<productionOrderWorkSchedule.ProductionOrder.Quantity; i++)
                {
                    var machine = machineList.Find(a => a.MachineGroupId == productionOrderWorkSchedule.MachineGroupId);
                    if (machine != null)
                        machineList[machineList.IndexOf(machine)].ProductionOrderWorkSchedulesByTimeSteps=AddToMachineGroup(machine, productionOrderWorkSchedule);
                    else
                    {
                        var schedule = new MachineGroupProductionOrderWorkSchedule
                        {
                            MachineGroupId = productionOrderWorkSchedule.MachineGroupId,
                            ProductionOrderWorkSchedulesByTimeSteps = new List<ProductionOrderWorkSchedulesByTimeStep>()
                        };
                        machineList.Add(schedule);
                        machineList.Last().ProductionOrderWorkSchedulesByTimeSteps = AddToMachineGroup(machineList.Last(), productionOrderWorkSchedule);
                    }
                }
            }
            return machineList;
        }

        /// <summary>
        /// checks if Capacity-leveling with Giffler-Thompson is necessary
        /// </summary>
        /// <param name="machineList"></param>
        /// <returns>true if existing plan exceeds capacity limits</returns>
        public bool CapacityLevelingCheck(List<MachineGroupProductionOrderWorkSchedule> machineList )
        {
            foreach (var machine in machineList)
            {
                foreach (var hour in machine.ProductionOrderWorkSchedulesByTimeSteps)
                {
                    var machines = _context.Machines.Where(a => a.MachineGroupId == machine.MachineGroupId).ToList();
                    if (!machines.Any()) continue;
                    if (machines.Count() < hour.ProductionOrderWorkSchedules.Count)
                        return true;
                }
            }
            
            return false;
        }

        private List<ProductionOrderWorkSchedulesByTimeStep> AddToMachineGroup(MachineGroupProductionOrderWorkSchedule machine, ProductionOrderWorkSchedule productionOrderWorkSchedule)
        { //Todo: replace provider.first()
            var start = productionOrderWorkSchedule.StartBackward;
            var end = productionOrderWorkSchedule.EndBackward;
            if (productionOrderWorkSchedule.ProductionOrder.DemandProviderProductionOrders.First().State == State.ForwardScheduleExists)
            {
                start = productionOrderWorkSchedule.StartForward;
                end = productionOrderWorkSchedule.EndForward;
            }

            for (var i = start; i < end; i++)
            {
                var found = false;
                foreach (var productionOrderWorkSchedulesByTimeStep in machine.ProductionOrderWorkSchedulesByTimeSteps)
                {
                    if (productionOrderWorkSchedulesByTimeStep.Time == i)
                    {
                        productionOrderWorkSchedulesByTimeStep.ProductionOrderWorkSchedules.Add(productionOrderWorkSchedule);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var timestep = new ProductionOrderWorkSchedulesByTimeStep
                    {
                            Time = i,
                            ProductionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>
                            {
                                productionOrderWorkSchedule
                            }
                        };
                    machine.ProductionOrderWorkSchedulesByTimeSteps.Add(timestep);
                }
                    
            }
            return machine.ProductionOrderWorkSchedulesByTimeSteps;
        }

       

        private bool IsTechnologicallyAllowed(ProductionOrderWorkSchedule schedule, List<ProductionOrderWorkSchedule> plannedSchedules, int timer)
        {//Todo check effectiveness
            //check for every child if its planned
            var child = GetHierarchyChild(schedule);
            if (child != null && !plannedSchedules.Any(a => a.ProductionOrderId == child.ProductionOrderId && a.HierarchyNumber == child.HierarchyNumber))
            {
                return false;
            }
            if (child == null)
            {
                var childs = GetBomChilds(schedule);
                if (childs == null) return true;
                foreach (var childSchedule in childs)
                {
                    if (!plannedSchedules.Any(a => a.ProductionOrderId == childSchedule.ProductionOrderId && a.HierarchyNumber == childSchedule.HierarchyNumber)) return false;
                }
            }
            
            return true;
        }

        private ProductionOrderWorkSchedule GetShortest(List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            ProductionOrderWorkSchedule shortest = null;
            foreach (var plannableSchedule in plannableSchedules)
            {
                if (shortest == null || shortest.ActivitySlack > plannableSchedule.ActivitySlack)
                    shortest = plannableSchedule;
            }
            return shortest;
        }

        private List<ProductionOrderWorkSchedule> CalculateWorkTimeWithParents(List<ProductionOrderWorkSchedule> schedules)
        {
            foreach (var schedule in schedules)
            {
                schedule.WorkTimeWithParents = GetRemainTimeFromParents(schedule, schedules);
            }
            _context.UpdateRange(schedules);
            _context.SaveChanges();
            return schedules;
        }

        private void CalculateActivitySlack(List<ProductionOrderWorkSchedule> plannableSchedules)
        { //replace provider.first()
            foreach (var plannableSchedule in plannableSchedules)
            {
                //get duetime
                var dueTime = plannableSchedule.ProductionOrder.Duetime;

                //get remaining time
                var slack = plannableSchedule.ActivitySlack;
                plannableSchedule.ActivitySlack = dueTime - plannableSchedule.WorkTimeWithParents - GetChildEndTime(plannableSchedule);
                if (slack == plannableSchedule.ActivitySlack) continue;
                _context.ProductionOrderWorkSchedules.Update(plannableSchedule);
                _context.SaveChanges();
            }
        }

        private decimal GetRemainTimeFromParents(ProductionOrderWorkSchedule schedule, List <ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            if (schedule == null) return 0;
            var parents = _context.GetParents(schedule);
            if (!parents.Any()) return schedule.Duration;
            var maxTime = 0;
            foreach (var parent in parents)
            {
                var time = GetRemainTimeFromParents(parent, productionOrderWorkSchedules) + schedule.Duration;
                if (time > maxTime) maxTime = (int)time;
            }

            return maxTime;
        }

        private decimal GetAmountOfMachines(ProductionOrderWorkSchedule schedule)
        {
            return _context.Machines.Count(a => a.MachineGroupId == schedule.MachineGroupId);
        }

        private List<ProductionOrderWorkSchedule> GetProductionSchedules(int currentTimer, int maxTimer)
        {
            var demandRequester = _context.Demands
                                            .Include(a => a.DemandProvider)
                                            .Include(a => a.DemandRequester)
                                            .ThenInclude(a => a.DemandRequester)
                                                    .Where(b => b.State == State.BackwardScheduleExists 
                                                            || b.State == State.ExistsInCapacityPlan 
                                                            || b.State == State.ForwardScheduleExists
                                                            || b.State == State.Injected)
                                                            .ToList();
            
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleDemandRequester in demandRequester)
            {
                if (_context.GetDueTimeByOrder(singleDemandRequester) <= currentTimer + maxTimer || singleDemandRequester.GetType() == typeof(DemandStock))
                    _context.GetWorkSchedulesFromDemand(singleDemandRequester, ref pows);
            }
            return pows.AsEnumerable().Distinct().ToList();
        }

        private List<ProductionOrderWorkSchedule> GetProductionSchedules(IDemandToProvider requester, int timer)
        {
            var provider =
                _context.Demands.OfType<DemandProviderProductionOrder>()
                    .Include(a => a.ProductionOrder)
                    .ThenInclude(c => c.ProductionOrderWorkSchedule)
                    .Include(b => b.ProductionOrder)
                    .ThenInclude(d => d.ProductionOrderBoms)
                    .Where(a => a.DemandRequester.DemandRequesterId == requester.Id
                        || a.DemandRequesterId == requester.Id)
                    .ToList();
            return (from prov in provider from schedule in prov.ProductionOrder.ProductionOrderWorkSchedule
                    where schedule.Start >= timer || schedule.End - schedule.Start != schedule.Duration select schedule).ToList();
        }

        private void GetInitialPlannables(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, 
            List<ProductionOrderWorkSchedule> plannedSchedules, List<ProductionOrderWorkSchedule> plannableSchedules, int timer)
        {
            plannableSchedules.AddRange(productionOrderWorkSchedules.Where(productionOrderWorkSchedule => 
                                            IsTechnologicallyAllowed(productionOrderWorkSchedule, plannedSchedules, timer)));
        }

        private bool ChildrenAreInProduction(ProductionOrderWorkSchedule productionOrderWorkSchedule, int timer)
        {
            var children = GetLatestChildren(productionOrderWorkSchedule);
            return children != null && children.All(child => child.Start <= timer && child.Duration == child.End-child.Start);
        }

        private List<ProductionOrderWorkSchedule> GetLatestChildren(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var children = new List<ProductionOrderWorkSchedule>();
            var child = GetHierarchyChild(productionOrderWorkSchedule);
            if (child == null) return GetBomChilds(productionOrderWorkSchedule);
            children.Add(child);
            return children;
        }

        private List<ProductionOrderWorkSchedule> GetBomChilds(
            ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var boms = productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms;
            var bomChilds = (from bom in boms
                             from provider in bom.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                             select provider.ProductionOrder.ProductionOrderWorkSchedule into schedules
                             select schedules.Single(a => a.HierarchyNumber == schedules.Max(b => b.HierarchyNumber))
                             ).ToList();
            return !bomChilds.Any() ? null : bomChilds;
        }
        

        private ProductionOrderWorkSchedule GetHierarchyChild(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var productionOrderWorkSchedules = _context.ProductionOrderWorkSchedules.AsNoTracking().Where(a => a.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId);
            productionOrderWorkSchedules = productionOrderWorkSchedules.Where(mainSchedule => mainSchedule.HierarchyNumber < productionOrderWorkSchedule.HierarchyNumber);
            if (!productionOrderWorkSchedules.Any()) return null;
            return productionOrderWorkSchedules.Single(a => a.HierarchyNumber == productionOrderWorkSchedules.Max(b => b.HierarchyNumber));
        }

        public void SetMachines(int timer)
        {  
            //gets called when plan is fitting to capacities
            var schedules = GetProductionSchedules(timer,_context.SimulationConfigurations.Last().MaxCalculationTime);
            foreach (var schedule in schedules)
            {
                var machines = _context.Machines.Where(a => a.MachineGroupId == schedule.MachineGroupId).ToList();
                if (!machines.Any()) continue;
                var schedulesOnMachineGroup = schedules.FindAll(a => a.MachineGroupId == schedule.MachineGroupId && a.MachineId != null);
                var crossingPows = (from scheduleMg in schedulesOnMachineGroup where DetectCrossing(schedule, scheduleMg) select schedule).ToList();
                if (!crossingPows.Any()) schedule.MachineId = machines.First().Id;
                else
                {
                    foreach (var machine in machines)
                    {
                        if (crossingPows.Find(a => a.MachineId == machine.Id) != null) continue;
                        schedule.MachineId = machine.Id;
                        break;
                    }
                }
            }
        }

        private bool DetectCrossing(ProductionOrderWorkSchedule schedule, ProductionOrderWorkSchedule scheduleMg)
        {
            return (scheduleMg.Start <= schedule.Start &&
                    scheduleMg.End > schedule.Start)
                   ||
                   (scheduleMg.Start < schedule.End &&
                    scheduleMg.End >= schedule.End)
                   ||
                   (scheduleMg.Start > schedule.Start &&
                    scheduleMg.End < schedule.End);
        }
        
        public void RebuildNets(int time)
        {
            //delete all demands but the head-demands
            _context.Demands.RemoveRange(_context.Demands.Where(a => (a.DemandRequesterId != null || a.GetType() == typeof(DemandProductionOrderBom)) && a.State != State.Delivered).ToList());
            _context.SaveChanges();
            var requester = _context.Demands.Where(a => null == a.DemandRequesterId && 
                                                   a.State != State.Delivered && 
                                                   (a.GetType() == typeof(DemandStock)||
                                                        _context.GetDueTimeByOrder(a) < _context.SimulationConfigurations.Last().Time
                                                                                        +_context.SimulationConfigurations.Last().MaxCalculationTime))
                                            .ToList();
            //delete all boms
            _context.ProductionOrderBoms.RemoveRange(_context.ProductionOrderBoms);
            _context.SaveChanges();
            //rebuild by using activity-slack to order demands
            while (requester.Any())
            {
                var nextRequester = GetNextByActivitySlack(requester, time);
                SatisfyRequest(nextRequester, null);
                requester.Remove(requester.Find(a => a.Id == nextRequester.Id));
            }
           
        }
        
        private void SatisfyRequest(IDemandToProvider demand, ProductionOrder parentProductionOrder)
        {//Todo: search for purchases
            
            var amount = demand.Quantity;
            
            //if anything is in stock, create demand
            var articleStock = _context.Stocks.Single(a => a.ArticleForeignKey==demand.ArticleId).Current;
            if (articleStock > 0)
            {
                var provider = _context.TryCreateStockReservation(demand);
                if (provider != null)
                {
                    amount -= provider.Quantity;
                }
            }
            if (amount == 0) return;
            //find matching productionOrders
            var possibleMatchingProductionOrders = _context.ProductionOrders.Where(a => a.ArticleId == demand.ArticleId && a.ProductionOrderWorkSchedule.Any(b => b.ProducingState!= ProducingState.Finished)).ToList();
            
            while (amount > 0 && possibleMatchingProductionOrders.Any())
            {
                var earliestProductionOrder = _context.GetEarliestProductionOrder(possibleMatchingProductionOrders);
                var availableAmountFromProductionOrder = _context.GetAvailableAmountFromProductionOrder(earliestProductionOrder);
                if (availableAmountFromProductionOrder == 0)
                {
                    possibleMatchingProductionOrders.Remove(possibleMatchingProductionOrders.Find(a => a.Id == earliestProductionOrder.Id));
                    continue;
                }
                var provider = _context.CreateProviderProductionOrder(demand, earliestProductionOrder, amount >= availableAmountFromProductionOrder ? availableAmountFromProductionOrder : amount);
                if (parentProductionOrder != null) _context.TryCreateProductionOrderBoms(demand, earliestProductionOrder, parentProductionOrder);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignProductionOrderToDemandProvider(earliestProductionOrder, provider);
                if (amount > availableAmountFromProductionOrder)
                {
                    amount -= availableAmountFromProductionOrder;
                }
                else
                {
                    amount = 0;
                }
                possibleMatchingProductionOrders.ToList().Remove(earliestProductionOrder);
            }

            if (amount == 0)
            {
                foreach (var dppo in demand.DemandProvider.OfType<DemandProviderProductionOrder>())
                {
                    CallChildrenSatisfyRequest(dppo.ProductionOrder);
                }
                return;
            }

            //must not occur, everything should be enough
            if (_context.HasChildren(demand))
            {
                //possible multiple iterations because of lotsize
                while (amount > 0)
                {
                    //create ProductionOrders
                    var productionOrder = _context.CreateProductionOrder(demand, parentProductionOrder?.Duetime ?? _context.GetDueTimeByOrder(demand));
                    if (parentProductionOrder != null) _context.TryCreateProductionOrderBoms(demand, productionOrder, parentProductionOrder);
                    _context.CreateProductionOrderWorkSchedules(productionOrder);
                    var provider = _context.CreateProviderProductionOrder(demand, productionOrder, amount);
                    _context.AssignProviderToDemand(demand, provider);
                    _context.AssignProductionOrderToDemandProvider(productionOrder, provider);
                    CallChildrenSatisfyRequest(productionOrder); 
                    
                    amount -= productionOrder.Quantity;
                }
            }
            else
            {
                //create Purchase
                var purchasePart = _context.CreatePurchase(demand, amount);
                var provider = _context.CreateProviderPurchase(demand, purchasePart, amount);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignPurchaseToDemandProvider(purchasePart, provider, purchasePart.Quantity);
            }
            
            
        }

        
        private void CallChildrenSatisfyRequest(ProductionOrder po)
        {
            //call method for each child
            var childrenArticleBoms = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == po.ArticleId).ToList();
            foreach (var childBom in childrenArticleBoms)
            {
                //check for the existence of a DemandProductionOrderBom
                if (po.ProductionOrderBoms.FirstOrDefault(a => a.DemandProductionOrderBoms.First().ArticleId == childBom.ArticleChildId) == null) continue;

                if (po.ProductionOrderWorkSchedule.Any(a => a.ProducingState == ProducingState.Producing)
                    || po.ProductionOrderWorkSchedule.Any(a => a.ProducingState == ProducingState.Finished)) continue;

                var neededAmount = childBom.Quantity * po.Quantity;
                var demandBom = _context.CreateDemandProductionOrderBom(childBom.ArticleChildId, neededAmount);

                SatisfyRequest(demandBom, po);
            }
        }

        private IDemandToProvider GetNextByActivitySlack(List<DemandToProvider> demandRequester, int time)
        {
            if (!demandRequester.Any()) return null;
            DemandToProvider mostUrgentRequester = null;
            foreach (var demand in demandRequester)
            {
                var dueTime = _context.GetDueTimeByOrder(demand);
                if (mostUrgentRequester == null || _context.GetDueTimeByOrder(mostUrgentRequester) > dueTime)
                    mostUrgentRequester = demand;
            }

            var lowestActivitySlack = _context.GetDueTimeByOrder(mostUrgentRequester);
            foreach (var singleRequester in demandRequester)
            {
                var activitySlack = GetActivitySlack(singleRequester, time);
                if (activitySlack >= lowestActivitySlack) continue;
                lowestActivitySlack = activitySlack;
                mostUrgentRequester = singleRequester;
            }
            return mostUrgentRequester;
        }

        private int GetActivitySlack(IDemandToProvider demandRequester, int time)
        {
            var dueTime = 999999;
            if (demandRequester.DemandProvider != null) dueTime = _context.GetDueTimeByOrder((DemandToProvider)demandRequester);
            return dueTime - time;
        }
    }
}

