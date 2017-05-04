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
    public class OrderPartsController : Controller
    {
        private readonly MasterDBContext _context;

        public OrderPartsController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: OrderParts
        public async Task<IActionResult> Index()
        {
            var masterDBContext = _context.OrderParts.Include(o => o.Article).Include(o => o.Order);
            return View(await masterDBContext.ToListAsync());
        }

        // GET: OrderParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.OrderParts
                .Include(o => o.Article)
                .Include(o => o.Order)
                .SingleOrDefaultAsync(m => m.OrderPartId == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return View(orderPart);
        }

        // GET: OrderParts/Create
        public IActionResult Create()
        {
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name");
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "Name");
            return View();
        }

        // POST: OrderParts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderPartId,OrderId,ArticleId,Amount")] OrderPart orderPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderPart);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "Name", orderPart.OrderId);
            return View(orderPart);
        }

        // GET: OrderParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.OrderParts.SingleOrDefaultAsync(m => m.OrderPartId == id);
            if (orderPart == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "Name", orderPart.OrderId);
            return View(orderPart);
        }

        // POST: OrderParts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderPartId,OrderId,ArticleId,Amount")] OrderPart orderPart)
        {
            if (id != orderPart.OrderPartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderPartExists(orderPart.OrderPartId))
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
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleID", "Name", orderPart.ArticleId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "Name", orderPart.OrderId);
            return View(orderPart);
        }

        // GET: OrderParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderPart = await _context.OrderParts
                .Include(o => o.Article)
                .Include(o => o.Order)
                .SingleOrDefaultAsync(m => m.OrderPartId == id);
            if (orderPart == null)
            {
                return NotFound();
            }

            return View(orderPart);
        }

        // POST: OrderParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderPart = await _context.OrderParts.SingleOrDefaultAsync(m => m.OrderPartId == id);
            _context.OrderParts.Remove(orderPart);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool OrderPartExists(int id)
        {
            return _context.OrderParts.Any(e => e.OrderPartId == id);
        }
    }
}
