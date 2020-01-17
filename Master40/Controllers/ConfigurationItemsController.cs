using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.Simulation;
using Master40.Simulation.CLI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master40.Controllers
{
    public class ConfigurationItemsController : Controller
    {
        private readonly ResultContext _context;
        private readonly AgentCore _agentSimulator;

        public ConfigurationItemsController(AgentCore agentSimulator, ResultContext context)
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
            for (int i = 1; i < 10; i++)
            {
                BackgroundJob.Enqueue(() => _agentSimulator.BackgroundSimulation(id, i));    
            }
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
