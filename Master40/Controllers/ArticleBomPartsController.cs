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
    public class ArticleBomPartsController : Controller
    {
        private readonly MasterDBContext _context;

        public ArticleBomPartsController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: ArticleBomParts
        public async Task<IActionResult> Index()
        {
            // .Where(m => m.MenuId == 1).ToList().Where(m => m.Parent == null)
            var masterDBContext = _context.ArticleBomParts
                .Where(a => a.ArticleBomId == 1)
               .Include(a => a.Article)
               .Include(a => a.ArticleBom)
               .Include(a => a.ParrentArticleBomPart)
               .Include(a => a.ChildArticleBomParts).ToList()
              .Where(a => a.ParrentArticleBomPart == null);
                ;
            return View(masterDBContext);
        }

        // GET: ArticleBomParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBomPart = await _context.ArticleBomParts
                .Include(a => a.Article)
                .Include(a => a.ArticleBom)
                .Include(a => a.ParrentArticleBomPart)
                .SingleOrDefaultAsync(m => m.ArticleBomPartsId == id);
            if (articleBomPart == null)
            {
                return NotFound();
            }

            return View(articleBomPart);
        }

        // GET: ArticleBomParts/Create
        public IActionResult Create()
        {
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name");
            ViewData["ArticleBomId"] = new SelectList(_context.ArticleBoms, "ArticleBomId", "Name");
            ViewData["ParrentArticleBomPartId"] = new SelectList(_context.ArticleBomParts, "ArticleBomPartsId", "Name");
            return View();
        }

        // POST: ArticleBomParts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleBomPartsId,ParrentArticleBomPartId,ArticleId,ArticleBomId,Count,Name")] ArticleBomPart articleBomPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(articleBomPart);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", articleBomPart.ArticleId);
            ViewData["ArticleBomId"] = new SelectList(_context.ArticleBoms, "ArticleBomId", "Name", articleBomPart.ArticleBomId);
            ViewData["ParrentArticleBomPartId"] = new SelectList(_context.ArticleBomParts, "ArticleBomPartsId", "Name", articleBomPart.ParrentArticleBomPartId);
            return View(articleBomPart);
        }

        // GET: ArticleBomParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBomPart = await _context.ArticleBomParts.SingleOrDefaultAsync(m => m.ArticleBomPartsId == id);
            if (articleBomPart == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", articleBomPart.ArticleId);
            ViewData["ArticleBomId"] = new SelectList(_context.ArticleBoms, "ArticleBomId", "Name", articleBomPart.ArticleBomId);
            ViewData["ParrentArticleBomPartId"] = new SelectList(_context.ArticleBomParts, "ArticleBomPartsId", "Name", articleBomPart.ParrentArticleBomPartId);
            return View(articleBomPart);
        }

        // POST: ArticleBomParts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleBomPartsId,ParrentArticleBomPartId,ArticleId,ArticleBomId,Count,Name")] ArticleBomPart articleBomPart)
        {
            if (id != articleBomPart.ArticleBomPartsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articleBomPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleBomPartExists(articleBomPart.ArticleBomPartsId))
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
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", articleBomPart.ArticleId);
            ViewData["ArticleBomId"] = new SelectList(_context.ArticleBoms, "ArticleBomId", "Name", articleBomPart.ArticleBomId);
            ViewData["ParrentArticleBomPartId"] = new SelectList(_context.ArticleBomParts, "ArticleBomPartsId", "Name", articleBomPart.ParrentArticleBomPartId);
            return View(articleBomPart);
        }

        // GET: ArticleBomParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBomPart = await _context.ArticleBomParts
                .Include(a => a.Article)
                .Include(a => a.ArticleBom)
                .Include(a => a.ParrentArticleBomPart)
                .SingleOrDefaultAsync(m => m.ArticleBomPartsId == id);
            if (articleBomPart == null)
            {
                return NotFound();
            }

            return View(articleBomPart);
        }

        // POST: ArticleBomParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articleBomPart = await _context.ArticleBomParts.SingleOrDefaultAsync(m => m.ArticleBomPartsId == id);
            _context.ArticleBomParts.Remove(articleBomPart);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ArticleBomPartExists(int id)
        {
            return _context.ArticleBomParts.Any(e => e.ArticleBomPartsId == id);
        }
    }
}
