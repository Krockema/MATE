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
                .Where(m => m.MenuId == 1)
                .Include(m => m.Children)
                .Where(m => m.Parent != null)
                .ToList();

            return View(model);
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
