using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.Controllers
{
    public class SimulationConfigurationsController : Controller
    {
        private readonly MasterDBContext _context;
        private readonly ResultContext _resultContext;

        public SimulationConfigurationsController(MasterDBContext context, ResultContext resultContext)
        {
            _context = context;
            _resultContext = resultContext;
        }

        // GET: SimulationConfigurations
        public async Task<IActionResult> Index()
        {
            return View(await _resultContext.SimulationConfigurations.ToListAsync());
        }

        // GET: SimulationConfigurations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulationConfiguration = await _resultContext.SimulationConfigurations
                .SingleOrDefaultAsync(m => m.Id == id);

            if (simulationConfiguration == null)
            {
                return NotFound();
            }

            return PartialView("Details", simulationConfiguration);
        }

        // GET: SimulationConfigurations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SimulationConfigurations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SimulationId,Name,Time,MaxCalculationTime,Lotsize,OrderQuantity,TimeSpanForOrders,Seed,ConsecutiveRuns,DynamicKpiTimeSpan,WorkTimeDeviation,SettlingStart,SimulationEndTime,RecalculationTime,OrderRate,Id")] SimulationConfiguration simulationConfiguration)
        {
            if (ModelState.IsValid)
            {
                _context.Add(simulationConfiguration);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return PartialView("Create", simulationConfiguration);
        }

        // GET: SimulationConfigurations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulationConfiguration = await _resultContext.SimulationConfigurations.SingleOrDefaultAsync(m => m.Id == id);
            if (simulationConfiguration == null)
            {
                return NotFound();
            }
            return PartialView("Edit", simulationConfiguration);
        }

        // POST: SimulationConfigurations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SimulationId,Name,Time,MaxCalculationTime,Lotsize,OrderQuantity,TimeSpanForOrders,Seed,ConsecutiveRuns,DynamicKpiTimeSpan,WorkTimeDeviation,SettlingStart,SimulationEndTime,RecalculationTime,OrderRate,Id")] SimulationConfiguration simulationConfiguration)
        {
            if (id != simulationConfiguration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(simulationConfiguration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SimulationConfigurationExists(simulationConfiguration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return PartialView("Details", simulationConfiguration);
        }

        // GET: SimulationConfigurations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulationConfiguration = await _resultContext.SimulationConfigurations
                .SingleOrDefaultAsync(m => m.Id == id);
            if (simulationConfiguration == null)
            {
                return NotFound();
            }

            return PartialView("Delete", simulationConfiguration);
        }

        // POST: SimulationConfigurations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var simulationConfiguration = await _resultContext.SimulationConfigurations.SingleOrDefaultAsync(m => m.Id == id);
            _resultContext.SimulationConfigurations.Remove(simulationConfiguration);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool SimulationConfigurationExists(int id)
        {
            return _resultContext.SimulationConfigurations.Any(e => e.Id == id);
        }

        [HttpGet("[Controller]/Central/{simulationId}")]
        public void Central(int simulationId)
        {
            var runs = _resultContext.SimulationConfigurations.Single(x => x.Id == simulationId).ConsecutiveRuns;
            string run = "";
            //  for (int i = 0; i < runs; i++)
            //  {
                 // if (run == "")
                 // { // initial Run.
                 //     run = BackgroundJob.Enqueue<ISimulator>(
                 //         x => _simulator.Simulate(simulationId));
                 // } // consecutive Runs 
                 // else
                 // {
                 //     run = BackgroundJob.ContinueWith<ISimulator>(run,
                 //         x => _simulator.Simulate(simulationId));
                 // }
            // }
        }


        [HttpGet("[Controller]/Decentral/{simulationId}")]
        public void Decentral(int simulationId)
        {
            var runs = _resultContext.SimulationConfigurations.Single(x => x.Id == simulationId).ConsecutiveRuns;
            string run = "";
            // for (int i = 0; i < runs; i++)
            // {
            //     if (run == "")
            //     { // initial Run.
            //         run = BackgroundJob.Enqueue<ISimulator>(
            //               x => _simulator.AgentSimulatioAsync(simulationId));
            //     } // consecutive Runs 
            //     else
            //     {
            //        run =  BackgroundJob.ContinueWith<ISimulator>(run ,
            //               x => _simulator.AgentSimulatioAsync(simulationId));
            //     }
            // }
        }

        [HttpGet("[Controller]/ConsolidateRuns/{simId1}/{simType1}")]
        public async Task<IActionResult> ConsolidateRuns(int simId1, string simType1)
        {
            return ViewComponent("MergedMachineWorkload", new List<string> {simId1.ToString(),simType1 });
        }

        [HttpGet("[Controller]/ConsolidateRuns/{simId1}/{simType1}/{simId2}/{simType2}/{simId3}/{simType3}/{simId4}/{simType4}")]
        public async Task<IActionResult> ConsolidateRuns(int simId1, string simType1, int simId2, string simType2, int simId3, string simType3, int simId4, string simType4)
        {
            return ViewComponent("MergedMachineWorkload", new List<string> { simId1.ToString(), simType1, simId2.ToString(), simType2, simId3.ToString(), simType3, simId4.ToString(), simType4 });
        }

        [HttpGet("[Controller]/ConsolidateRuns/{simId1}/{simType1}/{simId2}/{simType2}/{simId3}/{simType3}")]
        public async Task<IActionResult> ConsolidateRuns(int simId1, string simType1, int simId2, string simType2, int simId3, string simType3)
        {
            return ViewComponent("MergedMachineWorkload", new List<string> { simId1.ToString(), simType1, simId2.ToString(), simType2, simId3.ToString(), simType3 });
        }

        [HttpGet("[Controller]/ConsolidateRuns/{simId1}/{simType1}/{simId2}/{simType2}")]
        public async Task<IActionResult> ConsolidateRuns(int simId1, string simType1, int simId2, string simType2)
        {
            return ViewComponent("MergedMachineWorkload", new List<string> { simId1.ToString(), simType1, simId2.ToString(), simType2 });
        }

        [HttpGet("[Controller]/ConsolidateLeadTimes/{simId1}/{simType1}")]
        public async Task<IActionResult> ConsolidateLeadTimes(int simId1, string simType1)
        {
            return ViewComponent("ProductLeadTimeBoxPlot", new List<string> { simId1.ToString(), simType1});
        }
        [HttpGet("[Controller]/ConsolidateLeadTimes/{simId1}/{simType1}/{simId2}/{simType2}/{simId3}/{simType3}")]
        public async Task<IActionResult> ConsolidateLeadTimes(int simId1, string simType1, int simId2, string simType2, int simId3, string simType3)
        {
            return ViewComponent("ProductLeadTimeBoxPlot", new List<string> { simId1.ToString(), simType1 , simId2.ToString(), simType2, simId3.ToString(), simType3});
        }
        [HttpGet("[Controller]/ConsolidateLeadTimes/{simId1}/{simType1}/{simId2}/{simType2}")]
        public async Task<IActionResult> ConsolidateLeadTimes(int simId1, string simType1, int simId2, string simType2)
        {
            return ViewComponent("ProductLeadTimeBoxPlot", new List<string> { simId1.ToString(), simType1, simId2.ToString(), simType2 });
        }
        [HttpGet("[Controller]/ConsolidateLeadTimes/{simId1}/{simType1}/{simId2}/{simType2}/{simId3}/{simType3}/{simId4}/{simType4}")]
        public async Task<IActionResult> ConsolidateLeadTimes(int simId1, string simType1, int simId2, string simType2, int simId3, string simType3, int simId4, string simType4)
        {
            return ViewComponent("ProductLeadTimeBoxPlot", new List<string> { simId1.ToString(), simType1, simId2.ToString(), simType2, simId3.ToString(), simType3, simId4.ToString(), simType4 });
        }

    }
}
