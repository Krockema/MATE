using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.MRP;
using Master40.DB.Data.Repository;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Master40.DB.Models;
using Master40.Extensions;

namespace Master40.BusinessLogic.Simulation
{
    public class Simulator
    {
        private readonly ProductionDomainContext _context;
        private readonly IProcessMrp _processMrp;
        public Simulator(ProductionDomainContext context, IProcessMrp processMrp)
        {
            _context = context;
            _processMrp = processMrp;
        }
        
        private void CreateInitialTable(TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            var demands = _context.Demands.Where(a => a.State == State.ExistsInCapacityPlan).ToList();
            var provider = new List<DemandProviderProductionOrder>();
            foreach (var demand in demands)
            {
                provider.AddRange(_context.Demands.OfType<DemandProviderProductionOrder>()
                    .Where(a => a.DemandRequester.DemandRequester.DemandRequesterId == demand.DemandRequesterId));
            }
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleProvider in provider)
            {
                pows.AddRange(
                    _context.ProductionOrderWorkSchedule.Where(
                        a => a.ProductionOrderId == singleProvider.ProductionOrderId));
            }
            var spows = new List<SimulationProductionOrderWorkSchedule>();
            foreach (var singlePows in pows)
            {
                spows.Add(new SimulationProductionOrderWorkSchedule());
                singlePows.CopyPropertiesTo<ISimulationProductionOrderWorkSchedule>(spows.Last());
            }
            timeTable.Initial.AddRange(spows);
        }

        private void FillAbleToStartList(TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            var helperSpowsList = new List<SimulationProductionOrderWorkSchedule>();
            foreach (var initial in timeTable.Initial)
            {
                if (!_context.ProductionOrderWorkScheduleIsLowestHierarchy(initial))
                    continue;
                if (!_context.ProductionOrderHasChildren(initial))
                    continue;
                helperSpowsList.Add(initial);
            }
            
            foreach (var finished in timeTable.Finished)
            {
                var parent = _context.ProductionOrderWorkScheduleGetParent(finished);
                if (parent != null && timeTable.Initial.Contains(parent))
                {
                    helperSpowsList.Add(parent);
                }
            }
            foreach (var pows in helperSpowsList)
            {
                AddToAbleToStart(pows, timeTable);
            }
        }

        private void AddToAbleToStart(SimulationProductionOrderWorkSchedule pows, TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            timeTable.Initial.Remove(pows);
            timeTable.AbleToStart.Add(pows);
        }

        private void AddToInProgress(SimulationProductionOrderWorkSchedule pows, TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            timeTable.AbleToStart.Remove(pows);
            timeTable.InProgress.Add(pows);
        }

        private void AddToFinished(SimulationProductionOrderWorkSchedule pows, TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            timeTable.InProgress.Remove(pows);
            timeTable.Finished.Add(pows);
        }

        internal async Task Simulate()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (ProcessTimeline(new TimeTable<SimulationProductionOrderWorkSchedule>()))
                        break;
                }
            });

        }

        public bool ProcessTimeline(TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            //Todo: implement statistics

            if (!timeTable.Initial.Any())
                CreateInitialTable(timeTable);

            FillAbleToStartList(timeTable);

            // Timewarp - set Start Time
            timeTable.Timer = timeTable.AbleToStart.Min(x => x.Start);
            var init = timeTable.AbleToStart.Where(x => x.Start == timeTable.Timer).ToList();
            foreach (var item in init)
            {
                // Roll new Duration
                var rnd = new RandomNumbers().RandomInt();

                // set 0 to 0 if below 0 to prevent negativ starts
                if (timeTable.Timer - rnd <= 0)
                    rnd = 0;

                var newDuration = item.End - item.Start + rnd;
                if (newDuration != item.End - item.Start)
                {
                    var parent = _context.ProductionOrderWorkScheduleGetParent(item);
                    if (parent != null)
                    {
                        var parentStart = item.Start + newDuration;
                        // Check for sibling rquired. --> if its last and faster parrent can start early too (If mashine is empty)
                        if (parent.Start < parentStart)
                        {
                            parent.Start = parentStart;
                        }
                        else
                        {
                            // delay Start caus ists not required.
                        }
                    }
                }
                // set Time
                item.Start = item.Start;
                item.End = item.Start + newDuration;
                // Move Elements To progress List
                AddToInProgress(item, timeTable);

            }

            if (timeTable.Timer > timeTable.RecalculateTimer)
            {
                Recalculate(timeTable.Timer);
                timeTable.RecalculateTimer += 24;
            }

            // Check for finished Items.
            var finished = timeTable.InProgress.Where(x => x.End <= timeTable.Timer);
            foreach (var item in finished)
            {
                AddToFinished(item, timeTable);
                if (_context.ProductionOrderWorkScheduleGetParent(item) != null ||
                    timeTable.InProgress.Any(a => a.ProductionOrderId == item.ProductionOrderId))
                    continue;
                var demand = _context.GetDemand(item);
                demand.State = State.Produced;
            }


            // if Progress is empty Stop.
            return timeTable.InProgress.Any();
        }

        private void Recalculate(int timer)
        {
            _processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
        }
    }
    
}
