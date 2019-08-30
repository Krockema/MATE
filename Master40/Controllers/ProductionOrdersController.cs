using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class ProductionOrdersController : Controller
    {
        private readonly MasterDBContext _context;

        public ProductionOrdersController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: ProductionOrders
        public async Task<IActionResult> Index()
        {
            var masterDBContext = _context.ProductionOrders.Include(navigationPropertyPath: p => p.Article);
            return View(model: await masterDBContext.ToListAsync());
        }

        // GET: ProductionOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders
                .Include(navigationPropertyPath: p => p.Article)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (productionOrder == null)
            {
                return NotFound();
            }

            return View(model: productionOrder);
        }

        // GET: ProductionOrders/Create
        public IActionResult Create()
        {
            ViewData[index: "ArticleId"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "ArticleId");
            return View();
        }

        // POST: ProductionOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductionOrderId,ArticleId,Quantity,Name")] T_ProductionOrder productionOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: productionOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "ArticleId"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "ArticleId", selectedValue: productionOrder.ArticleId);
            return View(model: productionOrder);
        }

        // GET: ProductionOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (productionOrder == null)
            {
                return NotFound();
            }
            ViewData[index: "ArticleId"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "ArticleId", selectedValue: productionOrder.ArticleId);
            return View(model: productionOrder);
        }

        // POST: ProductionOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductionOrderId,ArticleId,Quantity,Name")] T_ProductionOrder productionOrder)
        {
            if (id != productionOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: productionOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductionOrderExists(id: productionOrder.Id))
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
            ViewData[index: "ArticleId"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "ArticleId", selectedValue: productionOrder.ArticleId);
            return View(model: productionOrder);
        }

        // GET: ProductionOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders
                .Include(navigationPropertyPath: p => p.Article)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (productionOrder == null)
            {
                return NotFound();
            }

            return View(model: productionOrder);
        }

        // POST: ProductionOrders/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productionOrder = await _context.ProductionOrders.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.ProductionOrders.Remove(entity: productionOrder);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool ProductionOrderExists(int id)
        {
            return _context.ProductionOrders.Any(predicate: e => e.Id == id);
        }
    }
}
