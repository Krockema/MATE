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
            
            M_Article rootArticle = ProductionDomainContext.Articles.Single(x => x.Id == 1);
            ArticleTree articleTree = new ArticleTree(rootArticle, ProductionDomainContext );
            
            Dictionary<int, int[]> expectedAdjacencyList = new Dictionary<int, int[]>()
            {
                { 1, new int[] { 23, 26, 21, 22, 5, 3, 25, 4, 2 } },
                { 10, new int[] {7} },
                { 14, new int[] {7} },
                { 15, new int[] {7} },
                { 16, new int[] {7} },
                { 17, new int[] {6} },
                { 18, new int[] {6} },
                { 21, new int[] {4, 2, 5, 17, 18} },
                { 22, new int[] {16, 15, 14, 13, 4, 2, 5, 3} },
                { 23, new int[] {5, 10, 3, 11, 9, 8} },
                
            }; 
            Dictionary<int, int[]> actualAdjacencyList = new Dictionary<int, int[]>();
            foreach (int articleId in expectedAdjacencyList.Keys)
            {
                M_Article article = ProductionDomainContext.Articles.Single(x => x.Id == articleId);
                List<M_Article> childNodes = articleTree.getChildNodes(article);
                if (childNodes != null)
                {
                    actualAdjacencyList[articleId] = childNodes.Select(x => x.Id).ToArray();
                }
            }
            
            System.Diagnostics.Debug.WriteLine("Expected: " + TreeTools<int>.AdjacencyListToString(expectedAdjacencyList));
            System.Diagnostics.Debug.WriteLine("Actual: " + articleTree);
            
            Assert.Equal(expectedAdjacencyList, actualAdjacencyList);
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
            M_Article rootArticle = ProductionDomainContext.Articles.Single(x => x.Id == 1);
            ArticleTree articleTree = new ArticleTree(rootArticle, ProductionDomainContext );
            List<M_Article> traversedNodes = TreeTools<M_Article>.traverseDepthFirst(articleTree, node => { counter++;});
            List<int> traversedArticleIds = traversedNodes.Select(x => x.Id).AsList();
            
            System.Diagnostics.Debug.WriteLine("Expected: " + string.Join(",", expectedTraversePath));
            System.Diagnostics.Debug.WriteLine("Actual: " + string.Join(",", traversedArticleIds));
            
            Assert.Equal(expectedTraversePath, traversedArticleIds );
        }
    }
}