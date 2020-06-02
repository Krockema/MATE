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

namespace Master40.Controllers
{
    public class AgentLiveController : Controller
    {
        private readonly AgentCore _agentSimulator;
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultCtx;

        public AgentLiveController(AgentCore agentSimulator, ProductionDomainContext context, ResultContext resultCtx)
        {
            _agentSimulator = agentSimulator;
            _resultCtx = resultCtx;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var resources = await _context.Resources.Where(x => x.IsPhysical)
                                              .Include(navigationPropertyPath: a => a.ResourceSetups).ToListAsync();

            ViewData[index: "machines"] = resources.Select(selector: x => x.Name).ToList();
            return View(model: resources);
        }

        [HttpGet(template: "[Controller]/RunAsync/{simulationType}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public async void RunAsync(int simulationType, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            var simKind = SimulationType.None;
            switch (simulationType)
            {
                case 1: simKind = SimulationType.None; break;
                case 2: simKind = SimulationType.DefaultSetup; break;
                case 3: simKind = SimulationType.DefaultSetupStack; break;
                case 4: simKind = SimulationType.BucketScope; break;
                default: return;
            }
            // using Default Test Values
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultCtx, 1);
            // update customized Items
            simConfig.ReplaceOption(new SimulationKind(value: simKind));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: orderAmount));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: estimatedThroughputTime));
            simConfig.ReplaceOption(new KpiTimeSpan(value: 480));
            simConfig.ReplaceOption(new Seed(value: 1337));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 20160));
            simConfig.ReplaceOption(new SaveToDB(value: false));
            // simConfig.ReplaceOption(new DebugSystem(true));
            // new DBConnectionString(value: "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true")
 
            await _agentSimulator.RunAkkaSimulation(configuration: simConfig);
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