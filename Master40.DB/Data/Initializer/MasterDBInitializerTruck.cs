using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerTruck
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

            context.ArticleTypes.AddRange(entities: articleTypes);
            context.SaveChanges();

            // Units
            var units = new M_Unit[]
            {
                new M_Unit {Name = "Kilo"},
                new M_Unit {Name = "Litre"},
                new M_Unit {Name = "Pieces"}
            };
            context.Units.AddRange(entities: units);
            context.SaveChanges();

            var resourceSkills = new M_ResourceSkill[]
           {
                new M_ResourceSkill { Name = "CuttingSkill", Stage = 1, ImageUrl = "/images/Production/saw.svg" , ResourceSetups = new List<M_ResourceSetup>() },
                new M_ResourceSkill { Name = "DrillSkill", Stage = 2, ImageUrl = "/images/Production/drill.svg" , ResourceSetups = new List<M_ResourceSetup>() },
                new M_ResourceSkill { Name = "AssemblyUnitSkill", Stage = 3, ImageUrl = "/images/Production/assemblys.svg" , ResourceSetups = new List<M_ResourceSetup>()},
           };

            var resourceTools = new M_ResourceTool[]
            {
                new M_ResourceTool{ Name="Saw blade big", ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool{ Name="Saw blade small", ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool{ Name="M6 head", ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool{ Name="M4 head", ResourceSetups = new List<M_ResourceSetup>()},
                new M_ResourceTool{ Name="Screwdriver universal cross size 2", ResourceSetups = new List<M_ResourceSetup>()},
            };
            context.ResourceTools.AddRange(entities: resourceTools);
            context.SaveChanges();

            var resources = new M_Resource[] {
                new M_Resource{Name="Saw 1", Count = 1, ResourceSetups = new List<M_ResourceSetup>() },
                new M_Resource{Name="Saw 2", Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource{Name="Drill 1", Count = 1 , ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource{Name="AssemblyUnit 1", Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
                new M_Resource{Name="AssemblyUnit 2", Count = 1, ResourceSetups = new List<M_ResourceSetup>()},
            };
            context.Resources.AddRange(entities: resources);
            context.SaveChanges();

            var resourceSetups = new M_ResourceSetup[]{
            new M_ResourceSetup { Name = "Saw1_Sawbladebig", Resource = resources.Single(predicate: s => s.Name == "Saw 1"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "Saw blade big"), SetupTime = 5 },
            new M_ResourceSetup { Name = "Saw2_Sawbladebig", Resource = resources.Single(predicate: s => s.Name == "Saw 2"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "Saw blade big"), SetupTime = 5 },
            new M_ResourceSetup { Name = "Saw2_Sawbladesmall", Resource = resources.Single(predicate: s => s.Name == "Saw 2"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "Saw blade small"), SetupTime = 5 },
            
            new M_ResourceSetup { Name = "Drill1_M4", Resource = resources.Single(predicate: s => s.Name == "Drill 1"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "M4 head"), SetupTime = 10 },
            new M_ResourceSetup { Name = "Drill1_M6", Resource = resources.Single(predicate: s => s.Name == "Drill 1"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "M6 head"), SetupTime = 10 },
            
            new M_ResourceSetup { Name = "AssemblyUnit1_Screwdriver universal cross size 2", Resource = resources.Single(predicate: s => s.Name == "AssemblyUnit 1"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "Screwdriver universal cross size 2"), SetupTime = 5 },
            new M_ResourceSetup { Name = "AssemblyUnit2_Screwdriver universal cross size 2", Resource = resources.Single(predicate: s => s.Name == "AssemblyUnit 2"), ResourceTool = resourceTools.Single(predicate: s => s.Name == "Screwdriver universal cross size 2"), SetupTime = 5 },
            
            };

            // register resourceToResourceTool at the resourceSkill
            resourceSkills.Single(predicate: s => s.Name == "CuttingSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "Saw1_Sawbladebig"));
            resourceSkills.Single(predicate: s => s.Name == "CuttingSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "Saw2_Sawbladebig"));
            resourceSkills.Single(predicate: s => s.Name == "CuttingSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "Saw2_Sawbladesmall"));


            resourceSkills.Single(predicate: s => s.Name == "DrillSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "Drill1_M4"));
            resourceSkills.Single(predicate: s => s.Name == "DrillSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "Drill1_M6"));
            
            resourceSkills.Single(predicate: s => s.Name == "AssemblyUnitSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "AssemblyUnit1_Screwdriver universal cross size 2"));
            resourceSkills.Single(predicate: s => s.Name == "AssemblyUnitSkill").ResourceSetups.Add(item: resourceSetups.Single(predicate: r => r.Name == "AssemblyUnit2_Screwdriver universal cross size 2"));
            
            context.ResourceSkills.AddRange(entities: resourceSkills);
            context.SaveChanges();

            // Articles
            var articles = new M_Article[]
            {
                // Final Product
                new M_Article{Name="Dump-Truck",  ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Product").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},
                new M_Article{Name="Race-Truck",  ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Product").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/06_Race-Truck_final.jpg"},
                new M_Article{Name="Skeleton",ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id,  CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/01_Bodenplatte.jpg"},
                new M_Article{Name="Truck-Bed", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/03_Ladefläche.jpg"},
                new M_Article{Name="Chassis Type: Dump", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/02_Gehäuse.jpg"},
                new M_Article{Name="Chassis Type: Race", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 20.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/08_Race-Truck_Chassie.jpg"},
                new M_Article{Name="Race Wing", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 5, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 5.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/07_Race-Wing.jpg"},
                
                // Chassies
                new M_Article{Name="Cabin", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 1.75, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Engine-Block", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 3.00, ToPurchase = false, ToBuild = true},
                
                // Truck Bed
                new M_Article{Name="Side wall long", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.35, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Side wall short", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Base plate Truck-Bed", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.40, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Dump Joint" /*Kippgelenk*/, ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.90, ToPurchase = true, ToBuild = false},
                
                // Engine Extension and Race Wing
                new M_Article{Name="Engine Race Extension", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.50, ToPurchase = false, ToBuild = true},
                // Skeleton
                new M_Article{Name="Wheel", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 1.00, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Base plate", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.80, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Semitrailer" /*Aufleger*/, ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.10, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Washer", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,UnitId = units.Single( predicate: s => s.Name == "Kilo").Id, Price = 0.02, ToPurchase = true, ToBuild = false},

                // base Materials
                new M_Article{Name="Timber Plate 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.20, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Timber Block 0,20m x 0,20m", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.70, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Glue", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Litre").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Pegs", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( predicate: s => s.Name == "Kilo").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Pole", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Button", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Material").Id, CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( predicate: s => s.Name == "Kilo").Id, Price = 0.05, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Packing", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( predicate: s => s.Name == "Kilo").Id, Price = 2.15, ToPurchase = true, ToBuild = false},
                new M_Article{Name="User Manual", ArticleTypeId = articleTypes.Single( predicate: s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( predicate: s => s.Name == "Kilo").Id, Price = 0.50, ToPurchase = true, ToBuild = false},


            };

            context.Articles.AddRange(entities: articles);
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
                context.Stocks.Add(entity: stock);
                context.SaveChanges();
            }
            var operations = new M_Operation[]
            {
                // assemble Truck
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, Name = "Dump-Truck: Wedding", Duration=15, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, Name = "Glue Truck-Bed", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },

                // assemble Truck
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, Name = "Race-Truck: Wedding", Duration=15, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, Name = "Glue Race Wing", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },
                
                // assemble chassie Dump-Truck
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, Name = "Dump-Truck: Assemble Lamps", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, Name = "Dump-Truck: Mount Engine to Cabin", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },
                
                // assemble chassie Race Truck
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, Name = "Race-Truck: Assemble Lamps", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, Name = "Mount Engine Extension", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, Name = "Race-Truck: Mount Engine to Cabin", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 30 },
                
                // assemble Skeleton
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Skeleton").Id, Name = "mount poles with wheels to Skeleton", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Skeleton").Id, Name = "Screw wheels onto poles", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Skeleton").Id, Name = "Glue Semitrailer", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 30 },
               
                // assemble Truck Bed
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, Name = "Glue side walls and base plate together", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, Name = "Mount hatchback", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="AssemblyUnitSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Screwdriver universal cross size 2"), HierarchyNumber = 20 },

                // assemble Race Wing
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Race Wing").Id, Name = "Race Wing: Cut shape", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade big"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Race Wing").Id, Name = "Race Wing: Drill Mount Holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },
                // Engine Race Extension
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Engine Race Extension").Id, Name = "Engine Race Extension: Cut shape", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade big"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Engine Race Extension").Id, Name = "Engine Race Extension: Drill Mount Holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },
                
                // side Walls for Truck-bed
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Side wall long").Id,  Name = "Side wall long: Cut long side", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade small"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Side wall long").Id,  Name = "Side wall long: Drill mount holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },

                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Side wall short").Id,  Name = "Side wall short: Cut short side", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade small"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Side wall short").Id,  Name = "Side wall short: Drill mount holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },

                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Base plate Truck-Bed").Id,  Name = "Base plate Truck-Bed: Cut Base plate Truck-Bed", ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade big"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Base plate Truck-Bed").Id,  Name = "Base plate Truck-Bed: Drill mount holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },
                // engin Block
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Engine-Block").Id,  Name = "Engine-Block: Cut Engine-Block", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade big"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Engine-Block").Id,  Name = "Engine-Block: Drill mount holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M6 head"), HierarchyNumber = 20 },
                // cabin 
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Cabin").Id,  Name = "Cabin: Cut Cabin", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade small"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Cabin").Id,  Name = "Cabin: Drill mount holes", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M4 head"), HierarchyNumber = 20 },
                // Base Plate
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Base plate").Id, Name = "Base plate: Cut Base plate", Duration=10, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="CuttingSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="Saw blade big"), HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(predicate: a => a.Name == "Base plate").Id, Name = "Base plate: drill holes for axis mount", Duration=5, ResourceSkill=resourceSkills.Single(predicate: s => s.Name =="DrillSkill"), ResourceTool=resourceTools.Single(predicate: t => t.Name =="M4 head"), HierarchyNumber = 20 },

            };
            context.Operations.AddRange(entities: operations);
            context.SaveChanges();

            // !!! - Important NOTE - !!!
            // For Boms without Link to an Operation all Materials have to be ready to completed the operation assigned to the Article.
            var articleBom = new List<M_ArticleBom>
            {
                // Final Products 
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, Name = "Dump-Truck" },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, Name = "Race-Truck" },
                // Bom For DumpTruck
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Wedding").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, Name = "Chassis Type: Dump", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Wedding").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Wedding").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, Name = "Truck-Bed", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Button").Id, Name = "Knopf", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Dump-Truck").Id },

                // Bom For Race Truck
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Wedding").Id  },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, Name = "Chassis Type: Race", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Wedding").Id  },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Race Wing").Id, Name = "Race Wing", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Wedding").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Button").Id, Name = "Knopf", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race-Truck").Id },

                // Bom for Skeleton
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Base plate").Id, Name = "Base plate", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "mount poles with wheels to Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pole").Id, Name = "Pole", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "mount poles with wheels to Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Washer").Id, Name = "Washer", Quantity=4, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "Screw wheels onto poles").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Wheel").Id, Name = "Wheel", Quantity=4, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "Screw wheels onto poles").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Semitrailer").Id, Name = "Semitrailer", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Semitrailer").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(predicate: a => a.Name == "Skeleton").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue Semitrailer").Id },
                
                // Bom For Chassis Dump
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Button").Id, Name = "Knopf", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Mount Engine to Cabin").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Dump").Id, OperationId = operations.Single(predicate: x => x.Name == "Dump-Truck: Mount Engine to Cabin").Id },

                // Bom For Chassis Race
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Button").Id, Name = "Knopf", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Assemble Lamps").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount Engine Extension").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Engine Race Extension").Id, Name = "Engine Race Extension", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount Engine Extension").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(predicate: a => a.Name == "Chassis Type: Race").Id, OperationId = operations.Single(predicate: x => x.Name == "Race-Truck: Mount Engine to Cabin").Id },


                // Bom for Truck-Bed
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Side wall long").Id, Name = "Side wall long", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue side walls and base plate together").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Side wall short").Id, Name = "Side wall short", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue side walls and base plate together").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Base plate Truck-Bed").Id, Name = "Base plate Truck-Bed", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Glue side walls and base plate together").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Dump Joint").Id, Name = "Dump Joint", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount hatchback").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=10, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount hatchback").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Button").Id, Name = "Knopf", Quantity=2, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount hatchback").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount hatchback").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Truck-Bed").Id, OperationId = operations.Single(predicate: x => x.Name == "Mount hatchback").Id },

                // Bom for some Assemblies
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Side wall long").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Side wall short").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Base plate Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Base plate").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Cabin").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Engine-Block").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(predicate: a => a.Name == "Engine Race Extension").Id },

            };
            context.ArticleBoms.AddRange(entities: articleBom);
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us toy department" };
            var businessPartner2 = new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
            context.BusinessPartners.Add(entity: businessPartner);
            context.BusinessPartners.Add(entity: businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Skeleton").Id,PackSize = 10,Price = 20.00, TimeToDelivery = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Truck-Bed").Id,PackSize = 10,Price = 20.00, TimeToDelivery = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Chassis Type: Dump").Id, PackSize = 10,Price = 20.00, TimeToDelivery = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Chassis Type: Race").Id, PackSize = 10,Price = 25.00, TimeToDelivery = 2880},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Cabin").Id,PackSize = 10,Price = 1.75, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Engine-Block").Id, PackSize = 10,Price = 0.40, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Engine Race Extension").Id, PackSize = 10,Price = 1.00, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Side wall long").Id,PackSize = 10,Price = 0.55, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Side wall short").Id,PackSize = 10,Price = 0.45, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Base plate Truck-Bed").Id, PackSize = 10,Price = 0.40, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Dump Joint").Id /*Kippgelenk*/,PackSize = 50,Price = 0.90, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Wheel").Id, PackSize = 150,Price = 0.35, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Base plate").Id, PackSize = 10,Price = 0.80, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Semitrailer" /*Aufleger*/).Id, PackSize = 25,Price = 0.10, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Race Wing").Id, PackSize = 10,Price = 1.50, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Washer").Id,PackSize = 150,Price = 0.02, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Timber Plate 1,5m x 3,0m").Id, PackSize = 100,Price = 0.20, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Timber Block 0,20m x 0,20m").Id, PackSize = 100,Price = 0.20, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Glue").Id, PackSize = 1000,Price = 0.01, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Pegs").Id, PackSize = 200,Price = 0.01, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Pole").Id, PackSize = 200,Price = 0.25, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Button").Id, PackSize = 500,Price = 0.05, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="Packing").Id, PackSize = 50,Price = 2.50, TimeToDelivery = 1440},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(predicate: x => x.Name=="User Manual").Id, PackSize = 50,Price = 0.20, TimeToDelivery = 1440},

            };
            context.ArticleToBusinessPartners.AddRange(entities: artToBusinessPartner);
            context.SaveChanges();
        }
    }
}