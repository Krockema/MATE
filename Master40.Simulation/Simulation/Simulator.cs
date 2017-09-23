using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Agents;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation.SimulationData;
using Master40.Tools.Simulation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Master40.Simulation.Simulation
{
    public interface ISimulator
    {
        Task Simulate(int simulationId);
        Task InitializeMrp(MrpTask task, int simulationId);
        Task AgentSimulatioAsync(int simulationConfigurationId);
    }

    public class Simulator : ISimulator
    {

        private readonly ProductionDomainContext _evaluationContext;
        private ProductionDomainContext _context;
        //private readonly CopyContext _copyContext;
        private IProcessMrp _processMrp;
        private readonly IMessageHub _messageHub;
        //private readonly HubCallback _hubCallback;
        public Simulator(ProductionDomainContext context, /*InMemmoryContext inMemmoryContext, */ IMessageHub messageHub)//, CopyContext copyContext)
        {
            _evaluationContext = context;
            _messageHub = messageHub;
            _context = context;
        }

        public Task PrepareSimulationContext()
        {
            return Task.Run(() =>
            {
                _processMrp = new ProcessMrpSim(_context, _messageHub);
                _context.SaveChanges();
            });
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


        public async Task InitializeMrp(MrpTask task, int simulationId)
        {
            await Task.Run(async () =>
            {
                _messageHub.SendToAllClients("Prepare InMemory Tables...", MessageType.info);
                await PrepareSimulationContext();
                OrderGenerator.GenerateOrders(_context, 0);

                //call initial central MRP-run
                await _processMrp.CreateAndProcessOrderDemand(task, _context, simulationId, _evaluationContext);

                _messageHub.EndScheduler();
            });
        }

        public async Task Simulate(int simulationId)
        {
            await Task.Run(async () =>
            {
                // send a Message to the Client that the Simulation has been started
                _messageHub.SendToAllClients("Start Simulation...", MessageType.info);
                _context = InMemoryContext.CreateInMemoryContext();
                InMemoryContext.LoadData(_evaluationContext, _context);
                await PrepareSimulationContext();
                OrderGenerator.GenerateOrders(_context, simulationId);
                // TODO: Process all orders for the current day, correct?, Why WIth 2x Context, Why Plan Capacitys with _evaluationContext -> !InMemmory
                await _processMrp.CreateAndProcessOrderDemand(MrpTask.All, _context, simulationId, _evaluationContext);
                var timeTable = new TimeTable<ISimulationItem>(_context.SimulationConfigurations.Single(a => a.Id == simulationId).RecalculationTime);
                var waitingItems = CreateInitialTable();
                CreateMachinesReady(timeTable);
                if (!_context.ProductionOrderWorkSchedules.Any()) return;
                timeTable = UpdateGoodsDelivery(timeTable,simulationId);
                timeTable = CreateInjectionOrders(timeTable);
                var itemCounter = 0;
                while (timeTable.Timer < _context.SimulationConfigurations.Single(a => a.Id == simulationId).SimulationEndTime)
                {
                    timeTable = await ProcessTimeline(timeTable, waitingItems, simulationId);
                    if (itemCounter == timeTable.Items.Count) continue;
                    itemCounter = timeTable.Items.Count;
                    _messageHub.SendToAllClients(itemCounter + "/" + (timeTable.Items.Count + waitingItems.Count) + " items processed.");
                    _processMrp.UpdateDemandsAndOrders(simulationId);
                    var test = _context.Stocks.Where(a => a.Current < 0);
                    if (!test.Any()) continue;
                    foreach (var article in test)
                    {
                        _messageHub.SendToAllClients("negative amount of " + article.Name + " in stock!");
                    }
                }
                
                // Save Current Context to Database as Complext Json
                // SaveContext();
                if (_context.Orders.Any(a => a.State != State.Finished))
                    _messageHub.SendToAllClients("still unfinished orders!");
                _processMrp.UpdateDemandsAndOrders(simulationId);
                var simNumber = _context.GetSimulationNumber(simulationId, SimulationType.Central);
                FillSimulationWorkSchedules(timeTable,simulationId, simNumber);
                _messageHub.SendToAllClients("last Item produced at: " +_context.SimulationWorkschedules.Max(a => a.End));
                CalculateKpis.CalculateAllKpis(_context, simulationId, SimulationType.Central, simNumber);
                CopyResults.Copy(_context, _evaluationContext);
                _messageHub.EndScheduler();
                _context.Database.CloseConnection();
            });

        }

        private TimeTable<ISimulationItem> CreateInjectionOrders(TimeTable<ISimulationItem> timeTable)
        {
            //CreateSimulationOrder(timeTable, new List<int>() { 1 }, new List<int>() { 1 }, 1000, 1500);
            //CreateSimulationOrder(timeTable, new List<int>() { 1 }, new List<int>() { 1 }, 1200, 1600);
            return timeTable;
        }

        private void FillSimulationWorkSchedules(TimeTable<ISimulationItem> timeTable, int simulationId, int simulationNumber)
        {
            foreach (var item in timeTable.Items.OfType<PowsSimulationItem>())
            {
                var po = _context.ProductionOrders.Include(b => b.Article).Single(a => a.Id == item.ProductionOrderId);
                var pows = _context.ProductionOrderWorkSchedules.Single(a => a.Id == item.ProductionOrderWorkScheduleId);
                var schedule = new SimulationWorkschedule()
                {
                    ParentId = JsonConvert.SerializeObject(from parents in _context.GetParents(pows) select parents.ProductionOrderId),
                    ProductionOrderId = "[" + po.Id.ToString() + "]",
                    Article = po.Article.Name,
                    DueTime = po.Duetime,
                    End = pows.EndSimulation,
                    EstimatedEnd = pows.End,
                    EstimatedStart = pows.Start,
                    HierarchyNumber = pows.HierarchyNumber,
                    Machine = _context.Machines.Single(a => a.Id == pows.MachineId).Name,
                    Start = pows.StartSimulation,
                    OrderId = JsonConvert.SerializeObject(_context.GetOrderIdsFromProductionOrder(po)),
                    SimulationConfigurationId = simulationId,
                    WorkScheduleId = pows.Id.ToString(),
                    WorkScheduleName = pows.Name,
                    SimulationType = SimulationType.Central,
                    SimulationNumber = simulationNumber

                };
                _context.Add(schedule);
                _evaluationContext.Add(schedule.CopyDbPropertiesWithoutId());
            }
            _context.SaveChanges();
            _evaluationContext.SaveChanges();
            foreach (var stockexchange in _context.StockExchanges)
            {
                stockexchange.SimulationConfigurationId = simulationId;
                stockexchange.SimulationNumber = _context.GetSimulationNumber(stockexchange.SimulationConfigurationId,SimulationType.Central);
                stockexchange.SimulationType = SimulationType.Central;
                var name = _context.Stocks.Single(b => b.Id == stockexchange.StockId).Name;
                stockexchange.StockId = _evaluationContext.Stocks.Single(a => a.Name.Equals(name)).Id;
                var exchange = new StockExchange();
                stockexchange.CopyDbPropertiesTo(exchange);
                _evaluationContext.StockExchanges.Add(exchange);
            }
            _evaluationContext.SaveChanges();

            foreach (var order in _context.Orders)
            {
                _evaluationContext.Add(new SimulationOrder()
                {
                    BusinessPartnerId = order.BusinessPartnerId,
                    CreationTime = order.CreationTime,
                    DueTime = order.DueTime,
                    FinishingTime = order.FinishingTime,
                    Name = order.Name,
                    SimulationConfigurationId = simulationId,
                    State = order.State,
                    SimulationType = SimulationType.Central,
                    
                });
            }
            _evaluationContext.SaveChanges();
            //_context.Database.CloseConnection();
        }

        private TimeTable<ISimulationItem> UpdateGoodsDelivery(TimeTable<ISimulationItem> timeTable, int simulationId)
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
                    timeTable.Items.Add(CreateNewPurchaseSimulationItem(purchasePart,simulationId));
                }
            }
            return timeTable;
        }

        private ISimulationItem CreateNewPurchaseSimulationItem(PurchasePart purchasePart, int simulationId)
        {
            return new PurchaseSimulationItem(_context)
            {
                Start = _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time,
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
            // TODO : do not forgett to change
            //later use this:
            //return new RandomNumbers().RandomInt();
            return -1;
        }

        private async Task<TimeTable<ISimulationItem>> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems, int simulationId)
        {
            //Todo: implement statistics
            timeTable = timeTable.ProcessTimeline(timeTable);
            _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time = timeTable.Timer;
            _context.SaveChanges();
            CheckForOrderRequests(timeTable);
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
                    if (!AllSimulationChildrenFinished(item, timeTable.Items) ||
                        (SimulationHierarchyChildrenFinished(item, timeTable.Items) == null && !ItemsInStock(item)))
                    {
                        // TODO : 2 test without use ? 
                        var test = ItemsInStock(item);
                        var test2 = AllSimulationChildrenFinished(item, timeTable.Items);
                        continue;
                    }

                    // Roll new Duration
                    var rnd = GetRandomDelay();

                    // ToDo: Why minus ?
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

                    timeTable.Items.Add(new PowsSimulationItem(_context)
                    {
                        End = item.EndSimulation,
                        Start = item.StartSimulation,
                        SimulationId = simulationId,
                        ProductionOrderId = item.ProductionOrderId,
                        ProductionOrderWorkScheduleId = item.Id,
                        SimulationState = SimulationState.Waiting,
                    });
                    waitingItems.Remove(item);
                    item.ProducingState = ProducingState.Waiting;
                    _context.ProductionOrderWorkSchedules.Update(item);
                    _context.SaveChanges();
                    timeTable.ListMachineStatus.Single(a => a.MachineId == freeMachineId).Free = false;
                }
            }
            
            if (timeTable.Timer < (timeTable.RecalculateCounter+1) * timeTable.RecalculateTimer) return timeTable;
            await Recalculate(simulationId);
            UpdateWaitingItems(timeTable, waitingItems);
            UpdateGoodsDelivery(timeTable,simulationId);
            timeTable.RecalculateCounter++;

            // if Progress is empty Stop.
            return timeTable;
        }

        private void CheckForOrderRequests(TimeTable<ISimulationItem> timeTable)
        {
            var osi = timeTable.Items.Where(a => a.GetType() == typeof(OrderSimulationItem) && ((OrderSimulationItem)a).AddOrder).ToList();
            if (!osi.Any() || !osi.Any(b=>((OrderSimulationItem)b).AddOrder)) return;
            foreach (var singleOsi in osi)
            {
                var order = (OrderSimulationItem) singleOsi;
                _context.CreateNewOrder(order.ArticleIds[0],order.Amounts[0],1,order.DueTime);
            }
            
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
                    // less then
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


        private bool? SimulationBomChildrenFinished(ISimulationProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
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
            return latestPows.Select(pows => timeTableItems.OfType<PowsSimulationItem>()
                    .FirstOrDefault(a => a.ProductionOrderWorkScheduleId == pows.Id 
                            && a.SimulationState == SimulationState.Finished)).All(psi => psi != null);
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

                // TODO: Return with status change?!
                return timeTableItems.OfType<PowsSimulationItem>().FirstOrDefault(a => a.ProductionOrderWorkScheduleId == pows.Id)
                           .SimulationState == SimulationState.Finished;
            return false;

        }

        private List<int> GetFreeMachines(TimeTable<ISimulationItem> timeTable)
        {
            var freeMachines = timeTable.ListMachineStatus.Where(a => a.Free).Select(a => a.MachineId).ToList();
            return freeMachines;
        }

        private async Task Recalculate(int simulationId)
        {
            _processMrp.UpdateDemandsAndOrders(simulationId);
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.All, _context, simulationId,_evaluationContext);
        }

        public async Task AgentSimulatioAsync(int simulationConfigurationId)
        {

            // In-memory database only exists while the connection is open
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());

            // create OptionsBuilder with InMemmory Context
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseSqlite(connection);


            using (var c = new InMemoryContext(builder.Options))
            {
                var simNumber = _evaluationContext.GetSimulationNumber(simulationConfigurationId, SimulationType.Decentral);
                c.Database.OpenConnection();
                c.Database.EnsureCreated();
                InMemoryContext.LoadData(_evaluationContext, c);

                var sim = new AgentSimulation(c, _messageHub);
                await sim.RunSim(simulationConfigurationId, simNumber);

                CalculateKpis.CalculateAllKpis(c, simulationConfigurationId, SimulationType.Decentral, simNumber
                    );
                CopyResults.Copy(c, _evaluationContext);
            }
            connection.Close();
            _messageHub.EndSimulation("Simulation with Id:" + simulationConfigurationId + " Completed.");
        }

    }
    
}



