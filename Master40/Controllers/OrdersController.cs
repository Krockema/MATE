using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Master40.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderDomainContext _context;

        public OrdersController(OrderDomainContext context)
        {
            _context = context;    
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orderDomainContext = _context.GetAllOrders;
            return View(await orderDomainContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders
                .Include(o => o.BusinessPartner)
                .Include(op => op.OrderParts)
                    .ThenInclude(a => a.Article)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return PartialView("Details", order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["BusinessPartnerId"] = new SelectList(_context.BusinessPartners, "Id", "Name");
            return PartialView();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,DueTime,BusinessPartnerId,Id")] T_CustomerOrder order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                var orders = await _context.GetAllOrders.ToListAsync();
                ViewData["OrderId"] = order.Id;
                return View("Index", orders);
            }
            ViewData["BusinessPartnerId"] = new SelectList(_context.BusinessPartners, "Id", "Name", order.BusinessPartnerId);
            return PartialView("Create", order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders.SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["BusinessPartnerId"] = new SelectList(_context.BusinessPartners, "Id", "Name", order.BusinessPartnerId);
            return PartialView("Edit", order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,DueTime,BusinessPartnerId,Id")] T_CustomerOrder order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["BusinessPartnerId"] = new SelectList(_context.BusinessPartners, "Id", "Name", order.BusinessPartnerId);
            return PartialView("Details", order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders
                .SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return PartialView("Delete", order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.ById(id).SingleAsync();
            _context.CustomerOrders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool OrderExists(int id)
        {
            return _context.CustomerOrders.Any(e => e.Id == id);
        }
    }
}
