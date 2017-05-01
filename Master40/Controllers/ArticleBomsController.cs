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
            /*
            var masterDBContext = _context.ArticleBoms
                .Where(a => a.ArticleParentId == 1)
                .Include(a => a.ArticleChild)
                .Include(a => a.ArticleParent)
                .ThenInclude(b => b.ArticleChilds).ToList().Where(a => 1 == 1);
                */

            var masterDBContext = _context.Articles
                .Where(a => a.ArticleID == 1).ToList();

            var articleList = new List<Article>();
            foreach (var item in masterDBContext)
            {
                 articleList.Add(getRecursive(item, item.ArticleID));
            }
            return View(articleList);
        }

        public Article getRecursive(Article article, int? id)
        {
            article.ArticleChilds = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == id);

            foreach (var item in article.ArticleChilds)
            {
                getRecursive(item.ArticleParent, item.ArticleChildId);
            }
            return article;
        }

        // GET: ArticleBoms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms
                .Include(a => a.ArticleChild)
                .Include(a => a.ArticleParent)
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
            ViewData["ArticleChildId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID");
            ViewData["ArticleParentId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID");
            return View();
        }

        // POST: ArticleBoms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleBomId,ArticleParentId,ArticleChildId,Quantity,Name")] ArticleBom articleBom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(articleBom);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleChildId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleChildId);
            ViewData["ArticleParentId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleParentId);
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
            ViewData["ArticleChildId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleChildId);
            ViewData["ArticleParentId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleParentId);
            return View(articleBom);
        }

        // POST: ArticleBoms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleBomId,ArticleParentId,ArticleChildId,Quantity,Name")] ArticleBom articleBom)
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
            ViewData["ArticleChildId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleChildId);
            ViewData["ArticleParentId"] = new SelectList(_context.Articles, "ArticleID", "ArticleID", articleBom.ArticleParentId);
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
                .Include(a => a.ArticleChild)
                .Include(a => a.ArticleParent)
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
