using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        //private readonly Client _client;
        public MrpController()
        {
            //_client = client;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet(template: "[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent(componentName: "ProductionTimeline", arguments: new List<int> { orderId, stateId });
        }
        
        [HttpGet(template: "[Controller]/ReloadChart/{stateId}")]
        public IActionResult ReloadChart(int stateId)
        {
            //call to ReloadChart Diagramm
            return ViewComponent(componentName: "MachineGroupCapacity", arguments: stateId);
        }
        [HttpGet(template: "[Controller]/ReloadGantt/{orderId}/{simulationType}/{state}/{simulationConfigurationId}/{simNumber}/{simulationPage}")]
        public IActionResult ReloadGantt(string orderId, string simulationType, string state, string simulationConfigurationId, string simNumber, string simulationPage)
        {
            //call to ReloadChart Diagramm 
            //return ViewComponent("ProductionSchedule");
            return ViewComponent(componentName: "SimulationTimeline", arguments: new List<string> {orderId, simulationType, state, simulationConfigurationId, simNumber, simulationPage });
        }
    }
}
