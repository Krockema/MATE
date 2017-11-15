using System.Collections.Generic;
using System.Linq;
using Master40.BusinessLogicCentral.HelperCapacityPlanning;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogicCentral.MRP
{
   public interface ICapacityScheduling
    {
        void GifflerThompsonScheduling(int simulationId);
        List<MachineGroupProductionOrderWorkSchedule> CapacityRequirementsPlanning(int simulationId);
        bool CapacityLevelingCheck(List<MachineGroupProductionOrderWorkSchedule> machineList);
        void SetMachines(int simulationId);
    }

    public class CapacityScheduling : ICapacityScheduling
    {
        private readonly ProductionDomainContext _context;
        public CapacityScheduling(ProductionDomainContext context)
        {
            _context = context;
        }



        /// <summary>
        /// An algorithm for capacity-leveling. Writes Start/End in ProductionOrderWorkSchedule.
        /// </summary>
        public void GifflerThompsonScheduling(int simulationId)
        {
            var currentTime = _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time;
            //var productionOrderWorkSchedules = GetProductionSchedules(simulationId);
            var productionOrderWorkSchedules = _context.ProductionOrderWorkSchedules.Where(a =>
                a.ProducingState == ProducingState.Created || a.ProducingState == ProducingState.Waiting).ToList();
            productionOrderWorkSchedules = ResetStartEnd(productionOrderWorkSchedules);
            productionOrderWorkSchedules = CalculateWorkTimeWithParents(productionOrderWorkSchedules);

            var plannableSchedules = new List<ProductionOrderWorkSchedule>();
            var plannedSchedules = new List<ProductionOrderWorkSchedule>();
            GetInitialPlannables(productionOrderWorkSchedules,plannedSchedules, plannableSchedules);
            while (plannableSchedules.Any())
            {
                //find next element by using the activity slack rule
                //Todo: Remove after testing
                //var shortest = GetHighestPriority(plannableSchedules);
                CalculateActivitySlack(plannableSchedules, currentTime);
                var shortest = GetShortest(plannableSchedules);
                plannableSchedules.Remove(shortest);
                //Add a fix spot on a machine with start/end
                AddMachine(plannedSchedules, shortest, currentTime);
                plannedSchedules.Add(shortest);

                //search for parents and if available and allowed add it to the schedule
                var parents = _context.GetParents(shortest);
                foreach (var parent in parents)
                {
                    if (!plannableSchedules.Contains(parent) && IsTechnologicallyAllowed(parent, plannedSchedules))
                        plannableSchedules.Add(parent);
                    _context.SaveChanges();
                }
                
            }
        }

        private ProductionOrderWorkSchedule GetHighestPriority(List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            return plannableSchedules.First(a =>
                a.ProductionOrder.Duetime == plannableSchedules.Min(b => b.ProductionOrder.Duetime));
        }

        private List<ProductionOrderWorkSchedule> GetInitialPlannedSchedules(int simulationId)
        {
            var timer = _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time;
            return timer == 0 ? null : _context.ProductionOrderWorkSchedules.Where(a => a.Start <= timer && a.End - a.Start == a.Duration).ToList();
        }
        
        private void AddMachine(List<ProductionOrderWorkSchedule> plannedSchedules, ProductionOrderWorkSchedule shortest, int currentTime)
        {
           
            var machines = _context.Machines.Where(a => a.MachineGroupId == shortest.MachineGroupId).ToList();
            if (machines.Count == 1)
            {
                shortest.Start = GetChildEndTime(shortest);
                if (shortest.Start == 0) shortest.Start = currentTime;
                shortest.End = shortest.Start + shortest.Duration;
                var earliestPlanned = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliestPows = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliest = (earliestPlanned > earliestPows) ? earliestPlanned : earliestPows;
                if (shortest.Start < earliest)
                    shortest.Start = earliest;
                shortest.MachineId = machines.First().Id;
                shortest.End = shortest.Start + shortest.Duration;
            }
               
            else if (machines.Count > 1)
            {
                //Todo: possible boosts: 
                //getchild, if same machinegroup take same machine default
                //else look for min free time if same start
                shortest.Start = GetChildEndTime(shortest);
                if (shortest.Start == 0) shortest.Start = currentTime;
                shortest.End = shortest.Start + shortest.Duration;
                var earliestPlanned = FindStartOnMachine(plannedSchedules, machines.First().Id, shortest);
                var earliest = earliestPlanned;
                var earliestMachine = machines.First();
                foreach (var machine in machines)
                {
                    var earliestThisMachine = FindStartOnMachine(plannedSchedules, machine.Id, shortest);
                    if (earliest <= earliestThisMachine || earliest <= shortest.Start) continue;
                    earliest = earliestThisMachine;
                    earliestMachine = machine;
                }
                
                
                if (shortest.Start < earliest)
                    shortest.Start = earliest;
                shortest.MachineId = earliestMachine.Id;
                shortest.End = shortest.Start + shortest.Duration;
            }
            _context.Update(shortest);

            _context.SaveChanges();
        }

        private int GetChildEndTime(ProductionOrderWorkSchedule shortest)
        {
            if (shortest.HierarchyNumber != shortest.ProductionOrder.ProductionOrderWorkSchedule.Min(a => a.HierarchyNumber))
            {
                var children = _context.ProductionOrderWorkSchedules.Where(a =>
                    a.ProductionOrderId == shortest.ProductionOrderId &&
                    a.HierarchyNumber < shortest.HierarchyNumber).ToList();
                return children.Max(b => b.End);
            }
            var childrenBoms = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == shortest.ProductionOrderId).ToList();
            var latestEnd = (from bom in childrenBoms where bom.DemandProductionOrderBoms.Any()
                            from provider in bom.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                            select provider.ProductionOrder.ProductionOrderWorkSchedule.Max(a => a.End)
                            ).Concat(new[] {0}).Max();
            return latestEnd;
        }

        private int FindStartOnMachine(List<ProductionOrderWorkSchedule> plannedSchedules, int machineId, ProductionOrderWorkSchedule shortest)
        {
            for (var i = plannedSchedules.Count-1; i >= 0; i--)
            {
                if (plannedSchedules[i].MachineId == machineId)
                {
                    return DetectCrossing(plannedSchedules[i], shortest) ? plannedSchedules[i].End : (plannedSchedules[i].End>shortest.Start ? plannedSchedules[i].End : shortest.Start);
                }
                    
            }
            return 0;
        }

        private List<ProductionOrderWorkSchedule> ResetStartEnd(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules)
        {
            foreach (var productionOrderWorkSchedule in productionOrderWorkSchedules)
            {
                productionOrderWorkSchedule.Start = 0;
                productionOrderWorkSchedule.End = 0;
            }
            return productionOrderWorkSchedules;
        }

        /// <summary>
        /// Calculates Capacities needed to use backward/forward termination
        /// </summary>
        /// <returns>capacity-plan</returns>
        public List<MachineGroupProductionOrderWorkSchedule> CapacityRequirementsPlanning(int simulationId)
        {   
            //Stack for every hour and machinegroup
            var productionOrderWorkSchedules = GetProductionSchedules(simulationId);
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
                        var schedule = new MachineGroupProductionOrderWorkSchedule
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
            return (from machine in machineList
                    from hour in machine.ProductionOrderWorkSchedulesByTimeSteps
                    let machines = _context.Machines.Where(a => a.MachineGroupId == machine.MachineGroupId).ToList()
                    where machines.Any()
                    where machines.Count < hour.ProductionOrderWorkSchedules.Count
                    select hour).Any();
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
                    var timestep = new ProductionOrderWorkSchedulesByTimeStep
                    {
                            Time = i,
                            ProductionOrderWorkSchedules = new List<ProductionOrderWorkSchedule>
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
        {
            //check for every child if its planned
            var child = GetHierarchyChild(schedule);
            if (child != null && !plannedSchedules.Any(a => a.ProductionOrderId == child.ProductionOrderId && a.HierarchyNumber == child.HierarchyNumber) && (child.ProducingState == ProducingState.Created || child.ProducingState == ProducingState.Waiting))
            {
                return false;
            }
            if (child != null) return true;
            var childs = GetBomChilds(schedule);
            if (childs == null) return true;
            return childs.All(childSchedule => plannedSchedules
                    .Any(a => a.Id == childSchedule.Id) 
                    || childSchedule.ProducingState == ProducingState.Producing 
                    || childSchedule.ProducingState == ProducingState.Finished);
            
        }

        private ProductionOrderWorkSchedule GetShortest(List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            return plannableSchedules.First(a => a.ActivitySlack == plannableSchedules.Min(b => b.ActivitySlack));
        }

        private List<ProductionOrderWorkSchedule> CalculateWorkTimeWithParents(List<ProductionOrderWorkSchedule> schedules)
        {
            foreach (var schedule in schedules)
            {
                schedule.WorkTimeWithParents = GetRemainTimeFromParents(schedule);
            }
            _context.UpdateRange(schedules);
            _context.SaveChanges();
            return schedules;
        }

        private void CalculateActivitySlack(List<ProductionOrderWorkSchedule> plannableSchedules, int currentTime)
        {
            foreach (var plannableSchedule in plannableSchedules)
            {
                var processDueTime = plannableSchedule.ProductionOrder.Duetime -
                                     ((int) plannableSchedule.WorkTimeWithParents - plannableSchedule.Duration);
                plannableSchedule.ActivitySlack = PriorityRules.ActivitySlack(currentTime, plannableSchedule.Duration,processDueTime );
                _context.Update(plannableSchedule);
            }
            _context.SaveChanges();
        }

        private decimal GetRemainTimeFromParents(ProductionOrderWorkSchedule schedule)
        {
            if (schedule == null) return 0;
            var parents = _context.GetParents(schedule);
            if (parents == null || !parents.Any()) return schedule.Duration;
            var maxTime = 0;
            foreach (var parent in parents)
            {
                var time = GetRemainTimeFromParents(parent) + schedule.Duration;
                if (time > maxTime) maxTime = (int)time;
            }
            return maxTime;
        }

        private List<ProductionOrderWorkSchedule> GetProductionSchedules(int simulationId)
        {
            var demandProvider = _context.Demands.OfType<DemandProviderProductionOrder>()
                .Include(a => a.DemandRequester)
                .Include(a => a.ProductionOrder)
                .ThenInclude(a => a.ProductionOrderWorkSchedule)
                .Where(b => b.State != State.Finished && b.State != State.Producing)
                .ToList();
            /*var demandRequester = _context.Demands.AsNoTracking()
                .Include(a => a.DemandProvider)
                .Include(a => a.DemandRequester)
                .ThenInclude(a => a.DemandRequester)
                .Where(b => b.State == State.BackwardScheduleExists
                            || b.State == State.ExistsInCapacityPlan
                            || b.State == State.ForwardScheduleExists
                            || b.State == State.ProviderExist)
                .ToList();*/

            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleProvider in demandProvider)
            {
                /*if (_context.GetDueTimeByOrder(singleProvider) 
                    <= _context.SimulationConfigurations
                        .Single(a => a.Id == simulationId).Time 
                        + _context.SimulationConfigurations.Single(a => a.Id == simulationId).MaxCalculationTime 
                    || singleProvider.GetType() == typeof(DemandStock))
                    _context.GetWorkSchedulesFromDemand(singleProvider, ref pows);*/
                pows.AddRange(singleProvider.ProductionOrder.ProductionOrderWorkSchedule);
            }
            return pows.AsEnumerable().Distinct().ToList();
        }

        private void GetInitialPlannables(List<ProductionOrderWorkSchedule> productionOrderWorkSchedules, 
            List<ProductionOrderWorkSchedule> plannedSchedules, List<ProductionOrderWorkSchedule> plannableSchedules)
        {
            plannableSchedules.AddRange(productionOrderWorkSchedules.Where(productionOrderWorkSchedule => 
                                            IsTechnologicallyAllowed(productionOrderWorkSchedule, plannedSchedules)));
        }

        private List<ProductionOrderWorkSchedule> GetBomChilds(
            ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var boms = productionOrderWorkSchedule.ProductionOrder.ProductionOrderBoms;
            var bomChilds = (from bom in boms where bom.DemandProductionOrderBoms.Any()
                             from provider in bom.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                             select provider.ProductionOrder.ProductionOrderWorkSchedule into schedules
                             select schedules.Single(a => a.HierarchyNumber == schedules.Max(b => b.HierarchyNumber))
                             ).ToList();
            return !bomChilds.Any() ? null : bomChilds;
        }
        

        private ProductionOrderWorkSchedule GetHierarchyChild(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var productionOrderWorkSchedules = _context.ProductionOrderWorkSchedules.AsNoTracking().Where(a => a.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId);
            productionOrderWorkSchedules = productionOrderWorkSchedules.Where(mainSchedule => mainSchedule.HierarchyNumber < productionOrderWorkSchedule.HierarchyNumber);
            if (!productionOrderWorkSchedules.Any()) return null;
            return productionOrderWorkSchedules.Single(a => a.HierarchyNumber == productionOrderWorkSchedules.Max(b => b.HierarchyNumber));
        }

        public void SetMachines(int simulationId)
        {  
            //gets called when plan is fitting to capacities
            var schedules = GetProductionSchedules(simulationId);
            foreach (var schedule in schedules)
            {
                var machines = _context.Machines.Where(a => a.MachineGroupId == schedule.MachineGroupId).ToList();
                if (!machines.Any()) continue;
                var schedulesOnMachineGroup = schedules.FindAll(a => a.MachineGroupId == schedule.MachineGroupId && a.MachineId != null);
                var crossingPows = (from scheduleMg in schedulesOnMachineGroup where DetectCrossing(schedule, scheduleMg) select schedule).ToList();
                if (!crossingPows.Any()) schedule.MachineId = machines.First().Id;
                else
                {
                    foreach (var machine in machines)
                    {
                        if (crossingPows.Find(a => a.MachineId == machine.Id) != null) continue;
                        schedule.MachineId = machine.Id;
                        break;
                    }
                }
            }
        }

        private bool DetectCrossing(ProductionOrderWorkSchedule schedule, ProductionOrderWorkSchedule scheduleMg)
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

