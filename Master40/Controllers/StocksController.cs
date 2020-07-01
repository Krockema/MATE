using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class StocksController : Controller
    {
        private readonly MasterDBContext _context;

        public StocksController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: Stocks
        public async Task<IActionResult> Index()
        {
            var masterDBContext = _context.Stocks.Include(navigationPropertyPath: s => s.Article);
            return View(model: await masterDBContext.ToListAsync());
        }

        // GET: Stocks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .Include(navigationPropertyPath: s => s.Article)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(model: stock);
        }

        // GET: Stocks/Create
        public IActionResult Create()
        {
            ViewData[index: "ArticleForeignKey"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "Name");
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockId,Name,Max,Min,Current,ArticleForeignKey")] M_Stock stock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: stock);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "ArticleForeignKey"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "Name", selectedValue: stock.ArticleForeignKey);
            return View(model: stock);
        }

        // GET: Stocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (stock == null)
            {
                return NotFound();
            }
            ViewData[index: "ArticleForeignKey"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "Name", selectedValue: stock.ArticleForeignKey);
            return View(model: stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockId,Name,Max,Min,Current,ArticleForeignKey")] M_Stock stock)
        {
            if (id != stock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: stock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockExists(id: stock.Id))
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
            ViewData[index: "ArticleForeignKey"] = new SelectList(items: _context.Articles, dataValueField: "ArticleId", dataTextField: "Name", selectedValue: stock.ArticleForeignKey);
            return View(model: stock);
        }

        // GET: Stocks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .Include(navigationPropertyPath: s => s.Article)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(model: stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stock = await _context.Stocks.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.Stocks.Remove(entity: stock);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool StockExists(int id)
        {
            return _context.Stocks.Any(predicate: e => e.Id == id);
        }
    }
}
