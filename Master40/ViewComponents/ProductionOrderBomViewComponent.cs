using Master40.Data;
using Master40.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Master40.Extensions.ExtensionMethods;

namespace Master40.ViewComponents
{
    public class ProductionOrderBomViewComponent : ViewComponent
    {
        private readonly MasterDBContext _context;

        public ProductionOrderBomViewComponent(MasterDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productionOrderId)
        {
            {
                var masterDBContext = _context.ProductionOrders
                                                .Where(a => a.ArticleId == 1).ToList();

                var articleList = new List<ProductionOrder>();
                foreach (var item in masterDBContext)
                {
                    var article = await MasterDbHelper.GetProductionOrderBomRecursive(_context, item, item.ProductionOrderId);
                    articleList.Add(article);
                }
                return View(articleList);
            }
        }
    }
}
