using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer.Tables;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerTruck
    {
        public static void DbInitialize(MasterDBContext context)
        {
            context.Database.EnsureCreated();

            // Look for any Entrys.
            if (context.Articles.Any())
                return;   // DB has been seeded
            
            // Resource Definitions
            var resourceTools = new MasterTableResourceTool();
                resourceTools.Init(context);
            // requires Tools
            var resource = new MasterTableResource();
                resource.Init(context);
            // requires Tools and Resources
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.Init(context);
            // requires Tools, Resources, and Capabilities
            var resourceSetups = new MasterTableResourceSetup(resource, resourceTools, resourceCapabilities);
                resourceSetups.Init(context);

            // Article Definitions
            var units = new MasterTableUnit();
                units.Init(context);
            var articleTypes = new MasterTableArticleType();
                articleTypes.Init(context);

            // requires Units and Article Types 
            var articleTable = new MasterTableArticle(articleTypes, units);
            var articles = articleTable.Init(context);

            MasterTableStock.Init(context, articles);

            var operations = new MasterTableOperation(articleTable, resourceCapabilities, resourceTools);
                operations.Init(context);

            var boms = new MasterTableBom();
                boms.Init(context, articleTable, operations);

            var businessPartner = new MasterTableBusinessPartner();
                businessPartner.Init(context);

            var articleToBusinessPartner = new MasterTableArticleToBusinessPartner();
                articleToBusinessPartner.Init(context, businessPartner, articleTable);

            var updateArticleLotSize = context.Articles
                .Include(x => x.ArticleType)
                .Include(x => x.ArticleToBusinessPartners)
                .ToList();
            // TODO noch gemogelt LotSize != PackSize
            foreach (var article in updateArticleLotSize)
            {
                if (article.ToPurchase)
                {
                    article.LotSize = article.ArticleToBusinessPartners.First().PackSize;
                    
                }
            }
            DbUtils.InsertOrUpdateRange(updateArticleLotSize, context.Articles, context);
            context.SaveChanges();
        }
    }
}