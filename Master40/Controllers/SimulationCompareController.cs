using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.MessageSystem.MessageReciever;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation;

namespace Master40.Controllers
{
    public class SimulationCompareController : Controller
    {
        private readonly MasterDBContext _context;

        public SimulationCompareController(MasterDBContext context)
        {
            _context = context;
        }


        [HttpGet("[Controller]/Index/{simId}/{simNumber}")]
        public IActionResult Index(int simId, int simNumber)
        {
            ViewData["simId"] = simId;
            ViewData["simNr"] = simNumber;
            ViewData["simName"] = _context.SimulationConfigurations.Single(x => x.Id == simId).Name.ToString();
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
        [HttpGet("[Controller]/OrderEvolution/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult OrderEvolution(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("OrderEvolution", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet("[Controller]/IdlePeriod/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult IdlePeriod(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("IdlePeriod", new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet("[Controller]/SimulationTimeline/{orderId}/{state}/{simulationPage}/{simulationType}/{simulationConfigurationId}/{simNumber}")]
        public IActionResult SimulationTimeline(string orderId, string state, string simulationPage, string simulationType, string simulationConfigurationId, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent("SimulationTimeline", new List<string> { orderId, state, simulationPage, simulationType, simulationConfigurationId, simNumber });
            return vc;
        }

    }
}
