using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FArticles;

namespace Master40.SimulationCore.Agents.StorageAgent.Types
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
