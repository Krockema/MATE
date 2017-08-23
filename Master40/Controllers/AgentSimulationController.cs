using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace Master40.Controllers
{
    public class AgentSimulationController : Controller
    {
        private readonly AgentSimulator _agentSimulator;
        private readonly ProductionDomainContext _context;

        public AgentSimulationController(AgentSimulator agentSimulator, ProductionDomainContext context)
        {
            _agentSimulator = agentSimulator;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["machines"] = _context.Machines.Select(x => x.Name).ToList();
            return View();
        }

        public async Task<IActionResult> Run()
        {
            
            await _agentSimulator.RunSimulation(1);
            return View("Index");
        }

        [HttpGet("[Controller]/RunAsync")]
        public async void RunAsync()
        {
            // using Default Test Values.
            await _agentSimulator.RunSimulation(1);
        }

        [HttpGet("[Controller]/ReloadGantt/{orderId}/{stateId}")]
        public IActionResult ReloadGantt(int orderId, int stateId)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent("SimulationTimeline", new List<int> { orderId, stateId });
        }

        [HttpGet("[Controller]/MachineWorkload/{Machine}")]
        public IActionResult MachineWorkload(string machine)
        {
            //call to ReloadGantt Diagramm
            return ViewComponent("MachineWorkload", new { machine });
        }
    }
}