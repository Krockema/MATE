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
        private readonly AdjacencyList<M_Article> _adjacencyList;
        private readonly Node<M_Article> _rootArticle;

        // interface impl
        public List<Node<M_Article>> GetChildNodes(Node<M_Article> node)
        {
            if (_adjacencyList.getAsDictionary().ContainsKey(node))
            {
                return _adjacencyList.getAsDictionary()[node];
            }

            return null;

        }

        public Node<M_Article> GetRootNode() => _rootArticle;

        public AdjacencyList<M_Article> GetAdjacencyList() => _adjacencyList;

        public SortedDictionary<int, List<int>> getAdjacencyListWithArticleIds()
        {
            // as a hack the first entry in each list contains the parentId
            SortedDictionary<int, List<int>> adjacencyListWithArticleIds = new SortedDictionary<int, List<int>>();


            foreach (Node<M_Article> articleNode in _adjacencyList.getAsDictionary().Keys)
            {
                if (_adjacencyList.getAsDictionary()[articleNode] != null)
                {
                    if (!adjacencyListWithArticleIds.ContainsKey(articleNode.Entity.Id))
                    {
                        adjacencyListWithArticleIds.Add(articleNode.Entity.Id, new List<int>());
                    }
                    adjacencyListWithArticleIds[articleNode.Entity.Id] = _adjacencyList.getAsDictionary()[articleNode].Select(x => x.Entity.Id).ToList();
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

        public ArticleTree(M_ArticleBom articleBom, ProductionDomainContext _productionDomainContext)
        {
            M_ArticleBom queriedArticleBom = _productionDomainContext.ArticleBoms.Include(m => m.ArticleChild)
               .Single(x => x.Id == articleBom.Id);
            _rootArticle = new Node<M_Article>(queriedArticleBom.Id, queriedArticleBom.ArticleChild);
            this._productionDomainContext = _productionDomainContext;
            Dictionary<Node<M_Article>, List<Node<M_Article>>> builtArticleTree = buildArticleTree(_rootArticle, 
                new Dictionary<Node<M_Article>, List<Node<M_Article>>>());
            _adjacencyList = new AdjacencyList<M_Article>(builtArticleTree);
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
        private Dictionary<Node<M_Article>, List<Node<M_Article>>> buildArticleTree(Node<M_Article> givenArticle,
            Dictionary<Node<M_Article>, List<Node<M_Article>>> AdjacencyList)
        {

            if (!AdjacencyList.ContainsKey(givenArticle))
            {
                AdjacencyList.Add(givenArticle, new List<Node<M_Article>>());
            }

            M_Article readArticle = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).Single(x => x.Id == givenArticle.Entity.Id);
            if (readArticle.ArticleBoms != null)
            {
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    Node<M_Article> createdArticleNode = new Node<M_Article>(articleBom.Id, articleBom.ArticleChild);
                    AdjacencyList[givenArticle].Add(createdArticleNode);
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
