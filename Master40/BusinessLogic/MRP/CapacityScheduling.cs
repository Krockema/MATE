using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;
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
            var plannableSchedule = new List<ProductionOrderWorkSchedule>();
            var plannedSchedule = new List<ProductionOrderWorkSchedule>();
            plannableSchedule = GetPlannables(schedule,plannedSchedule);
            while (plannableSchedule.Any())
            {
                var msg = "Plannable elements: " + plannableSchedule.First().Name;
                Logger.Add(new LogMessage() { MessageType = MessageType.info, Message = msg });
                plannableSchedule.Remove(plannableSchedule.First());
            }
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
                _context.Demands.OfType<DemandProviderProductionOrder>()
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
                    ProductionOrderWorkSchedule hierarchyParent = null;
                    int hierarchyParentNumber = 100000;
                    
                    //find next higher element
                    foreach (var mainSchedule in productionOrderWorkSchedules)
                    {
                        if (mainSchedule.ProductionOrderId == plannedSchedule.ProductionOrderId)
                        { 
                            if (mainSchedule.HierarchyNumber > plannedSchedule.ProductionOrderId &&
                                mainSchedule.HierarchyNumber < hierarchyParentNumber)
                            {
                                hierarchyParent = mainSchedule;
                                hierarchyParentNumber = mainSchedule.HierarchyNumber;
                            }
                        }

                    }
                    // if there is a higher hierarchy
                    if (hierarchyParent != null)
                    {
                        if (!plannedSchedules.Contains(hierarchyParent))
                            if (!plannableSchedules.Contains(hierarchyParent))
                                plannableSchedules.Add(hierarchyParent);
                    }
                    //search for parent
                    else
                    {
                        ProductionOrderWorkSchedule parentSchedule = null;
                        foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderChildId == plannedSchedule.ProductionOrderId))
                        {
                            //check if its the head element which points to itself
                            if (pob.ProductionOrderParentId != plannedSchedule.ProductionOrder.Id)
                            {
                                var parents = pob.ProductionOrderParent.ProductionOrderWorkSchedule;
                                ProductionOrderWorkSchedule lowestHierarchyMember = parents.First();
                                //find lowest hierarchy
                                foreach (var parent in parents)
                                {
                                    if (parent.HierarchyNumber < lowestHierarchyMember.HierarchyNumber)
                                        lowestHierarchyMember = parent;
                                }
                                if (!plannedSchedules.Contains(lowestHierarchyMember))
                                    if (!plannableSchedules.Contains(lowestHierarchyMember))
                                        plannableSchedules.Add(lowestHierarchyMember);
                            }
                        }
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
                    break;
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
    }
    
}
