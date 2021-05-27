using System.Threading.Tasks;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Nominal.Model;
using Microsoft.AspNetCore.Mvc;

namespace Mate.Controllers
{
    public class HomeController : Controller
    {
        private readonly MateProductionDb _context;
        public HomeController(MateProductionDb context)
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
                            MasterDBInitializerTruck.DbInitialize(context: _context, resourceModelSize: ModelSize.Medium, setupModelSize: ModelSize.Medium, ModelSize.Small, 3, false, false);
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
