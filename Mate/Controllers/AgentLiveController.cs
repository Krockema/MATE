using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mate.Production.Core.Environment;
using Mate.Production.Core.SignalR;

namespace Mate.Controllers
{
    public class AgentLiveController : Controller
    {
        private readonly AgentCore _agentSimulator;
        private readonly MateProductionDb _context;
        private readonly MateResultDb _resultCtx;
        private readonly IMessageHub _messageHub;

        public AgentLiveController(AgentCore agentSimulator, MateProductionDb context, MateResultDb resultCtx, IMessageHub messageHub)
        {
            _agentSimulator = agentSimulator;
            _resultCtx = resultCtx;
            _context = context;
            _messageHub = messageHub;
        }
        public async Task<IActionResult> Index()
        {
            var resources = await _context.Resources.Where(x => x.IsPhysical)
                                              .Include(navigationPropertyPath: a => a.ResourceSetups)
                                              .ThenInclude(x => x.ResourceCapabilityProvider)
                                              .ThenInclude(x => x.ResourceCapability).ToListAsync();

            ViewData[index: "machines"] = resources.Select(selector: x => x.Name).ToList();
            return View(model: resources);
        }

        [HttpGet(template: "[Controller]/InitalizeAR")]
        public void InitalizeAR()
        {
            var resources = _context.Resources.Where(x => x.IsPhysical && !x.IsBiological)
                                                .Select(x => new { Name = x.Name.Substring(0, 3), Id = x.Id.ToString() });

            _messageHub.SendToClient("AR",System.Text.Json.JsonSerializer.Serialize(resources));
        }    

        [HttpGet(template: "[Controller]/RunAsync/{simulationType}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public void RunAsync(int simulationType, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            _context.ClearCustomerOrders();

            var simConfig = ArgumentConverter.ConfigurationConverter(resultCtx: _resultCtx, 1);
            // update customized Items
            simConfig.AddOption(new ResultsDbConnectionString(_resultCtx.Database.GetDbConnection().ConnectionString));
            simConfig.ReplaceOption(new KpiTimeSpan(240));
            simConfig.ReplaceOption(new TimeToAdvance(value: TimeSpan.FromMilliseconds(1000 * 0.2)));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderAmount));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: 0));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Mate.Production.Core.Environment.Options.Seed(value: 169));
            simConfig.ReplaceOption(new Mate.Production.Core.Environment.Options.SeedWorkTime(value: 170));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 20160));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));
            simConfig.ReplaceOption(new MaxBucketSize(480));
            simConfig.ReplaceOption(new MinDeliveryTime(10));
            simConfig.ReplaceOption(new MaxDeliveryTime(15));
            simConfig.ReplaceOption(new Mate.Production.Core.Environment.Options.PriorityRule(value: DataCore.Nominal.PriorityRule.LST));
            

            switch (simulationType)
            {
                case 1:
                    simConfig.ReplaceOption(new SimulationKind(SimulationType.Default));
                    RunAgentSimulation(simConfig); 
                    return;
                case 2: 
                    RunGanttSimulation(simConfig); 
                    return;
                case 3: 
                    simConfig.ReplaceOption(new SimulationKind(SimulationType.Queuing));
                    RunAgentSimulation(simConfig);
                    return;
                default: return;
            }

        }

        public async void RunAgentSimulation(Configuration simConfig)
        {
            simConfig.ReplaceOption(new SimulationNumber(2));
            await _agentSimulator.RunAkkaSimulation(configuration: simConfig);
        }

        public async void RunGanttSimulation(Configuration simConfig)
        {
            GanttPlanDBContext.ClearDatabase(Dbms.GetGanttDataBase(DataBaseConfiguration.GP).ConnectionString.Value);
            
            //Synchronisation GanttPlan
            GanttPlanOptRunner.Inizialize();

            var simContext = new GanttSimulation(DataBaseConfiguration.MateDb, messageHub: _messageHub);
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Central));
            simConfig.ReplaceOption(new DebugSystem(value: false));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            //emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());
            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
            }
        }


        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /// <summary>
        /// TODO : Change from uri to json Body to post any parameter (more Dynamic)
        /// </summary>
        /// <param name="items"></param>
        /*
         Sample Json for Postman Body:
         
        [{ Id: '1', Property: 'SimNumber', PropertyValue: '1', Description: 'SimNumber' },
         { Id: '2', Property: 'SimType', PropertyValue: 'Bucket', Description: 'SimType' }]

        Header: 
        Content-Type 'application/json'
        */
        [HttpPost(template: "[Controller]/StartSim")]
        public void StartSim([FromBody] List<ConfigurationItem> items)
        {
            if (ModelState.IsValid)
            {

            }
        }



        [HttpGet(template: "[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent(componentName: "SimulationTimeline", arguments: new List<int> { orderId, stateId });
        }

        [HttpGet(template: "[Controller]/MachineWorkload/{Machine}")]
        public IActionResult MachineWorkload(string machine)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent(componentName: "MachineWorkload", arguments: new { machine });
        }

        [HttpGet(template: "[Controller]/MachineBreakdown/{Machine}")]
        public void MachineBreakdown(string machine)
        {
            _agentSimulator.ResourceBreakDown(name: machine);
        }
    }
}