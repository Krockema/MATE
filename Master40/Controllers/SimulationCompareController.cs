using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Master40.Controllers
{
    public class SimulationCompareController : Controller
    {
        private readonly ResultContext _context;

        public SimulationCompareController(ResultContext context)
        {
            _context = context;
        }


        [HttpGet(template: "[Controller]/Index/{simId}/{simNumber}")]
        public IActionResult Index(int simId, int simNumber)
        {
            ViewData[index: "simId"] = simId;
            ViewData[index: "simNr"] = simNumber;
            ViewData[index: "simName"] = _context.SimulationConfigurations.Single(predicate: x => x.Id == simId).Name.ToString();
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet(template: "[Controller]/MachinesWorkLoad/{simulationId}/{simulationType}/{simNumber}/{overTime}")]
        public IActionResult MachineWorkLoads(string simulationId, string simulationType, string simNumber, string overTime)
        {
            //call to Diagramm
            return ViewComponent(componentName: "MachinesWorkLoad", arguments: new List<string> { simulationId, simulationType, simNumber, overTime });
        }


        [HttpGet(template: "[Controller]/Timeliness/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult OrderTimeliness(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "OrderTimeliness", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }

        [HttpGet(template: "[Controller]/ProductLeadTime/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult ProductLeadTime(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "ProductLeadTime", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet(template: "[Controller]/ProductLeadTimeBoxPlot/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult ProductLeadTimeBoxPlot(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "ProductLeadTimeBoxPlot", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet(template: "[Controller]/StockEvolution/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult StockEvolution(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "StockEvolution", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet(template: "[Controller]/OrderEvolution/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult OrderEvolution(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "OrderEvolution", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet(template: "[Controller]/IdlePeriod/{simulationId}/{simulationType}/{simNumber}")]
        public IActionResult IdlePeriod(string simulationId, string simulationType, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "IdlePeriod", arguments: new List<string> { simulationId, simulationType, simNumber });
            return vc;
        }
        [HttpGet(template: "[Controller]/SimulationTimeline/{orderId}/{state}/{simulationPage}/{simulationType}/{simulationConfigurationId}/{simNumber}")]
        public IActionResult SimulationTimeline(string orderId, string state, string simulationPage, string simulationType, string simulationConfigurationId, string simNumber)
        {
            //call to Diagramm
            var vc = ViewComponent(componentName: "SimulationTimeline", arguments: new List<string> { orderId, state, simulationPage, simulationType, simulationConfigurationId, simNumber });
            return vc;
        }

    }
}
