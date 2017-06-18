using System.Collections.Generic;
using Master40.BusinessLogic.MRP;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Master40.BusinessLogic.Simulation;
using Master40.DB.Data.Context;
using Master40.DB.Data.Repository;
using Master40.DB.Models;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IProcessMrp _processMrp;
        private readonly ISimulator _simulator;        
        public MrpController(IProcessMrp processMrp, ISimulator simulator)
        {
            _processMrp = processMrp;
            _simulator = simulator;
        }
        public IActionResult Index()
        {
            if (_processMrp.Logger == null)
            {
                _processMrp.Logger = new List<LogMessage>() {
                    new LogMessage() { MessageType = MessageType.success, Message = "Nothing logged yet.", MessageNumber = 1 }
                };
            }


            return View(_processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpProcessing")]
        public async Task<IActionResult> MrpProcessing()
        {
            //call to process MRP I and II
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.All);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpBackward")]
        public async Task<IActionResult> MrpBackward()
        {
            //call to process MRP I and II
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpForward")]
        public async Task<IActionResult> MrpForward()
        {
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpGifflerThompson")]
        public async Task<IActionResult> MrpGifflerThompson()
        {
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/CapacityPlanning")]
        public async Task<IActionResult> CapacityPlanning()
        {
            await _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/Simulate")]
        public async Task<IActionResult> Simulate()
        {
            await _simulator.Simulate();

            await Task.Yield();

            return View("Index", _processMrp.Logger);
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
    }
}
