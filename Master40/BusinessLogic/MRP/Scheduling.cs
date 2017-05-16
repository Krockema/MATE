using System.Collections.Generic;
using System.Linq;
using Master40.Data;
using Master40.Extensions;
using Master40.Models;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{

    interface IScheduling
    {
        void CreateSchedule(int orderPartId, ProductionOrder productionOrder);
        void BackwardScheduling(OrderPart orderPart);
        void ForwardScheduling(OrderPart orderPart);
        void CapacityScheduling();
    }

    class Scheduling : IScheduling
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public Scheduling(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
            _context = context;
        }

        public void CreateSchedule(int orderPartId, ProductionOrder productionOrder)
        {
            var headOrder = _context.OrderParts.Include(a => a.Order).Single(a => a.OrderPartId == orderPartId);
            
            var timeHelper = headOrder.Order.DueTime;

            //get abstract workSchedule
            var abstractWorkSchedules = _context.WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workScheduleBackward = new ProductionOrderWorkSchedule()
                {
                    Duration = abstractWorkSchedule.Duration * (int) productionOrder.Quantity,
                    HierarchyNumber = abstractWorkSchedule.HierarchyNumber,
                    MachineGroupId = abstractWorkSchedule.MachineGroupId,
                    MachineGroup = abstractWorkSchedule.MachineGroup,
                    MachineTool = abstractWorkSchedule.MachineTool,
                    MachineToolId = abstractWorkSchedule.MachineToolId,
                    Name = abstractWorkSchedule.Name,
                    End = -1,
                    Start = timeHelper,
                    ProductionOrderId = productionOrder.ProductionOrderId,
                    PlanningType = PlanningType.Backward
                };
                var workScheduleForward = new ProductionOrderWorkSchedule();
                workScheduleBackward.CopyPropertiesTo(workScheduleForward);
                workScheduleForward.PlanningType = PlanningType.Forward;
                _context.ProductionOrderWorkSchedule.Add(workScheduleForward);
                _context.ProductionOrderWorkSchedule.Add(workScheduleBackward);
                _context.SaveChanges();
            }
        }
        
        

        public void BackwardScheduling(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(orderPart, PlanningType.Backward);
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                ProductionOrderBom parent = null;
                var hierarchy = 100000;
                foreach (var schedule in workSchedule.ProductionOrder.ProductionOrderWorkSchedule)
                {
                    if (schedule.HierarchyNumber > workSchedule.HierarchyNumber && schedule.HierarchyNumber < hierarchy)
                        hierarchy = schedule.HierarchyNumber;
                }
                if (hierarchy == 100000)
                {
                    foreach (
                        var pob in
                        _context.ProductionOrderBoms.Where(
                            a => a.ProductionOrderChildId == workSchedule.ProductionOrderId))
                    {
                        if (pob.ProductionOrderParentId != workSchedule.ProductionOrder.ProductionOrderId)
                            parent = pob;
                    }
                    //set start- and endtime 
                    if (parent != null)
                    {
                        int parentStart = workSchedule.Start;
                        foreach (var parentSchedule in parent.ProductionOrderParent.ProductionOrderWorkSchedule)
                        {
                            if (parentSchedule.Start < parentStart) parentStart = parentSchedule.Start;
                        }
                        
                        workSchedule.End = parentStart;
                    }
                    else
                    {
                        //initial value of start is duetime of the order
                        workSchedule.End = workSchedule.Start;
                    }
                }
                else
                {
                    workSchedule.End =
                        _context.ProductionOrderWorkSchedule.Single(
                            a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId) &&
                                (a.PlanningType == workSchedule.PlanningType)).Start;
                }
                workSchedule.Start = workSchedule.End - workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Update(workSchedule);
            }
            _context.SaveChanges();
        }


        public void ForwardScheduling(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(orderPart, PlanningType.Forward);
            productionOrderWorkSchedules.Reverse();
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                var children = new List<ProductionOrderBom>();
                var hierarchy = -1;
                foreach (var schedule in productionOrderWorkSchedules)
                {
                    if (schedule.HierarchyNumber < workSchedule.HierarchyNumber && schedule.HierarchyNumber > hierarchy)
                        hierarchy = schedule.HierarchyNumber;
                }
                if (hierarchy == -1)
                {
                    foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == workSchedule.ProductionOrderId))
                    {
                        if (pob.ProductionOrderChildId != workSchedule.ProductionOrder.ProductionOrderId)
                            children.Add(pob);
                    }
                    //set start- and endtime 
                    if (children.Any())
                    {
                        var childEnd = 0;
                        foreach (var child in children)
                        {
                            foreach (var childSchedule in child.ProductionOrderChild.ProductionOrderWorkSchedule)
                            {
                                if (childSchedule.End > childEnd) childEnd = childSchedule.End;
                            }
                        }
                        workSchedule.Start = childEnd;
                    }
                    else
                    {
                        //initial value of start is duetime of the order
                        workSchedule.Start = 0;
                    }
                }
                else
                {
                    workSchedule.Start =
                        _context.ProductionOrderWorkSchedule.Single(
                            a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId) &&
                                (a.PlanningType == workSchedule.PlanningType)).End;
                }
                workSchedule.End = workSchedule.Start + workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Update(workSchedule);
            }
            _context.SaveChanges();
        }

        public void CapacityScheduling()
        {

        }

        private List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(OrderPart orderPart, PlanningType planningType)
        {
            var productionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>();
            var demandOrderPart = _context.Demands.OfType<DemandOrderPart>().Single(a => a.OrderPartId == orderPart.OrderPartId);
            foreach (var demand in demandOrderPart.DemandProvider)
            {
                if (demand.GetType() == typeof(DemandProviderProductionOrder))
                    foreach (var schedule in _context.ProductionOrderWorkSchedule
                                                    .Where(a => a.ProductionOrderId == ((DemandProviderProductionOrder)demand).ProductionOrderId 
                                                            && a.PlanningType == planningType).OrderBy(a => a.ProductionOrder).ThenByDescending(a => a.HierarchyNumber).ToList())
                    {
                        productionOrderWorkSchedules.Add(schedule);
                    }
            }
            return productionOrderWorkSchedules;
        }
    }
}
