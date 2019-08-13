using Master40.DB.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;

namespace Zpp.Test.Configurations
{
    public class MasterDBInitializerDesk
    {
        public static void DbInitialize(ProductionDomainContext productionDomainContext)
        {
            productionDomainContext.Database.EnsureCreated();
            
            // Article Types
            var articleTypes = new M_ArticleType[]
            {
                new M_ArticleType {Name = "Assembly"},
                new M_ArticleType {Name = "Material"},
                new M_ArticleType {Name = "Consumable"}
            };
            productionDomainContext.ArticleTypes.AddRange(articleTypes);
            productionDomainContext.SaveChanges();

            // Units
            var units = new M_Unit[]
            {
                new M_Unit {Name = "Kilo"},
                new M_Unit {Name = "Litre"},
                new M_Unit {Name = "Pieces"}
            };
            productionDomainContext.Units.AddRange(units);
            productionDomainContext.SaveChanges();

            var machineGroupMontage = new M_MachineGroup {Name = "Montage"};
            var machineGroupSchweißen = new M_MachineGroup {Name = "Schweißen"};
            var machineGroupVerpacken = new M_MachineGroup {Name = "Verpacken"};
            var machines = new M_Machine[]
            {
                // Verpacken
                new M_Machine
                {
                    Capacity = 1, Name = "Verpacken 1", Count = 1,
                    MachineGroup = machineGroupVerpacken
                },
                new M_Machine
                {
                    Capacity = 1, Name = "Verpacken 2", Count = 1,
                    MachineGroup = machineGroupVerpacken
                },
                // Schweißen
                new M_Machine
                {
                    Capacity = 1, Name = "Schweißen 1", Count = 1, MachineGroup = machineGroupSchweißen
                },
                new M_Machine
                {
                    Capacity = 1, Name = "Schweißen 2", Count = 1, MachineGroup = machineGroupSchweißen
                },
                // Montage der Beine an Tisch
                new M_Machine
                {
                    Capacity = 1, Name = "Montage 1", Count = 1, MachineGroup = machineGroupMontage
                },
                new M_Machine
                {
                    Capacity = 1, Name = "Montage 2", Count = 1, MachineGroup = machineGroupMontage
                },
            };
            productionDomainContext.Machines.AddRange(machines);
            productionDomainContext.SaveChanges();

            // Tool has no meaning yet, ignore it
            var machineTools = new M_MachineTool[]
            {
                new M_MachineTool
                {
                    MachineId = machines.Single(m => m.Name == "Montage 2").Id, SetupTime = 1,
                    Name = "Schweißgerät"
                }
            };
            productionDomainContext.MachineTools.AddRange(machineTools);
            productionDomainContext.SaveChanges();

            // Articles
            var articles = new M_Article[]
            {
                // TODO: add lotsizes when useful
                
                // no prices except for articles that are sold
                new M_Article
                {
                    Name = "Tisch",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Assembly").Id,
                    CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, Price = 100.00,
                    ToBuild = true, ToPurchase = false
                },
                new M_Article
                {
                    Name = "Tischplatte",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    DeliveryPeriod = 10, UnitId = units.Single(s => s.Name == "Pieces").Id,
                    ToBuild = false, ToPurchase = true
                },
                new M_Article
                {
                    Name = "Tischbein",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Assembly").Id,
                    DeliveryPeriod = 10, UnitId = units.Single(s => s.Name == "Pieces").Id,
                    ToBuild = true, ToPurchase = false
                },
                new M_Article
                {
                    Name = "Schrauben",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true
                },
                // Anschraubplatte, Stahlrohr, Montageanleitung
                new M_Article
                {
                    Name = "Anschraubplatte",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true
                },
                new M_Article
                {
                    Name = "Stahlrohr",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true
                },
                new M_Article
                {
                    Name = "Filzgleiter",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 3,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true
                },
                new M_Article
                {
                    Name = "Montageanleitung",
                    ArticleTypeId = articleTypes.Single(s => s.Name == "Consumable").Id,
                    CreationDate = DateTime.Parse("2019-07-31"), DeliveryPeriod = 1,
                    UnitId = units.Single(s => s.Name == "Pieces").Id, ToBuild = false,
                    ToPurchase = true
                },

            };
            productionDomainContext.Articles.AddRange(articles);
            productionDomainContext.SaveChanges();

            // get the name -> id mappings
            var DBArticles = productionDomainContext.Articles.ToDictionary(p => p.Name, p => p.Id);

            // create Stock Entrys for each Article
            foreach (var article in DBArticles)
            {
                var stocks = new M_Stock[]
                {
                    new M_Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == "Schrauben") ? 50 : 0,
                        Max = 1000,
                        Current = (article.Key == "Schrauben") ? 100 : 0
                    }
                };
                productionDomainContext.Stocks.AddRange(stocks);
                productionDomainContext.SaveChanges();
            }
            
