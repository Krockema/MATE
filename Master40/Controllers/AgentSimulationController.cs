using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.MessageSystem.SignalR;
using Microsoft.AspNetCore.Mvc;

namespace Master40.Controllers
{
    public class AgentSimulationController : Controller
    {
        private readonly AgentSimulator _agentSimulator;

        public AgentSimulationController(AgentSimulator agentSimulator)
        {
            _agentSimulator = agentSimulator;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Run()
        {
            
            await _agentSimulator.RunSimulation();
            return View("Index");
        }

        [HttpGet("[Controller]/RunAsync")]
        public void RunAsync()
        {
            _agentSimulator.RunSimulation();
        }
    }
}