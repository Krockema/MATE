using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Controllers
{
    public class OrderPartsController : Controller
    {
        private readonly OrderDomainContext _context;

        public OrderPartsController(OrderDomainContext context)
        {
            _context = context;    
        }

        // GET: OrderParts
        public async Task<IActionResult> Index()
        {
            var orderDomainContext = _context.CustomerOrderParts.Include(o => o.Article).Include(o => o.CustomerOrder);
            return View(await orderDomainContext.ToListAsync());
        }

        // GET: OrderParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts
                .Include(o => o.Article)
                .Include(o => o.CustomerOrder)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return PartialView(@"..\OrderParts\Details", orderPart);
        }

        // GET: OrderParts/Create
        [HttpGet("OrderParts/Create/{OrderId}")]
        public IActionResult Create(string orderId)
        {
            ViewData["ArticleId"] = new SelectList(_context.GetSellableArticles, "Id", "Name");
            ViewData["OrderId"] = new SelectList(_context.CustomerOrders, "Id", "Name", orderId);
            return PartialView();
        }

        // POST: OrderParts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("OrderParts/Create/{OrderId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ArticleId,Quantity,IsPlanned,Id")] T_CustomerOrderPart orderPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderPart);
                await _context.SaveChangesAsync();
                var orders = await _context.GetAllOrders.ToListAsync();
                ViewData["OrderId"] = orderPart.CustomerOrderId;
                return View("../Orders/Index", orders);
            }
            ViewData["ArticleId"] = new SelectList(_context.GetSellableArticles, "Id", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.CustomerOrders, "Id", "Name", orderPart.CustomerOrderId);
            return PartialView("Create", orderPart);
        }

        // GET: OrderParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts.SingleOrDefaultAsync(m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.GetSellableArticles, "Id", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.CustomerOrders, "Id", "Name", orderPart.CustomerOrderId);
            return PartialView("Edit", orderPart);
        }

        // POST: OrderParts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ArticleId,Quantity,IsPlanned,Id")] T_CustomerOrderPart orderPart)
        {
            if (id != orderPart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderPartExists(orderPart.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                ViewData["OrderId"] = orderPart.CustomerOrderId;
                return View("../Orders/Index", await _context.GetAllOrders.ToListAsync());
            }
            ViewData["ArticleId"] = new SelectList(_context.GetSellableArticles, "Id", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.CustomerOrders, "Id", "Name", orderPart.CustomerOrderId);
            return PartialView("Details", orderPart);
        }

        // GET: OrderParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts
                .Include(o => o.Article)
                .Include(o => o.CustomerOrder)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return PartialView("Delete", orderPart);
        }

        // POST: OrderParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderPart = await _context.CustomerOrderParts.SingleOrDefaultAsync(m => m.Id == id);
            _context.CustomerOrderParts.Remove(orderPart);
            await _context.SaveChangesAsync();
            // return to Index
            ViewData["OrderId"] = orderPart.CustomerOrderId;
            return View("../Orders/Index", await _context.GetAllOrders.ToListAsync());
        }

        private bool OrderPartExists(int id)
        {
            return _context.CustomerOrderParts.Any(e => e.Id == id);
        }
    }
}
