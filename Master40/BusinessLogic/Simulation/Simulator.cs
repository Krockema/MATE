using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Repository;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Master40.DB.Models;
using Master40.Extensions;
using Master40.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
        TimeTable<SimulationProductionOrderWorkSchedule> ProcessTimeline(TimeTable<SimulationProductionOrderWorkSchedule> timeTable, int id);
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
        
        private void CreateInitialTable(TimeTable<SimulationProductionOrderWorkSchedule> timeTable, int id)
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
                var id = CreateSimulationId();
                if (!_context.ProductionOrderWorkSchedule.Any()) return;
                while (timeTable.Initial.Any() && timeTable.InProgress.Any() || !timeTable.Finished.Any())
                {
                    timeTable = ProcessTimeline(timeTable, id);
                }
            });

        }

        private int CreateSimulationId()
        {
            return _context.SimulationProductionOrderWorkSchedules.Any() ? _context.SimulationProductionOrderWorkSchedules.Max(a => a.SimulationId)+1 : 1;
        }

        public TimeTable<SimulationProductionOrderWorkSchedule> ProcessTimeline(TimeTable<SimulationProductionOrderWorkSchedule> timeTable, int simulationId)
        {
            //Todo: implement statistics

            if (!timeTable.Initial.Any() && !timeTable.Finished.Any() && !timeTable.InProgress.Any())
                CreateInitialTable(timeTable,simulationId);
            

            // Timewarp - set Start Time
            if (timeTable.Initial.Any())
            {
                timeTable.Timer = timeTable.Initial.Min(a => a.SimulatedStart);
                foreach (var item in(from tT in timeTable.Initial where tT.SimulatedStart == timeTable.Initial.Min(a => a.SimulatedStart) select tT).ToList())
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
                        var parent = _context.SimulationProductionOrderWorkScheduleGetParent(item,simulationId);
                        if (parent != null)
                        {
                            var parentStart = item.SimulatedStart + newDuration;
                            if (parent.SimulatedStart != parentStart)
                            {//wenn parentstart < parent.SimulatedStart -> nimmt er fälschlicherweise parent.SimulatedStart
                                var earliestStart = _context.GetLatestEndFromChild(parent);
                                if (earliestStart <= parentStart)
                                {
                                    parent = timeTable.Initial.Single(a => a.Id == parent.Id && a.SimulationId == simulationId);
                                    parent.SimulatedEnd -= parent.SimulatedStart - parentStart;
                                    parent.SimulatedStart = parentStart;
                                }
                                else
                                {
                                    parent = timeTable.Initial.Single(a => a.Id == parent.Id && a.SimulationId == simulationId);
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
                if (_context.SimulationProductionOrderWorkScheduleGetParent(finished.First(),simulationId) != null ||
                    timeTable.InProgress.Any(a => a.ProductionOrderId == finished.First().ProductionOrderId))
                    continue;
                //var demand = _context.GetDemand(finished.First());
                //demand.State = State.Produced;
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
