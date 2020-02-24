using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer.Tables;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerTruck
    {
        public static void DbInitialize(MasterDBContext context, ModelSize modelSize)
        {
            context.Database.EnsureCreated();

            // Look for any Entrys.
            if (context.Articles.Any())
                return;   // DB has been seeded
            
            // Resource Definitions
            var resourceTools = new MasterTableResourceTool();
            resourceTools.Init(context);

            // requires Tools and Resources
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.Init(context);

            // requires Tools

            // requires Tools, Resources, and Capabilities
            

            switch (modelSize)
                {
                    case ModelSize.Small: throw  new NotImplementedException();
                    case ModelSize.Medium:
                        var resourceMedium = new MasterTableResource();
                        resourceMedium.InitMedium(context);
                        var resourceSetupsMedium = new MasterTableResourceSetup(resourceMedium, resourceTools, resourceCapabilities);
                        resourceSetupsMedium.InitMedium(context);
                        break;
                    case ModelSize.Large:
                        var resource = new MasterTableResource();
                        resource.InitLarge(context);
                        var resourceSetupsLarge = new MasterTableResourceSetup(resource, resourceTools, resourceCapabilities);
                        resourceSetupsLarge.InitLarge(context);
                        break;
                    default: throw new ArgumentException();
                }


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