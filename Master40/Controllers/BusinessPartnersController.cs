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
            return View(await _context.BusinessPartners.ToListAsync());
        }

        // GET: BusinessPartners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners
                .SingleOrDefaultAsync(m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }

            return View(businessPartner);
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
        public async Task<IActionResult> Create([Bind("BusinessPartnerId,Name,Debitor,Kreditor")] BusinessPartner businessPartner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(businessPartner);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(businessPartner);
        }

        // GET: BusinessPartners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners.SingleOrDefaultAsync(m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }
            return View(businessPartner);
        }

        // POST: BusinessPartners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusinessPartnerId,Name,Debitor,Kreditor")] BusinessPartner businessPartner)
        {
            if (id != businessPartner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(businessPartner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessPartnerExists(businessPartner.Id))
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
            return View(businessPartner);
        }

        // GET: BusinessPartners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessPartner = await _context.BusinessPartners
                .SingleOrDefaultAsync(m => m.Id == id);
            if (businessPartner == null)
            {
                return NotFound();
            }

            return View(businessPartner);
        }

        // POST: BusinessPartners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessPartner = await _context.BusinessPartners.SingleOrDefaultAsync(m => m.Id == id);
            _context.BusinessPartners.Remove(businessPartner);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BusinessPartnerExists(int id)
        {
            return _context.BusinessPartners.Any(e => e.Id == id);
        }
    }
}
