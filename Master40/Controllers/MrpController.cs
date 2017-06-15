using System;
using System.Collections.Generic;
using Master40.BusinessLogic.MRP;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Master40.SignalR;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IProcessMrp _processMrp;
        private readonly MasterDBContext _context;
        private readonly IConnectionManager _connectionManager;
        public MrpController(IProcessMrp processMrp, MasterDBContext context, IConnectionManager connectionManager)
        {
            _processMrp = processMrp;
            _context = context;
            _connectionManager = connectionManager;
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
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.All);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpBackward")]
        public IActionResult MrpBackward()
        {
            //call to process MRP I and II
             var jobId = BackgroundJob.Enqueue(() => _processMrp.CreateAndProcessOrderDemand(MrpTask.Backward));


            _connectionManager.GetHubContext<ProcessingHub>()
                .Clients.All.clientListener("Forward has Finished.");


            return RedirectToAction("Index");
        }

        [HttpGet("[Controller]/MrpForward")]
        public async Task<IActionResult> MrpForward()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Forward);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpGifflerThompson")]
        public async Task<IActionResult> MrpGifflerThompson()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.GifflerThompson);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        [HttpGet("[Controller]/CapacityPlanning")]
        public async Task<IActionResult> CapacityPlanning()
        {
            //call to process MRP I and II
            //await _processMrp.CreateAndProcessOrderDemand(MrpTask.Capacity);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
