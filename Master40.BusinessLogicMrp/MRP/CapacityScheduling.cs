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
        public void GifflerThompsonScheduling(int timer)
        {
            var productionOrderWorkSchedules = GetProductionSchedules(timer);
            ResetStartEnd(productionOrderWorkSchedules);
            productionOrderWorkSchedules = CalculateWorkTimeWithParents(productionOrderWorkSchedules);

            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            var plannedSchedules = GetInitialPlannedSchedules(timer) ?? new List<ProductionOrderWorkSchedule>();
            GetInitialPlannables(productionOrderWorkSchedules,plannedSchedules, plannableSchedules,timer);
            while (plannableSchedules.Any())
            {
                //find next element by using the activity slack rule
                CalculateActivitySlack(plannableSchedules);
                var shortest = GetShortest(plannableSchedules);

                plannableSchedules.Remove(shortest);
                //Add a fix spot on a machine with start/end
                AddMachine(plannedSchedules, shortest);
                plannedSchedules.Add(shortest);

                //search for parent and if available and allowed add it to the schedule
                var parent = GetParent(shortest, productionOrderWorkSchedules);
                if (parent != null && !plannableSchedules.Contains(parent) && IsTechnologicallyAllowed(parent,plannedSchedules, timer))
                    plannableSchedules.Add(parent);
                _context.SaveChanges();
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
            //Get Children
            List<ProductionOrder> poChildren = new List<ProductionOrder>();
            List<ProductionOrderWorkSchedule> powsChildren = new List<ProductionOrderWorkSchedule>();
            if (shortest.HierarchyNumber > 10)
            {
                var children = _context.ProductionOrderWorkSchedules.Where(a =>
                    a.ProductionOrderId == shortest.ProductionOrderId &&
                    a.HierarchyNumber < shortest.HierarchyNumber).ToList();
                if (children.Any())
                {
                    powsChildren = children.Where(a => a.HierarchyNumber == children.Max(b => b.HierarchyNumber)).ToList();
                }
            }
            else
            {
                var childrenBoms = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == shortest.ProductionOrderId).ToList();
                poChildren = (from boms in childrenBoms
                            join po in _context.ProductionOrders on boms.ProductionOrderChildId equals po.Id
                            select po).ToList();
            }
            //iterate to find the latest end
            if (poChildren.Any())
                    return (from child in poChildren
                        from pows in child.ProductionOrderWorkSchedule
                        select pows.End).Concat(new[] {0}).Max();
            return powsChildren.Any() ? (from powsChild in powsChildren select powsChild.End).Concat(new[] {0}).Max() : 0;
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
            var productionOrderWorkSchedules = GetProductionSchedules(timer);
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

        private ProductionOrderWorkSchedule GetParent(ProductionOrderWorkSchedule schedule,List<ProductionOrderWorkSchedule> productionOrderWorkSchedules )
        {
            var parent = FindHierarchyParent(productionOrderWorkSchedules, schedule) ?? FindBomParent(schedule);
            return parent;
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
            var parent = GetParent(schedule,productionOrderWorkSchedules);
            if (parent == null) return schedule.Duration;
            
            return GetRemainTimeFromParents(parent, productionOrderWorkSchedules) + schedule.Duration;
        }

        private decimal GetAmountOfMachines(ProductionOrderWorkSchedule schedule)
        {
            return _context.Machines.Count(a => a.MachineGroupId == schedule.MachineGroupId);
        }

        private List<ProductionOrderWorkSchedule> GetProductionSchedules(int timer)
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
                pows.AddRange(_context.GetProductionOrderWorkSchedules(singleDemandRequester));
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
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                //check if child is planned
                if (IsTechnologicallyAllowed(productionOrderWorkSchedule, plannedSchedules, timer))
                {
                    plannableSchedules.Add(productionOrderWorkSchedule);
                }
                /*if (timer > 0 && ChildrenAreInProduction(productionOrderWorkSchedule, timer))
                {
                    plannableSchedules.Add(productionOrderWorkSchedule);
                    continue;
                }*/
                /*
                //var hasChildren = productionOrderWorkSchedule.ProductionOrder.ProdProductionOrderBomChilds.Any(bom => bom.ProductionOrderParent.Id == productionOrderWorkSchedule.ProductionOrderId);
                var hasChildren = productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms.Any(bom => bom.ProductionOrderParentId == productionOrderWorkSchedule.ProductionOrderId);
                if (hasChildren)
                {
                    
                    continue;
                }
                //find out if its the lowest element in hierarchy
                var isLowestHierarchy = productionOrderWorkSchedules.All(mainSchedule => mainSchedule.HierarchyNumber >= productionOrderWorkSchedule.HierarchyNumber);
                if (isLowestHierarchy && !plannedSchedules.Contains(productionOrderWorkSchedule) && !plannableSchedules.Contains(productionOrderWorkSchedule))
                   plannableSchedules.Add(productionOrderWorkSchedule);
                   */
            }
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
            if (child == null) return GetLatestBomChilds(productionOrderWorkSchedule);
            children.Add(child);
            return children;
        }

        private List<ProductionOrderWorkSchedule> GetBomChilds(
            ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var boms = productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms.Where(a => a.ProductionOrderParent.Id == productionOrderWorkSchedule.ProductionOrderId).ToList();
            return !boms.Any() ? null : boms.Select(bom => bom.ProductionOrderChild.ProductionOrderWorkSchedule.Single(a => a.HierarchyNumber == bom.ProductionOrderChild.ProductionOrderWorkSchedule.Max(b => b.HierarchyNumber))).ToList();
        }

        private List<ProductionOrderWorkSchedule> GetLatestBomChilds(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var boms = productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms.Where(a => a.ProductionOrderParent.Id == productionOrderWorkSchedule.ProductionOrderId).ToList();
            if (!boms.Any()) return null;
            var pows = boms.Select(bom => bom.ProductionOrderChild.ProductionOrderWorkSchedule.Single(a => a.HierarchyNumber == bom.ProductionOrderChild.ProductionOrderWorkSchedule.Max(b => b.HierarchyNumber))).ToList();
            //find latest childs
            return pows.Where(a => a.End == pows.Max(b => b.End)).ToList();
        }

        private ProductionOrderWorkSchedule GetHierarchyChild(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var productionOrderWorkSchedules = _context.ProductionOrderWorkSchedules.AsNoTracking().Where(a => a.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId);
            productionOrderWorkSchedules = productionOrderWorkSchedules.Where(mainSchedule => mainSchedule.HierarchyNumber < productionOrderWorkSchedule.HierarchyNumber);
            if (!productionOrderWorkSchedules.Any()) return null;
            return productionOrderWorkSchedules.Single(a => a.HierarchyNumber == productionOrderWorkSchedules.Max(b => b.HierarchyNumber));
        }

        private ProductionOrderWorkSchedule FindHierarchyParent(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, ProductionOrderWorkSchedule plannedSchedule  )
        {

            ProductionOrderWorkSchedule hierarchyParent = null;
            int hierarchyParentNumber = 100000;

            //find next higher element
            foreach (var mainSchedule in productionOrderWorkSchedules)
            {
                if (mainSchedule.ProductionOrderId != plannedSchedule.ProductionOrderId) continue;
                if (mainSchedule.HierarchyNumber <= plannedSchedule.HierarchyNumber ||
                    mainSchedule.HierarchyNumber >= hierarchyParentNumber) continue;
                hierarchyParent = mainSchedule;
                hierarchyParentNumber = mainSchedule.HierarchyNumber;
            }
            return hierarchyParent;
        }

        private ProductionOrderWorkSchedule FindBomParent(ProductionOrderWorkSchedule plannedSchedule)
        {
            ProductionOrderWorkSchedule lowestHierarchyMember = null;
            foreach (var pob in _context.ProductionOrderBoms
                                        .Include(a => a.ProductionOrderParent)
                                        .ThenInclude(b => b.ProductionOrderWorkSchedule)
                                        .Where(a => a.ProductionOrderChildId == plannedSchedule.ProductionOrderId).ToList())
            {
                if (pob.ProductionOrderParentId == plannedSchedule.ProductionOrder.Id) continue;
                var parents = pob.ProductionOrderParent.ProductionOrderWorkSchedule;
                lowestHierarchyMember = parents.First();
                //find lowest hierarchy
                foreach (var parent in parents)
                {
                    if (parent.HierarchyNumber < lowestHierarchyMember.HierarchyNumber)
                        lowestHierarchyMember = parent;
                }
                break;
            }
            return lowestHierarchyMember;
        }

        public void SetMachines(int timer)
        {
            //gets called when plan is fitting to capacities
            var schedules = GetProductionSchedules(timer);
            foreach (var schedule in schedules)
            {
                var machines = _context.Machines.Where(a => a.MachineGroupId == schedule.MachineGroupId).ToList();
                if (!machines.Any()) continue;
                var schedulesOnMachineGroup = schedules.FindAll(a => a.MachineGroupId == schedule.MachineGroupId && a.MachineId != null);
                var crossingPows = (from scheduleMg in schedulesOnMachineGroup where DetectCrossing(schedule, scheduleMg) select schedule).ToList();
                if (!crossingPows.Any()) schedule.MachineId = machines.First().Id;
                else
                {
                    for (var i = 0; i < machines.Count(); i++)
                    {
                        if (crossingPows.Find(a => a.MachineId == machines[i].Id) != null) continue;
                        schedule.MachineId = machines[i].Id;
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
        {/*
            //delete all demands but the head-demands
            _context.Demands.RemoveRange(_context.Demands.Where(a => a.DemandRequesterId != null && a.State != State.Delivered).ToList());
            _context.SaveChanges();
            var requester = _context.Demands.Where(a => null == a.DemandRequesterId && a.State != State.Delivered).ToList();

            //rebuild by using activity-slack
            while (requester.Any())
            {
                var nextRequester = GetNextByActivitySlack(requester, time);
                SatisfyRequest(nextRequester, nextRequester);
                requester.Remove(requester.Find(a => a.Id == nextRequester.Id));
            }*/
           
        }
        /*
        private void SatisfyRequest(IDemandToProvider demand, IDemandToProvider parent)
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

            var possibleMatchingProductionOrders = _context.ProductionOrders.Where(a => a.ArticleId == demand.ArticleId).ToList();
            
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
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignProductionOrderToDemandProvider(earliestProductionOrder, provider, amount >= availableAmountFromProductionOrder ? availableAmountFromProductionOrder : amount);
                _context.TryAssignProductionOrderBomToDemandProvider(earliestProductionOrder,demand);
                //Todo: assign pob(s) to dpob
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
                CallChildrenSatisfyRequest(demand);
                return;
            }
           
            if (_context.HasChildren(demand))
            {
                var productionOrder = _context.CreateProductionOrder(demand, _context.GetDueTime(parent));
                _context.TryCreateProductionOrderBoms(demand, productionOrder, parent);
                _context.CreateProductionOrderWorkSchedules(productionOrder);
                _context.AssignProductionOrderWorkSchedulesToProductionOrder(productionOrder);
                var provider = _context.CreateProviderProductionOrder(demand, productionOrder, amount);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignProductionOrderToDemandProvider(productionOrder,provider,productionOrder.Quantity);
                _context.TryAssignProductionOrderBomToDemandProvider(productionOrder, demand);
            }
            else
            {
                var purchasePart = _context.CreatePurchase(demand, amount);
                var provider = _context.CreateProviderPurchase(demand, purchasePart, amount);
                _context.AssignProviderToDemand(demand, provider);
                _context.AssignPurchaseToDemandProvider(purchasePart, provider, purchasePart.Quantity);
            }
            
            CallChildrenSatisfyRequest(demand);
        }

        
        private void CallChildrenSatisfyRequest(IDemandToProvider demand)
        {
            //call method for each child
            var childrenArticleBoms = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == demand.ArticleId).ToList();
            foreach (var childBom in childrenArticleBoms)
            {
                IDemandToProvider requester;
                if (demand.DemandRequester == null)
                {
                    requester = demand;
                }
                else
                {
                    requester = demand.DemandRequester.DemandRequester ?? demand.DemandRequester;
                }
                var demandBom = _context.CreateDemandProductionOrderBom(childBom, childBom.Quantity, requester );
                
                SatisfyRequest(demandBom, demand);
            }
        }

        private IDemandToProvider GetNextByActivitySlack(List<DemandToProvider> demandRequester, int time)
        {
            if (!demandRequester.Any()) return null;
            //Todo: first durch Rechnung oder so ersetzen
            IDemandToProvider mostUrgentRequester = null;
            foreach (var demand in demandRequester)
            {
                var dueTime = GetDueTime(demand);
                if (mostUrgentRequester == null || GetDueTime(mostUrgentRequester) > dueTime)
                    mostUrgentRequester = demand;
            }

            var lowestActivitySlack = GetDueTime(mostUrgentRequester);
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
            if (demandRequester.DemandProvider != null) dueTime = demandRequester.DemandProvider.First().
            return dueTime - time;
        }*/
    }
}

