using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.DB;
using Master40.DB.DB.Interfaces;
using Master40.DB.DB.Models;
using Master40.DB.Migrations;
using Master40.MessageSystem.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Master40.Simulation.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
    }
    
    public class Simulator : ISimulator
    {
        private readonly ProductionDomainContext _context = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        private readonly IProcessMrp _processMrp;
        private readonly IMessageHub _messageHub;
        //private readonly HubCallback _hubCallback;
        public Simulator(IMessageHub messageHub, ProductionDomainContext originalContext )
        {
            _messageHub = messageHub;
            //originalContext.CopyAllTables(_context);
            originalContext.SaveSimulationState(_context, "First", SimulationType.Central);
            _processMrp = new ProcessMrpSim(_context, messageHub);
            
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
                CreateMachinesReady(timeTable);
                if (!_context.ProductionOrderWorkSchedule.Any()) return;
                while (timeTable.Items.Any(a => a.SimulationState == SimulationState.Waiting) || timeTable.Items.Any(a => a.SimulationState == SimulationState.InProgress) || waitingItems.Any())
                {
                    timeTable = ProcessTimeline(timeTable, waitingItems, id);
                    _messageHub.SendToAllClients(timeTable.Items.Count + "/" + timeTable.Items.Count + waitingItems.Count + " items processed.");
                }
                // end simulation and Unlock Screen
                _messageHub.EndScheduler();
            });

        }

        private void CreateMachinesReady(TimeTable<ISimulationItem> timeTable)
        {
            foreach (var machine in _context.Machines)
            {
                timeTable.ListMachineStatus.Add(new TimeTable<ISimulationItem>.MachineStatus()
                {
                    MachineId = machine.Id,
                    Free = true
                });
            }
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
            var freeMachineIds = GetFreeMachines(timeTable);
            if (waitingItems.Any() && freeMachineIds.Any())
            {
                foreach (var freeMachineId in freeMachineIds)
                {
                    
                    var relevantItems = (from wI in waitingItems where wI.MachineId == freeMachineId select wI).ToList();
                    if (!relevantItems.Any()) continue;
                    var items = (from tT in relevantItems
                                 where tT.SimulatedStart == relevantItems.Min(a => a.SimulatedStart)
                                 select tT).ToList();
                    var item = items.Single(a => a.Start == items.Min(b => b.Start));
                    
                    //check children if they are finished
                    if (!AllSimulationChildrenFinished(item, timeTable.Items)) continue;

                    // Roll new Duration
                    var rnd = GetRandomDelay();

                    // set 0 to 0 if below 0 to prevent negativ starts
                    if (item.SimulatedEnd - item.SimulatedStart - rnd <= 0)
                        rnd = 0;

                    var newDuration = item.SimulatedEnd - item.SimulatedStart + rnd;
                    if (newDuration != item.SimulatedEnd - item.SimulatedStart)
                    {

                        // set Time
                        //if (item.SimulatedStart == 0) item.SimulatedStart = item.Start;
                        item.SimulatedEnd = item.SimulatedStart + newDuration;
                        item.SimulatedDuration = newDuration;
                    }

                    //add next in line for this machine
                    if (timeTable.Timer != item.SimulatedStart)
                    {
                        item.SimulatedStart = timeTable.Timer;
                        item.SimulatedEnd = item.SimulatedStart + item.SimulatedDuration;
                    }
                    _context.Update(item);
                    _context.SaveChanges();

                    timeTable.Items.Add(new PowsSimulationItem(item.ProductionOrderWorkScheduleId,
                        item.ProductionOrderId, item.SimulatedStart, item.SimulatedEnd, _context));
                    waitingItems.Remove(item);
                    timeTable.ListMachineStatus.Single(a => a.MachineId == freeMachineId).Free = false;
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
                        a => ((PowsSimulationItem)a).ProductionOrderWorkScheduleId == pows.Id && a.SimulationState == SimulationState.Finished);
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
            if (timeTableItems.Exists(a => ((PowsSimulationItem) a).ProductionOrderWorkScheduleId == pows.Id))
                return timeTableItems.Find(a => ((PowsSimulationItem) a).ProductionOrderWorkScheduleId == pows.Id)
                           .SimulationState == SimulationState.Finished;
            else return false;

        }

        private List<int> GetFreeMachines(TimeTable<ISimulationItem> timeTable)
        {
            var freeMachines = timeTable.ListMachineStatus.Where(a => a.Free).Select(a => a.MachineId).ToList();
            return freeMachines;
        }

        private void Recalculate(int timer)
        {
            //Todo: change capacityScheduling to Spows
            //_processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
            //
        }
    }
    
}

