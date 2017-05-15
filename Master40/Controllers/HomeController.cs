using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master40.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.MasterDBContext _context;
        public HomeController(MasterDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var model = new List<MenuItem>();
           
                model = _context.MenuItems
                .Where(m => m.MenuId == 1).ToList().Where(m => m.Parent == null)
                //.Include(m => m.Children).Where(m => m.MenuId == 1).Where(m => m.Parent != null)
                //.Where(m => m.ParentMenuItemId == m.MenuId)
                .ToList();

            return View(model);
        }


        public async Task<IActionResult> ReloadDb()
        {
            await Task.Run(() =>
                {
                    _context.Database.EnsureDeleted();
                    MasterDBInitializer.DbInitialize(_context);
                }
            );

            return View("Index");
        }

        public IActionResult Menu()
        {
            var model = new List<MenuItem>();

            model = _context.MenuItems
            .Where(m => m.MenuId == 1).ToList().Where(m => m.Parent == null)
            //.Include(m => m.Children).Where(m => m.MenuId == 1).Where(m => m.Parent != null)
            //.Where(m => m.ParentMenuItemId == m.MenuId)
            .ToList();

            return PartialView(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
