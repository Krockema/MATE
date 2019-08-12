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
            return this.FirstOrDefault(r => r.Key == key);
        }

        public bool RemoveByKey(Guid key)
        {
            return this.Remove(this.Single(x => x.Key == key));
        }

    }
}
