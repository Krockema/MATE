using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Master40.Data;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    public interface IDemandForecast
    {
        Collection<ArticleBom> NetRequirement(int orderId);
        void GrossRequirement(Collection<ArticleBom> articles);
    }

    public class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;

        public DemandForecast(MasterDBContext context)
        {
            _context = context;
        }

        Collection<ArticleBom> IDemandForecast.NetRequirement(int orderId)
        {

            var need = new Collection<ArticleBom>();
            //get Order
            //for every orderPart get the Bom
            var orders = _context.Orders.Include(a => a.OrderParts).Where(a => a.OrderId == orderId);

            foreach (var order in orders)
            {
                System.Diagnostics.Debug.WriteLine(order.Name, order.OrderId);
                //get Orderparts from Order
                var parts = _context.OrderParts.Include(a => a.Article).Where(a => a.OrderId == orderId);
                //TODO: skip getting order and put a where in the query to get directly to the parts

                foreach (var part in parts)
                {
                    System.Diagnostics.Debug.WriteLine(part.Article.Name + " " + part.OrderId);
                    //get Bom for every orderpart
                    var boms = _context.ArticleBoms.AsNoTracking()
                        .Include(a => a.ArticleChild)
                        .Include(a => a.ArticleParent)
                        .Where(a => a.ArticleParentId == part.ArticleId)
                        .ToList();
                    
                    var tempBom = _context.ArticleBoms.AsNoTracking().Single(a => a.ArticleBomId == 1);
                    need.Add(tempBom);
                    need[0].Quantity *= part.Amount;
                    //recursively going through bom
                    GetNeeds(ref need, boms, part.Amount);
                    
                }
            }
            
            return need;
        }

        private void GetNeeds(ref Collection<ArticleBom> need, IEnumerable<ArticleBom> boms,  decimal multiplier)
        {
            foreach (var bom in boms)
            {
                System.Diagnostics.Debug.WriteLine(bom.Name + " " + bom.Quantity + " " + bom.ArticleChild + " " +
                                                   bom.ArticleParent);

                var tempBom = bom;
                tempBom.Quantity *= multiplier;
                need.Add(tempBom);

                if (bom.ArticleChildId != null)
                {
                    GetNeeds(ref need, _context.ArticleBoms.AsNoTracking().Where(a => a.ArticleParentId == bom.ArticleChildId), bom.Quantity);
                }
            }
        }
  

        void IDemandForecast.GrossRequirement(Collection<ArticleBom> needs)
        {
            foreach (var need in needs)
            {
                var tempNeed = _context.ArticleBoms.Include(a => a.ArticleChild).Include(a => a.ArticleChild.Stock).Single(a => a.ArticleBomId == need.ArticleBomId);
                //TODO: parent auflösen und evt children überspringen
                var plannedStock = tempNeed.ArticleChild.Stock.Current - need.Quantity;
                if (tempNeed.ArticleChild.Stock.Current > 0)
                {
                    var amount = need.Quantity - tempNeed.ArticleChild.Stock.Current;
                    if (amount < 0) amount = need.Quantity;
                    DeleteChildren(ref needs,need, amount);
                }
                    
                if (plannedStock < 0)
                {
                    //TODO: implement productionOrder with orderId for -PlannedStock
                    System.Diagnostics.Debug.WriteLine("plannedStock < 0");
                    plannedStock = 0;
                }
                if (plannedStock < tempNeed.ArticleChild.Stock.Min)
                    //TODO: implement productionOrder with seperate Id for Max - (Current - Quantity)
                    System.Diagnostics.Debug.WriteLine("Order to produce "+ (need.ArticleChild.Stock.Max - plannedStock) + " pieces to fill up stock");
                
            }
        }

        private void DeleteChildren(ref Collection<ArticleBom> needs, ArticleBom BomNeed, decimal amount)
        {
            foreach (var need in needs)
            {

                if (need.ArticleParentId != null && need.ArticleParentId == BomNeed.ArticleChildId)
                {
                    if (need.ArticleChildId != null)
                        DeleteChildren(ref needs, need, need.Quantity);
                    needs[needs.IndexOf(need)].Quantity-=amount*_context.ArticleBoms.Single(a => a.ArticleChildId == need.ArticleChildId).Quantity;
                }
            }
            foreach (var need in needs)
            {
                if (need.Quantity == 0) System.Diagnostics.Debug.WriteLine("Delete Children: aktuelles Produkt auf 0 Stück: " + need.Name);
            }
        }
    }
}
