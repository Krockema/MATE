using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Types
{
    public class ArticleCache
    {
        public ArticleCache(string connectionString)
        {
            _connectionString = connectionString;
        }

        private Dictionary<int, M_Article> _cache = new Dictionary<int, M_Article>();
        private string _connectionString = "";

        public M_Article GetArticleById(int id, decimal transitionFactor)
        {
            if (!_cache.TryGetValue(key: id,value: out M_Article obj))
            {
                using (var ctx = MasterDBContext.GetContext(connectionString: _connectionString))
                {
                    obj = ctx.Articles.Include(navigationPropertyPath: x => x.ArticleType)
                                      .Include(navigationPropertyPath: x => x.Unit)
                                      .Include(navigationPropertyPath: x => x.Operations)
                                        .ThenInclude(navigationPropertyPath: x => x.ResourceSkill)
                                      .Include(navigationPropertyPath: x => x.ArticleBoms)
                                        .ThenInclude(navigationPropertyPath: x => x.ArticleChild)
                                            .ThenInclude(x => x.ArticleType)
                                      .Single(predicate: (x => x.Id == id));
                    _cache.Add(key: id, value: obj);
                    ctx.Dispose();
                }
            }
            // TODO: Üpdate Transition Times more Granular.
            UpdateTransitionTime(article: obj, factor: transitionFactor);
            return obj;
        }

        public void UpdateTransitionTime(M_Article article, decimal factor)
        {
            foreach (var operation in article.Operations)
            {
                operation.AverageTransitionDuration = Convert.ToInt32(value: Math.Round(d: operation.Duration * factor, decimals: 0, mode: MidpointRounding.AwayFromZero));
            }
        }
    }
}
