using Master40.DB.Repository.Interfaces;
using Master40.Models.DB;
using System.Threading.Tasks;

namespace Master40.DB.Repository.Data
{
    public interface IArticleRepository : IRepository<Article> {
        Task<Article> GetArticleBomRecursive(Article article, int ArticleId);
    }
}
