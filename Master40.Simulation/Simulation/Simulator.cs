using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation.SimulationData;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
    }

    public class Simulator : ISimulator
    {

        private readonly ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        private readonly ProductionDomainContext _context;
        private readonly CopyContext _copyContext;
        private readonly IProcessMrp _processMrp;
        private readonly IMessageHub _messageHub;
        private bool _orderInjected = false;
        //private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, IMessageHub messageHub, CopyContext copyContext)
        {
            _context = context;
            _copyContext = copyContext;
            _messageHub = messageHub;

            // Create Context Copy to Simulation Context
            _context.CopyAllTables(_ctx);
            // do it on HardDisk Databse
            _processMrp = new ProcessMrpSim(_context, messageHub);
            //// Dp it inmemory
            // _processMrp = new ProcessMrpSim(_context, messageHub);
            // _context.CopyAllTables(ctx);
        }

        private List<ProductionOrderWorkSchedule> CreateInitialTable()
        {
            var demands = _context.Demands.Where(a => a.State == State.ExistsInCapacityPlan).ToList();
            var provider = new List<DemandProviderProductionOrder>();
            foreach (var demand in demands)
            {
                provider.AddRange(_context.Demands.OfType<DemandProviderProductionOrder>()
                    .Where(a => a.DemandRequester.DemandRequesterId == demand.Id
                    || a.DemandRequester.DemandRequester.DemandRequesterId == demand.Id
                    || a.DemandRequesterId == demand.Id).ToList());
            }
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var singleProvider in provider)
            {
                pows.AddRange(
                    _context.ProductionOrderWorkSchedules.Where(
                        a => a.ProductionOrderId == singleProvider.ProductionOrderId).ToList());
            }
            foreach (var singlePows in pows)
            {
                if (singlePows.DurationSimulation != 0) continue;
                singlePows.StartSimulation = singlePows.Start;
                singlePows.EndSimulation = singlePows.End;
                singlePows.DurationSimulation = singlePows.Duration;
            }
            _context.SaveChanges();
            return pows;
        }

        public async Task Simulate()
        {
            await Task.Run(() =>
            {
                // send Message to Client that Simulation has been Startet.
                _messageHub.SendToAllClients("Start Simulation...", MessageType.info);
                var timeTable = new TimeTable<ISimulationItem>(24 * 60);
                var waitingItems = CreateInitialTable();
                CreateMachinesReady(timeTable);
                if (!_context.ProductionOrderWorkSchedules.Any()) return;

                while (timeTable.Items.Any(a => a.SimulationState == SimulationState.Waiting) || timeTable.Items.Any(a => a.SimulationState == SimulationState.InProgress) || waitingItems.Any())
                {
                    timeTable = ProcessTimeline(timeTable, waitingItems);
                    _messageHub.SendToAllClients(timeTable.Items.Count + "/" + (int)(timeTable.Items.Count + (int)waitingItems.Count) + " items processed.");
                }
                // end simulation and Unlock Screen

                // Save Current Context to Database as Complext Json
                // SaveContext();
                _messageHub.EndScheduler();
            });

        }

        private void SaveContext(ProductionDomainContext _contextToSave)
        {
            //load Simulation Results to Main data Context.
            var simState = new DB.Models.Simulation
            {
                CreationDate = DateTime.Now,
                SimulationDbState = Newtonsoft.Json.JsonConvert.SerializeObject(_contextToSave.SaveSimulationState()),
                SimulationType = SimulationType.Central,
            };
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

        public TimeTable<ISimulationItem> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems)
        {
            //Todo: implement statistics
            timeTable = timeTable.ProcessTimeline(timeTable);
            if (!_orderInjected && timeTable.Timer == 1)
            {
                CreateNewOrder(1, 1);
                Recalculate(timeTable.Timer);
                _orderInjected = true;
                UpdateWaitingItems(timeTable, waitingItems);
            }
            var freeMachineIds = GetFreeMachines(timeTable);
            if (waitingItems.Any() && freeMachineIds.Any())
            {
                foreach (var freeMachineId in freeMachineIds)
                {

                    var relevantItems = (from wI in waitingItems where wI.MachineId == freeMachineId select wI).ToList();
                    if (!relevantItems.Any()) continue;
                    var items = (from tT in relevantItems
                                 where tT.StartSimulation == relevantItems.Min(a => a.StartSimulation)
                                 select tT).ToList();
                    var item = items.Single(a => a.Start == items.Min(b => b.Start));

                    //check children if they are finished
                    if (!AllSimulationChildrenFinished(item, timeTable.Items)) continue;

                    // Roll new Duration
                    var rnd = GetRandomDelay();

                    // set 0 to 0 if below 0 to prevent negativ starts
                    if (item.EndSimulation - item.EndSimulation - rnd <= 0)
                        rnd = 0;

                    var newDuration = item.EndSimulation - item.StartSimulation + rnd;
                    if (newDuration != item.EndSimulation - item.StartSimulation)
                    {

                        // set Time
                        //if (item.SimulatedStart == 0) item.SimulatedStart = item.Start;
                        item.EndSimulation = item.StartSimulation + newDuration;
                        item.DurationSimulation = newDuration;
                    }

                    //add next in line for this machine
                    if (timeTable.Timer != item.StartSimulation)
                    {
                        item.StartSimulation = timeTable.Timer;
                        item.EndSimulation = item.StartSimulation + item.DurationSimulation;
                    }
                    _context.Update(item);
                    _context.SaveChanges();

                    timeTable.Items.Add(new PowsSimulationItem(item.Id,
                        item.ProductionOrderId, item.StartSimulation, item.EndSimulation, _context));
                    waitingItems.Remove(item);
                    timeTable.ListMachineStatus.Single(a => a.MachineId == freeMachineId).Free = false;
                }
            }

            //Todo: add Recalculate Event to timetable
            if (timeTable.Timer != timeTable.RecalculateTimer) return timeTable;
            Recalculate(timeTable.Timer);
            UpdateWaitingItems(timeTable, waitingItems);
            timeTable.RecalculateTimer += 24 * 60;

            // if Progress is empty Stop.
            return timeTable;
        }

        private void UpdateWaitingItems(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems)
        {
            var completeList = CreateInitialTable();
            foreach (var item in completeList)
            {
                if (timeTable.Items.Any(a => a.ProductionOrderWorkScheduleId == item.Id)) continue;
                if (waitingItems.Any(a => a.Id == item.Id)) continue;
                waitingItems.Add(item);
            }
        }

        private void CreateNewOrder(int articleId, int amount)
        {
            var orderPart = new OrderPart()
            {
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
            };
            _context.Orders.Add(new Order()
            {
                BusinessPartnerId = _context.BusinessPartners.First().Id,
                DueTime = 100,
                Name = "injected Order",
                OrderParts = new List<OrderPart>()
            });
            _context.SaveChanges();
            orderPart.OrderId = _context.Orders.Last().Id;
            _context.OrderParts.Add(orderPart);
            _context.SaveChanges();
            _context.Orders.Last().OrderParts.Add(orderPart);
            var dop = new DemandOrderPart()
            {
                ArticleId = orderPart.ArticleId,
                Quantity = orderPart.Quantity,
                DemandProvider = new List<DemandToProvider>(),
                DemandRequesterId = null,
                State = State.Injected,
                OrderPartId = orderPart.Id
            };
            _context.Demands.Add(dop);
            _context.SaveChanges();
            orderPart.DemandOrderParts.Add(dop);
            _context.OrderParts.Update(orderPart);
            _context.SaveChanges();
        }

        private bool AllSimulationChildrenFinished(ProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var hierarchyFinished = SimulationHierarchyChildrenFinished(item, timeTableItems);
            if (hierarchyFinished != null) return (bool)hierarchyFinished;
            var bomFinished = SimulationBomChildrenFinished(item, timeTableItems);
            if (bomFinished != null) return (bool)bomFinished;
            return true;
        }


        private bool? SimulationBomChildrenFinished(ProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
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

        private bool? SimulationHierarchyChildrenFinished(ProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var hierarchyChildren =
                       _context.ProductionOrderWorkSchedules.Where(a =>
                               a.ProductionOrderId == item.ProductionOrderId &&
                               a.HierarchyNumber < item.HierarchyNumber);
            if (!hierarchyChildren.Any()) return null;

            var pows = (from hC in hierarchyChildren where hC.HierarchyNumber == hierarchyChildren.Max(a => a.HierarchyNumber) select hC).Single();
            if (timeTableItems.Exists(a => ((PowsSimulationItem)a).ProductionOrderWorkScheduleId == pows.Id))
                return timeTableItems.Find(a => ((PowsSimulationItem)a).ProductionOrderWorkScheduleId == pows.Id)
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
            _processMrp.PlanCapacities(MrpTask.GifflerThompson, timer);
            //
        }
    }

}



