using Mate.DataCore.Data.Helper.Types;
using Mate.Production.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Agents.SupervisorAgent.Types
{
    public class ThroughputTimeAnalyzer
    {
        public ThroughputTimeAnalyzer()
        {
        }

        public (long, long) GetTimeForProduct(ArticleCache articleCache, int articleId, int level)
        {
            var article = articleCache.GetArticleById(articleId, 10.0);
            
            // totalduration 
            long totalDuration = 0L;

            long longestDuration = 0L;

            var articleDuration = 0L;

            foreach (var operation in article.Operations.OrderByDescending(x => x.HierarchyNumber))
            {
                articleDuration = operation.Duration + operation.AverageTransitionDuration;
            }

            var longestChild = 0L;

            foreach(var bom in article.ArticleBoms)
            {
                (var total, var longest) = GetTimeForProduct(articleCache, bom.ArticleChildId, level + 1);

                totalDuration += total;

                if(longest > longestChild)
                    longestChild = longest;
            }

            totalDuration += articleDuration;

            longestDuration = articleDuration + longestChild;

            string result = new String('-', level);
            System.Diagnostics.Debug.WriteLine($"{result} {article.Id} Duration: {totalDuration} | LongestPath: {longestDuration}");

            return (totalDuration, longestDuration);
        }

    }
}
