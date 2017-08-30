using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IProcessMrp _processMrp;
        private readonly ISimulator _simulator;
        //private readonly Client _client;
        public MrpController(ISimulator simulator, IProcessMrp processMrp, IMessageHub messageHub)
        {
            _processMrp = processMrp;
            _simulator = simulator;
            //_client = client;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[Controller]/MrpProcessing")]
        public async Task<IActionResult> MrpProcessing()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.All);
            await _simulator.InitializeMrp(MrpTask.All);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/MrpProcessingAjax")]
        public void MrpProcessingAjax()
        {
            // var jobId = 
            BackgroundJob.Enqueue<IProcessMrp>(x =>
                //_processMrp.CreateAndProcessOrderDemand(MrpTask.All)
                _processMrp.CreateAndProcessOrderDemand(MrpTask.All)
            );
        }


        [HttpGet("[Controller]/MrpBackward")]
        public async Task<IActionResult> MrpBackward()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward);
            await _simulator.InitializeMrp(MrpTask.Backward);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/MrpForward")]
        public async Task<IActionResult> MrpForward()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward);
            await _simulator.InitializeMrp(MrpTask.Forward);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/MrpGifflerThompson")]
        public async Task<IActionResult> MrpGifflerThompson()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson);
            await _simulator.InitializeMrp(MrpTask.GifflerThompson);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/CapacityPlanning")]
        public async Task<IActionResult> CapacityPlanning()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity);
            await _simulator.InitializeMrp(MrpTask.Capacity);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/Simulate")]
        public async Task<IActionResult> Simulate()
        {
            await _simulator.Simulate();

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/SimulateAjax")]
        public void SimulateAjax()
        {
            BackgroundJob.Enqueue<ISimulator>(x =>
                _simulator.Simulate());
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
        [HttpGet("[Controller]/ReloadGantt/Data")]
        public IActionResult ReloadGanttData(int stateId)
        {
            //call to ReloadChart Diagramm
            return ViewComponent("ProductionSchedule");
        }
    }
}
