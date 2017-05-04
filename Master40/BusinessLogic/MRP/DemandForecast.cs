using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Master40.Data;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    interface IDemandForecast
    {
        Collection<ArticleBom> NetRequirement(int orderId);
        void GrossRequirement(Collection<ArticleBom> articles);
    }

    internal class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;

        internal DemandForecast(MasterDBContext context)
        {
            _context = context;
        }

        Collection<ArticleBom> IDemandForecast.NetRequirement(int orderId)
        {
            var need = new Collection<ArticleBom>();
            //get Order
            //for every orderPart get the Bom
            var orders = _context.Orders.Include(a => a.OrderParts).Where(a => a.OrderId == orderId);

            foreach (Order order in orders)
            {
                System.Diagnostics.Debug.WriteLine(order.Name, order.OrderId);
                //get Orderparts from Order
                var parts = order.OrderParts;
                //TODO: skip getting order and put a where in the query to get directly to the parts

                foreach (OrderPart part in parts)
                {
                    System.Diagnostics.Debug.WriteLine(part.Article + " " + part.OrderId);
                    //get Bom for every orderpart
                    //var boms = _context.ArticleBoms.Include(a => a.ArticleParent).Where(a => a.ArticleParent == part.Article);
                    var boms = part.Article.ArticleBoms;
                    
                    //recursively going through bom
                    GetNeeds(ref need, boms, part.Amount);

                }
            }
            return need;
        }

        private static void GetNeeds(ref Collection<ArticleBom> need, IEnumerable<ArticleBom> boms, int multiplier)
        {
            foreach (var bom in boms)
            {
                System.Diagnostics.Debug.WriteLine(bom.Name + " " + bom.Quantity + " " + bom.ArticleChild + " " +
                                                   bom.ArticleParent);
                var needBom = new ArticleBom();
                needBom.ArticleChildId = bom.ArticleChildId;
                needBom.ArticleParentId = bom.ArticleParentId;
                needBom.Quantity = bom.Quantity * multiplier;
                need.Add(needBom);

                if (bom.ArticleChild.ArticleBoms != null)
                {
                    GetNeeds(ref need, bom.ArticleChild.ArticleBoms, multiplier * bom.Quantity);
                }
            }
        }
  

        void IDemandForecast.GrossRequirement(Collection<ArticleBom> needs)
        {
            foreach (var need in needs)
            {
                //TODO: implement reservation of material in stock
                //TODO: parent auflösen und evt children überspringen
                var plannedStock = need.ArticleChild.Stock.Current - need.Quantity;
                if (plannedStock < 0)
                {
                    //TODO: implement productionOrder with orderId for -PlannedStock
                    System.Diagnostics.Debug.WriteLine("plannedStock < 0");
                    plannedStock = 0;
                }
                if (plannedStock < need.ArticleChild.Stock.Min)
                    //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)
                    System.Diagnostics.Debug.WriteLine("Order to produce "+ (need.ArticleChild.Stock.Max - plannedStock) + " pieces to fill up stock");
                
            }
        }
    }
}
