using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Enums;
using Master40.MessageSystem.MessageReciever;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation;

namespace Master40.Controllers
{
    public class SimulationCompareController : Controller
    {
        [HttpGet("[Controller]/Index/{simId}/{simNumber}")]
        public IActionResult Index(int simId, int simNumber)
        {
            ViewData["simId"] = simId;
            ViewData["simNr"] = simNumber;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet("[Controller]/MachinesWorkLoad/{simulationId}/{simulationType}/{simNumber}/{overTime}")]
        public IActionResult MachineWorkLoads(string simulationId, string simulationType, string simNumber, string overTime)
        {
            //call to Diagramm
            return ViewComponent("MachinesWorkLoad", new List<string> { simulationId, simulationType, simNumber, overTime });
        }


        [HttpGet("[Controller]/Timeliness/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult OrderTimeliness(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("OrderTimeliness", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }

        [HttpGet("[Controller]/ProductLeadTime/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult ProductLeadTime(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("ProductLeadTime", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet("[Controller]/ProductLeadTimeBoxPlot/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult ProductLeadTimeBoxPlot(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("ProductLeadTimeBoxPlot", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet("[Controller]/StockEvolution/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult StockEvolution(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("StockEvolution", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet("[Controller]/IdlePeriod/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult IdlePeriod(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("IdlePeriod", new List<string> { simulationId, simulationType, simNumber });
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
