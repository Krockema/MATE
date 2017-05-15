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
            var masterDBContext = _context.ProductionOrders.Include(p => p.Article);
            return View(await masterDBContext.ToListAsync());
        }

        // GET: ProductionOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders
                .Include(p => p.Article)
                .SingleOrDefaultAsync(m => m.ProductionOrderId == id);
            if (productionOrder == null)
            {
                return NotFound();
            }

            return View(productionOrder);
        }

        // GET: ProductionOrders/Create
        public IActionResult Create()
        {
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId");
            return View();
        }

        // POST: ProductionOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductionOrderId,ArticleId,Quantity,Name")] ProductionOrder productionOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productionOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", productionOrder.ArticleId);
            return View(productionOrder);
        }

        // GET: ProductionOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders.SingleOrDefaultAsync(m => m.ProductionOrderId == id);
            if (productionOrder == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", productionOrder.ArticleId);
            return View(productionOrder);
        }

        // POST: ProductionOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductionOrderId,ArticleId,Quantity,Name")] ProductionOrder productionOrder)
        {
            if (id != productionOrder.ProductionOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productionOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductionOrderExists(productionOrder.ProductionOrderId))
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
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", productionOrder.ArticleId);
            return View(productionOrder);
        }

        // GET: ProductionOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionOrder = await _context.ProductionOrders
                .Include(p => p.Article)
                .SingleOrDefaultAsync(m => m.ProductionOrderId == id);
            if (productionOrder == null)
            {
                return NotFound();
            }

            return View(productionOrder);
        }

        // POST: ProductionOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productionOrder = await _context.ProductionOrders.SingleOrDefaultAsync(m => m.ProductionOrderId == id);
            _context.ProductionOrders.Remove(productionOrder);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProductionOrderExists(int id)
        {
            return _context.ProductionOrders.Any(e => e.ProductionOrderId == id);
        }
    }
}
