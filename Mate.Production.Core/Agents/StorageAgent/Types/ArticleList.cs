using Mate.Production.Core.Environment.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.Production.Core.Agents.StorageAgent.Types
{
    public class ArticleList : List<ArticleRecord>
    {
        public ArticleRecord GetByKey(Guid key)
        {
            return this.FirstOrDefault(predicate: r => r.Key == key);
        }

        public bool RemoveByKey(Guid key)
        {
            return this.Remove(item: this.Single(predicate: x => x.Key == key));
        }

    }
}
