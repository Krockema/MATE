using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Master40.DB.Migrations;
using Master40.MessageSystem.SignalR;
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
        private readonly MessageHub _messageHub;
        //private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, IProcessMrp processMrp, MessageHub messageHub)
        {
            _context = context;
            _messageHub = messageHub;
            _processMrp = processMrp;
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
                if (singlePows.MachineId != null) spows.Last().MachineId = (int)singlePows.MachineId;
                spows.Last().ProductionOrderWorkScheduleId = singlePows.Id;
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
                // send Message to Client that Simulation has been Startet.
                _messageHub.SendToAllClients("Start Simulation...", MessageType.info);
                var timeTable = new TimeTable<ISimulationItem>(24*60);
                var id = CreateSimulationId();
                var waitingItems = CreateInitialTable(id);
                timeTable = CreateInitialSimulationTable(timeTable,waitingItems);
                if (!_context.ProductionOrderWorkSchedule.Any()) return;
                while (timeTable.Items.Any(a => a.SimulationState == SimulationState.Waiting) || timeTable.Items.Any(a => a.SimulationState == SimulationState.InProgress) || waitingItems.Any())
                {
                    timeTable = ProcessTimeline(timeTable, waitingItems, id);
                }
                // end simulation and Unlock Screen
                _messageHub.EndScheduler();
            });

        }

        private TimeTable<ISimulationItem> CreateInitialSimulationTable(TimeTable<ISimulationItem> timeTable, List<SimulationProductionOrderWorkSchedule> waitingItems)
        {
            var items = new List<SimulationProductionOrderWorkSchedule>();
            foreach (var machine in _context.Machines)
            {
                if (waitingItems.Any(a => a.MachineId == machine.Id))
                {
                    items.AddRange(waitingItems.Where(a => a.MachineId == machine.Id).ToList()
                        .FindAll(b => b.Start ==
                            waitingItems.Where(a => a.MachineId == machine.Id).Min(a => a.Start)));
                }
            }
           
            foreach (var item in items)
            {
                item.SimulatedEnd = item.SimulatedStart + GetRandomDelay();
                timeTable.Items.Add(new PowsSimulationItem(item.ProductionOrderWorkScheduleId,item.ProductionOrderId,item.Start,item.End,_context));
                waitingItems.Remove(item);
            }
            
            
            //Add new Orders here


            return timeTable;
        }

        private int GetRandomDelay()
        {
            //later use this:
            //return new RandomNumbers().RandomInt();
            return -1;
        }

        private int CreateSimulationId()
        {
            return _context.SimulationProductionOrderWorkSchedules.Any() ? _context.SimulationProductionOrderWorkSchedules.Max(a => a.SimulationId)+1 : 1;
        }

        public TimeTable<ISimulationItem> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<SimulationProductionOrderWorkSchedule> waitingItems, int simulationId)
        {
            //Todo: implement statistics
            timeTable = timeTable.ProcessTimeline(timeTable);
            var needNextItems = NeedToAddNextItems(timeTable);
            if (waitingItems.Any() && needNextItems.Any())
            {
                foreach (var needNextItem in needNextItems)
                {
                    var machineId =
                        _context.ProductionOrderWorkSchedule.Single(
                            a => a.Id == needNextItem.ProductionOrderWorkScheduleId).MachineId;
                    var relevantItems = from wI in waitingItems where wI.MachineId == machineId select wI;
                    foreach (
                        var item in
                        (from tT in relevantItems
                            where tT.SimulatedStart == waitingItems.Min(a => a.SimulatedStart)
                            select tT).ToList())
                    {
                        // Roll new Duration
                        var rnd = GetRandomDelay();

                        // set 0 to 0 if below 0 to prevent negativ starts
                        if (item.SimulatedEnd - item.SimulatedStart - rnd <= 0)
                            rnd = 0;

                        var newDuration = item.End - item.Start + rnd;
                        if (newDuration != item.End - item.Start)
                        {

                            // set Time
                            //if (item.SimulatedStart == 0) item.SimulatedStart = item.Start;
                            item.SimulatedEnd = item.SimulatedStart + newDuration;
                            item.SimulatedDuration = newDuration;
                        }
                        //check children if they are finished
                        if (!AllSimulationChildrenFinished(item, timeTable.Items)) break;

                        //add next in line for this machine
                        if (timeTable.Timer > item.Start)
                        {
                            item.SimulatedStart = timeTable.Timer;
                            item.SimulatedEnd = item.SimulatedStart + item.SimulatedDuration;
                        }
                        _context.Update(item);
                        _context.SaveChanges();

                        timeTable.Items.Add(new PowsSimulationItem(item.ProductionOrderWorkScheduleId,
                            item.ProductionOrderId, item.Start, item.End, _context));
                        waitingItems.Remove(item);
                    }
                }
            }

            //Todo: add Recalculate Event to timetable
            if (timeTable.Timer != timeTable.RecalculateTimer) return timeTable;
            Recalculate(timeTable.Timer);
            timeTable.RecalculateTimer += 24*60;

            // if Progress is empty Stop.
            return timeTable;
        }

        private bool AllSimulationChildrenFinished(SimulationProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var hierarchyFinished = SimulationHierarchyChildrenFinished(item, timeTableItems);
            if (hierarchyFinished != null) return (bool)hierarchyFinished;
            var bomFinished = SimulationBomChildrenFinished(item, timeTableItems);
            if (bomFinished != null) return (bool)bomFinished;
            return true;
        }
        

        private bool? SimulationBomChildrenFinished(SimulationProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var childrenPos =
            _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == item.ProductionOrderId)
                .Select(a => a.ProductionOrderChild);
            if (!childrenPos.Any()) return null;
            var childrenPows = from pos in childrenPos
                                from pows in pos.ProductionOrderWorkSchedule
                                where pows.HierarchyNumber == pos.ProductionOrderWorkSchedule.Max(a => a.HierarchyNumber)
                                select pows;

            var latestPows = from cP in childrenPows where cP.End == childrenPows.Max(a => a.End) select cP;
            foreach (var pows in latestPows)
            {
                var psi = (PowsSimulationItem)
                    timeTableItems.Find(
                        a => ((PowsSimulationItem)a).ProductionOrderWorkScheduleId == pows.Id && a.SimulationState != SimulationState.Finished);
                if (psi == null) return false;
            }
            return true;

        }

        private bool? SimulationHierarchyChildrenFinished(SimulationProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems )
        {
            var hierarchyChildren =
                       _context.ProductionOrderWorkSchedule.Where(a =>
                               a.ProductionOrderId == item.ProductionOrderId &&
                               a.HierarchyNumber < item.HierarchyNumber);
            if (!hierarchyChildren.Any()) return null;
            
            var pows = (from hC in hierarchyChildren where hC.HierarchyNumber == hierarchyChildren.Max(a => a.HierarchyNumber) select hC).Single();
            return timeTableItems.Find(a => ((PowsSimulationItem)a).ProductionOrderWorkScheduleId == pows.Id)
                       .SimulationState == SimulationState.Finished;
            
        }

        private List<PowsSimulationItem> NeedToAddNextItems(TimeTable<ISimulationItem> timeTable)
        {
            var list = new List<PowsSimulationItem>();
            var finished = timeTable.Items.Where(a => a.SimulationState == SimulationState.Finished).ToList();
            if (!finished.Any()) return list;

            /* longer version of the below return statement
            foreach (var singleFinished in finished)
            {
                if (singleFinished.GetType() != typeof(PowsSimulationItem)) continue;
                if (((PowsSimulationItem)singleFinished).NeedToAddNext)
                    list.Add((PowsSimulationItem)singleFinished);
            }*/
            list.AddRange(finished.Where(singleFinished => singleFinished.GetType() == typeof(PowsSimulationItem)).Where(singleFinished => ((PowsSimulationItem) singleFinished).NeedToAddNext).Cast<PowsSimulationItem>());
            return list;
        }

        private void Recalculate(int timer)
        {
            //Todo: change capacityScheduling to Spows
            // _processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
        }
    }
    
}
