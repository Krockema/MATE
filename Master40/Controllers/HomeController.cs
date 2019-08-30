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


        [HttpGet(template: "[Controller]/ReloadDb/{products}")]
        public async Task<IActionResult> ReloadDb(string products)
        {
            await Task.Run(action: () =>
                {
                    switch (products)
                    {
                        case "Tables":
                            _context.Database.EnsureDeleted();
                            MasterDbInitializerTable.DbInitialize(context: _context);
                            break;
                        case "Trucks":
                            _context.Database.EnsureDeleted();
                            MasterDBInitializerTruck.DbInitialize(context: _context);
                            break;
                        default:
                            break;
                    }
                    
                }
            );

            return View(viewName: "Index");
        }

        public IActionResult About()
        {
            ViewData[index: "Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData[index: "Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
