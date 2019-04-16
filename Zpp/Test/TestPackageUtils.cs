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
            var sql = "Select * from dbo.M_Article;";
            TestOutputHelper.WriteLine(sql);
            List<M_Article> articles = ProductionDomainContext.Articles.FromSql(sql).AsList();
            M_Article article = articles[0];
            ArticleTree articleTree = new ArticleTree(article );
            
            TestOutputHelper.WriteLine(articleTree.ToString());
            TestOutputHelper.WriteLine(articleTree.ToString());
        }
    }
}