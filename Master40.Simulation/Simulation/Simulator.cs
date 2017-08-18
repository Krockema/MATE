using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation.SimulationData;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Master40.Simulation.Simulation
{
    public interface ISimulator
    {
        Task Simulate();
        Task InitializeMrp(MrpTask task);
    }

    public class Simulator : ISimulator
    {

        private readonly ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        private readonly ProductionDomainContext _context;
        //private readonly CopyContext _copyContext;
        private readonly IProcessMrp _processMrp;
        private readonly IMessageHub _messageHub;
        private bool _orderInjected = false;
        //private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, IMessageHub messageHub)//, CopyContext copyContext)
        {
            _context = context;
            //_copyContext = copyContext;
            _messageHub = messageHub;

            // Create Context Copy to Simulation Context
            //_context.CopyAllTables(_ctx);
            // do it on HardDisk Databse
            _processMrp = new ProcessMrpSim(_context, messageHub);
            //// Dp it inmemory
            // _processMrp = new ProcessMrpSim(_context, messageHub);
            // _context.CopyAllTables(ctx);
        }

        private List<ProductionOrderWorkSchedule> CreateInitialTable()
        {
            var demands = _context.Demands.Include(a => a.DemandProvider).Where(a => a.State == State.ExistsInCapacityPlan).ToList();
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var demand in demands)
            {
                _context.GetWorkSchedulesFromDemand(demand, ref pows);
            }
            pows = pows.Distinct().ToList();
            foreach (var singlePows in pows)
            {
                singlePows.StartSimulation = singlePows.Start;
                singlePows.EndSimulation = singlePows.End;
                singlePows.DurationSimulation = singlePows.Duration;
            }
            _context.SaveChanges();
            return pows;
        }


        public async Task InitializeMrp(MrpTask task)
        {
            await Task.Run(async () =>
            {
                //SimulationConfigurations
                var simulationConfiguration = new SimulationConfiguration()
                {
                    Lotsize = 1,
                    Time = 0,
                    MaxCalculationTime = 3000
                };
                _context.Add(simulationConfiguration);
                _context.SaveChanges();

                //call initial central MRP-run
                await _processMrp.CreateAndProcessOrderDemand(task);

                _messageHub.EndScheduler();
            });
        }

        public async Task Simulate()
        {
            await Task.Run(async () =>
            {
                // send Message to Client that Simulation has been Startet.
                _messageHub.SendToAllClients("Start Simulation...", MessageType.info);
                var timeTable = new TimeTable<ISimulationItem>(1440);
                var waitingItems = CreateInitialTable();
                CreateMachinesReady(timeTable);
                if (!_context.ProductionOrderWorkSchedules.Any()) return;
                timeTable = UpdateGoodsDelivery(timeTable);
                var itemCounter = 0;
                while (timeTable.Items.Any(a => a.SimulationState == SimulationState.Waiting) || timeTable.Items.Any(a => a.SimulationState == SimulationState.InProgress) || waitingItems.Any())
                {
                    timeTable = await ProcessTimeline(timeTable, waitingItems);
                    if (itemCounter == timeTable.Items.Count) continue;
                    _processMrp.UpdateDemandsAndOrders();
                    itemCounter = timeTable.Items.Count;
                    _messageHub.SendToAllClients(itemCounter + "/" + (int)(timeTable.Items.Count + (int)waitingItems.Count) + " items processed.");
                }
                // end simulation and Unlock Screen

                // Save Current Context to Database as Complext Json
                // SaveContext();

                _processMrp.UpdateDemandsAndOrders();
                FillSimulationWorkSchedules(timeTable);

                _messageHub.EndScheduler();
            });

        }

        private void FillSimulationWorkSchedules(TimeTable<ISimulationItem> timeTable)
        {
            foreach (var item in timeTable.Items.OfType<PowsSimulationItem>())
            {
                var po = _context.ProductionOrders.Single(a => a.Id == item.ProductionOrderId);
                var pows = _context.ProductionOrderWorkSchedules.Single(a => a.Id == item.ProductionOrderWorkScheduleId);
                _context.Add(new SimulationWorkschedule()
                {
                    ParentId = JsonConvert.SerializeObject(from parents in _context.GetParents(pows) select parents.Id),
                    ProductionOrderId = po.Id.ToString(),
                    Article = po.Name,
                    DueTime = po.Duetime,
                    End = pows.EndSimulation,
                    EstimatedEnd = pows.End,
                    EstimatedStart = pows.Start,
                    HierarchyNumber = pows.HierarchyNumber,
                    Machine = _context.Machines.Single(a => a.Id == pows.MachineId).Name,
                    Start = pows.StartSimulation,
                    OrderId = JsonConvert.SerializeObject(_context.GetOrderIdsFromProductionOrder(po)),
                    // TODO TO CHANGE
                    SimulationId = _context.SimulationConfigurations.Last().Id,
                    WorkScheduleId = pows.Id.ToString(),
                    WorkScheduleName = pows.Name
                });
            }
            _context.SaveChanges();
        }

        private TimeTable<ISimulationItem> UpdateGoodsDelivery(TimeTable<ISimulationItem> timeTable)
        {
            var purchases = _context.Purchases.Include(a => a.PurchaseParts).Where(a => a.DueTime > timeTable.Timer);
            if (purchases == null) return timeTable;
            var purchaseDeliveries = timeTable.Items.OfType<PurchaseSimulationItem>().ToList();
            foreach (var purchase in purchases)
            {
                foreach (var purchasePart in purchase.PurchaseParts)
                {
                    // check for existence in timeTable
                    var purchaseEvent = from pd in purchaseDeliveries
                               where purchasePart.PurchaseId == pd.PurchaseId && purchasePart.Id == pd.PurchasePartId
                               select pd;
                    if (purchaseEvent.Any()) continue;

                    // insert into timetable with rnd-duetime
                    timeTable.Items.Add(CreateNewPurchaseSimulationItem(purchasePart));
                }
            }
            return timeTable;
        }

        private ISimulationItem CreateNewPurchaseSimulationItem(PurchasePart purchasePart)
        {
            return new PurchaseSimulationItem(_context)
            {
                Start = _context.SimulationConfigurations.Last().Time,
                End = purchasePart.Purchase.DueTime,
                PurchaseId = purchasePart.PurchaseId,
                PurchasePartId = purchasePart.Id
            };
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

        public async Task<TimeTable<ISimulationItem>> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems)
        {
            //Todo: implement statistics
            timeTable = timeTable.ProcessTimeline(timeTable);
            _context.SimulationConfigurations.Last().Time = timeTable.Timer;
            _context.SaveChanges();
            if (!_orderInjected && timeTable.Timer > 0)
            {
                _context.CreateNewOrder(1, 1, 0, 100);
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
                    var item = items.First(a => a.Start == items.Min(b => b.Start));

                    //check children if they are finished
                    if (!AllSimulationChildrenFinished(item, timeTable.Items) || (SimulationHierarchyChildrenFinished(item, timeTable.Items) == null && !ItemsInStock(item))) continue;

                    // Roll new Duration
                    var rnd = GetRandomDelay();

                    // set 0 to 0 if below 0 to prevent negativ starts
                    if (item.DurationSimulation - rnd <= 0)
                        rnd = 0;

                    var newDuration = item.DurationSimulation + rnd;
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
                    item.ProducingState = ProducingState.Waiting;
                    _context.ProductionOrderWorkSchedules.Update(item);
                    _context.SaveChanges();
                    timeTable.ListMachineStatus.Single(a => a.MachineId == freeMachineId).Free = false;
                }
            }
            
            if (timeTable.Timer < (timeTable.RecalculateCounter+1) * timeTable.RecalculateTimer) return timeTable;
            await Recalculate();
            UpdateWaitingItems(timeTable, waitingItems);
            UpdateGoodsDelivery(timeTable);
            timeTable.RecalculateCounter++;

            // if Progress is empty Stop.
            return timeTable;
        }

        private bool ItemsInStock(ProductionOrderWorkSchedule item)
        {
            var boms = _context.ArticleBoms.Where(a => a.ArticleParentId == item.ProductionOrder.ArticleId);
            if (boms == null) return false;
            foreach (var bom in boms)
            {
                if (_context.Stocks
                        .Single(a => a.ArticleForeignKey == bom.ArticleChildId)
                        .Current
                    < item.ProductionOrder.Quantity * bom.Quantity)
                    return false;
            }
            return true;
        }

        private void UpdateWaitingItems(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems)
        {
            var completeList = CreateInitialTable();
            foreach (var item in completeList)
            {
                if (timeTable.Items.OfType<PowsSimulationItem>().Any(a => a.ProductionOrderWorkScheduleId == item.Id)) continue;
                if (waitingItems.Any(a => a.Id == item.Id))
                {
                    waitingItems.Remove(waitingItems.Find(a => a.Id == item.Id));
                    
                    waitingItems.Add(item);
                    continue;
                }
                waitingItems.Add(item);
            }
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
            var childBoms = item.ProductionOrder.ProductionOrderBoms;
            var childrenPos = (from bom in childBoms where bom.DemandProductionOrderBoms.Any()
                               from singleProvider in bom.DemandProductionOrderBoms.First().DemandProvider.OfType<DemandProviderProductionOrder>()
                               select singleProvider.ProductionOrder
                               ).ToList();
            if (!childrenPos.Any()) return null;
            var childrenPows = (from pos in childrenPos
                               from pows in pos.ProductionOrderWorkSchedule
                               where pows.HierarchyNumber == pos.ProductionOrderWorkSchedule.Max(a => a.HierarchyNumber)
                               select pows).ToList();

            var latestPows = from cP in childrenPows where cP.End == childrenPows.Max(a => a.End) select cP;
            return latestPows.Select(pows => timeTableItems.OfType<PowsSimulationItem>().FirstOrDefault(a => a.ProductionOrderWorkScheduleId == pows.Id && a.SimulationState == SimulationState.Finished)).All(psi => psi != null);
        }

        private bool? SimulationHierarchyChildrenFinished(ProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var hierarchyChildren =
                       _context.ProductionOrderWorkSchedules.Where(a =>
                               a.ProductionOrderId == item.ProductionOrderId &&
                               a.HierarchyNumber < item.HierarchyNumber);
            if (!hierarchyChildren.Any()) return null;

            var pows = (from hC in hierarchyChildren where hC.HierarchyNumber == hierarchyChildren.Max(a => a.HierarchyNumber) select hC).Single();
            if (timeTableItems.OfType<PowsSimulationItem>().FirstOrDefault(a => a.ProductionOrderWorkScheduleId == pows.Id) != null)
                return timeTableItems.OfType<PowsSimulationItem>().FirstOrDefault(a => a.ProductionOrderWorkScheduleId == pows.Id)
                           .SimulationState == SimulationState.Finished;
            return false;

        }

        private List<int> GetFreeMachines(TimeTable<ISimulationItem> timeTable)
        {
            var freeMachines = timeTable.ListMachineStatus.Where(a => a.Free).Select(a => a.MachineId).ToList();
            return freeMachines;
        }

        private async Task Recalculate()
        {
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.All);
        }
    }
    
}



