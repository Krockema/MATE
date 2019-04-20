using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Zpp.Utils
{
    public class ArticleTree : Tree<M_Article>
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly ProductionDomainContext _productionDomainContext;

        // int is the id of an article
        private readonly AdjacencyList<Node<M_Article>> _adjacencyList;
        private readonly Node<M_Article> _rootArticle;

        // interface impl
        public override List<Node<M_Article>> GetChildNodes(Node<M_Article> node)
        {
            if (_adjacencyList.getAsDictionary().ContainsKey(node))
            {
                return _adjacencyList.getAsDictionary()[node];
            }

            return null;

        }

        public override Node<M_Article> GetRootNode() => _rootArticle;

        public override AdjacencyList<Node<M_Article>> GetAdjacencyList() => _adjacencyList;

        public List<List<int>> getAdjacencyListWithArticleIds()
        {
            // as a hack the first entry in each list contains the parentId
            List<List<int>> adjacencyListWithArticleIds = new List<List<int>>();


            foreach (Node<M_Article> articleNode in _adjacencyList.getAsDictionary().Keys)
            {
                if (_adjacencyList.getAsDictionary()[articleNode] != null)
                {
                    List<int> tempList = new List<int>();
                    tempList.Add(articleNode.Entity.Id);
                    tempList.AddRange(_adjacencyList.getAsDictionary()[articleNode].Select(x => x.Entity.Id).ToList());
                    adjacencyListWithArticleIds.Add(tempList);
                }

                /*M_Article article = _productionDomainContext.Articles.Single(x => x.Id == articleId);
                List<M_Article> childNodes = getChildNodes(article);
                if (childNodes != null)
                {
                    actualAdjacencyList[articleId] = childNodes.Select(x => x.Id).ToList();
                    actualAdjacencyList[articleId].Sort();
                }*/
            }

            return adjacencyListWithArticleIds;
        }

        public ArticleTree(M_Article article, ProductionDomainContext _productionDomainContext)
        {
            _rootArticle = createNode(article);
            this._productionDomainContext = _productionDomainContext;
            _adjacencyList = new AdjacencyList<Node<M_Article>>(buildArticleTree(_rootArticle, null));
        }

        /**
         * Build up the tree from given articleBom. ATTENTION: recursive
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleId">root article node id</param>
        /// <param name="AdjacencyList">Always null at the begin of the recursion</param>
        /// <returns></returns>
        private Dictionary<Node<M_Article>, List<Node<M_Article>>> buildArticleTree(Node<M_Article> articleNode,
            Dictionary<Node<M_Article>, List<Node<M_Article>>> AdjacencyList)
        {
            if (AdjacencyList == null)
            {
                AdjacencyList = new Dictionary<Node<M_Article>, List<Node<M_Article>>>();
            }

            if (!AdjacencyList.ContainsKey(articleNode))
            {
                AdjacencyList.Add(articleNode, new List<Node<M_Article>>());
            }

            M_Article article = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).Single(x => x.Id == articleNode.Entity.Id);
            if (article.ArticleBoms != null)
            {
                foreach (M_ArticleBom articleBom in article.ArticleBoms)
                {
                    Node<M_Article> createdArticleNode = createNode(articleBom.ArticleChild);
                    AdjacencyList[articleNode].Add(createdArticleNode);
                    buildArticleTree(createdArticleNode, AdjacencyList);
                }
            }

            return AdjacencyList;
        }

        /**
         * prints the articleTree in following format (adjacencyList): parentId: child1, child2, ...
         */
        public override string ToString()
        {
            String mystring = "The ArticleTree of Article " + _rootArticle + Environment.NewLine;

            return mystring + _adjacencyList;
        }
    }
}
