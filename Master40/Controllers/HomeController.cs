using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;

namespace Master40.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductionDomainContext _context;
        public HomeController(ProductionDomainContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }


        [HttpGet("[Controller]/ReloadDb/{products}")]
        public async Task<IActionResult> ReloadDb(string products)
        {
            await Task.Run(() =>
                {
                    switch (products)
                    {
                        case "Tables":
                            _context.Database.EnsureDeleted();
                            MasterDBInitializerSimple.DbInitialize(_context);
                            break;
                        case "Trucks":
                            _context.Database.EnsureDeleted();
                            MasterDBInitializerTruck.DbInitialize(_context);
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
