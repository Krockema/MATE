using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Mate.DataCore.Data.Context;
using Mate.DataCore.ReportingModel;
using Mate.Production.CLI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mate.Controllers
{
    public class ConfigurationItemsController : Controller
    {
        private readonly MateResultDb _context;
        private readonly AgentCore _agentSimulator;

        public ConfigurationItemsController(AgentCore agentSimulator, MateResultDb context)
        {
            _context = context;
            _agentSimulator = agentSimulator;
        }

        // GET: ConfigurationItems
        public async Task<IActionResult> Index()
        {
            return View(await _context.ConfigurationItems.Where(x => x.Property == "SimulationId").ToListAsync());
        }

        public void Start(int id)
        {
            for (int i = 1; i <= 10; i++)
            {
                BackgroundJob.Enqueue(() => _agentSimulator.BackgroundSimulation(id, i, null));    
            }
        }
       
        [HttpGet(template: "[Controller]/Start/{simulationId}/iterateFrom/{iterateFrom}/iterateTo/{iterateTo}")]
        public void Start(int simulationId, int iterateFrom, int iterateTo, bool aggregateResults)
        {
            // Create Initial Job
            //string job =  BackgroundJob.Enqueue(() => _agentSimulator.BackgroundSimulation(simulationId, iterateFrom));    
            string job  = "";
            // Continue Jobs 
            for (int i = iterateFrom; i <= iterateTo; i++)
                job = BackgroundJob.Enqueue( () => _agentSimulator.BackgroundSimulation(simulationId, i, null));    
        }

        [HttpGet(template: "[Controller]/AggregateResults/{simulationId}")]
        public void AggregateResults(int simulationId)
        {
            BackgroundJob.Enqueue(() => _agentSimulator.AggregateResults(simulationId, null)); 
        }


        public IActionResult ChartStatusComponent()
        {
            return ViewComponent($"JobInformation");
        }

        // GET: ConfigurationItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationRelations.Include(x => x.ChildItem)
                .Where(m => m.ParentItemId == id).Select(x => x.ChildItem).ToListAsync();
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // GET: ConfigurationItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ConfigurationItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Property,PropertyValue,Description,Id")] ConfigurationItem configurationItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(configurationItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(configurationItem);
        }

        // GET: ConfigurationItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationItems.FindAsync(id);
            if (configurationItem == null)
            {
                return NotFound();
            }
            return View(configurationItem);
        }

        // POST: ConfigurationItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Property,PropertyValue,Description,Id")] ConfigurationItem configurationItem)
        {
            if (id != configurationItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(configurationItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfigurationItemExists(configurationItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(configurationItem);
        }

        // GET: ConfigurationItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configurationItem = await _context.ConfigurationItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (configurationItem == null)
            {
                return NotFound();
            }

            return View(configurationItem);
        }

        // POST: ConfigurationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configurationItem = await _context.ConfigurationItems.FindAsync(id);
            _context.ConfigurationItems.Remove(configurationItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConfigurationItemExists(int id)
        {
            return _context.ConfigurationItems.Any(e => e.Id == id);
        }
    }
}
