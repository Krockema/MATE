using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Simulation;
using Master40.SimulationCore.Environment.Options;
using Master40.DB.Enums;

namespace Master40.Controllers
{
    public class AgentLiveController : Controller
    {
        private readonly AgentCore _agentSimulator;
        private readonly ProductionDomainContext _context;

        public AgentLiveController(AgentCore agentSimulator, ProductionDomainContext context)
        {
            _agentSimulator = agentSimulator;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewData[index: "machines"] = _context.Resources.Select(selector: x => x.Name).ToList();
            var masterDBContext = _context.Resources.Include(navigationPropertyPath: a => a.ResourceSkills);
            return View(model: await masterDBContext.ToListAsync());
        }

        [HttpGet(template: "[Controller]/RunAsync/{simId}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public async void RunAsync(int simId, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            if (simId == 0) return;
            // using Default Test Values.
            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    new DBConnectionString(value: "")
                                                    , new SimulationId(value: simId)
                                                    , new SimulationNumber(value: 1)
                                                    , new SimulationKind(value: SimulationType.Decentral)
                                                    , new OrderArrivalRate(value: arivalRate)
                                                    , new OrderQuantity(value: orderAmount)
                                                    , new EstimatedThroughPut(value: estimatedThroughputTime)
                                                    , new DebugAgents(value: false)
                                                    , new DebugSystem(value: false)
                                                    , new KpiTimeSpan(value: 480)
                                                    , new MinDeliveryTime(value: 1160)
                                                    , new MaxDeliveryTime(value: 1600)
                                                    , new TransitionFactor(value: 3)
                                                    , new TimePeriodForThrougputCalculation(value: 1920)
                                                    , new Seed(value: 1337)
                                                    , new SettlingStart(value: 2880)
                                                    , new SimulationEnd(value: 40320)
                                                    , new WorkTimeDeviation(value: 0.2)
                                                    , new SaveToDB(value: false)
                                                });
            await _agentSimulator.RunAkkaSimulation(configuration: simConfig);
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