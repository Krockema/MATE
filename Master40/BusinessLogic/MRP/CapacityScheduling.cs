using System.Collections.Generic;
using System.Linq;
using Master40.Data;
using Master40.Models;
using Microsoft.EntityFrameworkCore;
using Master40.Data.Context;
using Master40.DB.Models;

namespace Master40.BusinessLogic.MRP
{
    interface ICapacityScheduling
    {
        void GifflerThompsonScheduling();
    }

    class CapacityScheduling : ICapacityScheduling
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
            var schedule = GetSchedule();
            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            var plannedSchedules = new List<ProductionOrderWorkSchedule>();
            plannableSchedules = GetPlannables(schedule,plannedSchedules);
            while (plannableSchedules.Any())
            {
                foreach (var plannableSchedule in plannableSchedules)
                {
                    var msg = "Plannable elements: " + plannableSchedule.Name;
                    Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
                }
                var shortest = ActivitySlackRule(plannableSchedules, schedule);


                plannableSchedules.Remove(shortest);
            }
        }

        private ProductionOrderWorkSchedule ActivitySlackRule(List<ProductionOrderWorkSchedule> plannableSchedules, List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            //Todo: remove activityslack in schedule and only call getremaintimefromparents if value is zeron
            ProductionOrderWorkSchedule shortest = null;
            foreach (var plannableSchedule in plannableSchedules)
            {
                //get duetime
                var orderPartId = _context.Demands.OfType<DemandOrderPart>().Single(a => a.DemandRequesterId == plannableSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequesterId).OrderPartId;
                var dueTime = _context.OrderParts.Include(a => a.Order).Single(a => a.Id == orderPartId).Order.DueTime;

                //get remaining time
                plannableSchedule.ActivitySlack = GetRemainTimeFromParents(plannableSchedule, productionOrderWorkSchedules);

                if (shortest == null || shortest.ActivitySlack > plannableSchedule.ActivitySlack)
                    shortest = plannableSchedule;
            }
            return shortest;
        }

        private int GetRemainTimeFromParents(ProductionOrderWorkSchedule schedule, List <ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var parent = FindHierarchyParent(productionOrderWorkSchedules, schedule);
            if (parent == null) parent = FindBomParent(schedule);
            if (parent == null) return schedule.Duration;
            
            return GetRemainTimeFromParents(parent, productionOrderWorkSchedules) + schedule.Duration;
        }

        private List<ProductionOrderWorkSchedule> GetSchedule()
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

        private List<ProductionOrderWorkSchedule> GetPlannables(
            List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, List<ProductionOrderWorkSchedule> plannedSchedules )
        {
            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            //are there any planned schedules
            if (plannedSchedules.Any())
            {
                //iterate through to check their parents in hierarchy/bom
                foreach (var plannedSchedule in plannedSchedules)
                {
                    var hierarchyParent = FindHierarchyParent(productionOrderWorkSchedules,plannedSchedule);
                    // if there is a higher hierarchy
                    if (hierarchyParent != null && !plannedSchedules.Contains(hierarchyParent) && plannableSchedules.Contains(hierarchyParent))
                        plannableSchedules.Add(hierarchyParent);
                    
                    //search for parent
                    else
                    {
                        var lowestHierarchyMember = FindBomParent(plannedSchedule);
                        if (lowestHierarchyMember != null && !plannedSchedules.Contains(lowestHierarchyMember) && !plannableSchedules.Contains(lowestHierarchyMember))
                            plannableSchedules.Add(lowestHierarchyMember);
                    }
                }
            }

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
            return plannableSchedules;
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
