using System.Linq;
using Master40.DB.Data.Repository;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class SchedulingDomainContext : SchedulingDomain
    { 
        public SchedulingDomainContext(DbContextOptions<MasterDBContext> options) : base(options)
        { }

        public IQueryable<Article> ById(int id)
        {
            return Articles.Include(x => x.ArticleType)
                .Include(x => x.Unit)
                .Where(x => x.Id == id);
        }
    }
}
