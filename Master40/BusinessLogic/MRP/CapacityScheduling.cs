using System.Collections.Generic;
using System.Linq;
using Master40.Models;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.Models;

namespace Master40.BusinessLogic.MRP
{
    internal interface ICapacityScheduling
    {
        void GifflerThompsonScheduling();
    }

    internal class CapacityScheduling : ICapacityScheduling
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public CapacityScheduling(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
            _context = context;
        }
        public void GifflerThompsonScheduling()
        {
            var productionOrderWorkSchedules = GetSchedules();
            productionOrderWorkSchedules = CalculateWorkTimeWithParents(productionOrderWorkSchedules);

            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            var plannedSchedules = new List<ProductionOrderWorkSchedule>();
            GetInitialPlannables(productionOrderWorkSchedules,plannedSchedules, plannableSchedules);

            while (plannableSchedules.Any())
            {
                //find next element by using the activity slack rule
                CalculateActivitySlack(plannableSchedules);
                var shortest = GetShortest(plannableSchedules);
                
                //build conflict set
                var conflictSet = GetConflictSet(shortest, plannableSchedules, productionOrderWorkSchedules);

                //set starttimes of conflicts after the shortest process
                SolveConflicts(shortest, conflictSet, productionOrderWorkSchedules);
                shortest.End = shortest.Start + shortest.Duration;

                plannedSchedules.Add(shortest);
                plannableSchedules.Remove(shortest);

                //search for parent and if available and allowed add it to the schedule
                var parent = GetParent(shortest, productionOrderWorkSchedules);
                if (parent != null && !plannableSchedules.Contains(parent) && IsTechnologicallyAllowed(parent,plannedSchedules)) plannableSchedules.Add(parent);
                _context.ProductionOrderWorkSchedule.Update(shortest);
                _context.SaveChanges();
            }
        }

        private bool IsTechnologicallyAllowed(ProductionOrderWorkSchedule schedule, List<ProductionOrderWorkSchedule> plannedSchedules)
        {
            var isAllowed = true;
            //check for every child if its planned
            foreach (var bom in schedule.ProductionOrder.ProductionOrderBoms)
            {
                if (bom.ProductionOrderChildId != schedule.ProductionOrderId)
                {
                    foreach (var childSchedule in bom.ProductionOrderChild.ProductionOrderWorkSchedule)
                    {
                        if (!plannedSchedules.Contains(childSchedule)) isAllowed = false;
                    }
                }
            }
            return isAllowed;
        }

        private List<ProductionOrderWorkSchedule> GetConflictSet(ProductionOrderWorkSchedule shortest,
            List<ProductionOrderWorkSchedule> plannableSchedules, List<ProductionOrderWorkSchedule> productionOrderWorkSchedules )
        {
            var conflictSet = new List<ProductionOrderWorkSchedule>();

            foreach (var plannableSchedule in plannableSchedules)
            {
                if (plannableSchedule.MachineGroupId == shortest.MachineGroupId && !plannableSchedule.Equals(shortest))
                    conflictSet.Add(plannableSchedule);
            }
            var parent = GetParent(shortest, productionOrderWorkSchedules);
            if (parent != null)
                conflictSet.Add(parent);
            return conflictSet;
        }

        private void SolveConflicts(ProductionOrderWorkSchedule shortest, List<ProductionOrderWorkSchedule> conflictSet,
            List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {

            foreach (var conflict in conflictSet)
            {
                var index = productionOrderWorkSchedules.IndexOf(conflict);
                if (shortest.Start + shortest.Duration > productionOrderWorkSchedules[index].Start)
                {
                    productionOrderWorkSchedules[index].Start = shortest.Start + shortest.Duration + 1;
                }
            }
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
                var orderPartId = _context.Demands.OfType<DemandOrderPart>()
                                        .Single(a => a.DemandRequesterId == plannableSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequesterId)
                                        .OrderPartId;
                var dueTime = _context.OrderParts
                                .Include(a => a.Order)
                                .Single(a => a.Id == orderPartId)
                                .Order
                                .DueTime;

                //get remaining time
                plannableSchedule.ActivitySlack = dueTime - plannableSchedule.WorkTimeWithParents - plannableSchedule.Start;
                _context.ProductionOrderWorkSchedule.Update(plannableSchedule);
                
            }
            _context.SaveChanges();
        }

        private int GetRemainTimeFromParents(ProductionOrderWorkSchedule schedule, List <ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var parent = GetParent(schedule,productionOrderWorkSchedules);
            if (parent == null) return schedule.Duration;
            
            return GetRemainTimeFromParents(parent, productionOrderWorkSchedules) + schedule.Duration;
        }

        private List<ProductionOrderWorkSchedule> GetSchedules()
        {
            var demandRequester = _context.Demands.Where(b => b.State == State.SchedulesExist).ToList();
           
            var productionOrderWorkSchedule = new List<ProductionOrderWorkSchedule>();
            foreach (var demandReq in demandRequester)
            {
                var schedules = GetProductionSchedules(demandReq);
                foreach (var schedule in schedules)
                {
                    productionOrderWorkSchedule.Add(schedule);
                }
            }
            return productionOrderWorkSchedule;
        }

        private List<ProductionOrderWorkSchedule> GetProductionSchedules(IDemandToProvider requester)
        {
            var provider =
                _context.Demands.OfType<DemandProviderProductionOrder>().Include(a => a.ProductionOrder).ThenInclude(b => b.ProductionOrderBoms)
                    .Where(a => a.DemandRequesterId == requester.Id)
                    .ToList();
            var schedules = new List<ProductionOrderWorkSchedule>();
            foreach (var prov in provider)
            {
                foreach (var schedule in prov.ProductionOrder.ProductionOrderWorkSchedule)
                {
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

    }

   
    
}
