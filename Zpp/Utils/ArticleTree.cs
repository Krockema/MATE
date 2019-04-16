using System;
using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.Utils
{
    public class ArticleTree
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        // int is the id of an article
        private readonly Dictionary<int, List<M_Article>> _adjacencyList = new Dictionary<int, List<M_Article>>() ;
        private int? rootArticleId = null;

        public ArticleTree(M_Article article)
        {
            rootArticleId = article.Id;
            buildArticleTree(article);
        }
        
        /**
         * Build up the tree from given articleBom. ATTENTION: recursive
         */
        private void buildArticleTree(M_Article article)
        {
            _adjacencyList.Add(article.Id, new List<M_Article>());
            if (article.ArticleBoms != null )
            {
                foreach (M_ArticleBom articleBom in article.ArticleBoms)
                {
                    _adjacencyList[article.Id].Add(articleBom.ArticleChild);
                    buildArticleTree(articleBom.ArticleChild);
                }
            }
        }

        public override string ToString()
        {
            String mystring = "The ArticleTree of Article " + rootArticleId.ToString() + Environment.NewLine;
            foreach (int articleId in _adjacencyList.Keys)
            {
                foreach (M_Article article in _adjacencyList[articleId])
                {
                    mystring += article.Name;
                }

                mystring += Environment.NewLine;
            }

            return mystring;
        }

    }
}