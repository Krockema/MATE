using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.DynamicInitializer;
using Mate.DataCore.Data.Initializer.StoredProcedures;
using Mate.DataCore.Data.Initializer.Tables;
using Mate.DataCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mate.DataCore.Data.Initializer
{
    public class MasterDBInitializerShoes
    {
        private const string BUSINESS_PARTNER_SPORTSCHARK = "Sportschark";
        private const string BUSINESS_PARTNER_WHOLESALE = "Wholesale";
        private static Random RANDOM = new Random(29);

        private static Dictionary<string, List<string>> shoes = new()
        {
            { "Soccers", new List<string>{ "COPA", "LUNA"} },
            { "Runers", new List<string> { "COPA", "Ultraboost" } },
            { "Works", new List<string> { "NINJA", "RESPONSE" } },
            { "Adilette", new List<string> { "ORIGINAL"} },
        };

        private static int[] Sizes = new int[] { 29, 30, 31, 32, 33 };
        private static Dictionary<string, double> ShoeType = new()
        {
            { "Soccer", 120.0},
            { "Run", 110.0 },
            { "Work", 80.0 },
            { "Adilette", 45.0 }
        };

      
        // Sockliner // 
        private static Dictionary<string, double> SocklinerType = new()
        {
            { "Sockliner: Lether", 15.0 },
            { "Sockliner: Softshell", 3.0},
            { "Sockliner: Cotton", 1.0 }
        };

        // Cover // 
        private static Dictionary<string, double> CoverType = new()
        {
            { "Cover: PU-Grid", 15.0 },
            { "Cover: PU-Lether", 3.0 },
        };

        // Outsole
        private static Dictionary<string, double> OuterSoleType = new()
        {
            { "Ribbon Sole", 5.0 },
            { "Spike Sole", 10.0 },
            { "Rubber Sole", 3.0 }
        };
        // Laces
        private static Dictionary<string, double> laceTypes = new()
        {
            { "Velcro", 0.10 },
            { "Elastic", 0.50 },
            { "Cords", 2.0 }
        };

        private static Dictionary<string, double> otherPurchases = new()
        {
            { "Glue", 0.10 },
            { "Box Lable", 0.50 },
            { "Package", 2.0 }
        };
        
        public static void DbInitialize(MateDb mateDb)
        {
            // reset context
            mateDb.Database.EnsureDeleted();
            mateDb.Database.EnsureCreated();

            ArticleStatistics.CreateProcedures(mateDb);
            // create resources
            var resourceProps = new List<ResourceProperty>();
            resourceProps.Add(new ResourceProperty { Name = "Heat Press", ToolCount = 6, ResourceCount = 2, SetupTime = TimeSpan.FromMinutes(5), OperatorCount = 0, IsBatchAble = false });
            resourceProps.Add(new ResourceProperty { Name = "Desma", ToolCount = 4, ResourceCount = 3, SetupTime = TimeSpan.FromMinutes(2), OperatorCount = 0, IsBatchAble = true, BatchSize = 20 });
            resourceProps.Add(new ResourceProperty { Name = "Wrapper", ToolCount = 4, ResourceCount = 1, SetupTime = TimeSpan.FromMinutes(2), OperatorCount = 0 , IsBatchAble = false });
            var capabilities = DynamicInitializer.ResourceInitializer.Initialize(mateDb, resourceProps);

            // Type and Units
            var articleTypeTable = new MasterTableArticleType();
            articleTypeTable.Init(mateDb);
            var units = new MasterTableUnit().Init(mateDb);

            // Business Partner
            var businessPartners = CreateBusinessPartners(mateDb);

            // Articles
            var purchase = CreatePurchaseArticleByType(mateDb, articleTypeTable.MATERIAL, units, otherPurchases);
            var laces = CreatePurchaseArticleByType(mateDb, articleTypeTable.MATERIAL, units, laceTypes);
            var outer_soles = CreatePurchaseArticleByType(mateDb, articleTypeTable.MATERIAL, units, OuterSoleType);
            var sockliner = CreatePurchaseArticleByType(mateDb, articleTypeTable.MATERIAL, units, SocklinerType);
            var cover = CreatePurchaseArticleByType(mateDb, articleTypeTable.MATERIAL, units, CoverType);

            // create Assemblies
            var upperShoes = CreateShoes(mateDb, "Upper" ,cover, sockliner, articleTypeTable.ASSEMBLY, units, capabilities);
            var shoes = CreateShoes(mateDb, "Shoe", upperShoes, outer_soles, articleTypeTable.ASSEMBLY, units, capabilities);
            var boxed = CreateShoes(mateDb, "Boxed", laces, shoes, articleTypeTable.PRODUCT, units, capabilities);
            CreateStockAndArticleToBusinessPartners(mateDb, businessPartners);
        }

        public static M_Article[] CreateShoes(MateDb context, string type,  M_Article[] covers, M_Article[] socks
            , M_ArticleType articleType, M_Unit[] units, DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            var upperShoes = new List<M_Article>();
            foreach (var cover in covers)
                foreach (var sock in socks)
                {
                    var prefix = type == "Boxed" ? GenerateShoeName() : type;
                    var article = new M_Article
                    {
                        Name = prefix + cover.Name + " " + sock.Name,
                        ArticleTypeId = articleType.Id,
                        CreationDate = DateTime.Now,
                        DeliveryPeriod = TimeSpan.FromMinutes(20),
                        UnitId = units.Single(s => s.Name == "Pieces").Id,
                        Price = (cover.Price + sock.Price) * 1.5,
                        ToBuild = true,
                        ToPurchase = false,
                        PictureUrl = type == "Boxed" ? "/images/Product/Shoe.jpeg" : ""
                    };
                    upperShoes.Add(article);
                    context.Articles.Add(article);
                    context.SaveChanges();
                    var upperShoeOperations = CreateOperationByType(context, article, type ,resourceCapabilities);
                    var bom = CreateBomBy(article, new[] { cover, sock }, upperShoeOperations.First());
                    context.ArticleBoms.AddRange(bom);
                    context.SaveChanges();
                }
            return upperShoes.ToArray();
        }

        private static string GenerateShoeName()
        {
                var shoe = RANDOM.Next(0, ShoeType.Keys.Count);
                var primaryModel = shoes.ToArray()[shoe];
                var ty = shoes.ToArray()[shoe].Value;
                var shoe2 = RANDOM.Next(0, ty.Count);
                var secondaryModel = ty[shoe2];
                return primaryModel.Key + " " + secondaryModel + " | with : ";
        }

        public static M_Operation[] CreateOperationByType(MateDb context, M_Article article, string stage
            , DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            if (stage == "Upper")
               return CreateUpperShoeOperations(context, article, resourceCapabilities);
            if (stage == "Shoe")
               return CreateShoeOperations(context, article, resourceCapabilities);
            if (stage == "Boxed")
                return CreatePackingOperations(context, article, resourceCapabilities);
            throw new Exception();
        }

        private static M_Operation[] CreatePackingOperations(MateDb context, M_Article articles
            , DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            var operations = new List<M_Operation>();
            operations.Add(
                new M_Operation
                {
                    ArticleId = articles.Id,
                    Name = "Pack",
                    Duration = TimeSpan.FromMinutes(3),
                    ResourceCapabilityId = RandomizedResourceCapabvilityIdByType("Wrapper", resourceCapabilities),
                    HierarchyNumber = 10,
                });
            context.Operations.AddRange(operations);
            context.SaveChanges();
            return operations.ToArray();
        }

        private static int RandomizedResourceCapabvilityIdByType(string type, DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            var capability = resourceCapabilities.Capabilities.Single(x => x.Name.Contains(type)  && x.ParentResourceCapabilityId == null);
            var getRandomOne = capability.ChildResourceCapabilities.ToArray()[RANDOM.Next(0, capability.ChildResourceCapabilities.Count)];
            return getRandomOne.Id;
        }

        private static M_Operation[] CreateUpperShoeOperations(MateDb context, M_Article articles
            , DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            var operations = new List<M_Operation>();
                operations.Add(
                    new M_Operation {
                        ArticleId = articles.Id,
                        Name = "Stretch and apply surface",
                        Duration = TimeSpan.FromMinutes(5),
                        ResourceCapabilityId = RandomizedResourceCapabvilityIdByType("Heat Press", resourceCapabilities),
                        HierarchyNumber = 10,
                    });
                operations.Add(
                    new M_Operation {
                        ArticleId = articles.Id,
                        Name = "Apply glue",
                        Duration = TimeSpan.FromMinutes(2),
                        ResourceCapabilityId = RandomizedResourceCapabvilityIdByType("Heat Press", resourceCapabilities),
                        HierarchyNumber = 20,
                    });
                operations.Add(
                    new M_Operation {
                        ArticleId = articles.Id,
                        Name = "Trimm",
                        Duration = TimeSpan.FromMinutes(3),
                        ResourceCapabilityId = RandomizedResourceCapabvilityIdByType("Heat Press", resourceCapabilities),
                        HierarchyNumber = 30,
                    });
                
            context.Operations.AddRange(operations);
            context.SaveChanges();
            return operations.ToArray();
        }

        private static M_Operation[] CreateShoeOperations(MateDb context, M_Article articles
                , DynamicInitializer.Tables.MasterTableResourceCapability resourceCapabilities)
        {
            var operations = new List<M_Operation>();
            operations.Add(
                new M_Operation
                {
                    ArticleId = articles.Id,
                    Name = "Injection Process",
                    Duration = TimeSpan.FromMinutes(20),
                    ResourceCapabilityId = RandomizedResourceCapabvilityIdByType("Desma", resourceCapabilities),
                    HierarchyNumber = 10,
                });
            context.Operations.AddRange(operations);
            context.SaveChanges();
            return operations.ToArray();
        }
        private static M_Article[] CreatePurchaseArticleByType(MateDb context, M_ArticleType articleType, M_Unit[] units, Dictionary<string, double> types)
        {
            var purchaseArticles = new List<M_Article>();
            foreach (var item in types)
            {
                purchaseArticles.Add(new M_Article
                {
                    Name = item.Key,
                    ArticleTypeId = articleType.Id,
                    CreationDate = DateTime.Now,
                    DeliveryPeriod = TimeSpan.FromMinutes(20),
                    UnitId = units.Single(s => s.Name == "Pieces").Id,
                    Price = item.Value,
                    ToBuild = false,
                    ToPurchase = true
                });
            }
            context.Articles.AddRange(purchaseArticles);
            context.SaveChanges();
            return purchaseArticles.ToArray();
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


        private static M_BusinessPartner[] CreateBusinessPartners(MateDb context)
        {
            var businessPartners = new M_BusinessPartner[]
            {
                new M_BusinessPartner() {Debitor = true, Kreditor = false, Name = BUSINESS_PARTNER_SPORTSCHARK},
                new M_BusinessPartner() {Debitor = false, Kreditor = true, Name = BUSINESS_PARTNER_WHOLESALE}
            };
            context.BusinessPartners.AddRange(businessPartners);
            context.SaveChanges();
            return businessPartners;
        }

        private static void CreateStockAndArticleToBusinessPartners(
            MateDb mateDb, M_BusinessPartner[] businessPartners)
        {
            // get the name -> id mappings
            var dbArticles = mateDb.Articles.ToList();
            M_BusinessPartner businessPartnerWholeSale = businessPartners.Single(x => x.Name.Equals(BUSINESS_PARTNER_WHOLESALE));
            
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
                        Min = article.ToPurchase  ? 500 : 0,
                        Max = 10000,
                        Current = article.ToPurchase ? 1000 : 0,
                        StartValue = article.ToPurchase ? 1000 : 0,
                    });
                articleToBusinessPartners.Add(
                    new M_ArticleToBusinessPartner
                    {
                        BusinessPartnerId = businessPartnerWholeSale.Id,
                        ArticleId = article.Id,
                        PackSize = 1000,
                        Price = 1000 * article.Price,
                        TimeToDelivery = TimeSpan.FromMinutes(1440)
                    });
            }
            mateDb.Stocks.AddRange(stocks);
            mateDb.ArticleToBusinessPartners.AddRange(articleToBusinessPartners);
            mateDb.SaveChanges();
        } 
    }
}