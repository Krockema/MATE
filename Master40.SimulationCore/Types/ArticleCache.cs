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
            if (!_cache.TryGetValue(id,out M_Article obj))
            {
                using (var ctx = MasterDBContext.GetContext(_connectionString))
                {
                    obj = Queryable.SingleOrDefault(source: ctx.Articles
                                        .Include(x => x.Operations)
                                        .ThenInclude(x => x.ResourceSkill)
                                        .Include(x => x.ArticleBoms)
                                            .ThenInclude(x => x.ArticleChild),
                                    predicate: (x => x.Id == id));
                    _cache.Add(id, obj);
                    ctx.Dispose();
                }
            }
            // TODO: Üpdate Transition Times more Granular.
            UpdateTransitionTime(obj, transitionFactor);
            return obj;
        }

        public void UpdateTransitionTime(M_Article article, decimal factor)
        {
            foreach (var operation in article.Operations)
            {
                operation.AverageTransitionDuration = Convert.ToInt32(Math.Round(operation.Duration * factor, 0, MidpointRounding.AwayFromZero));
            }
        }
    }
}
