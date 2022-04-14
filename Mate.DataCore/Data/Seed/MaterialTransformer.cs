using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer.StoredProcedures;
using Mate.DataCore.Data.Initializer.Tables;
using Mate.DataCore.DataModel;
using Seed.Data;
using Seed.Generator.Material;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.DataCore.Data.Seed
{
    public class MaterialTransformer
    {
        public static string BUSINESS_PARTNER_1 { get; private set; } = "BUSINESS_PARTNER_1";

        public static void Transform(MateDb mateDb, Materials materials, DynamicInitializer.Tables.MasterTableResourceCapability masterTableCapabilities)
        {
            //Add Procedure
            //TODO: Should be initialized because its required for simulation
            ArticleStatistics.CreateProcedures(mateDb);

            // Type and Units
            var articleTypeTable = new MasterTableArticleType();
            articleTypeTable.Init(mateDb);
            var units = new MasterTableUnit().Init(mateDb);

            // Business Partner
            var businessPartners = CreateBusinessPartners(mateDb);

            // Articles
            var articles = new List<M_Article>();

            foreach(var material in materials.NodesInUse)
            {
                // Purchase // Default
                M_ArticleType materialtype = articleTypeTable.MATERIAL;

                // Assembly // In and Out
                if (material.IncomingEdgeIds.Any() && material.OutgoingEdgeIds.Any())
                    materialtype = articleTypeTable.ASSEMBLY;
                
                // Sales only Out
                if (material.IncomingEdgeIds.Any() && !material.OutgoingEdgeIds.Any())
                    materialtype = articleTypeTable.PRODUCT;

                var article = CreateArticleByMaterial(material, materialtype, units.Single(x => x.Name.Equals("Pieces")));
                articles.Add(article);
            }

            mateDb.Articles.AddRange(articles);
            mateDb.SaveChanges();

            //Operations

            var operations = new List<M_Operation>();
            foreach (var article in articles.Where(x => !x.ArticleType.Name.Equals("Material")))
            {
                var materialnode = materials.NodesInUse.Single(x => x.Id.ToString().Equals(article.Name));
                var operationFromArticle = CreateOperations(article, materialnode, masterTableCapabilities);
                operations.AddRange(operationFromArticle);
            }

            mateDb.Operations.AddRange(operations);
            mateDb.SaveChanges();


            //BOM
            var boms = new List<M_ArticleBom>();
            foreach (var edge in materials.Edges)
            {
                var articleFrom = articles.Single(x => x.Name.Equals(edge.FromId.ToString()));
                var articleTo = articles.Single(x => x.Name.Equals(edge.ToId.ToString()));
                var operationId = operations.Where(x => x.ArticleId.Equals(articleTo.Id)).OrderBy(x => x.HierarchyNumber).First();
                //TODO: Check if operations are required for M_Bom
                var bom = CreateBOM(articleFrom, articleTo, operationId.Id);
                boms.Add(bom);
            }
            mateDb.ArticleBoms.AddRange(boms);
            mateDb.SaveChanges();

            CreateStocks(mateDb, businessPartners.First()); ;

        }

        private static M_BusinessPartner[] CreateBusinessPartners(MateDb mateDb)
        {
            var businessPartners = new M_BusinessPartner[]
            {
                new M_BusinessPartner() {Debitor = true, Kreditor = false, Name = BUSINESS_PARTNER_1}
            };
            mateDb.BusinessPartners.AddRange(businessPartners);
            mateDb.SaveChanges();
            return businessPartners;
        }

        private static M_Article CreateArticleByMaterial(MaterialNode material, M_ArticleType articleType, M_Unit unit)
        {
            return new M_Article
            {
                Name = material.Id.ToString(),
                ArticleTypeId = articleType.Id,
                CreationDate = DateTime.Now,
                DeliveryPeriod = 20,
                UnitId = unit.Id,
                Price = material.Cost,
                ToBuild = material.Operations.Count > 0 ? true : false,
                ToPurchase = material.Operations.Count == 0 ? true : false
            };
        }
        private static M_Operation[] CreateOperations(M_Article article, MaterialNode materialNode, DynamicInitializer.Tables.MasterTableResourceCapability masterTableCapabilities)
        {
            
            var operations = new List<M_Operation>();
            foreach (var operation in materialNode.Operations)
            {
                //TODO: Should be refactored at the dynamic initilizer - currently "working"
                var group = masterTableCapabilities.ParentCapabilities[operation.TargetResourceIdent];
                var capability = group.ChildResourceCapabilities.ElementAt(operation.TargetToolIdent);

                operations.Add(
                    new M_Operation
                    {
                        ArticleId = article.Id,
                        Name = article.Name,
                        Duration = (int)Math.Round(operation.Duration.TotalMinutes, 0, MidpointRounding.AwayFromZero),
                        ResourceCapabilityId = capability.Id,
                        HierarchyNumber = operation.SequenceNumber,
                    });
            }

            return operations.ToArray();
        }


        private static M_ArticleBom CreateBOM(M_Article articleFrom, M_Article articlesTo, int operationId)
        {
            return new M_ArticleBom
                {
                    ArticleChildId = articleFrom.Id,
                    OperationId = operationId,
                    Name = articleFrom.Name,
                    Quantity = 1,
                    ArticleParentId = articlesTo.Id
                };

        }

        private static void CreateStocks(MateDb mateDb, M_BusinessPartner businessPartners)
        {
            // get the name -> id mappings
            var dbArticles = mateDb.Articles.ToList();

            // create Stock and Businesspartner entrys for each Article
            var stocks = new List<M_Stock>();
            var articleToBusinessPartners = new List<M_ArticleToBusinessPartner>();
            foreach (var article in dbArticles)
            {
                stocks.Add(
                    new M_Stock
                    {
                        ArticleForeignKey = article.Id,
                        Name = "Stock: " + article.Name,
                        Min = article.ToPurchase ? 500 : 0,
                        Max = 10000,
                        Current = article.ToPurchase ? 1000 : 0,
                        StartValue = article.ToPurchase ? 1000 : 0,
                    });
                articleToBusinessPartners.Add(
                    new M_ArticleToBusinessPartner
                    {
                        BusinessPartnerId = businessPartners.Id,
                        ArticleId = article.Id,
                        PackSize = 1000,
                        Price = 1000 * article.Price,
                        TimeToDelivery = 1440
                    });
            }
            mateDb.Stocks.AddRange(stocks);
            mateDb.ArticleToBusinessPartners.AddRange(articleToBusinessPartners);
            mateDb.SaveChanges();
        }


        

    }
}
