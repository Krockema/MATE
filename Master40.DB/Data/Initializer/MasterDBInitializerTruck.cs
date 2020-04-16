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
        public static void DbInitialize(MasterDBContext context, ModelSize resourceModelSize, ModelSize setupModelSize, bool distributeSetupsExponentially = false)
        {
            context.Database.EnsureCreated();

            // Look for any Entrys.
            if (context.Articles.Any())
                return;   // DB has been seeded
            
            // requires Tools and Resources
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.InitBasicCapabilities(context);
            switch (setupModelSize)
            {
                case ModelSize.Small:
                    resourceCapabilities.CreateToolingCapabilities(context, 2, 2, 2);
                    break;
                case ModelSize.Medium:
                    resourceCapabilities.CreateToolingCapabilities(context, 4, 4, 7);
                    break;
                case ModelSize.Large:
                    resourceCapabilities.CreateToolingCapabilities(context, 8, 8, 14);
                    break;
                case ModelSize.TestModel:
                    resourceCapabilities.CreateToolingCapabilities(context, 4, 4, 7);
                    break;
                default: throw new ArgumentException();
            }
            
            var resources = new MasterTableResource(resourceCapabilities);
            switch (resourceModelSize)
            {
                case ModelSize.Small:
                    resources.InitSmall(context);
                    break;
                case ModelSize.Medium:
                    resources.InitMedium(context);

                    break;
                case ModelSize.Large:
                    resources.InitLarge(context);
                    break;
                case ModelSize.XLarge:
                    resources.InitXLarge(context);
                    break;
                case ModelSize.TestModel:
                    resources.InitMediumTest(context);
                    break;
                default: throw new ArgumentException();
            }

            resources.CreateResourceTools(setupTimeCutting: 10, setupTimeDrilling: 15, setupTimeAssembling: 20);
            resources.SaveToDB(context);

            // Article Definitions
            var units = new MasterTableUnit();
                units.Init(context);
            var articleTypes = new MasterTableArticleType();
                articleTypes.Init(context);

                // requires Units and Article Types 
            var articleTable = new MasterTableArticle(articleTypes, units);
            var articles = articleTable.Init(context);

            MasterTableStock.Init(context, articles);

            var operations = new MasterTableOperation(articleTable, resourceCapabilities, distributeSetupsExponentially);
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