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
            var orderDomainContext = _context.CustomerOrderParts.Include(navigationPropertyPath: o => o.Article).Include(navigationPropertyPath: o => o.CustomerOrder);
            return View(model: await orderDomainContext.ToListAsync());
        }

        // GET: OrderParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts
                .Include(navigationPropertyPath: o => o.Article)
                .Include(navigationPropertyPath: o => o.CustomerOrder)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return PartialView(viewName: @"..\OrderParts\Details", model: orderPart);
        }

        // GET: OrderParts/Create
        [HttpGet(template: "OrderParts/Create/{OrderId}")]
        public IActionResult Create(string orderId)
        {
            ViewData[index: "ArticleId"] = new SelectList(items: _context.GetSellableArticles, dataValueField: "Id", dataTextField: "Name");
            ViewData[index: "OrderId"] = new SelectList(items: _context.CustomerOrders, dataValueField: "Id", dataTextField: "Name", selectedValue: orderId);
            return PartialView();
        }

        // POST: OrderParts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(template: "OrderParts/Create/{OrderId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ArticleId,Quantity,IsPlanned,Id")] T_CustomerOrderPart orderPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: orderPart);
                await _context.SaveChangesAsync();
                var orders = await _context.GetAllOrders.ToListAsync();
                ViewData[index: "OrderId"] = orderPart.CustomerOrderId;
                return View(viewName: "../Orders/Index", model: orders);
            }
            ViewData[index: "ArticleId"] = new SelectList(items: _context.GetSellableArticles, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.ArticleId);
            ViewData[index: "OrderId"] = new SelectList(items: _context.CustomerOrders, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.CustomerOrderId);
            return PartialView(viewName: "Create", model: orderPart);
        }

        // GET: OrderParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }
            ViewData[index: "ArticleId"] = new SelectList(items: _context.GetSellableArticles, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.ArticleId);
            ViewData[index: "OrderId"] = new SelectList(items: _context.CustomerOrders, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.CustomerOrderId);
            return PartialView(viewName: "Edit", model: orderPart);
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
                    _context.Update(entity: orderPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderPartExists(id: orderPart.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                ViewData[index: "OrderId"] = orderPart.CustomerOrderId;
                return View(viewName: "../Orders/Index", model: await _context.GetAllOrders.ToListAsync());
            }
            ViewData[index: "ArticleId"] = new SelectList(items: _context.GetSellableArticles, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.ArticleId);
            ViewData[index: "OrderId"] = new SelectList(items: _context.CustomerOrders, dataValueField: "Id", dataTextField: "Name", selectedValue: orderPart.CustomerOrderId);
            return PartialView(viewName: "Details", model: orderPart);
        }

        // GET: OrderParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.CustomerOrderParts
                .Include(navigationPropertyPath: o => o.Article)
                .Include(navigationPropertyPath: o => o.CustomerOrder)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return PartialView(viewName: "Delete", model: orderPart);
        }

        // POST: OrderParts/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderPart = await _context.CustomerOrderParts.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.CustomerOrderParts.Remove(entity: orderPart);
            await _context.SaveChangesAsync();
            // return to Index
            ViewData[index: "OrderId"] = orderPart.CustomerOrderId;
            return View(viewName: "../Orders/Index", model: await _context.GetAllOrders.ToListAsync());
        }

        private bool OrderPartExists(int id)
        {
            return _context.CustomerOrderParts.Any(predicate: e => e.Id == id);
        }
    }
}
