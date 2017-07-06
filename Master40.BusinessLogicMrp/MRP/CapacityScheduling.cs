using System.Collections.Generic;
using System.Linq;
using Master40.BusinessLogicCentral.HelperCapacityPlanning;
using Master40.DB.Data.Context;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Helper;
using System;
using Master40.DB.DB.Interfaces;

namespace Master40.BusinessLogicCentral.MRP
{//Todo: multi-machines fix
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
        private readonly MasterDBContext _context;
        public CapacityScheduling(MasterDBContext context)
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
            var plannedSchedules = new List<ProductionOrderWorkSchedule>();
            GetInitialPlannables(productionOrderWorkSchedules,plannedSchedules, plannableSchedules);

            while (plannableSchedules.Any())
            {
                //find next element by using the activity slack rule
                CalculateActivitySlack(plannableSchedules);
                var shortest = GetShortest(plannableSchedules);

                plannableSchedules.Remove(shortest);
                //Add a fix spot on a machine with start/end
                var shortestList = AddMachineToPows(plannedSchedules, shortest);
                plannedSchedules.AddRange(shortestList);

                //search for parent and if available and allowed add it to the schedule
                var parent = GetParent(shortest, productionOrderWorkSchedules);
                if (parent != null && !plannableSchedules.Contains(parent) && IsTechnologicallyAllowed(parent,plannedSchedules))
                    plannableSchedules.Add(parent);
                _context.SaveChanges();
            }

