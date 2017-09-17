using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.Models;

namespace Master40.Controllers
{
    public class SimulationConfigurationsController : Controller
    {
        private readonly MasterDBContext _context;

        public SimulationConfigurationsController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: SimulationConfigurations
        public async Task<IActionResult> Index()
        {
            return View(await _context.SimulationConfigurations.ToListAsync());
        }

        // GET: SimulationConfigurations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulationConfiguration = await _context.SimulationConfigurations
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
        public async Task<IActionResult> Create([Bind("SimulationId,Name,Time,MaxCalculationTime,Lotsize,OrderQuantity,TimeSpanForOrders,Seed,Id")] SimulationConfiguration simulationConfiguration)
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

            var simulationConfiguration = await _context.SimulationConfigurations.SingleOrDefaultAsync(m => m.Id == id);
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
        public async Task<IActionResult> Edit(int id, [Bind("SimulationId,Name,Time,MaxCalculationTime,Lotsize,OrderQuantity,TimeSpanForOrders,Seed,Id")] SimulationConfiguration simulationConfiguration)
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

            var simulationConfiguration = await _context.SimulationConfigurations
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
            var simulationConfiguration = await _context.SimulationConfigurations.SingleOrDefaultAsync(m => m.Id == id);
            _context.SimulationConfigurations.Remove(simulationConfiguration);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool SimulationConfigurationExists(int id)
        {
            return _context.SimulationConfigurations.Any(e => e.Id == id);
        }
    }
}
