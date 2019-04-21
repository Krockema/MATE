using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Dispatch.SysMsg;
using Dapper;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Zpp.Utils;

namespace Zpp.Test
{
    public class TestPackageUtils : AbstractTest
    {
        
// @before
        public TestPackageUtils(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            
        }

        [Fact]
        public void testArticleTree()
        {
            System.Diagnostics.Debug.WriteLine("Starting: testArticleTree");
            
            M_ArticleBom rootArticle = ProductionDomainContext.ArticleBoms.Single(x => x.Id == 1);
            ArticleTree articleTree = new ArticleTree(rootArticle, ProductionDomainContext );
            
            SortedDictionary<int, List<int>> expectedAdjacencyList = new SortedDictionary<int, List<int>>()
            {
             // bomId       ArticleChildId=ArticleChildNode
                { 1, new List<int> { 23, 26, 21, 22, 5, 3, 25, 4, 2 } },
                { 10, new List<int> {7} },
                { 14, new List<int> {7} },
                { 15, new List<int> {7} },
                { 16, new List<int> {7} },
                { 17, new List<int> {6} },
                { 18, new List<int> {6} },
                { 21, new List<int> {4, 2, 5, 17, 18} },
                { 22, new List<int> {16, 15, 14, 13, 4, 2, 5, 3} },
                { 23, new List<int> {5, 10, 3, 11, 9, 8} },
                
            };
            foreach (int key in expectedAdjacencyList.Keys)
            {
                expectedAdjacencyList[key].Sort();
            }

            SortedDictionary<int, List<int>> actualAdjacencyList = articleTree.getAdjacencyListWithArticleIds();
            foreach (int key in actualAdjacencyList.Keys)
            {
                actualAdjacencyList[key].Sort();
            }
            
            System.Diagnostics.Debug.WriteLine("Expected: " + Environment.NewLine + TreeTools<int>.AdjacencyListToString(expectedAdjacencyList));
            System.Diagnostics.Debug.WriteLine("Actual: " + Environment.NewLine + TreeTools<int>.AdjacencyListToString(actualAdjacencyList));
            if (Constants.IsWindows)
            {
                Assert.Equal(expectedAdjacencyList, actualAdjacencyList);
            }
            else
            {
                // ids are not constant, compare only sizeOf
                List<int> countsExpected = new List<int>();
                List<int> countsActual = new List<int>();
                foreach (int key in expectedAdjacencyList.Keys)
                {
                    countsExpected.Add(expectedAdjacencyList[key].Count());
                }
                foreach (int key in actualAdjacencyList.Keys)
                {
                    countsActual.Add(actualAdjacencyList[key].Count());
                }
                countsExpected.Sort();
                countsActual.Sort();
                Assert.Equal(countsExpected, countsActual);
            }
        }

        [Fact]
        public void testTreeToolTraverseTree()
        {
            System.Diagnostics.Debug.WriteLine("Starting: testTreeToolTraverseTree");

            int[] expectedTraversePath = new int[]
            {
                1, 2, 4, 25, 3, 5, 22, 3, 5, 2, 4, 13, 14, 7, 15, 7, 16, 7, 21, 18, 6, 17, 6, 5, 2, 4, 26, 23, 8, 9, 11,
                3, 10, 7, 5
            };
            int counter = 0;
            M_ArticleBom rootArticle = ProductionDomainContext.ArticleBoms.Single(x => x.Id == 1);
            ArticleTree articleTree = new ArticleTree(rootArticle, ProductionDomainContext );
            
            // sort the tree
            foreach (int key in articleTree.GetAdjacencyList().getAsDictionary().Keys)
            {
                articleTree.GetAdjacencyList().getAsDictionary()[key].Sort();
            }
            
            // traverse tree and execute an action
            List<Node<M_Article>> traversedNodes = TreeTools<M_Article>.traverseDepthFirst(articleTree, node => { counter++;});
            List<int> traversedArticleIds = traversedNodes.Select(x => x.Entity.Id).AsList();
            
            System.Diagnostics.Debug.WriteLine("Expected: " + string.Join(",", expectedTraversePath));
            System.Diagnostics.Debug.WriteLine("Actual: " + string.Join(",", traversedArticleIds));
            
            // order is not constant, compare only sizeOf
            Assert.Equal(expectedTraversePath.Count(), traversedArticleIds.Count() );
            Assert.Equal(expectedTraversePath.Count(), counter);
        }
    }
}