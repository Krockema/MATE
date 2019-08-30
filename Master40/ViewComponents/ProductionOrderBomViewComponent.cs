using Master40.DB.Data;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.DataModel;

namespace Master40.ViewComponents
{
    public class ProductionOrderBomViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public ProductionOrderBomViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productionOrderId)
        {

            var masterDBContext = _context.ProductionOrders
                                            .Where(predicate: a => a.ArticleId == 1).ToList();

            var articleList = new List<T_ProductionOrder>();
            foreach (var item in masterDBContext)
            {
                //var article = await _context.GetProductionOrderBomRecursive(item, item.Id);
                //articleList.Add(article);
            }
            return View(model: articleList);

        }
    }
}
