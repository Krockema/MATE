using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.Tools.SignalR;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IMessageHub _messageHub;
        //private readonly Client _client;
        public MrpController(IMessageHub messageHub)
        {
            _messageHub = messageHub;
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

        [HttpGet("[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent("ProductionTimeline", new List<int> { orderId, stateId });
        }
        
        [HttpGet("[Controller]/ReloadChart/{stateId}")]
        public IActionResult ReloadChart(int stateId)
        {
            //call to ReloadChart Diagramm
            return ViewComponent("MachineGroupCapacity", stateId);
        }
        [HttpGet("[Controller]/ReloadGantt/{orderId}/{simulationType}/{state}/{simulationConfigurationId}/{simNumber}/{simulationPage}")]
        public IActionResult ReloadGantt(string orderId, string simulationType, string state, string simulationConfigurationId, string simNumber, string simulationPage)
        {
            //call to ReloadChart Diagramm 
            //return ViewComponent("ProductionSchedule");
            return ViewComponent("SimulationTimeline", new List<string> {orderId, simulationType, state, simulationConfigurationId, simNumber, simulationPage });
        }
    }
}
