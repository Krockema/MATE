using System.Collections.Generic;
using System.Linq;
using Master40.Extensions;
using Master40.Models;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Models;
using Master40.DB.Models.Interfaces;
using Master40.DB.Data.Context;

namespace Master40.BusinessLogic.MRP
{

    interface IScheduling
    {
        void CreateSchedule(IDemandToProvider demand, ProductionOrder productionOrder);
        void BackwardScheduling(IDemandToProvider demand);
        void ForwardScheduling(IDemandToProvider demand);
        void SetActivitySlack(IDemandToProvider demand);  
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
        /// <param name="demand"></param>
        /// <param name="productionOrder"></param>
        public void CreateSchedule(IDemandToProvider demand, ProductionOrder productionOrder)
        {
            var dueTime = GetDueTime(demand);
            var abstractWorkSchedules = _context.WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workSchedule = new ProductionOrderWorkSchedule();
                abstractWorkSchedule.CopyPropertiesTo<IWorkSchedule>(workSchedule);
                workSchedule.Duration *= (int) productionOrder.Quantity;
                workSchedule.ProductionOrderId = productionOrder.Id;
                workSchedule.End = -1;
                workSchedule.Start = dueTime;
                workSchedule.EndBackward = -1;
                workSchedule.StartBackward = dueTime;
                workSchedule.EndForward = -1;
                workSchedule.StartForward = dueTime;

                _context.ProductionOrderWorkSchedule.Add(workSchedule);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Set Start- and Endtime for ProductionOrderWorkSchedules for the given OrderPart excluding capacities in a backward schedule
        /// </summary>
        /// <param name="demand"></param>
        public void BackwardScheduling(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(demand);
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
        /// <param name="demand"></param>
        public void ForwardScheduling(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(demand);
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
        public void SetActivitySlack(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = GetProductionOrderWorkSchedules(demand);
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

        private int GetDueTime(IDemandToProvider demand)
        {
            var dueTime = 9999;
            if (demand.GetType() == typeof(DemandOrderPart))
                dueTime = _context.OrderParts.Include(a => a.Order).Single(a => a.Id == ((DemandOrderPart)demand).OrderPartId).Order.DueTime;
            return dueTime;
        }

        internal int GetMinForward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var minForward = -100000;
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                if (productionOrderWorkSchedule.StartForward < minForward)
                    minForward = productionOrderWorkSchedule.StartForward;
            }
            return minForward;
        }

        internal int GetMinBackward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            var minBackward = -100000;
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                if (productionOrderWorkSchedule.StartBackward < minBackward)
                    minBackward = productionOrderWorkSchedule.StartBackward;
            }
            return minBackward;
        }

        //returns a list of all workSchedules for the given orderPart and planningType
        private List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>();
            foreach (var demandProvider in demand.DemandProvider)
            {
                if (demandProvider.GetType() == typeof(DemandProviderProductionOrder))
                    foreach (var schedule in _context.ProductionOrderWorkSchedule.Where(a =>
                                                    a.ProductionOrderId == ((DemandProviderProductionOrder)demandProvider).ProductionOrderId)
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
                if (pob.ProductionOrderParentId != workSchedule.ProductionOrder.Id)
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
                if (pob.ProductionOrderChildId != workSchedule.ProductionOrder.Id)
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
