using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{

    public interface IScheduling
    {
        void BackwardScheduling(IDemandToProvider demand);
        void ForwardScheduling(IDemandToProvider demand); 
    }

    public class Scheduling : IScheduling
    {
        private readonly ProductionDomainContext _context;
        public Scheduling(ProductionDomainContext context)
        {
            _context = context;
        }
        

        /// <summary>
        /// Set Start- and Endtime for ProductionOrderWorkSchedules for the given OrderPart excluding capacities in a backward schedule
        /// </summary>
        /// <param name="demand"></param>
        public void BackwardScheduling(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>();
            _context.GetWorkSchedulesFromDemand(demand, ref productionOrderWorkSchedules);
            productionOrderWorkSchedules = productionOrderWorkSchedules.OrderBy(a => a.ProductionOrderId).ThenByDescending(b => b.HierarchyNumber).ToList();
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                //Set hierarchy to something high that every workSchedule found will overwrite this value
                var hierarchyParent = _context.GetHierarchyParent(workSchedule);
                //if no hierarchy has been found
                if (hierarchyParent == null)
                {
                    if (demand.State == State.ForwardScheduleExists) 
                        workSchedule.EndForward = SetBackwardTimeFromParent(workSchedule, demand.State);
                    else
                        workSchedule.EndBackward = SetBackwardTimeFromParent(workSchedule, demand.State);
                }
                else
                {
                    if (demand.State == State.ForwardScheduleExists)
                        workSchedule.EndForward = _context.ProductionOrderWorkSchedules.Single(a =>
                                (a.HierarchyNumber == hierarchyParent.HierarchyNumber) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).StartForward;
                    else
                    {
                        workSchedule.EndBackward = _context.ProductionOrderWorkSchedules.Single(a =>
                                (a.HierarchyNumber == hierarchyParent.HierarchyNumber) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).StartBackward;
                    }
                }
                if (demand.State == State.ForwardScheduleExists)
                    workSchedule.StartForward = workSchedule.EndForward - workSchedule.Duration;
                else
                    workSchedule.StartBackward = workSchedule.EndBackward - workSchedule.Duration;
               
            }
            _context.ProductionOrderWorkSchedules.UpdateRange(productionOrderWorkSchedules);

            _context.SaveChanges();
        }
        
        /// <summary>
        /// Set Start- and Endtime for ProductionOrderWorkSchedules for the given OrderPart excluding capacities in a forward schedule
        /// </summary>
        /// <param name="demand"></param>
        public void ForwardScheduling(IDemandToProvider demand)
        {
            var productionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>();
            _context.GetWorkSchedulesFromDemand(demand, ref productionOrderWorkSchedules);
            productionOrderWorkSchedules =
                productionOrderWorkSchedules.OrderByDescending(a => a.ProductionOrderId).ThenBy(b => b.HierarchyNumber).ToList();
            foreach (var workSchedule in productionOrderWorkSchedules)
            {
                var hierarchy = -1;
                foreach (var schedule in workSchedule.ProductionOrder.ProductionOrderWorkSchedule)
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
                    workSchedule.StartForward = _context.ProductionOrderWorkSchedules.AsNoTracking().Single(a =>
                                (a.HierarchyNumber == hierarchy) &&
                                (a.ProductionOrderId == workSchedule.ProductionOrderId)).EndForward;
                }
                workSchedule.EndForward = workSchedule.StartForward + workSchedule.Duration;
                _context.ProductionOrderWorkSchedules.Update(workSchedule);
                _context.SaveChanges();
            }
        }
        

        private int GetDueTime(IDemandToProvider demand)
        {
            demand = _context.Demands.Single(a => a.Id == demand.Id);
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
                latestEnd =  (from provider in demand.DemandProvider where provider.GetType() == typeof(DemandProviderProductionOrder)
                    from schedule in ((DemandProviderProductionOrder) provider).ProductionOrder.ProductionOrderWorkSchedule
                    select schedule.EndForward).Concat(new[] {latestEnd}).Max();
                return latestEnd;
            }
            demand = _context.Demands.AsNoTracking().Include(a => a.DemandRequester).Single(a => a.Id == demand.Id);
            if (demand.DemandRequesterId != null && demand.DemandRequester.GetType() == typeof(DemandOrderPart))
                return _context.OrderParts.Include(a => a.Order).Single(a => a.Id == ((DemandOrderPart)demand.DemandRequester).OrderPartId).Order.DueTime;
            if (demand.DemandRequesterId == null && demand.GetType() == typeof(DemandOrderPart))
                return _context.OrderParts.Include(a => a.Order).Single(a => a.Id == ((DemandOrderPart)demand).OrderPartId).Order.DueTime;
            return 999999;
        }

        internal int GetMinForward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            return productionOrderWorkSchedules.Select(productionOrderWorkSchedule => productionOrderWorkSchedule.StartForward).Concat(new[] {-100000}).Min();
        }

        internal int GetMinBackward(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            return productionOrderWorkSchedules.Select(productionOrderWorkSchedule => productionOrderWorkSchedule.StartBackward).Concat(new[] {-100000}).Min();
        }

       //find starttime from parent and set the endtime of the workschedule to it
        private int SetBackwardTimeFromParent(ProductionOrderWorkSchedule productionOrderWorkSchedule, State state)
        {
            var parents = _context.GetBomParents(productionOrderWorkSchedule);
            int end;
            if (!parents.Any()) end = state == State.ForwardScheduleExists ? productionOrderWorkSchedule.EndForward : productionOrderWorkSchedule.ProductionOrder.Duetime;
            else if (state != State.ForwardScheduleExists)
            {
                end = parents.Min(a => a.StartBackward);
                productionOrderWorkSchedule.EndBackward = end;
            }
            else
            {
                end = parents.Min(a => a.StartForward);
                productionOrderWorkSchedule.EndForward = end;
            }
            return end;
        }

        private int GetEarliestDueTime(ICollection<DemandProviderProductionOrder> demandProviderProductionOrders)
        {
            /*long version:
             * foreach (var demandProviderProductionOrder in demandProviderProductionOrders)
            {
                var requester = demandProviderProductionOrder.DemandRequester.DemandRequester ??
                                demandProviderProductionOrder.DemandRequester;
                var duetime = GetDueTime(requester);
                if (earliestDuetime > duetime)
                {
                    earliestDuetime = duetime;
                }
            }*/
            return demandProviderProductionOrders
                .Select(demandProviderProductionOrder => 
                demandProviderProductionOrder.DemandRequester.DemandRequester
                                                         ?? demandProviderProductionOrder.DemandRequester)
                .Select(GetDueTime).Concat(new[] {999999}).Min();
        }

        //sets Starttime to the latest endtime of the children
        private int SetForwardTimeFromChild(ProductionOrderWorkSchedule workSchedule)
        {
            var pobs = workSchedule.ProductionOrder.ProductionOrderBoms;

            var latestEnd = (from pob in pobs where pob.DemandProductionOrderBoms.Any()
                             from provider in pob.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                             select provider.ProductionOrder.ProductionOrderWorkSchedule.Max(a => a.EndForward)
                             ).Concat(new[] {0}).Max();
            workSchedule.StartForward = latestEnd;
            
            return workSchedule.StartForward;
        }

        
    }
}
