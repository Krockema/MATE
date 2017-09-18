using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.MessageSystem.MessageReciever;
using Master40.Simulation.Simulation;

namespace Master40.Controllers
{
    public class SimulationCompareController : Controller
    {
        [HttpGet("[Controller]/Index/{simId}")]
        public IActionResult Index(int simId)
        {
            ViewData["simId"] = simId;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet("[Controller]/MachinesWorkLoad/{simulationId}/{simulationType}")]
        public IActionResult MachineWorkLoads(string simulationId, string simulationType)
        {
            //call to Diagramm
            return ViewComponent("MachinesWorkLoad", new List<string> { simulationId, simulationType });
        }


        [HttpGet("[Controller]/Timeliness/{simulationId}/{simulationType}")]
        public IActionResult OrderTimeliness(string simulationId, string simulationType)
        {
            //call to Diagramm
            var vc = ViewComponent("OrderTimeliness", new List<string> { simulationId, simulationType });
            return vc;
        }

        [HttpGet("[Controller]/ProductLeadTime/{simulationId}/{simulationType}")]
        public IActionResult ProductLeadTime(string simulationId, string simulationType)
        {
            //call to Diagramm
            var vc = ViewComponent("ProductLeadTime", new List<string> { simulationId, simulationType });
            return vc;
        }
        [HttpGet("[Controller]/StockEvolution/{simulationId}/{simulationType}")]
        public IActionResult StockEvolution(string simulationId, string simulationType)
        {
            //call to Diagramm
            var vc = ViewComponent("StockEvolution", new List<string> { simulationId, simulationType });
            return vc;
        }
        [HttpGet("[Controller]/SimulationTimeline/{orderId}/{simulationType}/{state}/{simulationConfigurationId}/{simNumber}")]
        public IActionResult SimulationTimeline(string orderId, string simulationType, string state, string simulationConfigurationId, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("SimulationTimeline", new List<string> { orderId, simulationType, state, simulationConfigurationId, simNumber });
            return vc;
        }
    }
}
