using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
        //TimeTable<ISimulationItem> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<SimulationProductionOrderWorkSchedule> waitingItems, int id);
    }
    
    public class Simulator : ISimulator
    {
        private readonly ProductionDomainContext _context;
        private readonly IProcessMrp _processMrp;
        //private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, IProcessMrp processMrp)//, HubCallback hubCallback)
        {
            _context = context;
            _processMrp = processMrp;
            //_hubCallback = hubCallback;
        }
        
        private List<SimulationProductionOrderWorkSchedule> CreateInitialTable(int id)
        {
            var waitingItems = new List<SimulationProductionOrderWorkSchedule>();
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
                    _context.ProductionOrderWorkSchedule.AsNoTracking().Where(
                        a => a.ProductionOrderId == singleProvider.ProductionOrderId).ToList());
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
                singleSpows.SimulationId = id;
            }
            
            waitingItems.AddRange(spows);
            _context.SimulationProductionOrderWorkSchedules.AddRange(spows);
            _context.SaveChanges();
            return waitingItems;
        }

        public async Task Simulate()
        {
            await Task.Run(() =>
            {
                var timeTable = new TimeTable<ISimulationItem>(24);
                var id = CreateSimulationId();
                var waitingItems = CreateInitialTable(id);
                if (!_context.ProductionOrderWorkSchedule.Any()) return;
                while (timeTable.Items.Any(a => a.SimulationState == SimulationState.Waiting) || timeTable.Items.Any(a => a.SimulationState == SimulationState.InProgress) || waitingItems.Any())
                {
                    timeTable = ProcessTimeline(timeTable, waitingItems, id);
                }
            });

        }

        private int CreateSimulationId()
        {
            return _context.SimulationProductionOrderWorkSchedules.Any() ? _context.SimulationProductionOrderWorkSchedules.Max(a => a.SimulationId)+1 : 1;
        }

        public TimeTable<ISimulationItem> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<SimulationProductionOrderWorkSchedule> waitingItems, int simulationId)
        {
            //Todo: implement statistics
            
            // Timewarp - set Start Time
            if (waitingItems.Any())
            {
                timeTable.Timer = waitingItems.Min(a => a.SimulatedStart);
                foreach (var item in (from tT in waitingItems where tT.SimulatedStart == waitingItems.Min(a => a.SimulatedStart) select tT).ToList())
                {
                    //Todo: bomparents don´t change time
                    // Roll new Duration
                    //var rnd = new RandomNumbers().RandomInt();
                    var rnd = -1;
                    // set 0 to 0 if below 0 to prevent negativ starts
                    if (item.SimulatedEnd - item.SimulatedStart - rnd <= 0)
                        rnd = 0;

                    var newDuration = item.End - item.Start + rnd;
                    if (newDuration != item.End - item.Start)
                    {
                        //nachfolger auf maschine prüfen

                        var parent = _context.SimulationProductionOrderWorkScheduleGetParent(item, simulationId);
                        if (parent != null)
                        {
                            var parentStart = item.SimulatedStart + newDuration;
                            if (parent.SimulatedStart != parentStart)
                            {//wenn parentstart < parent.SimulatedStart -> nimmt er fälschlicherweise parent.SimulatedStart
                                var earliestStart = _context.GetLatestEndFromChild(parent);
                                if (earliestStart <= parentStart)
                                {
                                    parent = waitingItems.Single(a => a.Id == parent.Id && a.SimulationId == simulationId);
                                    parent.SimulatedEnd -= parent.SimulatedStart - parentStart;
                                    parent.SimulatedStart = parentStart;
                                }
                                else
                                {
                                    parent = waitingItems.Single(a => a.Id == parent.Id && a.SimulationId == simulationId);
                                    parent.SimulatedEnd -= parent.SimulatedStart - earliestStart;
                                    parent.SimulatedStart = earliestStart;
                                }
                                _context.Update(parent);
                                _context.SaveChanges();
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

                    //AddToInProgress(item, timeTable);
                }
            }

            if (timeTable.Timer > timeTable.RecalculateTimer)
            {
                Recalculate(timeTable.Timer);
                timeTable.RecalculateTimer += 24;
            }

            // if Progress is empty Stop.
            return timeTable;
        }

        private void Recalculate(int timer)
        {
            //Todo: change capacityScheduling to Spows
            // _processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
        }
    }
    
}
