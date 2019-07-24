using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Simulation;
using Master40.SimulationCore.Environment.Options;
using System.Configuration;
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
            ViewData["machines"] = _context.Machines.Select(x => x.Name).ToList();
            var masterDBContext = _context.Machines.Include(a => a.MachineGroup);
            return View(await masterDBContext.ToListAsync());
        }

        [HttpGet("[Controller]/RunAsync/{simId}/orderAmount/{orderAmount}/arivalRate/{arivalRate}/estimatedThroughputTime/{estimatedThroughputTime}")]
        public async void RunAsync(int simId, int orderAmount, double arivalRate,int estimatedThroughputTime)
        {
            if (simId == 0) return;
            // using Default Test Values.
            var simConfig = SimulationCore.Environment.Configuration.Create(new object[]
                                                {
                                                    new DBConnectionString("")
                                                    , new SimulationId(simId)
                                                    , new SimulationNumber(1)
                                                    , new SimulationKind(SimulationType.Decentral)
                                                    , new OrderArrivalRate(arivalRate)
                                                    , new OrderQuantity(orderAmount)
                                                    , new EstimatedThroughPut(estimatedThroughputTime)
                                                    , new DebugAgents(false)
                                                    , new DebugSystem(false)
                                                    , new KpiTimeSpan(480)
                                                    , new MinDeliveryTime(1160)
                                                    , new MaxDeliveryTime(1600)
                                                    , new Seed(1337)
                                                    , new SettlingStart(2880)
                                                    , new SimulationEnd(40320)
                                                    , new WorkTimeDeviation(0.2)
                                                    , new SaveToDB(false)
                                                });
            await _agentSimulator.RunAkkaSimulation(simConfig);
        }

        [HttpGet("[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent("SimulationTimeline", new List<int> { orderId, stateId });
        }

        [HttpGet("[Controller]/MachineWorkload/{Machine}")]
        public IActionResult MachineWorkload(string machine)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent("MachineWorkload", new { machine });
        }

        [HttpGet("[Controller]/MachineBreakdown/{Machine}")]
        public void MachineBreakdown(string machine)
        {
            _agentSimulator.ResourceBreakDown(machine);
        }
    }
}