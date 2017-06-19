using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.MRP;
using Master40.DB.Data.Repository;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Master40.DB.Models;
using Master40.Extensions;
using Master40.SignalR;

namespace Master40.BusinessLogic.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
        TimeTable<SimulationProductionOrderWorkSchedule> ProcessTimeline(TimeTable<SimulationProductionOrderWorkSchedule> timeTable);
    }

    public class Simulator : ISimulator
    {
        private readonly ProductionDomainContext _context;
        private readonly IProcessMrp _processMrp;
        private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, IProcessMrp processMrp, HubCallback hubCallback)
        {
            _context = context;
            _processMrp = processMrp;
            _hubCallback = hubCallback;
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
            foreach (var singleSpows in spows)
            {
                singleSpows.SimulatedStart = singleSpows.Start;
                singleSpows.SimulatedEnd = singleSpows.End;
                singleSpows.SimulatedDuration = singleSpows.SimulatedDuration;
            }
            timeTable.Initial.AddRange(spows);
            _context.SimulationProductionOrderWorkSchedules.AddRange(spows);
            _context.SaveChanges();
        }

        private void AddToInProgress(SimulationProductionOrderWorkSchedule pows, TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            timeTable.Initial.Remove(pows);
            timeTable.InProgress.Add(pows);
        }

        private void AddToFinished(SimulationProductionOrderWorkSchedule pows, TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            timeTable.InProgress.Remove(pows);
            timeTable.Finished.Add(pows);
        }

        public async Task Simulate()
        {
            await Task.Run(() =>
            {
                var timeTable = new TimeTable<SimulationProductionOrderWorkSchedule>();
                while (timeTable.Initial.Any() && timeTable.InProgress.Any() || !timeTable.Finished.Any())
                {
                    timeTable = ProcessTimeline(timeTable);
                }
            });

        }

        public TimeTable<SimulationProductionOrderWorkSchedule> ProcessTimeline(TimeTable<SimulationProductionOrderWorkSchedule> timeTable)
        {
            //Todo: implement statistics

            if (!timeTable.Initial.Any() && !timeTable.Finished.Any() && !timeTable.InProgress.Any())
                CreateInitialTable(timeTable);
            

            // Timewarp - set Start Time
            if (timeTable.Initial.Any())
            {
                timeTable.Timer = timeTable.Initial.Min(a => a.SimulatedStart);
                foreach (var item in(from tT in timeTable.Initial where tT.SimulatedStart == timeTable.Initial.Min(a => a.SimulatedStart) select tT).ToList())
                {
                    //Todo: change right object
                    // Roll new Duration
                    var rnd = new RandomNumbers().RandomInt();

                    // set 0 to 0 if below 0 to prevent negativ starts
                    if (item.SimulatedEnd - item.SimulatedStart - rnd <= 0)
                        rnd = 0;

                    var newDuration = item.End - item.Start + rnd;
                    if (newDuration != item.End - item.Start)
                    {
                        var parent = _context.ProductionOrderWorkScheduleGetParent(item);
                        if (parent != null)
                        {
                            var parentStart = item.SimulatedStart + newDuration;
                            // Check for sibling rquired. --> if its last and faster parrent can start early too (If mashine is empty)
                            if (parent.SimulatedStart < parentStart)
                            {
                                parent.SimulatedStart = parentStart;
                            }
                            else
                            {
                                // delay Start caus ists not required.
                            }
                        }
                    }
                    // set Time
                    //if (item.SimulatedStart == 0) item.SimulatedStart = item.Start;
                    item.SimulatedEnd = item.SimulatedStart + newDuration;
                    item.SimulatedDuration = newDuration;
                    _context.Update(item);
                    _context.SaveChanges();
                    // Move Elements To progress List
                    AddToInProgress(item, timeTable);
                }
            }

            if (timeTable.Timer > timeTable.RecalculateTimer)
            {
                Recalculate(timeTable.Timer);
                timeTable.RecalculateTimer += 24;
            }

            // Check for finished Items.
            var finished = timeTable.InProgress.Where(x => x.End <= timeTable.Timer).ToList();
            var max = finished.Count();
            for (var i = 0; i < max; i++)
            {
                AddToFinished(finished.First(), timeTable);
                if (_context.ProductionOrderWorkScheduleGetParent(finished.First()) != null ||
                    timeTable.InProgress.Any(a => a.ProductionOrderId == finished.First().ProductionOrderId))
                    continue;
                var demand = _context.GetDemand(finished.First());
                demand.State = State.Produced;
            }


            // if Progress is empty Stop.
            return timeTable;
        }

        private void Recalculate(int timer)
        {
            _processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
        }
    }
    
}
