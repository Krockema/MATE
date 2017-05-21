using Master40.DB.Repository.Context;
using Master40.DB.Repository.Data;
using Master40.Models.DB;

namespace Master40.DB.Repository.Domains
{
    class ArticleBomRepository : Repository<ArticleBom>, IArticleBomRepository
    {
        public ArticleBomRepository(MasterDBContext context) : base(context)
        {
        }
        public MasterDBContext MasterDBContext
        {
            get { return Context as MasterDBContext; }
        }
    }
}
