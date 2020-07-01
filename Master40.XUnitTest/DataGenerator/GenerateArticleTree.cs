using System.Linq;
using Master40.DataGenerator;
using Master40.DataGenerator.Configuration;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Xunit;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateArticleTree
    {
        [Fact]
        public void GenerateModel()
        {
            var articleTree = new ArticleTree();
            var parameterSet = ParameterSet.Create(ParameterSet.Defaults);
            var dataBase = parameterSet.GetOption<DataBase<ProductionDomainContext>>();

            // Your Method.
            articleTree.Create(parameterSet);

            Assert.True(dataBase.DbContext.ResourceCapabilities.Count() != 0);
        }
    }
}