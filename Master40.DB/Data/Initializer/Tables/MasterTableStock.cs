using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal static class MasterTableStock
    {
        public static void Init(MasterDBContext context, M_Article[] articles)
        {
            // create Stock Entrys for each Article
            foreach (var article in articles)
            {
                var stock = new M_Stock
                {
                    ArticleForeignKey = article.Id,
                    Name = "Stock: " + article.Name,
                    Min = (article.ToPurchase) ? 1000 : 0,
                    Max = (article.ToPurchase) ? 2000 : 0,
                    Current = (article.ToPurchase) ? 1000 : 0,
                    StartValue = (article.ToPurchase) ? 1000 : 0,
                };
                context.Stocks.Add(entity: stock);
                context.SaveChanges();
            }
        }
    }
}