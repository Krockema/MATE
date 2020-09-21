using Master40.DataGenerator.Configuration;
using Master40.DataGenerator.SampleData1;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.Nominal.Model;

namespace Master40.DataGenerator
{
    public class ArticleTree
    {
        public void Create(ParameterSet parameterSet)
        {
            var dataBase = parameterSet.GetOption<DataBase<ProductionDomainContext>>();

            dataBase.DbContext.Database.EnsureDeleted();
            dataBase.DbContext.Database.EnsureCreated();

            var resourceCapabilities = ResourceInitializer.MasterTableResourceCapability(dataBase.DbContext,
                                                            parameterSet.GetOption<Resource>().Value,
                                                            parameterSet.GetOption<Setup>().Value,
                                                            parameterSet.GetOption<Operator>().Value);

            // Article Unit and Type Definitions
            var units = new MasterTableUnit();
            units.Init(dataBase.DbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dataBase.DbContext);

            var articlesData = new ArticleData(articleTypes, units);
            var articles = articlesData.Init(dataBase.DbContext);

            MasterTableStock.Init(dataBase.DbContext, articles);

            var operations = new OperationData(articlesData, resourceCapabilities);
            operations.Init(dataBase.DbContext);

            var boms = new BomData();
            boms.Init(dataBase.DbContext, articlesData, operations);


            // Add your Methods here to extract transition Matrix (Übergangsmatrix)

            // Generating Bom's (Stücklisten)

            // GenerateBillOfMaterial Operations

        }
    }
}
