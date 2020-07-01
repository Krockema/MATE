using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly MasterDBContext _context;

        public ArticlesController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var masterDBContext = _context.Articles.Include(navigationPropertyPath: a => a.ArticleType).Include(navigationPropertyPath: a => a.Unit);
            return View(model: await masterDBContext.ToListAsync());
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(navigationPropertyPath: a => a.ArticleType)
                .Include(navigationPropertyPath: a => a.Unit)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(model: article);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewData[index: "ArticleTypeId"] = new SelectList(items: _context.ArticleTypes, dataValueField: "ArticleTypeId", dataTextField: "Name");
            ViewData[index: "UnitId"] = new SelectList(items: _context.Units, dataValueField: "UnitId", dataTextField: "Name");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Name,UnitId,ArticleTypeId,Price,DeliveryPeriod,CreationDate")] M_Article article)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: article);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "ArticleTypeId"] = new SelectList(items: _context.ArticleTypes, dataValueField: "ArticleTypeId", dataTextField: "Name", selectedValue: article.ArticleTypeId);
            ViewData[index: "UnitId"] = new SelectList(items: _context.Units, dataValueField: "UnitId", dataTextField: "Name", selectedValue: article.UnitId);
            return View(model: article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            ViewData[index: "ArticleTypeId"] = new SelectList(items: _context.ArticleTypes, dataValueField: "ArticleTypeId", dataTextField: "Name", selectedValue: article.ArticleTypeId);
            ViewData[index: "UnitId"] = new SelectList(items: _context.Units, dataValueField: "UnitId", dataTextField: "Name", selectedValue: article.UnitId);
            return View(model: article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Name,UnitId,ArticleTypeId,Price,DeliveryPeriod,CreationDate")] M_Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(id: article.Id))
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
            ViewData[index: "ArticleTypeId"] = new SelectList(items: _context.ArticleTypes, dataValueField: "ArticleTypeId", dataTextField: "Name", selectedValue: article.ArticleTypeId);
            ViewData[index: "UnitId"] = new SelectList(items: _context.Units, dataValueField: "UnitId", dataTextField: "Name", selectedValue: article.UnitId);
            return View(model: article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(navigationPropertyPath: a => a.ArticleType)
                .Include(navigationPropertyPath: a => a.Unit)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(model: article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.Articles.Remove(entity: article);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(predicate: e => e.Id == id);
        }
    }
}
