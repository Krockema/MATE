using Master40.DB.Repository.Context;
using Master40.DB.Repository.Data;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.DB.Repository.Domains
{
    public class ArticleRepository : Repository<Article>, IArticleRepository
    {
        public ArticleRepository(MasterDBContext context) : base(context)
        {
        }
        public MasterDBContext MasterDBContext
        {
            get { return Context as MasterDBContext; }
        }
        public async Task<Article> GetArticleBomRecursive(Article article, int ArticleId)
        {
            article.ArticleChilds = MasterDBContext.ArticleBoms.Include(a => a.ArticleChild)
                                                        .ThenInclude(w => w.WorkSchedules)
                                                        .Where(a => a.ArticleParentId == ArticleId).ToList();

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }
    }
}
