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
            return View(model: await orderDomainContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders
                .Include(navigationPropertyPath: o => o.BusinessPartner)
                .Include(navigationPropertyPath: op => op.CustomerOrderParts)
                    .ThenInclude(navigationPropertyPath: a => a.Article)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return PartialView(viewName: "Details", model: order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData[index: "BusinessPartnerId"] = new SelectList(items: _context.BusinessPartners, dataValueField: "Id", dataTextField: "Name");
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
                _context.Add(entity: order);
                await _context.SaveChangesAsync();
                var orders = await _context.GetAllOrders.ToListAsync();
                ViewData[index: "OrderId"] = order.Id;
                return View(viewName: "Index", model: orders);
            }
            ViewData[index: "BusinessPartnerId"] = new SelectList(items: _context.BusinessPartners, dataValueField: "Id", dataTextField: "Name", selectedValue: order.BusinessPartnerId);
            return PartialView(viewName: "Create", model: order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData[index: "BusinessPartnerId"] = new SelectList(items: _context.BusinessPartners, dataValueField: "Id", dataTextField: "Name", selectedValue: order.BusinessPartnerId);
            return PartialView(viewName: "Edit", model: order);
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
                    _context.Update(entity: order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id: order.Id))
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
            ViewData[index: "BusinessPartnerId"] = new SelectList(items: _context.BusinessPartners, dataValueField: "Id", dataTextField: "Name", selectedValue: order.BusinessPartnerId);
            return PartialView(viewName: "Details", model: order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.CustomerOrders
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return PartialView(viewName: "Delete", model: order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.ById(id: id).SingleAsync();
            _context.CustomerOrders.Remove(entity: order);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool OrderExists(int id)
        {
            return _context.CustomerOrders.Any(predicate: e => e.Id == id);
        }
    }
}
