using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer.Tables;
using Mate.DataCore.DataModel;
using Seed.Data;
using Seed.Generator.Material;
using Seed.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.DataCore.Data.Seed
{
    public class MaterialTransformer
    {
        public static string BUSINESS_PARTNER_1 { get; private set; } = "BUSINESS_PARTNER_1";

        public static void Transform(MateDb mateDb, Materials materials, List<M_ResourceCapability> capabilities)
        {
            // Type and Units
            var articleTypeTable = new MasterTableArticleType();
            articleTypeTable.Init(mateDb);
            var units = new MasterTableUnit().Init(mateDb);

            // Business Partner
            var businessPartners = CreateBusinessPartners(mateDb);

            // Articles
            var articles = new List<M_Article>();
            var operations = new List<M_Operation>();
            // Purchase
            foreach(var material in materials.NodesPurchaseOnly())
            {
                var article = CreateArticleByMaterial(mateDb, material, articleTypeTable.MATERIAL, units.Single(x => x.Name.Equals("Pieces")));
                articles.Add(article);
            }
            // Sales
            foreach(var sale in materials.NodesSalesOnly())
            {
                var article = CreateArticleByMaterial(mateDb, sale, articleTypeTable.PRODUCT, units.Single(x => x.Name.Equals("Pieces")));
                articles.Add(article);
            }
            // Assembly
            foreach (var sale in materials.NodesInUse.Where(x => x.OutgoingEdges.Count > 0 && x.IncomingEdges.Count > 0 ))
            {
                var article = CreateArticleByMaterial(mateDb, sale, articleTypeTable.ASSEMBLY, units.Single(x => x.Name.Equals("Pieces")));
                articles.Add(article);
            }

            mateDb.Articles.AddRange(articles);
            mateDb.SaveChanges();

            //Operations
            foreach (var article in articles.Where(x => x.ArticleType.Name.Equals("Material")))
            {
                //var materialnode = materials.NodesInUse.Single(x => x.Id.Equals(article.Name));
                //var operationFromArticle = CreateOperations(mateDb, article, materialnode, capabilities);
                
            }

            mateDb.Operations.AddRange(operations);
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

        private static M_Article CreateArticleByMaterial(MateDb context, MaterialNode material, M_ArticleType articleType, M_Unit unit)
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
        private static M_Operation[] CreateOperations(MateDb context, M_Article article, MaterialNode materialNode, List<M_ResourceCapability> resourceCapabilities)
        {
            var operations = new List<M_Operation>();
            foreach(var operation in materialNode.Operations)
            { 
                operations.Add(
                    new M_Operation
                    {
                        ArticleId = article.Id,
                        Name = article.Name,
                        Duration = (int)operation.Duration.TotalMinutes,
                        ResourceCapabilityId = operation.TargetToolIdent,
                        HierarchyNumber = operation.SequenceNumber,
                    });
            }
            return operations.ToArray();
        }


        private static M_ArticleBom[] CreateBomBy(M_Article article, M_Article[] articles,
            M_Operation operation)
        {
            var boms = new List<M_ArticleBom>();

            foreach (var item in articles)
            {
                boms.Add(
                new M_ArticleBom
                {
                    ArticleChildId = item.Id,
                    Name = item.Name,
                    Quantity = 1,
                    ArticleParentId = article.Id,
                    OperationId = operation.Id
                });
            }
            return boms.ToArray();

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
