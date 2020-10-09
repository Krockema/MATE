using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DataGenerator.Repository
{
    public class ArticleRepository
    {

        public static Dictionary<string, M_Article> GetArticlesByNames(HashSet<string> names, MasterDBContext ctx)
        {
            
            return ctx.Articles
                .Include(navigationPropertyPath: x => x.Operations)
                    .ThenInclude(navigationPropertyPath: x => x.ResourceCapability).Where(a => names.Contains(a.Name))
                .ToDictionary(a => a.Name);
        }

        public static Dictionary<string, int> GetArticleNamesAndCountForEachUsedArticleInSimulation(ResultContext ctx, int simNumber)
        {
            var articleCount = new Dictionary<string, int>();

            using (var command = ctx.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText =
                    "SELECT source.article, COUNT(source.article) AS quantity\r\nFROM (\r\n\tSELECT [ProductionOrderId], [Article], COUNT([ProductionOrderId]) AS quantity\r\n\tFROM [TestResultContext].[dbo].[SimulationJobs]\r\n\tWHERE [SimulationNumber] = " +
                    simNumber +
                    " AND [JobType] = 'Operation'\r\n\tGROUP BY [ProductionOrderId], [Article]\r\n) AS source\r\nGROUP BY source.article\r\nORDER BY quantity DESC";
                ctx.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        articleCount.Add(result.GetString(0), result.GetInt32(1));
                    }
                }
            }

            return articleCount;
        }
    }
}
