using Master40.DataGenerator.Configuration;
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

            ResourceInitializer.MasterTableResourceCapability(dataBase.DbContext,
                                                            parameterSet.GetOption<Resource>().Value,
                                                            parameterSet.GetOption<Setup>().Value,
                                                            parameterSet.GetOption<Operator>().Value);

            // Article Unit and Type Definitions
            var units = new MasterTableUnit();
            units.Init(dataBase.DbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dataBase.DbContext);


            // Add your Methods here to extract transition Matrix (Übergangsmatrix)

            // Generating Bom's (Stücklisten)

            // Generate Operations

        }
    }
}
