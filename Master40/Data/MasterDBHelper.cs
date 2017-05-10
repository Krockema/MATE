using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Data
{
    public class MasterDbHelper
    {
        public async static Task<Article> GetArticleBomRecursive(MasterDBContext context, Article article, int ArticleId)
        {
            article.ArticleChilds = context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == ArticleId);

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(context, item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }

    }
}
