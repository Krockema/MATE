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
            
            ArticleTree articleTree = new ArticleTree(1, ProductionDomainContext );
            TestOutputHelper.WriteLine(articleTree.ToString());
            TestOutputHelper.WriteLine(articleTree.ToString());
        }
    }
}