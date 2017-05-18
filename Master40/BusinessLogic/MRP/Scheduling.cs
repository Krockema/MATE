using System.Collections.Generic;
using System.Linq;
using Master40.BusinessLogic.Helper;
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
        void SetActivitySlack(OrderPart orderPart);
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

        /// <summary>
        /// Creates a ProductionOrderWorkScheduleItem in the database
        /// </summary>
        /// <param name="orderPartId"></param>
        /// <param name="productionOrder"></param>
        public void CreateSchedule(int orderPartId, ProductionOrder productionOrder)
        {
            var headOrder = _context.OrderParts.Include(a => a.Order).Single(a => a.OrderPartId == orderPartId);
            var timeHelper = headOrder.Order.DueTime;
            var abstractWorkSchedules = _context.WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workSchedule = new ProductionOrderWorkSchedule();
                abstractWorkSchedule.CopyPropertiesTo<IWorkSchedule>(workSchedule);
                workSchedule.Duration *= (int) productionOrder.Quantity;
                workSchedule.ProductionOrderId = productionOrder.ProductionOrderId;
                workSchedule.End = -1;
                workSchedule.Start = timeHelper;
                workSchedule.EndBackward = -1;
                workSchedule.StartBackward = timeHelper;
                workSchedule.EndForward = -1;
                workSchedule.StartForward = timeHelper;

                _context.ProductionOrderWorkSchedule.Add(workSchedule);
                _context.SaveChanges();
            }
        }
        
        /// <summary>
        /// Set Start- and Endtime for ProductionOrderWorkSchedules for the given OrderPart excluding capacities in a backward schedule
        /// </summary>
        /// <param name="orderPart"></param>
        public void BackwardScheduling(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(orderPart);
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                //Set hierarchy to something high that every workSchedule found will overwrite this value
                var hierarchy = 100000;
                foreach (var schedule in workSchedule.ProductionOrder.ProductionOrderWorkSchedule)
                {
                    if (schedule.HierarchyNumber > workSchedule.HierarchyNumber && schedule.HierarchyNumber < hierarchy)
                        hierarchy = schedule.HierarchyNumber;
                }
                //if no hierarchy has been found
                if (hierarchy == 100000)
                {
                    workSchedule.EndBackward = SetBackwardTimeFromParent(workSchedule);
                }
                else
                {
                    workSchedule.EndBackward = _context.ProductionOrderWorkSchedule.Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).StartBackward;
                }
                workSchedule.StartBackward = workSchedule.EndBackward - workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Update(workSchedule);
            }
            _context.SaveChanges();
        }

        /// <summary>
        /// Set Start- and Endtime for ProductionOrderWorkSchedules for the given OrderPart excluding capacities in a forward schedule
        /// </summary>
        /// <param name="orderPart"></param>
        public void ForwardScheduling(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(orderPart);
            productionOrderWorkSchedules.Reverse();
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                
                var hierarchy = -1;
                foreach (var schedule in productionOrderWorkSchedules)
                {
                    if (schedule.HierarchyNumber < workSchedule.HierarchyNumber && schedule.HierarchyNumber > hierarchy)
                        hierarchy = schedule.HierarchyNumber;
                }
                //if no hierarchy has been found
                if (hierarchy == -1)
                {
                    workSchedule.StartForward = SetForwardTimeFromChild(workSchedule);
                }
                else
                {
                    workSchedule.StartForward = _context.ProductionOrderWorkSchedule.Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).EndForward;
                }
                workSchedule.EndForward = workSchedule.StartForward + workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Update(workSchedule);
            }
            _context.SaveChanges();
        }

        //find activity slack between backward- and forwardSchedule
        public void SetActivitySlack(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(orderPart);
            //Get minimum of each schedule to make the numbers relative and comparable
            var minForward = GetMinForward(productionOrderWorkSchedules);
            var minBackward = GetMinBackward(productionOrderWorkSchedules);
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                if ((productionOrderWorkSchedule.StartBackward - minBackward) > (productionOrderWorkSchedule.StartForward - minForward))
                    productionOrderWorkSchedule.ActivitySlack = (productionOrderWorkSchedule.StartBackward - minBackward) - (productionOrderWorkSchedule.StartForward - minForward);
                else
                    productionOrderWorkSchedule.ActivitySlack = (productionOrderWorkSchedule.StartForward - minForward) - (productionOrderWorkSchedule.StartBackward - minBackward);
                _context.ProductionOrderWorkSchedule.Update(productionOrderWorkSchedule);
            }
            _context.SaveChanges();
        }

        public void CapacityScheduling()
        {

        }

        private int GetMinForward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var minForward = productionOrderWorkSchedules.First().StartForward;
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                if (productionOrderWorkSchedule.StartForward < minForward)
                    minForward = productionOrderWorkSchedule.StartForward;
            }
            return minForward;
        }

        private int GetMinBackward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var minBackward = productionOrderWorkSchedules.First().StartBackward;
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                if (productionOrderWorkSchedule.StartBackward < minBackward)
                    minBackward = productionOrderWorkSchedule.StartBackward;
            }
            return minBackward;
        }

        //returns a list of all workSchedules for the given orderPart and planningType
        private List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(OrderPart orderPart)
        {
            var productionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>();
            var demandOrderPart = _context.Demands.OfType<DemandOrderPart>().Single(a => a.OrderPartId == orderPart.OrderPartId);
            foreach (var demand in demandOrderPart.DemandProvider)
            {
                if (demand.GetType() == typeof(DemandProviderProductionOrder))
                    foreach (var schedule in _context.ProductionOrderWorkSchedule.Where(a =>
                                                    a.ProductionOrderId == ((DemandProviderProductionOrder)demand).ProductionOrderId)
                                                    .OrderBy(a => a.ProductionOrder).ThenByDescending(a => a.HierarchyNumber).ToList())
                    {
                        productionOrderWorkSchedules.Add(schedule);
                    }
            }
            return productionOrderWorkSchedules;
        }

        //find starttime from parent and set the endtime of the workschedule to it
        private int SetBackwardTimeFromParent(ProductionOrderWorkSchedule workSchedule)
        {
            ProductionOrderBom parent = null;
            
            //search for parents
            foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderChildId == workSchedule.ProductionOrderId))
            {
                if (pob.ProductionOrderParentId != workSchedule.ProductionOrder.ProductionOrderId)
                    parent = pob;
            }
            if (parent != null)
            {
                int parentStart = workSchedule.StartBackward;
                foreach (var parentSchedule in parent.ProductionOrderParent.ProductionOrderWorkSchedule)
                {
                    if (parentSchedule.StartBackward < parentStart) parentStart = parentSchedule.StartBackward;
                }

                workSchedule.EndBackward = parentStart;
            }
            else
            {
                //initial value of start is duetime of the order
                workSchedule.EndBackward = workSchedule.StartBackward;
            }
            return workSchedule.EndBackward;
        }

        //sets Starttime to the latest endtime of the children
        private int SetForwardTimeFromChild(ProductionOrderWorkSchedule workSchedule)
        {
            var children = new List<ProductionOrderBom>();
            foreach (var pob in _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == workSchedule.ProductionOrderId))
            {
                if (pob.ProductionOrderChildId != workSchedule.ProductionOrder.ProductionOrderId)
                    children.Add(pob);
            }
            
            if (children.Any())
            {
                var childEnd = 0;
                foreach (var child in children)
                {
                    foreach (var childSchedule in child.ProductionOrderChild.ProductionOrderWorkSchedule)
                    {
                        if (childSchedule.EndForward > childEnd) childEnd = childSchedule.EndForward;
                    }
                }
                workSchedule.StartForward = childEnd;
            }
            else
            {
                //initial value of start is duetime of the order
                workSchedule.StartForward = 0;
            }
            return workSchedule.StartForward;
        }

        
    }
}
