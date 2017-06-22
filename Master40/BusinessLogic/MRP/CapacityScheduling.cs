using System.Collections.Generic;
using System.Linq;
using Master40.Models;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using Master40.Extensions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Clauses;

namespace Master40.BusinessLogic.MRP
{//Todo: multi-machines fix
    public interface ICapacityScheduling
    {
        void GifflerThompsonScheduling(int timer);
        List<MachineGroupProductionOrderWorkSchedule> CapacityRequirementsPlanning(int timer);
        bool CapacityLevelingCheck(List<MachineGroupProductionOrderWorkSchedule> machineList);
        void SetMachines(int timer);
    }

    public class CapacityScheduling : ICapacityScheduling
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public CapacityScheduling(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
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
                    var machines = _context.Machines.Where(a => a.MachineGroupId == machine.MachineGroupId);
                    if (!machines.Any()) continue;
                    if (machines.Count() < hour.ProductionOrderWorkSchedules.Count)
                        return true;
                }
            }
            
            return false;
        }

        private List<ProductionOrderWorkSchedulesByTimeStep> AddToMachineGroup(MachineGroupProductionOrderWorkSchedule machine, ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
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

        private void CalculateActivitySlack(List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            foreach (var plannableSchedule in plannableSchedules)
            {
                //get duetime
                var demand = _context.Demands.AsNoTracking().Single(a => a.Id == plannableSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequesterId);
                var dueTime = 9999;
                if (demand.GetType() == typeof(DemandOrderPart))
                {
                    dueTime = _context.OrderParts
                                .Include(a => a.Order)
                                .Single(a => a.Id == ((DemandOrderPart)demand).OrderPartId)
                                .Order
                                .DueTime;
                }

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
                    .Where(a => a.DemandRequester.DemandRequesterId == requester.Id);
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
            foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderChildId == plannedSchedule.ProductionOrderId))
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
    }
}
