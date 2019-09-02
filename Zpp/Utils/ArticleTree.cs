using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Zpp.DbCache;

namespace Zpp.Utils
{
    public class ArticleTree : ITree<M_Article>
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDbTransactionData _dbTransactionData;

        // int is the id of an article
        private readonly AdjacencyList<M_Article> _adjacencyList;
        private readonly Node<M_Article> _rootArticle;

        // interface impl
        public List<Node<M_Article>> GetChildNodes(Node<M_Article> node)
        {
            if (_adjacencyList.getAsDictionary().ContainsKey(node.Entity.Id))
            {
                return _adjacencyList.getAsDictionary()[node.Entity.Id];
            }

            return null;
        }

        public Node<M_Article> GetRootNode() => _rootArticle;

        public AdjacencyList<M_Article> GetAdjacencyList() => _adjacencyList;

        public SortedDictionary<int, List<int>> getAdjacencyListWithArticleIds()
        {
            // as a hack the first entry in each list contains the parentId
            SortedDictionary<int, List<int>> adjacencyListWithArticleIds =
                new SortedDictionary<int, List<int>>();

            foreach (int articleId in _adjacencyList.getAsDictionary().Keys)
            {
                if (_adjacencyList.getAsDictionary()[articleId] != null)
                {
                    if (!adjacencyListWithArticleIds.ContainsKey(articleId))
                    {
                        adjacencyListWithArticleIds.Add(articleId, new List<int>());
                    }

                    adjacencyListWithArticleIds[articleId] =
                        _adjacencyList.getAsDictionary()[articleId].Select(x => x.Entity.Id)
                            .ToList();
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

        public ArticleTree(M_ArticleBom articleBom,
            IDbTransactionData dbTransactionData)
        {
            M_ArticleBom queriedArticleBom = dbTransactionData.M_ArticleBomGetById(articleBom.GetId());
            _rootArticle = new Node<M_Article>(queriedArticleBom.ArticleChild.Id,
                queriedArticleBom.Id,
                queriedArticleBom.ArticleChild);
            _dbTransactionData = dbTransactionData;
            Dictionary<int, List<Node<M_Article>>> builtArticleTree = buildArticleTree(
                _rootArticle,
                new Dictionary<int, List<Node<M_Article>>>());
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
        private Dictionary<int, List<Node<M_Article>>> buildArticleTree(
            Node<M_Article> givenArticle,
            Dictionary<int, List<Node<M_Article>>> AdjacencyList)
        {
            M_Article readArticle = _dbTransactionData.M_ArticleGetById(givenArticle.Entity.GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                if (!AdjacencyList.ContainsKey(givenArticle.Entity.Id))
                {
                    AdjacencyList.Add(givenArticle.Entity.Id, new List<Node<M_Article>>());
                }

                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    Node<M_Article> createdArticleNode = new Node<M_Article>(
                        articleBom.ArticleChildId,
                        articleBom.Id,
                        articleBom.ArticleChild);
                    AdjacencyList[givenArticle.Entity.Id].Add(createdArticleNode);
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
            String mystring =
                "The ArticleTree of Article " + _rootArticle + Environment.NewLine;

            return mystring + _adjacencyList;
        }
    }
}