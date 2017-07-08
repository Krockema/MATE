using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{

    public interface IScheduling
    {
        void CreateSchedule(IDemandToProvider demand, ProductionOrder productionOrder);
        void BackwardScheduling(IDemandToProvider demand);
        void ForwardScheduling(IDemandToProvider demand); 
    }

    public class Scheduling : IScheduling
    {
        private readonly MasterDBContext _context;
        public Scheduling(MasterDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a ProductionOrderWorkScheduleItem in the database
        /// </summary>
        /// <param name="demand"></param>
        /// <param name="productionOrder"></param>
        public void CreateSchedule(IDemandToProvider demand, ProductionOrder productionOrder)
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
                    if (demand.State == State.ForwardScheduleExists) 
                        workSchedule.EndForward = SetBackwardTimeFromParent(workSchedule, demand.State);
                    else
                        workSchedule.EndBackward = SetBackwardTimeFromParent(workSchedule, demand.State);
                }
                else
                {
                    if (demand.State == State.ForwardScheduleExists)
                        workSchedule.EndForward = _context.ProductionOrderWorkSchedule.Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).StartForward;
                    else
                        workSchedule.EndBackward = _context.ProductionOrderWorkSchedule.Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).StartBackward;
                }
                if (demand.State == State.ForwardScheduleExists)
                    workSchedule.StartForward = workSchedule.EndForward - workSchedule.Duration;
                else
                    workSchedule.StartBackward = workSchedule.EndBackward - workSchedule.Duration;
               
            }
            _context.ProductionOrderWorkSchedule.UpdateRange(productionOrderWorkSchedules);

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
                    workSchedule.StartForward = _context.ProductionOrderWorkSchedule.AsNoTracking().Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).EndForward;
                }
                workSchedule.EndForward = workSchedule.StartForward + workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Update(workSchedule);
                _context.SaveChanges();
            }
        }
        

        private int GetDueTime(IDemandToProvider demand)
        {
            var latestEnd = 0;
            if (demand.State == State.ForwardScheduleExists)
            {
                /*long version of the below line
                 * foreach (var provider in demand.DemandProvider)
                {
                    if (provider.GetType() == typeof(DemandProviderProductionOrder))
                    {
                        foreach (var schedule in ((DemandProviderProductionOrder)provider).ProductionOrder.ProductionOrderWorkSchedule)
                        {
                            if (schedule.EndForward > latestEnd) latestEnd = schedule.EndForward;
                        }

                    }
                }*/
                latestEnd =  Enumerable.Concat((from provider in demand.DemandProvider where provider.GetType() == typeof(DemandProviderProductionOrder)
                    from schedule in ((DemandProviderProductionOrder) provider).ProductionOrder.ProductionOrderWorkSchedule
                    select schedule.EndForward), new[] {latestEnd}).Max();
                return latestEnd;
            }
            demand = _context.Demands.AsNoTracking().Include(a => a.DemandRequester).Single(a => a.Id == demand.Id);
            var dueTime = 9999;
            if (demand.DemandRequesterId != null && demand.DemandRequester.GetType() == typeof(DemandOrderPart))
                dueTime = _context.OrderParts.Include(a => a.Order).Single(a => a.Id == ((DemandOrderPart)demand.DemandRequester).OrderPartId).Order.DueTime;
            return dueTime;
        }

        internal int GetMinForward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            return Enumerable.Concat(productionOrderWorkSchedules.Select(productionOrderWorkSchedule => productionOrderWorkSchedule.StartForward), new[] {-100000}).Min();
        }

        internal int GetMinBackward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            return Enumerable.Concat(productionOrderWorkSchedules.Select(productionOrderWorkSchedule => productionOrderWorkSchedule.StartBackward), new[] {-100000}).Min();
        }

        //returns a list of all workSchedules for the given orderPart and planningType
        private List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(IDemandToProvider demand)
        {
            //get child(bom)-Demands
            var bomDemands = 
                _context.Demands.OfType<DemandProductionOrderBom>()
                .Include(a => a.DemandProvider)
                .Where(a => a.DemandRequesterId == demand.Id).ToList();
            
            //get initial pows
            var productionOrderWorkSchedules = demand.DemandProvider.Where(demandProvider => demandProvider.GetType() == typeof(DemandProviderProductionOrder))
                .SelectMany(demandProvider => _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder)
                    .Where(a => a.ProductionOrderId == ((DemandProviderProductionOrder) demandProvider).ProductionOrderId)
                    .OrderBy(a => a.ProductionOrder)
                    .ThenByDescending(a => a.HierarchyNumber)
                    .ToList())
                .ToList();

            //get pows for bomDemands
            productionOrderWorkSchedules.AddRange(from bomDemand in bomDemands
                from demandProvider in bomDemand.DemandProvider
                where demandProvider.GetType() == typeof(DemandProviderProductionOrder)
                from schedule in _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder)
                    .Where(a => a.ProductionOrderId == ((DemandProviderProductionOrder) demandProvider).ProductionOrderId)
                    .OrderBy(a => a.ProductionOrder)
                    .ThenByDescending(a => a.HierarchyNumber)
                    .ToList()
                select schedule);
            return productionOrderWorkSchedules;
        }

        //find starttime from parent and set the endtime of the workschedule to it
        private int SetBackwardTimeFromParent(ProductionOrderWorkSchedule workSchedule, State state)
        { //Todo: replace provider.first 
            ProductionOrderBom parent = null;
            
            //search for parents
            foreach (var pob in _context.ProductionOrderBoms.Include(a => a.ProductionOrderParent).ThenInclude(b => b.ProductionOrderWorkSchedule).Where(a => a.ProductionOrderChildId == workSchedule.ProductionOrderId))
            {
                if (pob.ProductionOrderParentId != workSchedule.ProductionOrder.Id)
                    parent = pob;
            }
            int end;
            if (parent != null)
            {
                int parentStart;
                if (
                    workSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequester != null)
                    parentStart = GetDueTime(workSchedule.ProductionOrder.DemandProviderProductionOrders.First()
                                .DemandRequester.DemandRequester);
                else
                    parentStart = GetDueTime(workSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester);
                foreach (var parentSchedule in parent.ProductionOrderParent.ProductionOrderWorkSchedule)
                {
                    if (state == State.ForwardScheduleExists)
                    {
                        if (parentSchedule.StartForward < parentStart) parentStart = parentSchedule.StartForward;
                    }
                    else
                    {
                        if (parentSchedule.StartBackward < parentStart) parentStart = parentSchedule.StartBackward;
                    }
                       
                }

               end = parentStart;
            }
            else
            {
                end = GetDueTime(workSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequester 
                    ?? workSchedule.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester);
            }
            return end;
        }

        //sets Starttime to the latest endtime of the children
        private int SetForwardTimeFromChild(ProductionOrderWorkSchedule workSchedule)
        {
            var children = new List<ProductionOrderBom>();
            foreach (var pob in _context.ProductionOrderBoms.Include(a => a.ProductionOrderChild).ThenInclude(a => a.ProductionOrderWorkSchedule).Where(a => a.ProductionOrderParentId == workSchedule.ProductionOrderId))
            {
                if (pob.ProductionOrderChildId != workSchedule.ProductionOrder.Id)
                    children.Add(pob);
            }
            
            if (children.Any())
            {
                var childEnd = Enumerable.Concat((from child in children
                    from childSchedule in child.ProductionOrderChild.ProductionOrderWorkSchedule
                    select childSchedule.EndForward), new[] {0}).Max();
                workSchedule.StartForward = childEnd;
            }
            else
            {
                workSchedule.StartForward = 0;
            }
            return workSchedule.StartForward;
        }

        
    }
}