            // Tool has no meaning yet, ignore it
            int machineTool = machineTools.Single(a => a.Name == "Schweißgerät").Id;

                // Tischbeine 
                M_Operation operationTischbeinAnschraubplatte = new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == "Tischbein").Id,
                    Name = "Anschraubplatte anschweißen", Duration = 20,
                    MachineGroupId = machineGroupSchweißen.Id, HierarchyNumber = 10,
                    MachineToolId = machineTool
                };
                M_Operation operationTischbeinFilzgleiter = new M_Operation
                {
                    ArticleId = articles.Single(a => a.Name == "Tischbein").Id,
                    Name = "Flizgleiter anstecken", Duration = 10,
                    MachineGroupId = machineGroupMontage.Id, HierarchyNumber = 20,
                    MachineToolId = machineTool
                };
 
            productionDomainContext.Operations.Add(operationTischbeinAnschraubplatte);
            productionDomainContext.Operations.Add(operationTischbeinFilzgleiter);
            productionDomainContext.SaveChanges();

            var articleBom = new List<M_ArticleBom>
            {
                // Tisch
                new M_ArticleBom
                    {ArticleChildId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch"},
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Tischplatte").Id,
                    Name = "Tischplatte",
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id,
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Tischbein").Id,
                    Name = "Tischbein", Quantity = 4,
                    ArticleParentId = articles.Single(a => a.Name == "Tisch").Id,
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Schrauben").Id,
                    Name = "Schrauben", Quantity = 16,
                    ArticleParentId = articles.Single(a => a.Name == "Tisch").Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Montageanleitung").Id,
                    Name = "Montageanleitung",
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id,
                },

                // Tischbein
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Anschraubplatte").Id,
                    Name = "Anschraubplatte",
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == "Tischbein").Id,
                    OperationId = operationTischbeinAnschraubplatte.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Stahlrohr").Id,
                    Name = "Stahlrohr",
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == "Tischbein").Id,
                    OperationId = operationTischbeinAnschraubplatte.Id
                },
                new M_ArticleBom
                {
                    ArticleChildId = articles.Single(a => a.Name == "Filzgleiter").Id,
                    Name = "Filzgleiter",
                    Quantity = 1, ArticleParentId = articles.Single(a => a.Name == "Tischbein").Id,
                    OperationId = operationTischbeinFilzgleiter.Id
                },
            };
            productionDomainContext.ArticleBoms.AddRange(articleBom);
            productionDomainContext.SaveChanges();

            // TODO
            //create Businesspartner
            var businessPartnerDeskDistributor = new M_BusinessPartner()
                {Debitor = true, Kreditor = false, Name = "Tischverkäufer"};
            var businessPartnerWholeSale = new M_BusinessPartner()
                {Debitor = false, Kreditor = true, Name = "Teile Großhandel"};
            var businessPartnerPrintShop = new M_BusinessPartner()
                {Debitor = false, Kreditor = true, Name = "Druckerei"};
            productionDomainContext.BusinessPartners.Add(businessPartnerDeskDistributor);
            productionDomainContext.BusinessPartners.Add(businessPartnerWholeSale);
            productionDomainContext.BusinessPartners.Add(businessPartnerPrintShop);
            productionDomainContext.SaveChanges();

            var articleToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == "Tischplatte").Id, PackSize = 1,
                    Price = 20,
                    DueTime = 1000
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == "Schrauben").Id, PackSize = 100,
                    Price = 5,
                    DueTime = 1000
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == "Anschraubplatte").Id, PackSize = 10,
                    Price = 10,
                    DueTime = 1000
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == "Stahlrohr").Id, PackSize = 10,
                    Price = 20,
                    DueTime = 1000
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerWholeSale.Id,
                    ArticleId = articles.Single(x => x.Name == "Filzgleiter").Id, PackSize = 10,
                    Price = 2,
                    DueTime = 1000
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartnerPrintShop.Id,
                    ArticleId = articles.Single(x => x.Name == "Montageanleitung").Id, PackSize = 100,
                    Price = 0.05,
                    DueTime = 1000
                },

            };
            productionDomainContext.ArticleToBusinessPartners.AddRange(articleToBusinessPartner);
            productionDomainContext.SaveChanges();
        }
    }
}