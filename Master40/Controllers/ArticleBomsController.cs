using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.Data;
using Master40.Models.DB;

namespace Master40.Controllers
{
    public class ArticleBomsController : Controller
    {
        private readonly MasterDBContext _context;

        public ArticleBomsController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: ArticleBoms
        public async Task<IActionResult> Index()
        {
            var masterDBContext = _context.ArticleBoms.Include(a => a.Article);
            return View(await masterDBContext.ToListAsync());
        }

        // GET: ArticleBoms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms
                .Include(a => a.Article)
                .SingleOrDefaultAsync(m => m.ArticleBomId == id);
            if (articleBom == null)
            {
                return NotFound();
            }

            return View(articleBom);
        }

        // GET: ArticleBoms/Create
        public IActionResult Create()
        {
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name");
            return View();
        }

        // POST: ArticleBoms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleBomId,Name,ArticleId")] ArticleBom articleBom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(articleBom);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", articleBom.ArticleId);
            return View(articleBom);
        }

        // GET: ArticleBoms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms.SingleOrDefaultAsync(m => m.ArticleBomId == id);
            if (articleBom == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", articleBom.ArticleId);
            return View(articleBom);
        }

        // POST: ArticleBoms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleBomId,Name,ArticleId")] ArticleBom articleBom)
        {
            if (id != articleBom.ArticleBomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articleBom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleBomExists(articleBom.ArticleBomId))
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
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleId);
            return View(articleBom);
        }

        // GET: ArticleBoms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms
                .Include(a => a.Article)
                .SingleOrDefaultAsync(m => m.ArticleBomId == id);
            if (articleBom == null)
            {
                return NotFound();
            }

            return View(articleBom);
        }

        // POST: ArticleBoms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articleBom = await _context.ArticleBoms.SingleOrDefaultAsync(m => m.ArticleBomId == id);
            _context.ArticleBoms.Remove(articleBom);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ArticleBomExists(int id)
        {
            return _context.ArticleBoms.Any(e => e.ArticleBomId == id);
        }
    }
}
