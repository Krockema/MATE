using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int i;

        private readonly ProductionDomainContext _evaluationContext;
        private ProductionDomainContext _context;
        //private readonly CopyContext _copyContext;
        private IProcessMrp _processMrp;
        private readonly IMessageHub _messageHub;
        //private readonly HubCallback _hubCallback;
        private CapacityScheduling capacityScheduling;

        private WorkTimeGenerator _workTimeGenerator;

        private bool firstRunOfTheDay = true;

        private RebuildNets rebuildNets;
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
            //Todo: Revert changes after testing
            //var demands = _context.Demands.Include(a => a.DemandProvider).Where(a => a.State == State.ExistsInCapacityPlan).ToList();
            var demands = new List<IDemandToProvider>();
            demands.AddRange(_context.Demands.OfType<DemandOrderPart>().Include(a => a.DemandProvider).Where(a => a.State != State.Finished && a.DemandProvider.Any(b => b.State != State.Finished)).ToList());
            demands.AddRange(_context.Demands.OfType<DemandProductionOrderBom>().Include(a => a.DemandProvider).Where(a => a.State != State.Finished && a.DemandProvider.Any(b => b.State != State.Finished)).ToList());
            demands.AddRange(_context.Demands.OfType<DemandStock>().Include(a => a.DemandProvider).Where(a => a.State != State.Finished && a.DemandProvider.Any(b => b.State != State.Finished)).ToList());
            
            var pows = new List<ProductionOrderWorkSchedule>();
            foreach (var demand in demands)
            {
                _context.GetWorkSchedulesFromDemand(demand, ref pows);
            }
            pows = pows.Distinct().ToList();
            var test = pows.Where(a => a.ProductionOrder.DemandProviderProductionOrders.Any(b => b.State == State.Finished)).ToList();
            while (test.Any())
            {
                var item = test.First();
                pows.Remove(item);
                if (item != null)
                    test.Remove(item);
            }
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
                var simConfig =  _context.SimulationConfigurations.Single(x => x.Id == simulationId);
                OrderGenerator.GenerateOrders(_context, simConfig, 1);

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
                capacityScheduling = new CapacityScheduling(_context);
                rebuildNets = new RebuildNets(_context);
                await PrepareSimulationContext();
                var simNumber = _context.GetSimulationNumber(simulationId, SimulationType.Central);
                var simConfig = _context.SimulationConfigurations.Single(a => a.Id == simulationId);
                simConfig.Time = 0;
                OrderGenerator.GenerateOrders(_context, simConfig, simNumber);
                _workTimeGenerator = new WorkTimeGenerator(simConfig.Seed,simConfig.WorkTimeDeviation, simNumber);
                var timeTable = new TimeTable<ISimulationItem>(simConfig.RecalculationTime, simConfig.DynamicKpiTimeSpan);
                UpdateStockExchangesWithInitialValues(simulationId, simNumber);
                await Recalculate(timeTable,simulationId, simNumber, new List<ProductionOrderWorkSchedule>());

                //var waitingItems = CreateInitialTable();
                var waitingItems = _context.ProductionOrderWorkSchedules.Where(a =>
                    a.ProducingState == ProducingState.Created || a.ProducingState == ProducingState.Waiting).ToList();
                CreateMachinesReady(timeTable);
                timeTable = UpdateGoodsDelivery(timeTable,simulationId);
                //timeTable = CreateInjectionOrders(timeTable);
                var itemCounter = 0;
                while (timeTable.Timer < simConfig.SimulationEndTime)
                {
                    timeTable = await ProcessTimeline(timeTable, waitingItems, simulationId, simNumber);
                    if (itemCounter == timeTable.Items.Count) continue;
                    itemCounter = timeTable.Items.Count;
                    _messageHub.SendToAllClients(itemCounter + "/" + (timeTable.Items.Count + waitingItems.Count) + " items processed. Time: "+timeTable.Timer);
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
                _messageHub.SendToAllClients(waitingItems.Count+" waiting Items!");
                var stocks = _context.Stocks.Where(a => a.Current > 0 && a.Article.ToBuild);
                foreach (var stock in stocks)
                {
                    _messageHub.SendToAllClients("Article: "+stock.ArticleForeignKey + " Current: "+stock.Current);
                }
                var bom = _context.Demands.OfType<DemandProductionOrderBom>();
                _messageHub.SendToAllClients(bom.Count(a => a.State != State.Finished)+" bom unfinished of "+bom.Count());
                var boms = bom.Where(a => a.State != State.Finished).Select(a => a.ArticleId).Distinct();
                foreach (var b in boms)
                {
                    _messageHub.SendToAllClients(b + " Article bom unfinished");
                }
                var op = _context.Demands.OfType<DemandOrderPart>();
                _messageHub.SendToAllClients(op.Count(a => a.State != State.Finished) + " op unfinished of " + op.Count());
                var ds = _context.Demands.OfType<DemandStock>();
                _messageHub.SendToAllClients(ds.Count(a => a.State != State.Finished) + " ds unfinished of " + ds.Count());
                _processMrp.UpdateDemandsAndOrders(simulationId);
                var pows = _context.ProductionOrderWorkSchedules;
                _messageHub.SendToAllClients(pows.Count(a => a.ProducingState != ProducingState.Finished)+" unfinished pows of " +pows.Count());
                var po = _context.ProductionOrders;
                
                SaveCompletedContext(timeTable,simulationId,simNumber);
                FillSimulationWorkSchedules(timeTable.Items.OfType<PowsSimulationItem>().ToList(),simulationId, simNumber);
                _messageHub.SendToAllClients("last Item produced at: " +_context.SimulationWorkschedules.Max(a => a.End));
                CalculateKpis.CalculateAllKpis(_context, simulationId, SimulationType.Central, simNumber, true, simConfig.Time);
                CopyResults.Copy(_context, _evaluationContext);
                _messageHub.EndScheduler();
                _context.Database.CloseConnection();
            });

        }

        private void UpdateStockExchangesWithInitialValues(int simulationId, int simulationNumber)
        {
            foreach (var stock in _context.Stocks.Where(a => a.Current > 0))
            {
                _context.Add(new StockExchange()
                {
                    ExchangeType = ExchangeType.Insert,
                    Quantity = stock.Current,
                    StockId = stock.Id,
                    Time = 0,
                    State = State.Finished,
                    SimulationType = SimulationType.Central,
                    SimulationConfigurationId = simulationId,
                    SimulationNumber = simulationNumber
                });
            }
            _context.SaveChanges();
        }

        private void SaveCompletedContext(TimeTable<ISimulationItem> timetable,int simulationId, int simulationNumber)
        {                  
            var finishedOrders = _context.Orders.Where(a => a.State == State.Finished).Include(a => a.OrderParts).ThenInclude(b => b.DemandOrderParts);
            var counterPows = _context.ProductionOrderWorkSchedules.Count();
            var counter = (from order in finishedOrders.ToList()
                           from orderPart in order.OrderParts.ToList()
                           from dop in orderPart.DemandOrderParts.ToList()
                            select CopyDemandsAndPows(dop, timetable, simulationId, simulationNumber)).Sum();
            _messageHub.SendToAllClients(counterPows + " Pows -> now " + _context.ProductionOrderWorkSchedules.Count());
            _messageHub.SendToAllClients(counter+" simPows deleted, now there are "+timetable.Items.Count+" left");
            
        }

        private int CopyDemandsAndPows(IDemandToProvider dop, TimeTable<ISimulationItem> timetable, int simulationId, int simulationNumber)
        {
            i++;
            Debug.WriteLine("Schleifendurchlauf: "+i);
            var providerlist = dop.DemandProvider.OfType<DemandProviderProductionOrder>().ToList();
            var counter = 0;
            while (providerlist.Any())
            {
                var provider = providerlist.First();
                foreach (var pob in provider.ProductionOrder.ProductionOrderBoms.ToList())
                {
                    foreach (var dpob in pob.DemandProductionOrderBoms.ToList())
                    {
                        CopyDemandsAndPows(dpob, timetable,simulationId,simulationNumber);
                    }
                    _context.ProductionOrderBoms.Remove(pob);
                }
                if (provider.ProductionOrder.DemandProviderProductionOrders.Any(a => a.Id != provider.Id))
                {
                    _context.Demands.Remove(_context.Demands.Single(a => a.Id == provider.Id));
                    var po = _context.ProductionOrders.Single(a => a.Id == provider.ProductionOrderId);
                    po.DemandProviderProductionOrders.Remove(provider);
                    _context.Update(po);
                }
                else
                {
                    var items = new List<PowsSimulationItem>();
                    foreach (var schedule in provider.ProductionOrder.ProductionOrderWorkSchedule.ToList())
                    {
                        _context.ProductionOrderWorkSchedules.Remove(schedule);
                        var itemList = timetable.Items.OfType<PowsSimulationItem>()
                            .Where(a => a.ProductionOrderWorkScheduleId == schedule.Id).ToList();
                        if (!itemList.Any()) continue;
                        items.Add(itemList.First());
                        timetable.Items.Remove(itemList.First());
                        counter++;
                    }
                    FillSimulationWorkSchedules(items, simulationId, simulationNumber);

                    _context.ProductionOrders.Remove(provider.ProductionOrder);
                    foreach (var pob in provider.ProductionOrder.ProductionOrderBoms)
                    {
                        _context.ProductionOrderBoms.Remove(pob);
                    }
                }
                
                _context.Demands.Remove(provider);
                
                providerlist.RemoveAt(0);
            }
            _context.Demands.Remove((DemandToProvider)dop);
            _context.SaveChanges();
            return counter;
        }

        private void FillSimulationWorkSchedules(List<PowsSimulationItem> items, int simulationId, int simulationNumber)
        {
            foreach (var item in items)
            {
                var po = _context.ProductionOrders.Include(b => b.Article).Single(a => a.Id == item.ProductionOrderId);
                var pows = _context.ProductionOrderWorkSchedules.Single(a => a.Id == item.ProductionOrderWorkScheduleId);
                var schedule = new SimulationWorkschedule()
                {
                    ParentId = JsonConvert.SerializeObject(from parent in _context.GetParents(pows) select parent.Id),
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
            
        }

        private TimeTable<ISimulationItem> UpdateGoodsDelivery(TimeTable<ISimulationItem> timeTable, int simulationId)
        {
            var purchases = _context.Purchases.Include(a => a.PurchaseParts).Where(a =>
                a.DueTime > timeTable.Timer &&
                timeTable.Items.OfType<PurchaseSimulationItem>().All(b => b.PurchasePartId != a.Id));
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

        private async Task<TimeTable<ISimulationItem>> ProcessTimeline(TimeTable<ISimulationItem> timeTable, List<ProductionOrderWorkSchedule> waitingItems, int simulationId, int simNumber)
        {
            if (!firstRunOfTheDay)
            {
                timeTable = timeTable.ProcessTimeline(timeTable);
            }
            firstRunOfTheDay = false;
            _context.SimulationConfigurations.Single(a => a.Id == simulationId).Time = timeTable.Timer;
            _context.SaveChanges();
            CheckForOrderRequests(timeTable);
            var freeMachineIds = GetFreeMachines(timeTable);
            if (waitingItems.Any() && freeMachineIds.Any())
            {
                //Todo: no items fulfill AllSimulationChildrenFinished and exception at savecompletedcontext (invalidoperationexception)
                foreach (var freeMachineId in freeMachineIds)
                {
                    var relevantItems = (from wI in waitingItems where wI.MachineId == freeMachineId select wI).ToList();
                    if (!relevantItems.Any()) continue;
                    var items = (from tT in relevantItems
                                 where tT.StartSimulation == relevantItems.Min(a => a.StartSimulation)
                                 select tT).ToList();
                    var item = items.First(a => a.Start == items.Min(b => b.Start));
                    var test = waitingItems.Where(a => a.Id == 1451);
                    var test2 = waitingItems.Where(a => a.Name.Equals("Wedding"));
                    var test3 = test2.Where(a => a.Start < 1440);
                    //check children if they are finished
                    if (!AllSimulationChildrenFinished(item, timeTable.Items) ||
                        (SimulationHierarchyChildrenFinished(item, timeTable.Items) == null && !ItemsInStock(item)))
                        continue;

                    var newDuration = _workTimeGenerator.GetRandomWorkTime(item.Duration);
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
            var recalc = (timeTable.RecalculateCounter + 1) * timeTable.RecalculateTimer;
            var kpi = (timeTable.KpiCounter + 1) * timeTable.KpiTimer;
            var nextcalc = recalc < kpi ? recalc : kpi;
            if (timeTable.Timer < nextcalc) return timeTable;
            if (kpi == nextcalc)
            {
                SaveCompletedContext(timeTable,simulationId,simNumber);
                var simConfig = _context.SimulationConfigurations.Single(a => a.Id == simulationId);
                CalculateKpis.CalculateMachineUtilization(_context,simulationId,SimulationType.Central,simNumber,false, simConfig.Time);
                timeTable.KpiCounter++;
                return timeTable;
            }
            await Recalculate(timeTable,simulationId,simNumber, waitingItems);
            timeTable.Items.RemoveAll(a => a.GetType() == typeof(PowsSimulationItem) && a.SimulationState != SimulationState.InProgress);
            UpdateWaitingItems(timeTable, waitingItems);
            UpdateGoodsDelivery(timeTable,simulationId);
            timeTable.RecalculateCounter++;
            firstRunOfTheDay = true;
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
            var completeList =
                _context.ProductionOrderWorkSchedules.Where(a => a.ProducingState == ProducingState.Created);//CreateInitialTable();
            foreach (var item in completeList)
            {
                if (timeTable.Items.OfType<PowsSimulationItem>().Any(a => a.ProductionOrderWorkScheduleId == item.Id)) continue;
                if (waitingItems.Any(a => a.Id == item.Id))
                {
                    waitingItems.Remove(waitingItems.Find(a => a.Id == item.Id));
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
            return latestPows.All(a => a.ProducingState == ProducingState.Finished);
        }

        private bool? SimulationHierarchyChildrenFinished(ProductionOrderWorkSchedule item, List<ISimulationItem> timeTableItems)
        {
            var hierarchyChildren =
                       _context.ProductionOrderWorkSchedules.Where(a =>
                               a.ProductionOrderId == item.ProductionOrderId &&
                               a.HierarchyNumber < item.HierarchyNumber);
            if (!hierarchyChildren.Any()) return null;

            var pows = (from hC in hierarchyChildren where hC.HierarchyNumber == hierarchyChildren.Max(a => a.HierarchyNumber) select hC).Single();
            if (pows == null) return null;
            return pows.ProducingState == ProducingState.Finished;

        }

        private List<int> GetFreeMachines(TimeTable<ISimulationItem> timeTable)
        {
            var freeMachines = timeTable.ListMachineStatus.Where(a => a.Free).Select(a => a.MachineId).ToList();
            return freeMachines;
        }

        private async Task Recalculate(TimeTable<ISimulationItem> timetable,int simulationId,int simNumber, List<ProductionOrderWorkSchedule> waitingItems)
        {
            var simConfig = _context.SimulationConfigurations.Single(a => a.Id == simulationId);
            var filestream = System.IO.File.Create("D://stocks.csv");
            var sw = new System.IO.StreamWriter(filestream);
            foreach (var item in _context.Stocks)
            {
                sw.WriteLine(item.Name + ";" + item.Current);
            }
            sw.Dispose();
            filestream = System.IO.File.Create("D://waiting POs.csv");
            sw = new System.IO.StreamWriter(filestream);
            foreach (var item in waitingItems)
            {
                sw.WriteLine(item.Name);
            }
            sw.Dispose();
            //SaveCompletedContext(timetable,simulationId,simNumber);
            _processMrp.UpdateDemandsAndOrders(simulationId);
            var time = simConfig.Time;
            var maxAllowedTime = simConfig.Time + simConfig.MaxCalculationTime;
            var orderParts = _context.OrderParts.Include(a => a.Order).Where(a => a.IsPlanned == false
                                                                                  && a.Order.CreationTime <= time
                                                                                  && a.Order.DueTime < maxAllowedTime).Include(a => a.Article).ToList();
            _messageHub.SendToAllClients("before orderParts");
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.All, _context, simulationId,_evaluationContext);
            foreach (var orderPart in orderParts.ToList())
            {
                _messageHub.SendToAllClients("Requirements: orderPart: "+orderPart.Id+ ", DueTime: "+orderPart.Order.DueTime);
                var demand = _processMrp.GetDemand(orderPart);
                //run the requirements planning and backward/forward termination algorithm
                if (demand.State != State.Created) continue;
                _processMrp.ExecutePlanning(demand, MrpTask.All, 1);
                orderPart.IsPlanned = true;
            }
            _messageHub.SendToAllClients("before Rebuild");
            rebuildNets.Rebuild(1, _evaluationContext);
            _messageHub.SendToAllClients("before GT");
            capacityScheduling.GifflerThompsonScheduling(1);
            _messageHub.SendToAllClients("finished GT");
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.All,_context,simulationId,_evaluationContext);
            var test = _context.ProductionOrderWorkSchedules.Where(a => a.Name.Equals("Wedding") && a.Start != 0 && a.Start != 935).Min(a => a.Start);
        }

        public async Task AgentSimulatioAsync(int simulationConfigurationId)
        {

            // In-memory database only exists while the connection is open
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());

            // create OptionsBuilder with InMemmory Context
            var builder = new DbContextOptionsBuilder<MasterDBContext>();
            builder.UseSqlite(connection);
            var simNumber = _evaluationContext.GetSimulationNumber(simulationConfigurationId, SimulationType.Decentral);

            using (var c = new InMemoryContext(builder.Options))
            {   
                c.Database.OpenConnection();
                c.Database.EnsureCreated();
                InMemoryContext.LoadData(_evaluationContext, c);

                var sim = new AgentSimulation(c, _messageHub);
                await sim.RunSim(simulationConfigurationId, simNumber);


                CalculateKpis.MachineSattleTime(c, simulationConfigurationId, SimulationType.Decentral, simNumber);

                CalculateKpis.CalculateAllKpis(c, simulationConfigurationId, SimulationType.Decentral, simNumber, true);
                CopyResults.Copy(c, _evaluationContext);
            }
            connection.Close();
            _messageHub.EndSimulation("Simulation with Id:" + simulationConfigurationId + " Completed."
                                            , simulationConfigurationId.ToString()
                                            , simNumber.ToString());
        }

    }
    
}



