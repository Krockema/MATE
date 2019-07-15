using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerSimple
    {
        public static void DbInitialize(MasterDBContext context)
        {
            context.Database.EnsureCreated();


            // Look for any Entrys.
            if (context.Articles.Any())
            {
                return;   // DB has been seeded
            }
            // Article Types
            var articleTypes = new M_ArticleType[]
            {
                new M_ArticleType {Name = "Assembly"},
                new M_ArticleType {Name = "Material"},
                new M_ArticleType {Name = "Consumable"},
                new M_ArticleType {Name = "Product"}
            };

            context.ArticleTypes.AddRange(articleTypes);
            context.SaveChanges();

            // Units
            var units = new M_Unit[]
            {
                new M_Unit {Name = "Kilo"},
                new M_Unit {Name = "Litre"},
                new M_Unit {Name = "Pieces"}
            };
            context.Units.AddRange(units);
            context.SaveChanges();

            var resourceSkills = new M_ResourceSkill[]
            {
                new M_ResourceSkill { Name = "CuttingLarge", Stage = 1, ImageUrl = "/images/Production/saw.svg" },
                new M_ResourceSkill { Name = "CuttingSmall", Stage = 1, ImageUrl = "/images/Production/saw.svg" },
                new M_ResourceSkill { Name = "Drills", Stage = 2, ImageUrl = "/images/Production/drill.svg" },
                new M_ResourceSkill { Name = "AssemblyUnits", Stage = 3, ImageUrl = "/images/Production/assemblys.svg" },
            };

            var resourceTools = new M_ResourceTool[]
            {
                new M_ResourceTool{ Name="Saw blade big"},
                new M_ResourceTool{ Name="Saw blade small"},
                new M_ResourceTool{ Name="M6 head"},
                new M_ResourceTool{ Name="M4 head"},
                new M_ResourceTool{ Name="Screwdriver universal cross size 2"},
            };
            context.ResourceTools.AddRange(resourceTools);
            context.SaveChanges();

            var resources = new M_Resource[] {
                new M_Resource{Name="Saw 1", Count = 1},
                new M_Resource{Name="Saw 2", Count = 1},
                new M_Resource{Name="Drill 1", Count = 1 },
                new M_Resource{Name="AssemblyUnit 1", Count = 1},
                new M_Resource{Name="AssemblyUnit 2", Count = 1},
            };
            context.Resources.AddRange(resources);
            context.SaveChanges();

            var resourceToResourceTools = new M_ResourceToResourceTool[]{
            new M_ResourceToResourceTool { Name = "Saw1_Sawbladebig", Resource = resources.Single(s => s.Name == "Saw 1"), ResourceTool = resourceTools.Single(s => s.Name == "Saw blade big"), SetupTime = 5 },
            new M_ResourceToResourceTool { Name = "Saw1_Sawbladesmall", Resource = resources.Single(s => s.Name == "Saw 1"), ResourceTool = resourceTools.Single(s => s.Name == "Saw blade small"), SetupTime = 5 },
            new M_ResourceToResourceTool { Name = "Saw2_Sawbladebig", Resource = resources.Single(s => s.Name == "Saw 2"), ResourceTool = resourceTools.Single(s => s.Name == "Saw blade big"), SetupTime = 5 },
            new M_ResourceToResourceTool { Name = "Saw2_Sawbladesmall", Resource = resources.Single(s => s.Name == "Saw 2"), ResourceTool = resourceTools.Single(s => s.Name == "Saw blade small"), SetupTime = 5 },

            new M_ResourceToResourceTool { Name = "Drill1_M4", Resource = resources.Single(s => s.Name == "Drill 1"), ResourceTool = resourceTools.Single(s => s.Name == "M4"), SetupTime = 10 },
            new M_ResourceToResourceTool { Name = "Drill1_M6", Resource = resources.Single(s => s.Name == "Drill 1"), ResourceTool = resourceTools.Single(s => s.Name == "M6"), SetupTime = 10 },

            new M_ResourceToResourceTool { Name = "AssemblyUnit1_Screwdriver universal cross size 2", Resource = resources.Single(s => s.Name == "AssemblyUnit 1"), ResourceTool = resourceTools.Single(s => s.Name == "Screwdriver universal cross size 2"), SetupTime = 5 },
            new M_ResourceToResourceTool { Name = "AssemblyUnit2_Screwdriver universal cross size 2", Resource = resources.Single(s => s.Name == "AssemblyUnit 2"), ResourceTool = resourceTools.Single(s => s.Name == "Screwdriver universal cross size 2"), SetupTime = 5 },

            };

            // register resourceToResourceTool at the resourceSkill
            resourceSkills.Single(s => s.Name == "CuttingLarge").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladebig"));
            resourceSkills.Single(s => s.Name == "CuttingLarge").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladesmall"));
            resourceSkills.Single(s => s.Name == "CuttingSmall").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw2_Sawbladesmall"));

            context.ResourceSkills.AddRange(resourceSkills);
            context.SaveChanges();

            // register resourceToResourceTool at resources
            //ToDo foreach
            resources.Single(s => s.Name == "Saw 1").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladebig"));
            resources.Single(s => s.Name == "Saw 1").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladesmall"));
            resources.Single(s => s.Name == "Saw 2").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw2_Sawbladebig"));
            resources.Single(s => s.Name == "Saw 2").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw2_Sawbladesmall"));
            // register resourceToResourceTool at resources
            //ToDo foreach
            resourceTools.Single(s => s.Name == "Saw blade big").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladesbig"));
            resourceTools.Single(s => s.Name == "Saw blade small").ResourceToResourceTools.Add(resourceToResourceTools.Single(r => r.Name == "Saw1_Sawbladesmall"));

            // Articles
            var articles = new M_Article[]
            {
                // Final Products
                new M_Article{Name="Tisch",  ArticleTypeId = articleTypes.Single( s => s.Name == "Product").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 25.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},

                // Intermediate Products
                new M_Article{Name="Tischbein",  ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 2.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},
                new M_Article{Name="Tischplatte",  ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 10.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},
                
                // base Materials
                new M_Article{Name="Holzplatte 1,5m x 3,0m x 0,03m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Holzpflock 1,20m x 0,15m x 0,15m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.70, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Schrauben", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.50, ToPurchase = true, ToBuild = false},

            };

            context.Articles.AddRange(articles);
            context.SaveChanges();

            // create Stock Entrys for each Article
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
                context.Stocks.Add(stock);
                context.SaveChanges();
            }
            var operations = new M_Operation[]
            {
                // Final Product Tisch 
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch zusammenstellen", Duration=15,ResourceSkill=resourceSkills.Single(s => s.Name =="AssemblyUnits"), HierarchyNumber = 10 },
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch verschrauben", Duration=15, ResourceSkill=resourceSkills.Single(s => s.Name =="AssemblyUnits"), HierarchyNumber = 20 },

                // Bom For Tischbein
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tischbein").Id, Name = "Tischbein saegen", Duration=15, ResourceSkill=resourceSkills.Single(s => s.Name =="CuttingLarge"), HierarchyNumber = 10 },
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tischbein").Id, Name = "Tischbein bohren", Duration=5, ResourceSkill=resourceSkills.Single(s => s.Name =="Drills"), HierarchyNumber = 20 },

                // Bom For Tischbein
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tischplatte").Id, Name = "Tischplatte saegen", Duration=15, ResourceSkill=resourceSkills.Single(s => s.Name =="CuttingSmall"), HierarchyNumber = 10 },
                new M_Operation{  ArticleId = articles.Single(a => a.Name == "Tischplatte").Id, Name = "Tischplatte bohren", Duration=5, ResourceSkill=resourceSkills.Single(s => s.Name =="Drills"), HierarchyNumber = 20 },


            };
            context.Operations.AddRange(operations);
            context.SaveChanges();

            // !!! - Important NOTE - !!!
            // For Boms without Link to an Opperation all Materials have to be ready to compleate the opperation assignet to the Article.
            var articleBom = new List<M_ArticleBom>
            {
                // Final Product Tisch 
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch" },

                // Bom For Tisch
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tischbein").Id, Name = "Tischbein", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id, OperationId = operations.Single(x => x.Name == "Tisch verschrauben").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tischplatte").Id, Name = "Tischplatte", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id, OperationId = operations.Single(x => x.Name == "Tisch zusammenstellen").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Schrauben").Id, Name = "Schrauben", Quantity=8, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id, OperationId = operations.Single(x => x.Name == "Tisch zusammenstellen").Id },
              
                // Bom For Tischplatte
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holzplatte 1,5m x 3,0m x 0,03m").Id, Name = "Holzplatte 1,5m x 3,0m x 0,03m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tischplatte").Id, OperationId = operations.Single(x => x.Name == "Tischplatte saegen").Id },

                // Bom For Tischbein
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holzpflock 1,20m x 0,15m x 0,15m").Id, Name = "Holzpflock 1,20m x 0,15m x 0,15m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tischbein").Id, OperationId = operations.Single(x => x.Name == "Tischbein saegen").Id },

            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us toy department" };
            var businessPartner2 = new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Tisch").Id,PackSize = 10,Price = 20.00, DueTime = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Tischbein").Id,PackSize = 10,Price = 20.00, DueTime = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Tischplatte").Id, PackSize = 500,Price = 0.05, DueTime = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Schrauben").Id, PackSize = 50,Price = 2.50, DueTime = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Holzpflock 1,20m x 0,15m x 0,15m").Id, PackSize = 50,Price = 0.20, DueTime = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Holzplatte 1,5m x 3,0m x 0,03m").Id, PackSize = 50,Price = 0.20, DueTime = 1440},

            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

        }
    }
}
