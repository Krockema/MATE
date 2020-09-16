using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class ArticleInitializer
    {
        public static void init(List<Dictionary<long, Node>> nodesPerLevel, MasterDBContext context)
        {
            List<List<M_Article>> articles = new List<List<M_Article>>();
            var currentList = new List<M_Article>();
            articles.Add(currentList);
            var counter = 0;
            foreach (var article in nodesPerLevel.SelectMany(articleSet => articleSet.Values))
            {
                currentList.Add(article.Article);
                counter++;
                if (counter == Int32.MaxValue)
                {
                    currentList = new List<M_Article>();
                    articles.Add(currentList);
                    counter = 0;
                }
            }

            foreach (var articleSet in currentList)
            {
                context.Articles.AddRange(entities: articleSet);
                context.SaveChanges();
            }
        }
    }
}