            //_context.ProductionOrderWorkSchedule.UpdateRange(plannedSchedules);
            //_context.SaveChanges();
            //SetMachines(timer);
        }

        private List<ProductionOrderWorkSchedule> AddMachineToPows(List<ProductionOrderWorkSchedule> plannedSchedules, ProductionOrderWorkSchedule shortest)
        {
            var pows = new List<ProductionOrderWorkSchedule>();
            ProductionOrderWorkSchedule lastPows=null;
            for (var i = 0;i < shortest.ProductionOrder.Quantity;i++)
            {
                var currentPows = AddMachine(plannedSchedules, pows, CreateNewPows(shortest));
                if (lastPows == null)
                {
                    lastPows = currentPows;
                    pows.Add(currentPows);
                    shortest.MachineId = currentPows.MachineId;
                    shortest.Start = currentPows.Start;
                    shortest.End = currentPows.End;
                    _context.ProductionOrderWorkSchedule.Update(shortest);
                    _context.SaveChanges();
                    continue;
                }
                ProductionOrderWorkSchedule foundSchedule = null;
                //check if before pows belongs to this process
                foreach (var schedule in pows)
                {
                    if (currentPows.MachineId == schedule.MachineId && currentPows.Start == schedule.End)
                    {
                        foundSchedule = schedule;
                        break;
                    }
                        
                }
                if (foundSchedule != null)
                {
                    pows[pows.IndexOf(foundSchedule)].End += currentPows.Duration;
                    pows[pows.IndexOf(foundSchedule)].Duration += currentPows.Duration;
                }
                else
                {
                    var pow = CreateNewPows(currentPows);
                    pows.Add(pow);
                }
                
                lastPows = currentPows;
            }
            if (pows.Any())
            {
                shortest.End = pows.First().End;
                shortest.Duration = pows.First().Duration;
                _context.Update(shortest);
                _context.AddRange(pows.GetRange(pows.IndexOf(pows.First()) + 1, pows.Count - 1));
                _context.SaveChanges();
            }
           
            return pows;
        }

        private ProductionOrderWorkSchedule CreateNewPows(ProductionOrderWorkSchedule currentPows)
        {
            ProductionOrderWorkSchedule item = new ProductionOrderWorkSchedule();
            currentPows.CopyPropertiesTo(item);
            return item;
            /*
            return new ProductionOrderWorkSchedule()
            {
                Duration = currentPows.Duration,
                MachineId = currentPows.MachineId,
                EndForward = currentPows.EndForward,
                EndBackward = currentPows.EndBackward,
                End = currentPows.End,
                StartForward = currentPows.StartForward,
                StartBackward = currentPows.StartBackward,
                Start = currentPows.Start,
                HierarchyNumber = currentPows.HierarchyNumber,
                ProductionOrderId = currentPows.ProductionOrderId,
                MachineGroupId = currentPows.MachineGroupId,
                MachineToolId = currentPows.MachineToolId,
                WorkTimeWithParents = currentPows.WorkTimeWithParents,
                Name = currentPows.Name,
                ActivitySlack = currentPows.ActivitySlack
            };
            */
        }

        private ProductionOrderWorkSchedule AddMachine(List<ProductionOrderWorkSchedule> plannedSchedules,List<ProductionOrderWorkSchedule> pows, ProductionOrderWorkSchedule shortest)
        {
            var machines = _context.Machines.Where(a => a.MachineGroupId == shortest.MachineGroupId).ToList();
            if (machines.Count == 1)
            {
                shortest.Start = GetChildEndTime(shortest);
                shortest.End = shortest.Start + shortest.Duration;
                var earliestPlanned = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliestPows = FindStartOnMachine(pows, machines.First().Id, shortest);
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
                var earliestPows = FindStartOnMachine(pows, machines.First().Id, shortest);
                var earliest = (earliestPlanned > earliestPows) ? earliestPlanned : earliestPows;
                var earliestMachine = machines.First();
                foreach (var machine in machines)
                {
                    earliestPlanned = FindStartOnMachine(plannedSchedules, machine.Id, shortest);
                    earliestPows = FindStartOnMachine(pows, machine.Id, shortest);
                    var earliestThisMachine = (earliestPlanned > earliestPows) ? earliestPlanned : earliestPows;
                    if (earliest <= earliestThisMachine) continue;
                    earliest = earliestThisMachine;
                    earliestMachine = machine;
                }
                
                
                if (shortest.Start < earliest)
                    shortest.Start = earliest;
                shortest.MachineId = earliestMachine.Id;
                shortest.End = shortest.Start + shortest.Duration;
            }
            return shortest;
        }

        private int GetChildEndTime(ProductionOrderWorkSchedule shortest)
        {
            //Get Children
            List<ProductionOrder> poChildren = new List<ProductionOrder>();
            List<ProductionOrderWorkSchedule> powsChildren = new List<ProductionOrderWorkSchedule>();
            if (shortest.HierarchyNumber > 10)
            {
                var children = _context.ProductionOrderWorkSchedule.Where(a =>
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
                    return Enumerable.Concat((from child in poChildren
                        from pows in child.ProductionOrderWorkSchedule
                        select pows.End), new[] {0}).Max();
            return powsChildren.Any() ? Enumerable.Concat((from powsChild in powsChildren select powsChild.End), new[] {0}).Max() : 0;
        }

        private int FindStartOnMachine(List<ProductionOrderWorkSchedule> plannedSchedules, int machineId, ProductionOrderWorkSchedule shortest)
        {
            for (var i = plannedSchedules.Count-1; i >= 0; i--)
            {
                if (plannedSchedules[i].MachineId == machineId)
                {
                    return detectCrossing(plannedSchedules[i], shortest) ? plannedSchedules[i].End : (plannedSchedules[i].End>shortest.Start ? plannedSchedules[i].End : shortest.Start);
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
                        var schedule = new MachineGroupProductionOrderWorkSchedule()
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
                    var timestep = new ProductionOrderWorkSchedulesByTimeStep()
                        {
                            Time = i,
                            ProductionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>()
                            {
                                productionOrderWorkSchedule
                            }
                        };
                    machine.ProductionOrderWorkSchedulesByTimeSteps.Add(timestep);
                }
                    
            }
            return machine.ProductionOrderWorkSchedulesByTimeSteps;
        }

       

        private bool IsTechnologicallyAllowed(ProductionOrderWorkSchedule schedule, List<ProductionOrderWorkSchedule> plannedSchedules)
        {//Todo check effectiveness
            var isAllowed = true;
            //check for every child if its planned
            foreach (var bom in schedule.ProductionOrder.ProductionOrderBoms)
            {
                if (bom.ProductionOrderChildId != schedule.ProductionOrderId)
                {
                    foreach (var childSchedule in bom.ProductionOrderChild.ProductionOrderWorkSchedule)
                    {
                        if (!plannedSchedules.Any(a => a.ProductionOrderId == childSchedule.ProductionOrderId && a.HierarchyNumber == childSchedule.HierarchyNumber)) isAllowed = false;
                    }
                }
            }
            return isAllowed;
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
            return schedules;
        }

        private int GetDueTime(IDemandToProvider demand)
        {
            var dueTime = 9999;
            if (demand.GetType() == typeof(DemandOrderPart))
            {
                dueTime = _context.OrderParts
                            .Include(a => a.Order)
                            .Single(a => a.Id == ((DemandOrderPart)demand).OrderPartId)
                            .Order
                            .DueTime;
            }
            return dueTime;
        }

        private void CalculateActivitySlack(List<ProductionOrderWorkSchedule> plannableSchedules)
        { //replace provider.first()
            foreach (var plannableSchedule in plannableSchedules)
            {
                //get duetime
                var demands = _context.Demands.AsNoTracking().Where(a => a.Id == plannableSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequesterId
                || a.Id == plannableSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequesterId).ToList();
                var demand = (from d in demands where d.DemandRequesterId == null select d).Single();
                var dueTime = GetDueTime(demand);

                //get remaining time
                var slack = plannableSchedule.ActivitySlack;
                plannableSchedule.ActivitySlack = dueTime - plannableSchedule.WorkTimeWithParents - GetChildEndTime(plannableSchedule);
                if (slack == plannableSchedule.ActivitySlack) continue;
                _context.ProductionOrderWorkSchedule.Update(plannableSchedule);
                _context.SaveChanges();
            }
        }

        private decimal GetRemainTimeFromParents(ProductionOrderWorkSchedule schedule, List <ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var parent = GetParent(schedule,productionOrderWorkSchedules);
            if (parent == null) return (schedule.ProductionOrder.Quantity * schedule.Duration)/GetAmountOfMachines(schedule);
            
            return GetRemainTimeFromParents(parent, productionOrderWorkSchedules) + schedule.Duration * schedule.ProductionOrder.Quantity;
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
                                                            || b.State == State.ForwardScheduleExists)
                                                            .ToList();
           
            var productionOrderWorkSchedule = new List<ProductionOrderWorkSchedule>();
            foreach (var demandReq in demandRequester)
            {
                var schedules = GetProductionSchedules(demandReq, timer);
                foreach (var schedule in schedules)
                {
                    productionOrderWorkSchedule.Add(schedule);
                }
            }
            return productionOrderWorkSchedule;
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
            var schedules = new List<ProductionOrderWorkSchedule>();
            foreach (var prov in provider)
            {
                foreach (var schedule in prov.ProductionOrder.ProductionOrderWorkSchedule)
                {
                    if (schedule.Start >= timer)
                        schedules.Add(schedule);
                }
            }
            return schedules;
        }

        private void GetInitialPlannables(
            List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, List<ProductionOrderWorkSchedule> plannedSchedules, List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                var hasChildren = false;
                foreach (var bom in productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms)
                {
                    if (bom.ProductionOrderParent.Id == productionOrderWorkSchedule.ProductionOrderId)
                    {
                        hasChildren = true;
                        break;
                    }
                }
                if (hasChildren)
                    continue;
                //find out if its the lowest element in hierarchy
                var isLowestHierarchy = true;
                foreach (var mainSchedule in productionOrderWorkSchedules)
                {
                    if (mainSchedule.HierarchyNumber < productionOrderWorkSchedule.HierarchyNumber)
                        isLowestHierarchy = false;
                }
                if (isLowestHierarchy && !plannedSchedules.Contains(productionOrderWorkSchedule) && !plannableSchedules.Contains(productionOrderWorkSchedule))
                   plannableSchedules.Add(productionOrderWorkSchedule);

            }
        }

        private ProductionOrderWorkSchedule FindHierarchyParent(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, ProductionOrderWorkSchedule plannedSchedule  )
        {

            ProductionOrderWorkSchedule hierarchyParent = null;
            int hierarchyParentNumber = 100000;

            //find next higher element
            foreach (var mainSchedule in productionOrderWorkSchedules)
            {
                if (mainSchedule.ProductionOrderId == plannedSchedule.ProductionOrderId)
                {
                    if (mainSchedule.HierarchyNumber > plannedSchedule.HierarchyNumber &&
                        mainSchedule.HierarchyNumber < hierarchyParentNumber)
                    {
                        hierarchyParent = mainSchedule;
                        hierarchyParentNumber = mainSchedule.HierarchyNumber;
                    }
                }

            }
            return hierarchyParent;
        }

        private ProductionOrderWorkSchedule FindBomParent(ProductionOrderWorkSchedule plannedSchedule)
        {
            ProductionOrderWorkSchedule lowestHierarchyMember = null;
            foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderChildId == plannedSchedule.ProductionOrderId).ToList())
            {
                //check if its the head element which points to itself
                if (pob.ProductionOrderParentId != plannedSchedule.ProductionOrder.Id)
                {
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
                var crossingPows = new List<ProductionOrderWorkSchedule>();
                foreach (var scheduleMg in schedulesOnMachineGroup)
                {
                    if (detectCrossing(schedule, scheduleMg))
                        crossingPows.Add(schedule);
                }
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

        private bool detectCrossing(ProductionOrderWorkSchedule schedule, ProductionOrderWorkSchedule scheduleMg)
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
            _context.Demands.RemoveRange(_context.Demands.Where(a => a.DemandRequesterId != null && a.State != State.Delivered).ToList());
            _context.SaveChanges();
            var requester = _context.Demands.Where(a => null == a.DemandRequesterId && a.State != State.Delivered).ToList();

            //rebuild by using activity-slack
            while (requester.Any())
            {
                var nextRequester = GetNextByActivitySlack(requester, time);
                SatisfyRequest(nextRequester, nextRequester);
                requester.Remove(requester.Find(a => a.Id == nextRequester.Id));
            }
           
        }

        private void SatisfyRequest(IDemandToProvider demand, IDemandToProvider requester)
        {//Todo: search for purchases
            
            var amount = demand.Quantity;
            
            //if anything is in stock, create demand
            var articleStock = _context.Stocks.Single(a => a.ArticleForeignKey==demand.ArticleId).Current;
            if (articleStock > 0)
            {
                var provider = TryCreateStockReservation(demand);
                if (provider != null)
                {
                    amount -= provider.Quantity;
                }
            }
            if (amount == 0)
            {
                CallChildrenSatisfyRequest(demand, requester);
                return;
            }
            

            var possibleMatchingProductionOrders = _context.ProductionOrders.Where(a => a.ArticleId == demand.ArticleId).ToList();
            
            while (amount > 0 && possibleMatchingProductionOrders.Any())
            {
                var earliestProductionOrder = GetEarliestProductionOrder(possibleMatchingProductionOrders);
                var availableAmountFromProductionOrder = GetAvailableAmountFromProductionOrder(earliestProductionOrder);
                if (availableAmountFromProductionOrder == 0)
                {
                    possibleMatchingProductionOrders.Remove(possibleMatchingProductionOrders.Find(a => a.Id == earliestProductionOrder.Id));
                    continue;
                }
                var provider = CreateProviderProductionOrder(requester, earliestProductionOrder, amount >= availableAmountFromProductionOrder ? availableAmountFromProductionOrder : amount);
                AssignProviderToDemand(demand, provider);
                AssignProductionOrderToDemandProvider(earliestProductionOrder, provider, amount >= availableAmountFromProductionOrder ? availableAmountFromProductionOrder : amount);
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
                CallChildrenSatisfyRequest(demand, requester);
                return;
            }
           
            if (HasChildren(demand))
            {
                var productionOrder = CreateProductionOrder(demand.ArticleId, amount);
                TryCreateProductionOrderBom(demand, productionOrder);
                CreateProductionOrderWorkSchedules(productionOrder);
                AssignProductionOrderWorkSchedulesToProductionOrder(productionOrder);
                var provider = CreateProviderProductionOrder(requester, productionOrder, amount);
                AssignProviderToDemand(demand, provider);
                AssignProductionOrderToDemandProvider(productionOrder,provider,productionOrder.Quantity);
            }
            else
            {
                var purchasePart = CreatePurchase(demand, amount);
                var provider = CreateProviderPurchase(demand, purchasePart, amount);
                AssignProviderToDemand(demand, provider);
                AssignPurchaseToDemandProvider(purchasePart, provider, purchasePart.Quantity);
            }
            
            CallChildrenSatisfyRequest(demand, requester);
        }

        private void AssignProviderToDemand(IDemandToProvider demand, DemandToProvider provider)
        {
            demand.DemandProvider.Add(provider);
            _context.Update(demand);
            _context.SaveChanges();
        }

        private void TryCreateProductionOrderBom(IDemandToProvider demand, ProductionOrder productionOrder)
        {//Todo: pob n:m parents and children
            var bom = _context.ArticleBoms.Where(a => a.ArticleChildId == demand.ArticleId);
            if (!bom.Any()) return;
            var id = demand.DemandRequesterId ?? demand.Id;
            var demands = _context.Demands.Where(a => a.DemandRequesterId == id || a.Id == id).ToList();
            var parentDemand = (from d in demands
                                where d.ArticleId == demand.ArticleId 
                                && (d.GetType() == typeof(DemandOrderPart) 
                                    || d.GetType() == typeof(DemandStock) 
                                    || d.GetType() == typeof(DemandProductionOrderBom))
                                select d)
                                .Single();
            var pob = new ProductionOrderBom()
            {
                ProductionOrderChildId = productionOrder.Id,
                Quantity = parentDemand.Quantity,
                
            };
            foreach (var provider in parentDemand.DemandProvider)
            {
                if (typeof(DemandProviderProductionOrder) == provider.GetType())
                pob.ProductionOrderParentId =
                    ((DemandProviderProductionOrder) provider).ProductionOrderId;
            }
            _context.Add(pob);
            _context.SaveChanges();
        }

        private void AssignProductionOrderWorkSchedulesToProductionOrder(ProductionOrder productionOrder)
        {
            foreach (var pows in _context.ProductionOrderWorkSchedule.Where(a => a.ProductionOrderId == productionOrder.Id))
            {
                productionOrder.ProductionOrderWorkSchedule.Add(pows);
            }
            _context.Update(productionOrder);
            _context.SaveChanges();
        }

        private void CreateProductionOrderWorkSchedules(ProductionOrder productionOrder)
        {
            var abstractWorkSchedules = _context.WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workSchedule = new ProductionOrderWorkSchedule();
                abstractWorkSchedule.CopyPropertiesTo<IWorkSchedule>(workSchedule);
                workSchedule.ProductionOrderId = productionOrder.Id;
                workSchedule.MachineId = null;
                _context.ProductionOrderWorkSchedule.Add(workSchedule);
                _context.SaveChanges();
            }
        }

        private void AssignPurchaseToDemandProvider(PurchasePart purchasePart, DemandProviderPurchasePart provider, int quantity)
        {
            provider.PurchasePartId = purchasePart.Id;
            provider.Quantity = quantity;
            _context.Update(provider);
            _context.SaveChanges();
        }

        private ProductionOrder CreateProductionOrder(int demandArticleId, decimal amount)
        {
            var po = new ProductionOrder()
            {
                ArticleId = demandArticleId,
                Quantity = amount
            };
            _context.Add(po);
            _context.SaveChanges();
            return po;
        }
        private PurchasePart CreatePurchase(IDemandToProvider demand, decimal amount)
        {
            var articleToPurchase = _context.ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new Purchase()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Ceiling(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new PurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<DemandProviderPurchasePart>(),
                PurchaseId = purchase.Id
            };
            purchase.PurchaseParts = new List<PurchasePart>()
            {
                purchasePart
            };
            _context.Purchases.Add(purchase);
            _context.PurchaseParts.Add(purchasePart);
            _context.SaveChanges();
            return purchasePart;
        }

        private int GetAvailableAmountFromProductionOrder(ProductionOrder productionOrder)
        {
            return (int)productionOrder.Quantity - productionOrder.DemandProviderProductionOrders.Sum(provider => (int) provider.Quantity);
        }

        private ProductionOrder GetEarliestProductionOrder(List<ProductionOrder> productionOrders)
        {
            ProductionOrder earliestProductionOrder = null;
            foreach (var productionOrder in productionOrders)
            {
                if (earliestProductionOrder == null ||
                    productionOrder.ProductionOrderWorkSchedule.Min(a => a.Start) <
                    earliestProductionOrder.ProductionOrderWorkSchedule.Min(a => a.Start))
                    earliestProductionOrder = productionOrder;
            }
            return earliestProductionOrder;
        }

        private DemandProviderStock TryCreateStockReservation(IDemandToProvider demand)
        {
            var stock = _context.Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId);
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetAmountBought(demand.ArticleId);
            //get the current amount of free available articles
            var current = stock.Current + bought - stockReservations;
            decimal quantity;
            //either reserve all that are in stock or the amount needed
            quantity = demand.Quantity > current ? current : demand.Quantity;
            return quantity == 0 ? null : CreateDemandProviderStock(demand, quantity);
        }

        private decimal GetAmountBought(int articleId)
        {
            var purchaseParts = _context.PurchaseParts.Where(a => a.ArticleId == articleId);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
                purchasedAmount += purchasePart.Quantity;
            return purchasedAmount;
        }
        private decimal GetReserved(int articleId)
        {
            decimal amountReserved = 0;
            IQueryable<IDemandToProvider> reservations = _context.Demands.OfType<DemandProviderStock>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            //Todo check logic
            reservations = _context.Demands.OfType<DemandProviderPurchasePart>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            return amountReserved;
        }

        private void CallChildrenSatisfyRequest(IDemandToProvider demand, IDemandToProvider requester)
        {
            //call method for each child
            var demands = _context.Demands;
            var childrenArticleBoms = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == demand.Id).ToList();
            foreach (var childBom in childrenArticleBoms)
            {
                SatisfyRequest(CreateDemandProductionOrderBom(childBom, childBom.Quantity, demand), requester);
            }
        }

        private DemandProductionOrderBom CreateDemandProductionOrderBom(ArticleBom articleBom, decimal quantity, IDemandToProvider requester)
        {
            var dpob = new DemandProductionOrderBom()
            {
                Quantity = quantity,
                ArticleId = articleBom.ArticleChildId,
                State = State.Created,
                ProductionOrderBomId = articleBom.Id,
                DemandRequesterId = requester.Id,
                DemandProvider = new List<DemandToProvider>()
            };
            _context.Add(dpob);
            _context.SaveChanges();
            return dpob;
        }
        

        private void AssignProductionOrderToDemandProvider(ProductionOrder earliestProductionOrder, DemandProviderProductionOrder provider, decimal amount)
        {
            provider.ProductionOrderId = earliestProductionOrder.Id;
            provider.Quantity = amount;
            _context.Update(provider);
            _context.SaveChanges();
        }

        private bool HasChildren(IDemandToProvider demand)
        {
            return _context.ArticleBoms.Any(a => a.ArticleParentId == demand.ArticleId);
        }

        private DemandProviderStock CreateDemandProviderStock(IDemandToProvider demand, decimal amount)
        {
           var dps = new DemandProviderStock()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                StockId = _context.Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Id,
                DemandRequesterId = demand.Id,
            };
            _context.Add(dps);
            _context.SaveChanges();
            return dps;
        }
        private DemandProviderProductionOrder CreateProviderProductionOrder(IDemandToProvider demand, ProductionOrder productionOrder, decimal amount)
        {
            var dppo = new DemandProviderProductionOrder()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                ProductionOrderId = productionOrder.Id,
                DemandRequesterId = demand.DemandRequesterId,
            };
            _context.Add(dppo);
            _context.SaveChanges();
            return dppo;
        }

        private DemandProviderPurchasePart CreateProviderPurchase(IDemandToProvider demand, PurchasePart purchase, decimal amount)
        {
            var dppp = new DemandProviderPurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                PurchasePartId = purchase.Id,
                DemandRequesterId = demand.DemandRequesterId,
            };
            _context.Add(dppp);
            _context.SaveChanges();
            return dppp;
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
            
            //var mostUrgentRequester = demandRequester.FirstOrDefault(a => a.GetType() == typeof(DemandOrderPart) && ((DemandOrderPart)a.DemandRequester).OrderPart.Order.DueTime == demandRequester.Where(b => b.GetType() == typeof(DemandOrderPart)).Min(b =>((DemandOrderPart)b).OrderPart.Order.DueTime));
            //if (mostUrgentRequester == null)
            //    demandRequester.FirstOrDefault(a => a.GetType() == typeof(DemandStock));

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
            var dueTime = GetDueTime(demandRequester);
            return dueTime - time;
        }
    }
}
