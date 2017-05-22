using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.Data.Context;
using Master40.DB.Models;

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
            var masterDBContext = _context.Articles.Include(a => a.ArticleType).Include(a => a.Unit);
            return View(await masterDBContext.ToListAsync());
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.ArticleType)
                .Include(a => a.Unit)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewData["ArticleTypeId"] = new SelectList(_context.ArticleTypes, "ArticleTypeId", "Name");
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "Name");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Name,UnitId,ArticleTypeId,Price,DeliveryPeriod,CreationDate")] Article article)
        {
            if (ModelState.IsValid)
            {
                _context.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleTypeId"] = new SelectList(_context.ArticleTypes, "ArticleTypeId", "Name", article.ArticleTypeId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "Name", article.UnitId);
            return View(article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.SingleOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            ViewData["ArticleTypeId"] = new SelectList(_context.ArticleTypes, "ArticleTypeId", "Name", article.ArticleTypeId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "Name", article.UnitId);
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Name,UnitId,ArticleTypeId,Price,DeliveryPeriod,CreationDate")] Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.Id))
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
            ViewData["ArticleTypeId"] = new SelectList(_context.ArticleTypes, "ArticleTypeId", "Name", article.ArticleTypeId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "Name", article.UnitId);
            return View(article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.ArticleType)
                .Include(a => a.Unit)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.SingleOrDefaultAsync(m => m.Id == id);
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
