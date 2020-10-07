using System;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
using Master40.DB.ReportingModel;
using Master40.Simulation;
using Master40.Simulation.CLI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.SimulationCore.Environment.Options;
using Master40.Tools.Connectoren.Ganttplan;
using Master40.SimulationCore;
using Master40.Tools.SignalR;

namespace Master40.Controllers
{
    public class AgentLiveController : Controller
    {
        private readonly AgentCore _agentSimulator;
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultCtx;
        private readonly IMessageHub _messageHub;
        private const string GanttPlanCtxString = "SERVER=(localdb)\\MSSQLLocalDB;DATABASE=GanttPlanImportTestDB;Trusted_connection=Yes;UID=;PWD=;MultipleActiveResultSets=true";

        public AgentLiveController(AgentCore agentSimulator, ProductionDomainContext context, ResultContext resultCtx, IMessageHub messageHub)
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

        [HttpGet(template: "[Controller]/RunAsync/{simulationType}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public async void RunAsync(int simulationType, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            var simKind = SimulationType.None;
            switch (simulationType)
            {
                case 1: simKind = SimulationType.Default; break;
                case 2: RunGanttSimulation(orderAmount, arivalRate, estimatedThroughputTime);
                    return;
                default: return;
            }
            // using Default Test Values
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultCtx, 1);
            // update customized Items
            simConfig.ReplaceOption(new SimulationKind(value: simKind));
            simConfig.ReplaceOption(new TimeToAdvance(value: TimeSpan.FromMilliseconds(50)));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderAmount));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: estimatedThroughputTime));
            simConfig.ReplaceOption(new KpiTimeSpan(value: 60));
            simConfig.ReplaceOption(new Seed(value: 1337));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 20160));
            simConfig.ReplaceOption(new SaveToDB(value: false));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            // simConfig.ReplaceOption(new DebugSystem(true));
            // new DBConnectionString(value: "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true")
 
            await _agentSimulator.RunAkkaSimulation(configuration: simConfig);
        }


        public async void RunGanttSimulation(int orderAmount, double arivalRate, int estimatedThroughputTime)
        {
            var ganttPlanContext = GanttPlanDBContext.GetContext(GanttPlanCtxString);
            ganttPlanContext.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'DELETE FROM ? '");

            //Synchronisation GanttPlan
            GanttPlanOptRunner.RunOptAndExport("Init");

            var simContext = new GanttSimulation(ganttPlanContext, _context.Database.GetDbConnection().ConnectionString, messageHub: _messageHub);
            var simConfig = ArgumentConverter.ConfigurationConverter(resultCtx: _resultCtx, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(_resultCtx.Database.GetDbConnection().ConnectionString));
            simConfig.ReplaceOption(new KpiTimeSpan(480));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Central));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderAmount));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: estimatedThroughputTime));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 1920));
            simConfig.ReplaceOption(new Seed(value: 169));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 10080));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.2));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            //emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());

            var simWasReady = false;
            if (simulation.IsReady())
            {
                // set for Assert 
                simWasReady = true;
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
        /// TODO : Change from uri to json Body
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