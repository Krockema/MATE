using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.MessageSystem.SignalR;
using Master40.Simulation.Simulation;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IProcessMrp _processMrp;
        private readonly ISimulator _simulator;
        private readonly IMessageHub _messageHub;
        //private readonly Client _client;
        public MrpController(ISimulator simulator, IProcessMrp processMrp, IMessageHub messageHub)
        {
            _processMrp = processMrp;
            _simulator = simulator;
            _messageHub = messageHub;
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
            await _simulator.InitializeMrp(MrpTask.All,1);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/MrpProcessingAjax")]
        public void MrpProcessingAjax()
        {
            var jobId = 
            BackgroundJob.Enqueue<ISimulator>(x =>
                //_processMrp.CreateAndProcessOrderDemand(MrpTask.All)
                //_processMrp.CreateAndProcessOrderDemand(MrpTask.All, null,1, null)
                //_simulator.InitializeMrp(MrpTask.All, 1)
                _simulator.Simulate(1)
            );
            BackgroundJob.ContinueWith(jobId, 
                () => _messageHub.EndScheduler());

        }


        [HttpGet("[Controller]/MrpBackward")]
        public async Task<IActionResult> MrpBackward()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward);

            var jobId =
                BackgroundJob.Enqueue<ISimulator>(x =>
                            _simulator.InitializeMrp(MrpTask.Backward, 1)
                );
            BackgroundJob.ContinueWith(jobId,
                () => _messageHub.EndScheduler());

            return View("Index");
        }

        [HttpGet("[Controller]/MrpForward")]
        public async Task<IActionResult> MrpForward()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward);

            var jobId =
                BackgroundJob.Enqueue<ISimulator>(x =>
                        _simulator.InitializeMrp(MrpTask.Forward, 1)
                );
            BackgroundJob.ContinueWith(jobId,
                () => _messageHub.EndScheduler());

            return View("Index");
        }

        [HttpGet("[Controller]/MrpGifflerThompson")]
        public async Task<IActionResult> MrpGifflerThompson()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson);
            await _simulator.InitializeMrp(MrpTask.GifflerThompson,1);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/CapacityPlanning")]
        public async Task<IActionResult> CapacityPlanning()
        {
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity);
            await _simulator.InitializeMrp(MrpTask.Capacity,1);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/Simulate")]
        public async Task<IActionResult> Simulate()
        {
            await _simulator.Simulate(3);

            await Task.Yield();

            return View("Index");
        }

        [HttpGet("[Controller]/SimulateAjax")]
        public void SimulateAjax()
        {
            BackgroundJob.Enqueue<ISimulator>(x =>
                _simulator.Simulate(1));
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
