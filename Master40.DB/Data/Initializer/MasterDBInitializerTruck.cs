using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.Nominal.Model;
using Microsoft.EntityFrameworkCore;
using static Master40.DB.Data.Initializer.ResourceInitializer;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerTruck
    {
        public static void DbInitialize(MasterDBContext context, ModelSize resourceModelSize, ModelSize setupModelSize, ModelSize operatorsModelSize, int numberOfWorkersForProcessing, bool secondResource, bool distributeSetupsExponentially = false)
        {
            context.Database.EnsureCreated();

            // Look for any Entrys.
            if (context.Articles.Any())
                return;   // DB has been seeded
            
            var resourceCapabilities = MasterTableResourceCapability(context, resourceModelSize, setupModelSize, operatorsModelSize, numberOfWorkersForProcessing, secondResource);

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
                
            context.SaveChanges();
            
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

            // Extension for QUality Assurance
            var characteristics = new MasterTableCharacteristic(articleTable, operations);
            characteristics.Init(context);
            var valueTypes = new MasterTableValueType();
            valueTypes.Init(context);
            var attributes = new MasterTableAttribute();
            attributes.Init(context, characteristics, valueTypes);
        }
    }
}