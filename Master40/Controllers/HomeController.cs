using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Master40.Data.Context;

namespace Master40.Controllers
{
    public class HomeController : Controller
    {
        private readonly MasterDBContext _context;
        public HomeController(MasterDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet("[Controller]/ReloadDb/{size}")]
        public async Task<IActionResult> ReloadDb(string size)
        {
            await Task.Run(() =>
                {
                    switch (size)
                    {
                        case "small":
                            _context.Database.EnsureDeleted();
                            MasterDBInitializerSmall.DbInitialize(_context);
                            break;
                        case "Medium":
                            _context.Database.EnsureDeleted();
                            MasterDBInitializerMedium.DbInitialize(_context);
                            break;
                        default:
                            break;
                    }
                    
                }
            );

            return View("Index");
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
