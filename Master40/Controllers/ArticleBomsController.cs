using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class ArticleBomsController : Controller
    {
        private readonly ProductionDomainContext _context;

        public ArticleBomsController(ProductionDomainContext context)
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

            var masterDBContext = _context.Articles.Include(navigationPropertyPath: w => w.Operations)
                .Where(predicate: x => x.ArticleTypeId == 10027 /* Equals("Product") */).ToList();

            var articleList = new List<M_Article>();
            foreach (var item in masterDBContext)
            {
                var article = await _context.GetArticleBomRecursive(article: item, articleId: item.Id);
                articleList.Add(item: article);
            }
            return View(model: articleList);
        }

        // GET: ArticleBoms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms
                .Include(navigationPropertyPath: a => a.ArticleChild)
                .Include(navigationPropertyPath: a => a.ArticleParent)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (articleBom == null)
            {
                return NotFound();
            }

            return View(model: articleBom);
        }

        // GET: ArticleBoms/Create
        public IActionResult Create()
        {
            ViewData[index: "ArticleChildId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name");
            ViewData[index: "ArticleParentId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name");
            return View();
        }

        // POST: ArticleBoms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleBomId,ArticleParentId,ArticleChildId,Quantity,Name")] M_ArticleBom articleBom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: articleBom);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "ArticleChildId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleChildId);
            ViewData[index: "ArticleParentId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleParentId);
            return View(model: articleBom);
        }

        // GET: ArticleBoms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (articleBom == null)
            {
                return NotFound();
            }
            ViewData[index: "ArticleChildId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleChildId);
            ViewData[index: "ArticleParentId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleParentId);
            return View(model: articleBom);
        }

        // POST: ArticleBoms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleBomId,ArticleParentId,ArticleChildId,Quantity,Name")] M_ArticleBom articleBom)
        {
            if (id != articleBom.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: articleBom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleBomExists(id: articleBom.Id))
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
            ViewData[index: "ArticleChildId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleChildId);
            ViewData[index: "ArticleParentId"] = new SelectList(items: _context.Articles, dataValueField: "Id", dataTextField: "Name", selectedValue: articleBom.ArticleParentId);
            return View(model: articleBom);
        }

        // GET: ArticleBoms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleBom = await _context.ArticleBoms
                .Include(navigationPropertyPath: a => a.ArticleChild)
                .Include(navigationPropertyPath: a => a.ArticleParent)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (articleBom == null)
            {
                return NotFound();
            }

            return View(model: articleBom);
        }

        // POST: ArticleBoms/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articleBom = await _context.ArticleBoms.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.ArticleBoms.Remove(entity: articleBom);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool ArticleBomExists(int id)
        {
            return _context.ArticleBoms.Any(predicate: e => e.Id == id);
        }
    }
}