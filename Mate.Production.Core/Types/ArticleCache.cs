using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper.Types;
using Mate.DataCore.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Mate.Production.Core.Types
{
    public class ArticleCache
    {
        public ArticleCache(DbConnectionString connectionString)
        {
            _connectionString = connectionString;
        }

        private Dictionary<int, M_Article> _cache = new ();
        private readonly DbConnectionString _connectionString = new DbConnectionString(string.Empty);

        public M_Article GetArticleById(int id, decimal transitionFactor)
        {
            if (!_cache.TryGetValue(key: id,value: out M_Article obj))
            {
                using (var ctx = MateDb.GetContext(connectionString: _connectionString.Value))
                {
                    obj = ctx.Articles.Include(navigationPropertyPath: x => x.ArticleType)
                                      .Include(navigationPropertyPath: x => x.Unit)
                                      .Include(navigationPropertyPath: x => x.Operations)
                                        .ThenInclude(navigationPropertyPath: x => x.ResourceCapability)
                                            .ThenInclude(x => x.ResourceCapabilityProvider)
                                                .ThenInclude(x => x.ResourceSetups)
                                                    .ThenInclude(x => x.Resource)
                                      .Include(navigationPropertyPath: x => x.Operations)
                                        .ThenInclude(navigationPropertyPath: x => x.Characteristics)
                                            .ThenInclude(x => x.Attributes)
                                      .Include(navigationPropertyPath: x => x.Operations)
                                        .ThenInclude(navigationPropertyPath: x => x.ArticleBoms)
                                            .ThenInclude(navigationPropertyPath: x => x.ArticleChild)
                                                .ThenInclude(x => x.ArticleType)
                                      .Include(navigationPropertyPath: x => x.ArticleBoms)
                                        .ThenInclude(navigationPropertyPath: x => x.ArticleChild)
                                            .ThenInclude(x => x.ArticleType)
                                      .Single(predicate: (x => x.Id == id));
                    _cache.Add(key: id, value: obj);
                    ctx.Dispose();
                }
            }
            // TODO: Update Transition Times more Granular.
            UpdateTransitionTime(article: obj, factor: transitionFactor);
            return obj;
        }

        public void UpdateTransitionTime(M_Article article, decimal factor)
        {
            foreach (var operation in article.Operations)
            {
                operation.AverageTransitionDuration = TimeSpan.FromMinutes((double)Math.Round(d: (decimal)operation.Duration.TotalMinutes * factor, decimals: 0, mode: MidpointRounding.AwayFromZero));
            }
        }
    }
}
