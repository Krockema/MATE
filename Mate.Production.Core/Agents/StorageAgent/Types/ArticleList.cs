using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;

namespace Mate.Production.Core.Agents.StorageAgent.Types
{
    public class ArticleList : List<FArticle>
    {
        public FArticle GetByKey(Guid key)
        {
            return this.FirstOrDefault(predicate: r => r.Key == key);
        }

        public bool RemoveByKey(Guid key)
        {
            return this.Remove(item: this.Single(predicate: x => x.Key == key));
        }

    }
}
