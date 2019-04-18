using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Zpp.Utils
{
    public class ArticleTree : ITree<M_Article>
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly ProductionDomainContext _productionDomainContext;
        
        // int is the id of an article
        private readonly Dictionary<int, List<M_Article>> _adjacencyList = new Dictionary<int, List<M_Article>>() ;
        private readonly M_Article _rootArticle;

        // interface impl
        public List<M_Article> getChildNodes(M_Article node)
        {
            if (_adjacencyList.ContainsKey(node.Id))
            {
                return _adjacencyList[node.Id];    
            }
            return null;

        }

        public M_Article getRootNode() => _rootArticle;

        public ArticleTree(M_Article article, ProductionDomainContext _productionDomainContext)
        {
            _rootArticle = article;
            this._productionDomainContext = _productionDomainContext;
            buildArticleTree(article.Id);
        }
        
        /**
         * Build up the tree from given articleBom. ATTENTION: recursive
         */
        private void buildArticleTree(int articleId)
        {
            if (!_adjacencyList.ContainsKey(articleId))
            {
                _adjacencyList.Add(articleId, new List<M_Article>());
            }
            
            M_Article article = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).Single(x => x.Id == articleId);
            if (article.ArticleBoms != null )
            {
                foreach (M_ArticleBom articleBom in article.ArticleBoms)
                {
                    _adjacencyList[article.Id].Add(articleBom.ArticleChild);
                    buildArticleTree(articleBom.ArticleChild.Id);
                }
            }
        }

        /**
         * prints the articleTree in following format (adjacencyList): parentId: child1, child2, ...
         */
        public override string ToString()
        {
            String mystring = "The ArticleTree of Article " + _rootArticle + Environment.NewLine;
            
            return mystring + TreeTools<M_Article>.AdjacencyListToString(_adjacencyList);
        }
    }
}