using Mate.Production.Core.Types;
using System;
using System.Linq;

namespace Mate.Production.Core.Agents.SupervisorAgent.Types
{
    public class ThroughputTimeAnalyzer
    {
        public ThroughputTimeAnalyzer()
        {
        }

        /// <summary>
        /// Method return totalDuration and longest duration (which is ultimalty the critical path of the bom)
        /// </summary>
        /// <param name="articleCache">ArticleCache</param>
        /// <param name="articleId">Head ArticleId for the tree search</param>
        /// <param name="level">Level (product default 0) </param>
        /// <returns></returns>
        public (long, long) GetTimeForProduct(ArticleCache articleCache, int articleId, int level)
        {
            var article = articleCache.GetArticleById(articleId, 3.0);
            
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

                totalDuration += (long)(bom.Quantity * total);

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
