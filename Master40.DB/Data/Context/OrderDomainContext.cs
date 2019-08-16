using System.Linq;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class OrderDomainContext : MasterDBContext
    {
        public OrderDomainContext(DbContextOptions<MasterDBContext> options) : base(options: options) { }


        //complex Querys
        public IQueryable<T_CustomerOrder> GetAllOrders
        {
            get
            {
                return CustomerOrders.Include(navigationPropertyPath: x => x.CustomerOrderParts)
                                .Include(navigationPropertyPath: x => x.BusinessPartner)
                                .Where(predicate: x => x.BusinessPartner.Debitor)
                                .AsNoTracking();
            }
        }

        public IQueryable<T_CustomerOrder> ById(int id) => CustomerOrders.Include(navigationPropertyPath: x => x.CustomerOrderParts)
            .Include(navigationPropertyPath: x => x.BusinessPartner)
            .Where(predicate: x => x.BusinessPartner.Debitor)
            .Where(predicate: x => x.Id == id);

        public IQueryable<M_Article> GetSellableArticles => Articles.Include(navigationPropertyPath: x => x.ArticleType)
                .Where(predicate: t => t.ArticleType.Name == "Assembly")
                .AsNoTracking();
        

        public IQueryable<M_Article> GetPuchaseableArticles => Articles.Include(navigationPropertyPath: x => x.ArticleType)
                    .Where(predicate: t => t.ArticleType.Name == "Material")
                    .AsNoTracking();
            
        



    }
}
