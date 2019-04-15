using System.Linq;
using Master40.DB.Data.Repository;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class OrderDomainContext : OrderDomain
    {
        public OrderDomainContext(DbContextOptions<OrderDomainContext> options) : base(options) { }


        //complex Querys
        public IQueryable<T_CustomerOrder> GetAllOrders
        {
            get
            {
                return Orders.Include(x => x.OrderParts)
                                .Include(x => x.BusinessPartner)
                                .Where(x => x.BusinessPartner.Debitor)
                                .AsNoTracking();
            }
        }

        public IQueryable<T_CustomerOrder> ById(int id) => Orders.Include(x => x.OrderParts)
            .Include(x => x.BusinessPartner)
            .Where(x => x.BusinessPartner.Debitor)
            .Where(x => x.Id == id);

        public IQueryable<M_Article> GetSellableArticles => Articles.Include(x => x.ArticleType)
                .Where(t => t.ArticleType.Name == "Assembly")
                .AsNoTracking();
        

        public IQueryable<M_Article> GetPuchaseableArticles => Articles.Include(x => x.ArticleType)
                    .Where(t => t.ArticleType.Name == "Material")
                    .AsNoTracking();
            
        



    }
}
