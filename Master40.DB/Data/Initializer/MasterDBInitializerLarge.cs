using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerLarge
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
            var cutting = new M_MachineGroup { Name = "Cutting", Stage = 1, ImageUrl = "/images/Production/saw.svg" };
            var drills = new M_MachineGroup { Name = "Drills", Stage = 2, ImageUrl = "/images/Production/drill.svg" };
            var assemblyUnit = new M_MachineGroup { Name = "AssemblyUnits", Stage = 3, ImageUrl = "/images/Production/assemblys.svg" };

            var ressources = new M_Resource[] {
                new M_Resource{Capacity=1, Name="Saw 1", Count = 1, MachineGroup = cutting },
                new M_Resource{Capacity=1, Name="Saw 2", Count = 1, MachineGroup = cutting },
                new M_Resource{Capacity=1, Name="Drill 1", Count = 1, MachineGroup = drills },
                new M_Resource{Capacity=1, Name="AssemblyUnit 1", Count=1, MachineGroup = assemblyUnit},
                new M_Resource{Capacity=1, Name="AssemblyUnit 2", Count=1, MachineGroup = assemblyUnit}
            };
            context.Resources.AddRange(ressources);
            context.SaveChanges();

            var ressourceTools = new M_ResourceTool[]
            {
                new M_ResourceTool{Name="Saw blade"},
                new M_ResourceTool{Name="M6 head"},
            };
            context.ResourceTools.AddRange(ressourceTools);
            context.SaveChanges();

            // Articles
            var articles = new M_Article[]
            {
                // Final Product
                new M_Article{Name="Dump-Truck",  ArticleTypeId = articleTypes.Single( s => s.Name == "Product").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},
                new M_Article{Name="Race-Truck",  ArticleTypeId = articleTypes.Single( s => s.Name == "Product").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/06_Race-Truck_final.jpg"},
                new M_Article{Name="Skeleton",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id,  CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/01_Bodenplatte.jpg"},
                new M_Article{Name="Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/03_Ladefläche.jpg"},
                new M_Article{Name="Chassis Type: Dump", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/02_Gehäuse.jpg"},
                new M_Article{Name="Chassis Type: Race", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 20.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/08_Race-Truck_Chassie.jpg"},
                new M_Article{Name="Race Wing", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/07_Race-Wing.jpg"},
                
                // Chassies
                new M_Article{Name="Cabin", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.75, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Engine-Block", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00, ToPurchase = false, ToBuild = true},
                
                // Truck Bed
                new M_Article{Name="Side wall long", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.35, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Side wall short", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Base plate Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.40, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Dump Joint" /*Kippgelenk*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.90, ToPurchase = true, ToBuild = false},
                
                // Engine Extension and Race Wing
                new M_Article{Name="Engine Race Extension", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.50, ToPurchase = false, ToBuild = true},
                // Skeleton
                new M_Article{Name="Wheel", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Base plate", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.80, ToPurchase = false, ToBuild = true},
                new M_Article{Name="Semitrailer" /*Aufleger*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.10, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Washer", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10,UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.02, ToPurchase = true, ToBuild = false},

                // base Materials
                new M_Article{Name="Timber Plate 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.20, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Timber Block 0,20m x 0,20m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.70, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Glue", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Litre").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Pegs", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Pole", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Button", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05, ToPurchase = true, ToBuild = false},
                new M_Article{Name="Packing", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 2.15, ToPurchase = true, ToBuild = false},
                new M_Article{Name="User Manual", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.50, ToPurchase = true, ToBuild = false},
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
            var workSchedule = new M_Operation[]
            {
                // assemble Truck
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Wedding", Duration=15, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Glue Truck-Bed", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },

                // assemble Truck
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Wedding", Duration=15, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Glue Race Wing", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },



                // assemble chassie
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Assemble Lamps", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Mount Engine to Cabin", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                
                // assemble chassie
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Assemble Lamps", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Mount Engine Extension", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Mount Engine to Cabin", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 30 },


                // assemble Skeleton
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "mount poles with wheels to Skeleton", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Screw wheels onto poles", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Glue Semitrailer", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 30 },
                                 
                // assemble TruckBed
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Glue side walls and base plate together", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Mount hatchback", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                                 
                // assemble Race Wing
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Cut shape", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Drill Mount Holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // Engine Race Extension
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Cut shape", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Drill Mount Holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                                  
                // side Walls for Truck-bed
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Side wall long").Id,  Name = "Cut long side", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Side wall long").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Side wall short").Id,  Name = "Cut short side", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Side wall short").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id,  Name = "Cut Base plate Truck-Bed", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // engin Block 
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Engine-Block").Id,  Name = "Cut Engine-Block", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Engine-Block").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // cabin       
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Cabin").Id,  Name = "Cut Cabin", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Cabin").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // Base Plate     
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Base plate").Id,  Name = "Cut Base plate", Duration=10, MachineGroupId=ressources.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Base plate").Id, Name = "drill holes for axis mount", Duration=5, MachineGroupId=ressources.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

            };
            context.Operations.AddRange(workSchedule);
            context.SaveChanges();



            var articleBom = new List<M_ArticleBom>
            {
                // Final Products 
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Dump-Truck", },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Race-Truck", },
                // Bom For DumpTruck
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Chassis Type: Dump", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Truck-Bed", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },

                // Bom For Race Truck
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Chassis Type: Race", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Race Wing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                

                // Bom for Skeleton
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Base plate").Id, Name = "Base plate", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Wheel").Id, Name = "Wheel", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Semitrailer").Id, Name = "Semitrailer", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Washer").Id, Name = "Washer", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                
                // Bom For Chassis Dump
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },

                // Bom For Chassis Race
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Engine Race Extension", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },


                // Bom for Truck-Bed
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Side wall long").Id, Name = "Side wall long", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Side wall short").Id, Name = "Side wall short", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id, Name = "Base plate Truck-Bed", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dump Joint").Id, Name = "Dump Joint", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=10, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },

                // Bom for some Assemblies
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Side wall long").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Side wall short").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Base plate").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race Wing").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Cabin").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Engine-Block").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Engine Race Extension").Id },

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
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Skeleton").Id,PackSize = 10,Price = 20.00, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Truck-Bed").Id,PackSize = 10,Price = 20.00, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Chassis Type: Dump").Id, PackSize = 10,Price = 20.00, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Chassis Type: Race").Id, PackSize = 10,Price = 25.00, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Cabin").Id,PackSize = 10,Price = 1.75, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Engine-Block").Id, PackSize = 10,Price = 0.40, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Engine Race Extension").Id, PackSize = 10,Price = 1.00, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Side wall long").Id,PackSize = 10,Price = 0.55, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Side wall short").Id,PackSize = 10,Price = 0.45, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Base plate Truck-Bed").Id, PackSize = 10,Price = 0.40, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Dump Joint").Id /*Kippgelenk*/,PackSize = 50,Price = 0.90, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Wheel").Id, PackSize = 150,Price = 0.35, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Base plate").Id, PackSize = 10,Price = 0.80, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Semitrailer" /*Aufleger*/).Id, PackSize = 25,Price = 0.10, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Race Wing").Id, PackSize = 10,Price = 1.50, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Washer").Id,PackSize = 150,Price = 0.02, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Timber Plate 1,5m x 3,0m").Id, PackSize = 100,Price = 0.20, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Timber Block 0,20m x 0,20m").Id, PackSize = 100,Price = 0.20, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Glue").Id, PackSize = 1000,Price = 0.01, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Pegs").Id, PackSize = 200,Price = 0.01, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Pole").Id, PackSize = 200,Price = 0.25, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Button").Id, PackSize = 500,Price = 0.05, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Packing").Id, PackSize = 50,Price = 2.50, DueTime = 15},
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="User Manual").Id, PackSize = 50,Price = 0.20, DueTime = 15},

            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();
        }
    }
}
