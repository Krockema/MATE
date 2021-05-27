namespace Mate.DataCore.Data.Initializer
{
    public class MasterDBInitializerDesk
    {
        /*
        private const string RESOURCE_CAPABILITY_WELDING = "Schweißen";
        private const string RESOURCE_CAPABILITY_ASSEMBLING = "Montage";
        private const string RESOURCE_CAPABILITY_PACKING = "Verpacken";
        private const string RESOURCE_TOOL_WELDER = "Schweißgerät";
        private const string RESOURCE_WRAPPER = "Verpackungseinhheit 1";
        private const string RESOURCE_ASSEMBLY_1 = "Montage 1";
        private const string RESOURCE_ASSEMBLY_2 = "Montage 2";
        private const string RESOURCE_WELDING_1 = "Schweißen 1";
        private const string RESOURCE_WELDING_2 = "Schweißen 2";

        private const string OPERATION_DESK = "Tisch zusammenbauen";
        private const string OPERATION_DESK_LEG_1 = "Anschraubplatte anschweißen";
        private const string OPERATION_DESK_LEG_2 = "Filzgleiter anstecken";

        private const string BUSINESS_PARTNER_DESK_DISTRIBUTOR = "Tischverkäufer";
        private const string BUSINESS_PARTNER_WHOLESALE = "Teile Großhandel";

        private const string ARTICLE_DESK = "Tisch";
        private const string ARTICLE_DESK_SURFACE = "Tischplatte";
        private const string ARTICLE_DESK_LEG = "Tischbein";
        private const string ARTICLE_MOUNTING_PLATE = "Anschraubplatte";
        private const string ARTICLE_SCREWS = "Schrauben";
        private const string ARTICLE_STEEL_PIPE = "Stahlrohr";
        private const string ARTICLE_FELT_GLIDERS = "Filzgleiter";

        public static void DbInitialize(ProductionDomainContext productionDomainContext)
        {
            productionDomainContext.Database.EnsureCreated();

            // Article Types
            var articleTypes = CreateArticleTypes();
            productionDomainContext.ArticleTypes.AddRange(articleTypes);
            productionDomainContext.SaveChanges();

            // Units
            var units = CreateUnits();
            productionDomainContext.Units.AddRange(units);
            productionDomainContext.SaveChanges();


            // resource Tools
            var resourceTools = CreateResourceTools();
            productionDomainContext.ResourceTools.AddRange(resourceTools);
            productionDomainContext.SaveChanges();

            // resources
            var resources = CreateResources();
            productionDomainContext.Resources.AddRange(resources);
            productionDomainContext.SaveChanges();

            var resourceCapabilities = CreateResourceCapabilities();
            productionDomainContext.ResourceCapabilities.AddRange(resourceCapabilities);
            productionDomainContext.SaveChanges();


            // resource Setups 
            var resourceSetups = CreateResourceSetups(resourceTools, resources, resourceCapabilities);
            productionDomainContext.ResourceSetups.AddRange(resourceSetups);
            productionDomainContext.SaveChanges();

            // Articles
            var articles = CreateArticles(articleTypes, units);
            productionDomainContext.Articles.AddRange(articles);
            productionDomainContext.SaveChanges();

            // get the name -> id mappings
            var dbArticles = productionDomainContext.Articles.ToDictionary(p => p.Name, p => p.Id);

            // create Stock entrys for each Article
            foreach (var article in dbArticles)
            {
                var stocks = new M_Stock[]
                {
                    new M_Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == ARTICLE_SCREWS) ? 50 : 0,
                        Max = 1000,
                        Current = (article.Key == ARTICLE_SCREWS) ? 100 : 0
                    }
                };
                productionDomainContext.Stocks.AddRange(stocks);
                productionDomainContext.SaveChanges();
            }

            var operations = CreateOperations(resourceTools, articles, resourceCapabilities);
            productionDomainContext.Operations.AddRange(operations);
            productionDomainContext.SaveChanges();

            var articleBom = CreateArticleBoms(articles, operations);
            productionDomainContext.ArticleBoms.AddRange(articleBom);
            productionDomainContext.SaveChanges();

            var businessPartners = CreateBusinessPartners();
            productionDomainContext.BusinessPartners.AddRange(businessPartners);
            productionDomainContext.SaveChanges();

            var articleToBusinessPartner = CreateArticleToBusinessPartners(articles, businessPartners);
            productionDomainContext.ArticleToBusinessPartners.AddRange(articleToBusinessPartner);
            productionDomainContext.SaveChanges();
        }

        private static M_BusinessPartner[] CreateBusinessPartners()
        {
            return new M_BusinessPartner[]
            {
                new M_BusinessPartner()
                    {Debitor = true, Kreditor = false, Name = BUSINESS_PARTNER_DESK_DISTRIBUTOR},
                new M_BusinessPartner()
                    {Debitor = false, Kreditor = true, Name = BUSINESS_PARTNER_WHOLESALE}
            };
        }

        private static M_ResourceTool[] CreateResourceTools()
        {
            return new M_ResourceTool[]
            {
                new M_ResourceTool
                {
                    Name = RESOURCE_TOOL_WELDER
                }
            };
        }

        private static M_ResourceCapability[] CreateResourceCapabilities()
        {
            return new M_ResourceCapability[]
            {
                new M_ResourceCapability {Name = RESOURCE_CAPABILITY_ASSEMBLING},
                new M_ResourceCapability {Name = RESOURCE_CAPABILITY_WELDING},
                new M_ResourceCapability {Name = RESOURCE_CAPABILITY_PACKING}
            };
        }

        private static M_ResourceSetup[] CreateResourceSetups(M_ResourceTool[] resourceTools
            , M_Resource[] resources
            , M_ResourceCapability[] resourceCapabilities)
        {

            var resource1 = new M_ResourceSetup
            {
                Name = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_PACKING).Name + " Setup",
                ResourceId = resources.Single(x => x.Name == RESOURCE_WRAPPER).Id,
                ResourceToolId = resourceTools.Single(x => x.Name == RESOURCE_TOOL_WELDER).Id,
                ResourceCapabilityId = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_PACKING).Id
            };
            var resource2 = new M_ResourceSetup
            {
                Name = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_ASSEMBLING).Name + " Setup",
                ResourceId = resources.Single(x => x.Name == RESOURCE_ASSEMBLY_1).Id,
                ResourceToolId = resourceTools.Single(x => x.Name == RESOURCE_TOOL_WELDER).Id,
                ResourceCapabilityId = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_ASSEMBLING).Id
            };
            var resource3 = new M_ResourceSetup
            {
                Name = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_ASSEMBLING).Name + " Setup",
                ResourceId = resources.Single(x => x.Name == RESOURCE_ASSEMBLY_2).Id,
                ResourceToolId = resourceTools.Single(x => x.Name == RESOURCE_TOOL_WELDER).Id,
                ResourceCapabilityId = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_ASSEMBLING).Id
            };
            var resource4 = new M_ResourceSetup
            {
                Name = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_WELDING).Name + " Setup",
                ResourceId = resources.Single(x => x.Name == RESOURCE_WELDING_1).Id,
                ResourceToolId = resourceTools.Single(x => x.Name == RESOURCE_TOOL_WELDER).Id,
                ResourceCapabilityId = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_WELDING).Id
            };
            var resource5 = new M_ResourceSetup
            {
                Name = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_WELDING).Name + " Setup",
                ResourceId = resources.Single(x => x.Name == RESOURCE_WELDING_2).Id,
                ResourceToolId = resourceTools.Single(x => x.Name == RESOURCE_TOOL_WELDER).Id,
                ResourceCapabilityId = resourceCapabilities.Single(x => x.Name == RESOURCE_CAPABILITY_WELDING).Id
            };

            var resourceSetups = new M_ResourceSetup[] {resource1, resource2, resource3, resource4, resource5};
            return resourceSetups;
        }

        private static M_Unit[] CreateUnits()
        {
            return new M_Unit[]
            {
                new M_Unit {Name = "Kilo"},
                new M_Unit {Name = "Litre"},
                new M_Unit {Name = "Pieces"}
            };
        }

        private static M_ArticleType[] CreateArticleTypes()
        {
            return new M_ArticleType[]
            {
                new M_ArticleType {Name = "Product"},
                new M_ArticleType {Name = "Assembly"},
                new M_ArticleType {Name = "Material"},
                new M_ArticleType {Name = "Consumable"}
            };
        }

        private static M_Operation[] CreateOperations(M_ResourceTool[] resourceTools,
            M_Article[] articles, M_ResourceCapability[] resourceCapabilities)
        {
            // Tool has no meaning yet, ignore it and use always the same
            M_ResourceTool resourceTool = resourceTools.Single(a => a.Name == RESOURCE_TOOL_WELDER);
            return new M_Operation[]
            {
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    Name = OPERATION_DESK, Duration = 11,
                    ResourceCapabilityId = resourceCapabilities.Single(x => x.Name.Equals(RESOURCE_CAPABILITY_PACKING))
                        .Id,
                    HierarchyNumber = 10,
                    ResourceToolId = resourceTool.Id
                },
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    Name = OPERATION_DESK_LEG_1, Duration = 20,
                    ResourceCapabilityId = resourceCapabilities.Single(x => x.Name.Equals(RESOURCE_CAPABILITY_WELDING))
                        .Id,
                    HierarchyNumber = 10,
                    ResourceToolId = resourceTool.Id
                },
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    Name = OPERATION_DESK_LEG_2, Duration = 3,
                    ResourceCapabilityId = resourceCapabilities
                        .Single(x => x.Name.Equals(RESOURCE_CAPABILITY_ASSEMBLING)).Id,
                    HierarchyNumber = 20,
                    ResourceToolId = resourceTool.Id
                }
            };
        }

        private static M_Resource[] CreateResources()
        {
            return new M_Resource[]
            {
                // Verpacken
                new M_Resource
                {
                    Capacity = 1, Name = RESOURCE_WRAPPER, Count = 1,
                },
                // Schweißen
                new M_Resource
                {
                    Capacity = 1, Name = RESOURCE_WELDING_1, Count = 1,
                },
                new M_Resource
                {
                    Capacity = 1, Name = RESOURCE_WELDING_2, Count = 1,
                },
                // Montage der Beine an Tisch
                new M_Resource
                {
                    Capacity = 1, Name = RESOURCE_ASSEMBLY_1, Count = 1,
                },
                new M_Resource
                {
                    Capacity = 1, Name = RESOURCE_ASSEMBLY_2, Count = 1,
                }

            };
        }

        private static M_Article[] CreateArticles(M_ArticleType[] articleTypes, M_Unit[] units)
        {
            return new M_Article[]
            {
                // no prices except for articles that are sold
                new M_Article
                {
                    Name = ARTICLE_DESK,
                    ArticleTypeId = articleTypes.Single(s => s.Name == MasterTableArticle.ARTICLE_PRODUCTS).Id,
                    CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, Price = 100.00,
                    ToBuild = true, ToPurchase = false
                },
                new M_Article
                {
                    Name = ARTICLE_DESK_SURFACE,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    DeliveryPeriod = 10, UnitId = units.Single(s => s.Name == "Pieces").Id,
                    ToBuild = false, ToPurchase = true,
                    LotSize = 2
                },
                new M_Article
                {
                    Name = ARTICLE_DESK_LEG,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Assembly").Id,
                    DeliveryPeriod = 10, UnitId = units.Single(s => s.Name == "Pieces").Id,
                    ToBuild = true, ToPurchase = false,
                    LotSize = 8
                },
                new M_Article
                {
                    Name = ARTICLE_SCREWS,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 100
                },
                new M_Article
                {
                    Name = ARTICLE_MOUNTING_PLATE,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 10
                },
                new M_Article
                {
                    Name = ARTICLE_STEEL_PIPE,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 10
                },
                new M_Article
                {
                    Name = ARTICLE_FELT_GLIDERS,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 10
                },
            };
        }

        private static M_ArticleBom[] CreateArticleBoms(M_Article[] articles,
            M_Operation[] operations)
        {
            M_Operation operationDesk = operations.Single(x => x.Name.Equals(OPERATION_DESK));
            M_Operation operationLeg1 = operations.Single(x => x.Name.Equals(OPERATION_DESK_LEG_1));
            M_Operation operationLeg2 = operations.Single(x => x.Name.Equals(OPERATION_DESK_LEG_2));

            return new M_ArticleBom[]
            {
                // Tisch
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_DESK_SURFACE).Id,
                    Name = ARTICLE_DESK_SURFACE,
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    OperationId = operationDesk.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    Name = ARTICLE_DESK_LEG, Quantity = 4,
                    ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    OperationId = operationDesk.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_SCREWS).Id,
                    Name = ARTICLE_SCREWS, Quantity = 16,
                    ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    OperationId = operationDesk.Id
                },

                // Tischbein
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_MOUNTING_PLATE).Id,
                    Name = ARTICLE_MOUNTING_PLATE,
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    OperationId = operationLeg1.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_STEEL_PIPE).Id,
                    Name = ARTICLE_STEEL_PIPE,
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    OperationId = operationLeg1.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_FELT_GLIDERS).Id,
                    Name = ARTICLE_FELT_GLIDERS,
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    OperationId = operationLeg2.Id
                },
            };
        }

        private static M_ArticleToBusinessPartner[] CreateArticleToBusinessPartners(
            M_Article[] articles, M_BusinessPartner[] businessPartners)
        {
            M_BusinessPartner businessPartnerWholeSale =
                businessPartners.Single(x => x.Name.Equals(BUSINESS_PARTNER_WHOLESALE));
            return new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_DESK_SURFACE).Id, PackSize = 1,
                    Price = 20,
                    TimeToDelivery = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_SCREWS).Id, PackSize = 100,
                    Price = 5,
                    TimeToDelivery = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_MOUNTING_PLATE).Id, PackSize = 10,
                    Price = 10,
                    TimeToDelivery = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_STEEL_PIPE).Id, PackSize = 10,
                    Price = 20,
                    TimeToDelivery = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_FELT_GLIDERS).Id, PackSize = 10,
                    Price = 2,
                    TimeToDelivery = 100
                },
            };
        }
        */
    }
}