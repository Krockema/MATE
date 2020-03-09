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
        public static void DbInitialize(MasterDBContext context, ModelSize resourceModelSize, ModelSize setupModelSize)
        {
            context.Database.EnsureCreated();

            // Look for any Entrys.
            if (context.Articles.Any())
                return;   // DB has been seeded
            
            // requires Tools and Resources
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.Init(context);

            // Resource Definitions
            MasterTableResourceTool resourceTools;

            switch (setupModelSize)
            {
                case ModelSize.Small:
                    resourceTools = new MasterTableResourceTool(resourceCapabilities, 2,2,2);
                    resourceTools.Init(context);
                    break;
                case ModelSize.Medium:
                    resourceTools = new MasterTableResourceTool(resourceCapabilities, 4,4,7);
                    resourceTools.Init(context);
                    break;
                case ModelSize.Large:
                    resourceTools = new MasterTableResourceTool(resourceCapabilities, 8,8,14);
                    resourceTools.Init(context);
                    break;
                default: throw new ArgumentException();
            }

            // requires Tools

            // requires Tools, Resources, and Capabilities
            var resource = new MasterTableResource(resourceCapabilities);

            switch (resourceModelSize)
                {
                    case ModelSize.Small: 
                        resource.InitSmall(context);
                        break;
                case ModelSize.Medium:
                        resource.InitMedium(context);
                        break;
                    case ModelSize.Large:
                        resource.InitLarge(context);
                        break;
                    default: throw new ArgumentException();
                }

            var resourceSetupsMedium = new MasterTableResourceSetup();
            resourceSetupsMedium.InitCustom(context, resource, resourceTools, resourceCapabilities);

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