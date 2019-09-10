using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class BusinessPartnersController : Controller
    {
        private readonly MasterDBContext _context;

        public BusinessPartnersController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: BusinessPartners
        public async Task<IActionResult> Index()
        {
            return View(model: await _context.BusinessPartners.ToListAsync());
        }

        // GET: BusinessPartners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }

            return View(model: businessPartner);
        }

        // GET: BusinessPartners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusinessPartners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessPartnerId,Name,Debitor,Kreditor")] M_BusinessPartner businessPartner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: businessPartner);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            return View(model: businessPartner);
        }

        // GET: BusinessPartners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }
            return View(model: businessPartner);
        }

        // POST: BusinessPartners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusinessPartnerId,Name,Debitor,Kreditor")] M_BusinessPartner businessPartner)
        {
            if (id != businessPartner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: businessPartner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessPartnerExists(id: businessPartner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(actionName: "Index");
            }
            return View(model: businessPartner);
        }

        // GET: BusinessPartners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }

            return View(model: businessPartner);
        }

        // POST: BusinessPartners/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessPartner = await _context.BusinessPartners.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.BusinessPartners.Remove(entity: businessPartner);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool BusinessPartnerExists(int id)
        {
            return _context.BusinessPartners.Any(predicate: e => e.Id == id);
        }
    }
}
