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
        public async static Task<ArticleBom> GetArticleBomRecursive(MasterDBContext context, ArticleBom articleBom, int ArticleId)
        {
            articleBom.ArticleBomItems = await context.ArticleBomItems
            .Include(a => a.Article)
                .ThenInclude(b => b.ArticleBoms)
            .Where(a => a.ArticleBom.ArticleId == ArticleId)
            .ToListAsync();

            foreach (var bomItem in articleBom.ArticleBomItems)
            {
                foreach (var articleBomItem in bomItem.Article.ArticleBoms)
                {
                    await GetArticleBomRecursive(context, articleBomItem, articleBomItem.ArticleId);
                }
            }
            await Task.Yield();
            return articleBom;

        }

    }
}
