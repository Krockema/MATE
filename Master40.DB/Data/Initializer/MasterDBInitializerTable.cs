using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDbInitializerTable
    {
        private const string RESCOURCESKILL_CUT = "CuttingSkill";
        private const string RESCOURCESKILL_DRILL = "DrillSkill";
        private const string RESCOURCESKILL_ASSEBMLY_SCREW = "AssemblyUnitScrewSkill";
        

        private const string RESCOURCETOOL_SAWBIG = "Saw blade big";
        private const string RESCOURCETOOL_SAWSMALL = "Saw blade small";
        private const string RESCOURCETOOL_M6 = "M6 head";
        private const string RESCOURCETOOL_M4 = "M4 head";
        private const string RESCOURCETOOL_SCREWDRIVERCROSS2 = "Screwdriver universal cross size 2";

        private const string RESCOURCE_SAW1 = "Saw 1";
        private const string RESCOURCE_SAW2 = "Saw 2";
        private const string RESCOURCE_DRILL1 = "Drill 1";
        private const string RESCOURCE_ASSEMBLY1 = "AssemblyUnit 1";
        private const string RESCOURCE_ASSEMBLY2 = "AssemblyUnit 2";

        private const string RESCOURCESETUP_SAW1_SAWBIG = "Saw1_Sawbladebig";
        private const string RESCOURCESETUP_SAW2_SAWBIG = "Saw2_Sawbladebig";
        private const string RESCOURCESETUP_SAW2_SAWSMALL = "Saw2_Sawbladesmall";
        private const string RESCOURCESETUP_DRILL1_M6 = "Drill1_M6";
        private const string RESCOURCESETUP_DRILL1_M4 = "Drill1_M4";

        private const string RESCOURCESETUP_ASSEMBLY1_SCREW2 = "ASSEMBLY1_SCREW2";
        private const string RESCOURCESETUP_ASSEMBLY2_SCREW2 = "ASSEMBLY2_SCREW2";

        private const string ARTICLE_TABLE = "Tisch";
        private const string ARTICLE_TABLE_LEG = "Tischbein";
        private const string ARTICLE_TABLETOP = "Tischplatte";
        private const string ARTICLE_PEG = "Holzpflock 1,20m x 0,15m x 0,15m";
        private const string ARTICLE_WOOD_PANEL = "Holzplatte 1,5m x 3,0m x 0,03m";
        private const string ARTICLE_SCREW = "Schrauben";

        private const string OPERATION_ASSEMBLY_TABLE = "Tisch zusammenstellen";
        private const string OPERATION_SCREW_TOGETHER_TABLE = "Tisch verschrauben";
        private const string OPERATION_CUT_TABLE_LEG = "Tischbein saegen";
        private const string OPERATION_DRILL_TABLE_LEG = "Tischbein bohren";
        private const string OPERATION_CUT_TABLETOP = "Tischplatte saegen";
        private const string OPERATION_DRILL_TABLETOP = "Tischplatte bohren";


        public static void DbInitialize(MasterDBContext context)
        {
            context.Database.EnsureCreated();

            // Look for any entries
            if (context.Articles.Any())
            {
                return;   // DB has already been seeded
            }
            // article types
            var articleTypes = CreateArticleTypes();
            context.ArticleTypes.AddRange(entities: articleTypes);
            context.SaveChanges();

            // Units
            var units = CreateUnits();
            context.Units.AddRange(entities: units);
            context.SaveChanges();

            //ResourceSkills
            var resourceSkills = CreateResourceSkills();

            //ResourceTools
            var resourceTools = CreateResourceTools();
            context.ResourceTools.AddRange(entities: resourceTools);
            context.SaveChanges();

            //Resources
            var resources = CreateResources();
            context.Resources.AddRange(entities: resources);
            context.SaveChanges();

            //ResourceSetups
            var resourceSetups = CreateResourceSetups(resources: resources, resourceTools: resourceTools);

            // register resourceSetups at the resourceSkills
            resourceSkills = UpdateResourceSkills(resourceSkills: resourceSkills, resourceSetups: resourceSetups);
            context.ResourceSkills.AddRange(entities: resourceSkills);
            context.ResourceSetups.AddRange(entities: resourceSetups);
            context.SaveChanges();

            // Articles
            var articles = CreateArticles(articleTypes: articleTypes, units: units);
            context.Articles.AddRange(entities: articles);
            context.SaveChanges();

            // create stock entries for each article
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

            //Operations
            var operations = CreateOperations(articles: articles, resourceSkills: resourceSkills, resourceTools: resourceTools);
            context.Operations.AddRange(entities: operations);
            context.SaveChanges();

            var articleBom = CreateArticleBoms(articles: articles, operations: operations);
            context.ArticleBoms.AddRange(entities: articleBom);
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us toy department" };
            var businessPartner2 = new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
            context.BusinessPartners.Add(entity: businessPartner);
            context.BusinessPartners.Add(entity: businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = CreateArticleToBusinessPartners(businessPartner2: businessPartner2, articles: articles);
            context.ArticleToBusinessPartners.AddRange(entities: artToBusinessPartner);
            context.SaveChanges();

        }

        private static M_ArticleToBusinessPartner[] CreateArticleToBusinessPartners(M_BusinessPartner businessPartner2,
            M_Article[] articles)
        {
            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_TABLE).Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_TABLE_LEG).Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_TABLETOP).Id, PackSize = 500, Price = 0.05,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_SCREW).Id, PackSize = 50, Price = 2.50,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_PEG).Id, PackSize = 50, Price = 0.20,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner2.Id,
                    ArticleId = articles.Single(predicate: x => x.Name == ARTICLE_WOOD_PANEL).Id, PackSize = 50, Price = 0.20,
                    TimeToDelivery = 1440
                },
            };
            return artToBusinessPartner;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articles"></param>
        /// <param name="operations"></param>
        /// <returns></returns>
        private static List<M_ArticleBom> CreateArticleBoms(M_Article[] articles, M_Operation[] operations)
        {
            var articleBom = new List<M_ArticleBom>
            {
                // Final Product Tisch 
                new M_ArticleBom
                    {ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id, Name = ARTICLE_TABLE},

                // Bom For Tisch
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE_LEG).Id, Name = ARTICLE_TABLE_LEG,
                    Quantity = 4, ArticleParentId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id,
                    OperationId = operations.Single(predicate: x => x.Name == OPERATION_ASSEMBLY_TABLE).Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_TABLETOP).Id, Name = ARTICLE_TABLETOP,
                    Quantity = 1, ArticleParentId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id,
                    OperationId = operations.Single(predicate: x => x.Name == OPERATION_ASSEMBLY_TABLE).Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_SCREW).Id, Name = ARTICLE_SCREW,
                    Quantity = 8, ArticleParentId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id,
                    OperationId = operations.Single(predicate: x => x.Name == OPERATION_ASSEMBLY_TABLE).Id
                },

                // Bom For Tischplatte
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_WOOD_PANEL).Id,
                    Name = ARTICLE_WOOD_PANEL, Quantity = 1,
                    ArticleParentId = articles.Single(predicate: a => a.Name == ARTICLE_TABLETOP).Id,
                    OperationId = operations.Single(predicate: x => x.Name == OPERATION_CUT_TABLETOP).Id
                },

                // Bom For Tischbein
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(predicate: a => a.Name == ARTICLE_PEG).Id, Name = ARTICLE_PEG,
                    Quantity = 1, ArticleParentId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE_LEG).Id,
                    OperationId = operations.Single(predicate: x => x.Name == OPERATION_CUT_TABLE_LEG).Id
                },
            };
            return articleBom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articles"></param>
        /// <param name="resourceSkills"></param>
        /// <param name="resourceTools"></param>
        /// <returns></returns>
        private static M_Operation[] CreateOperations(M_Article[] articles, M_ResourceSkill[] resourceSkills,
            M_ResourceTool[] resourceTools)
        {
            var operations = new M_Operation[]
            {
                // Final Product Tisch
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id, Name = OPERATION_ASSEMBLY_TABLE,
                    Duration = 5, ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_ASSEBMLY_SCREW),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_SCREWDRIVERCROSS2),
                    HierarchyNumber = 10
                },
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE).Id,
                    Name = OPERATION_SCREW_TOGETHER_TABLE, Duration = 20,
                    ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_ASSEBMLY_SCREW),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_SCREWDRIVERCROSS2),
                    HierarchyNumber = 20
                },

                // Bom For Tischbein
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE_LEG).Id, Name = OPERATION_CUT_TABLE_LEG,
                    Duration = 10, ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_CUT),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_SAWSMALL), HierarchyNumber = 10
                },
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLE_LEG).Id,
                    Name = OPERATION_DRILL_TABLE_LEG, Duration = 5,
                    ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_DRILL),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_M6), HierarchyNumber = 20
                },

                // Bom For Tischplatte
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLETOP).Id, Name = OPERATION_CUT_TABLETOP,
                    Duration = 20, ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_CUT),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_SAWBIG), HierarchyNumber = 10
                },
                new M_Operation
                {
                    ArticleId = articles.Single(predicate: a => a.Name == ARTICLE_TABLETOP).Id, Name = OPERATION_DRILL_TABLETOP,
                    Duration = 5, ResourceSkill = resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_DRILL),
                    ResourceTool = resourceTools.Single(predicate: t => t.Name == RESCOURCETOOL_M6), HierarchyNumber = 20
                },
            };
            return operations;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleTypes"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        private static M_Article[] CreateArticles(M_ArticleType[] articleTypes, M_Unit[] units)
        {
            var articles = new M_Article[]
            {
                // Final Products
                new M_Article
                {
                    Name = ARTICLE_TABLE, ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Product").Id,
                    CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20,
                    UnitId = units.Single(predicate: s => s.Name == "Pieces").Id, Price = 25.00, ToPurchase = false,
                    ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"
                },

                // Intermediate Products
                new M_Article
                {
                    Name = ARTICLE_TABLE_LEG, ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Assembly").Id,
                    CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 5,
                    UnitId = units.Single(predicate: s => s.Name == "Pieces").Id, Price = 2.00, ToPurchase = false,
                    ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"
                },
                new M_Article
                {
                    Name = ARTICLE_TABLETOP, ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Assembly").Id,
                    CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 5,
                    UnitId = units.Single(predicate: s => s.Name == "Pieces").Id, Price = 10.00, ToPurchase = false,
                    ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"
                },

                // base Materials
                new M_Article
                {
                    Name = ARTICLE_WOOD_PANEL,
                    ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Material").Id,
                    CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5,
                    UnitId = units.Single(predicate: s => s.Name == "Pieces").Id, Price = 3.00, ToPurchase = true,
                    ToBuild = false
                },
                new M_Article
                {
                    Name = ARTICLE_PEG,
                    ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Material").Id,
                    CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5,
                    UnitId = units.Single(predicate: s => s.Name == "Pieces").Id, Price = 0.70, ToPurchase = true,
                    ToBuild = false
                },
                new M_Article
                {
                    Name = ARTICLE_SCREW, ArticleTypeId = articleTypes.Single(predicate: s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 4,
                    UnitId = units.Single(predicate: s => s.Name == "Kilo").Id, Price = 0.50, ToPurchase = true, ToBuild = false
                },
            };
            return articles;
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="resourceSkills"></param>
        /// <param name="resourceSetups"></param>
        /// <returns></returns>
        private static M_ResourceSkill[] UpdateResourceSkills(M_ResourceSkill[] resourceSkills, M_ResourceSetup[] resourceSetups)
        {
            var _resourceSkills = resourceSkills;
            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_CUT).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_SAW1_SAWBIG));
            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_CUT).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_SAW2_SAWBIG));
            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_CUT).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_SAW2_SAWSMALL));

            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_DRILL).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_DRILL1_M4));
            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_DRILL).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_DRILL1_M6));

            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_ASSEBMLY_SCREW).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_ASSEMBLY1_SCREW2));
            _resourceSkills.Single(predicate: s => s.Name == RESCOURCESKILL_ASSEBMLY_SCREW).ResourceSetups
                .Add(item: resourceSetups.Single(predicate: r => r.Name == RESCOURCESETUP_ASSEMBLY2_SCREW2));

            return _resourceSkills;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="resourceTools"></param>
        /// <returns></returns>
        private static M_ResourceSetup[] CreateResourceSetups(M_Resource[] resources, M_ResourceTool[] resourceTools)
        {
            var resourceSetups = new M_ResourceSetup[]
            {
                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_SAW1_SAWBIG, Resource = resources.Single(predicate: s => s.Name == RESCOURCE_SAW1),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_SAWBIG), SetupTime = 5
                },
                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_SAW2_SAWBIG, Resource = resources.Single(predicate: s => s.Name == RESCOURCE_SAW2),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_SAWBIG), SetupTime = 5
                },
                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_SAW2_SAWSMALL, Resource = resources.Single(predicate: s => s.Name == RESCOURCE_SAW2),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_SAWSMALL), SetupTime = 5
                },

                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_DRILL1_M6, Resource = resources.Single(predicate: s => s.Name == RESCOURCE_DRILL1),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_M6), SetupTime = 10
                },
                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_DRILL1_M4, Resource = resources.Single(predicate: s => s.Name == RESCOURCE_DRILL1),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_M4), SetupTime = 10
                },

                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_ASSEMBLY1_SCREW2,
                    Resource = resources.Single(predicate: s => s.Name == RESCOURCE_ASSEMBLY1),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_SCREWDRIVERCROSS2), SetupTime = 5
                },
                new M_ResourceSetup
                {
                    Name = RESCOURCESETUP_ASSEMBLY2_SCREW2,
                    Resource = resources.Single(predicate: s => s.Name == RESCOURCE_ASSEMBLY2),
                    ResourceTool = resourceTools.Single(predicate: s => s.Name == RESCOURCETOOL_SCREWDRIVERCROSS2), SetupTime = 5
                },
            };
            return resourceSetups;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static M_Resource[] CreateResources()
        {
            var resources = new M_Resource[]
            {
                new M_Resource {Name = RESCOURCE_SAW1, Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource {Name = RESCOURCE_SAW2, Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource {Name = RESCOURCE_DRILL1, Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource {Name = RESCOURCE_ASSEMBLY1, Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource {Name = RESCOURCE_ASSEMBLY2, Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
            };
            return resources;
        }

        private static M_ResourceTool[] CreateResourceTools()
        {
            var resourceTools = new M_ResourceTool[]
            {
                new M_ResourceTool {Name = RESCOURCETOOL_SAWBIG, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool {Name = RESCOURCETOOL_SAWSMALL, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool {Name = RESCOURCETOOL_M6, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool {Name = RESCOURCETOOL_M4, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool {Name = RESCOURCETOOL_SCREWDRIVERCROSS2, ResourceSetups = new List<M_ResourceSetup>()},
            };
            return resourceTools;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static M_ResourceSkill[] CreateResourceSkills()
        {
            var resourceSkills = new M_ResourceSkill[]
            {
                new M_ResourceSkill {Name = RESCOURCESKILL_CUT, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceSkill {Name = RESCOURCESKILL_DRILL, ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceSkill {Name = RESCOURCESKILL_ASSEBMLY_SCREW, ResourceSetups = new List<M_ResourceSetup>()},
            };
            return resourceSkills;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static M_Unit[] CreateUnits()
        {
            var units = new M_Unit[]
            {
                new M_Unit {Name = "Kilo"},
                new M_Unit {Name = "Litre"},
                new M_Unit {Name = "Pieces"}
            };
            return units;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static M_ArticleType[] CreateArticleTypes()
        {
            var articleTypes = new M_ArticleType[]
            {
                new M_ArticleType {Name = "Assembly"},
                new M_ArticleType {Name = "Material"},
                new M_ArticleType {Name = "Consumable"},
                new M_ArticleType {Name = "Product"}
            };
            return articleTypes;
        }
    }
}
