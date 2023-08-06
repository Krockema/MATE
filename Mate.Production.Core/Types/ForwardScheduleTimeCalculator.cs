using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.Production.Core.Types
{
    public class ForwardScheduleTimeCalculator
    {
        /// <summary>
        /// Element to calculate the latest returned forward schedule and provides a check to test if all required schedules for this article are returned.
        /// </summary>
        /// <param name="fArticle">required Article Type in ArticleBom.Child !</param>
        public ForwardScheduleTimeCalculator(ArticleRecord articleRec)
        {
           RequiredQuantity = CalculateRequiredQuantity(articleRec: articleRec);
        }

        private List<DateTime> ForwardScheduledEndingTimes { get; } = new ();

        private int RequiredQuantity { get; }

        internal DateTime Max => ForwardScheduledEndingTimes.Max();
        internal long GetRequiredQuantity => RequiredQuantity;
        internal long Count => ForwardScheduledEndingTimes.Count;
        /// <summary>
        /// Add Scheduling Element to the internal List
        /// </summary>
        /// <param name="earliestStartForForwardScheduling"></param>
        internal void Add(DateTime earliestStartForForwardScheduling)
        {
            ForwardScheduledEndingTimes.Add(item: earliestStartForForwardScheduling);
        }

        /// <summary>
        /// Check if all required schedules returned
        /// </summary>
        /// <param name="fArticle"></param>
        /// <returns></returns>
        internal bool AllRequirementsFullFilled(ArticleRecord articleRec)
        {
            return ForwardScheduledEndingTimes.Count == RequiredQuantity;
        }

        public static int CalculateRequiredQuantity(ArticleRecord articleRec)
        {
            var quantityMaterials = articleRec.Article.ArticleBoms.Count(x =>
                x.ArticleChild.ArticleType.Name == "Material" || x.ArticleChild.ArticleType.Name == "Consumable");

            var quantityProducts = articleRec.Article.ArticleBoms.Where(x =>
                    x.ArticleChild.ArticleType.Name == "Assembly" || x.ArticleChild.ArticleType.Name == "Product")
                .Sum(x => x.Quantity);

            var quantityAll = quantityMaterials + quantityProducts;

            return Convert.ToInt32(quantityAll);
        }
    }
}
