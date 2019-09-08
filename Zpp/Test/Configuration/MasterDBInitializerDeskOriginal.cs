using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Zpp.Test.Configuration
{
    public class MasterDBInitializerDeskOriginal
    {
        private const string MACHINE_GROUP_WELDING = "Schweißen";
        private const string MACHINE_GROUP_ASSEMBLING = "Montage";
        private const string MACHINE_GROUP_PACKING = "Verpacken";
        private const string MACHINE_TOOL_WELDER = "Schweißgerät";
        private const string OPERATION_DESK = "Tisch verpacken";
        private const string OPERATION_DESK_LEG_1 = "Anschraubplatte anschweißen";
        private const string OPERATION_DESK_LEG_2 = "Flizgleiter anstecken";

        private const string BUSINESS_PARTNER_PRINT_SHOP = "Druckerei";
        private const string BUSINESS_PARTNER_DESK_DISTRIBUTOR = "Tischverkäufer";
        private const string BUSINESS_PARTNER_WHOLESALE = "Teile Großhandel";
        
        private const string ARTICLE_DESK = "Tisch";
        private const string ARTICLE_PACKAGE = "Verpackung";
        private const string ARTICLE_DESK_SURFACE = "Tischplatte";
        private const string ARTICLE_DESK_LEG = "Tischbein";
        private const string ARTICLE_MANUAL = "Montageanleitung";
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

            // machine groups
            var machineGroups = CreateMachineGroups();
            var machines = CreateMachines(machineGroups);
            productionDomainContext.Machines.AddRange(machines);
            productionDomainContext.SaveChanges();


            var machineTools = CreateMachineTools(machines);
            productionDomainContext.MachineTools.AddRange(machineTools);
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
            
            var operations = CreateOperations(machineTools, articles, machineGroups);
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
                    {Debitor = false, Kreditor = true, Name = BUSINESS_PARTNER_PRINT_SHOP},
                new M_BusinessPartner()
                    {Debitor = true, Kreditor = false, Name = BUSINESS_PARTNER_DESK_DISTRIBUTOR},
                new M_BusinessPartner()
                    {Debitor = false, Kreditor = true, Name = BUSINESS_PARTNER_WHOLESALE}
            };
        }

        private static M_MachineTool[] CreateMachineTools(M_Machine[] machines)
        {
            return new M_MachineTool[]
            {
                new M_MachineTool
                {
                    MachineId = machines.Single(m => m.Name == "Montage 2").Id, SetupTime = 1,
                    Name = MACHINE_TOOL_WELDER
                }
            };
        }

        private static M_MachineGroup[] CreateMachineGroups()
        {
            return new M_MachineGroup[]
            {
                new M_MachineGroup {Name = MACHINE_GROUP_ASSEMBLING},
                new M_MachineGroup {Name = MACHINE_GROUP_WELDING},
                new M_MachineGroup {Name = MACHINE_GROUP_PACKING}
            };
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
                new M_ArticleType {Name = "Assembly"},
                new M_ArticleType {Name = "Material"},
                new M_ArticleType {Name = "Consumable"}
            };
        }

        private static M_Operation[] CreateOperations(M_MachineTool[] machineTools,
            M_Article[] articles, M_MachineGroup[] machineGroups)
        {
            // Tool has no meaning yet, ignore it and use always the same
            M_MachineTool machineTool = machineTools.Single(a => a.Name == MACHINE_TOOL_WELDER);
            return new M_Operation[]
            {
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    Name = OPERATION_DESK, Duration = 11,
                    MachineGroupId = machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_PACKING))
                        .Id,
                    HierarchyNumber = 10,
                    MachineToolId = machineTool.Id
                },
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    Name = OPERATION_DESK_LEG_1, Duration = 20,
                    MachineGroupId = machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_WELDING))
                        .Id,
                    HierarchyNumber = 10,
                    MachineToolId = machineTool.Id
                },
                new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == ARTICLE_DESK_LEG).Id,
                    Name = OPERATION_DESK_LEG_2, Duration = 3,
                    MachineGroupId = machineGroups
                        .Single(x => x.Name.Equals(MACHINE_GROUP_ASSEMBLING)).Id,
                    HierarchyNumber = 20,
                    MachineToolId = machineTool.Id
                }
            };
        }

        private static M_Machine[] CreateMachines(M_MachineGroup[] machineGroups)
        {
            return new M_Machine[]
            {
                // Verpacken
                new M_Machine
                {
                    Capacity = 1, Name = "Verpacken 1", Count = 1,
                    MachineGroup = machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_PACKING))
                },
                // Schweißen
                new M_Machine
                {
                    Capacity = 1, Name = "Schweißen 1", Count = 1,
                    MachineGroup = machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_WELDING))
                },
                new M_Machine
                {
                    Capacity = 1, Name = "Schweißen 2", Count = 1,
                    MachineGroup = machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_WELDING))
                },
                // Montage der Beine an Tisch
                new M_Machine
                {
                    Capacity = 1, Name = "Montage 1", Count = 1,
                    MachineGroup =
                        machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_ASSEMBLING))
                },
                new M_Machine
                {
                    Capacity = 1, Name = "Montage 2", Count = 1,
                    MachineGroup =
                        machineGroups.Single(x => x.Name.Equals(MACHINE_GROUP_ASSEMBLING))
                },
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
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Assembly").Id,
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
                new M_Article
                {
                    Name = ARTICLE_MANUAL,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 1,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 100
                },
                new M_Article
                {
                    Name = ARTICLE_PACKAGE,
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-08-26"), DeliveryPeriod = 1,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true, LotSize = 50
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
                    {ArticleChildId = articles.Single(a => a.Name == ARTICLE_DESK).Id, Name = ARTICLE_DESK},
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_PACKAGE).Id,
                    Name = ARTICLE_PACKAGE,
                    Quantity = 1,
                    ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
                    OperationId = operationDesk.Id
                },
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
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == ARTICLE_MANUAL).Id,
                    Name = ARTICLE_MANUAL,
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == ARTICLE_DESK).Id,
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
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_SCREWS).Id, PackSize = 100,
                    Price = 5,
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_MOUNTING_PLATE).Id, PackSize = 10,
                    Price = 10,
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_STEEL_PIPE).Id, PackSize = 10,
                    Price = 20,
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_FELT_GLIDERS).Id, PackSize = 10,
                    Price = 2,
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartners.Single(x => x.Name.Equals(BUSINESS_PARTNER_PRINT_SHOP)).Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_MANUAL).Id,
                    PackSize = 100,
                    Price = 0.05,
                    DueTime = 100
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == ARTICLE_PACKAGE).Id, PackSize = 10,
                    Price = 0.50,
                    DueTime = 100
                },
            };
        }
    }
}