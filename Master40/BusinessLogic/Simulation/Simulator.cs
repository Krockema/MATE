using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Repository;
using Master40.DB.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Master40.BusinessLogic.Simulation
{
    public class Simulator
    {
        private readonly ProductionDomainContext _context;
        public Simulator(ProductionDomainContext context)
        {
            _context = context;
        }
        
        private void CreateInitialTable(TimeTable<ProductionOrderWorkSchedule> timeTable)
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
            timeTable.Initial.AddRange(pows);
        }

        private void FillAbleToStartList(TimeTable<ProductionOrderWorkSchedule> timeTable)
        {
            var helperPowsList = new List<ProductionOrderWorkSchedule>();
            foreach (var initial in timeTable.Initial)
            {
                if (!_context.ProductionOrderWorkScheduleIsLowestHierarchy(initial))
                    continue;
                if (!_context.ProductionOrderHasChildren(initial))
                    continue;
                helperPowsList.Add(initial);
            }
            
            foreach (var finished in timeTable.Finished)
            {
                var parent = _context.ProductionOrderWorkScheduleGetParent(finished);
                if (parent != null && timeTable.Initial.Contains(parent))
                {
                    helperPowsList.Add(parent);
                }
            }
            foreach (var pows in helperPowsList)
            {
                AddToAbleToStart(pows, timeTable);
            }
        }

        private void AddToAbleToStart(ProductionOrderWorkSchedule pows, TimeTable<ProductionOrderWorkSchedule> timeTable)
        {
            timeTable.Initial.Remove(pows);
            timeTable.AbleToStart.Add(pows);
        }

        private void AddToInProgress(ProductionOrderWorkSchedule pows, TimeTable<ProductionOrderWorkSchedule> timeTable)
        {
            timeTable.AbleToStart.Remove(pows);
            timeTable.InProgress.Add(pows);
        }

        private void AddToFinished(ProductionOrderWorkSchedule pows, TimeTable<ProductionOrderWorkSchedule> timeTable)
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
                    if (ProcessTimeline(new TimeTable<ProductionOrderWorkSchedule>()))
                        break;
                }
            });

        }

        public bool ProcessTimeline(TimeTable<ProductionOrderWorkSchedule> timeTable)
        {
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

                var newDuration = item.Duration + rnd;
                if (newDuration != item.Duration)
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

            // Check for finished Items.
            var finished = timeTable.InProgress.Where(x => x.End <= timeTable.Timer);
            foreach (var item in finished)
            {
                AddToFinished(item, timeTable);
            }


            // if Progress is empty Stop.
            return timeTable.InProgress.Any();
        }
    }
    
}
