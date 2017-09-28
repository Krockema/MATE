using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Models;

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
            var articleTypes = new ArticleType[]
            {
                new ArticleType {Name = "Assembly"},
                new ArticleType {Name = "Material"},
                new ArticleType {Name = "Consumable"}
            };

            context.ArticleTypes.AddRange(articleTypes);
            context.SaveChanges();

            // Units
            var units = new Unit[]
            {
                new Unit {Name = "Kilo"},
                new Unit {Name = "Litre"},
                new Unit {Name = "Pieces"}
            };
            context.Units.AddRange(units);
            context.SaveChanges();
            var cutting = new MachineGroup { Name = "Cutting", Stage = 1, ImageUrl = "/images/Production/saw.svg" };
            var drills = new MachineGroup { Name = "Drills", Stage = 2, ImageUrl = "/images/Production/drill.svg" };
            var assemblyUnit = new MachineGroup {Name = "AssemblyUnits", Stage=3, ImageUrl= "/images/Production/assemblys.svg" };
            
            var machines = new Machine[] {
                new Machine{Capacity=1, Name="Saw 1", Count = 1, MachineGroup = cutting },
                new Machine{Capacity=1, Name="Saw 2", Count = 1, MachineGroup = cutting },
                new Machine{Capacity=1, Name="Drill 1", Count = 1, MachineGroup = drills },
                new Machine{Capacity=1, Name="AssemblyUnit 1", Count=1, MachineGroup = assemblyUnit},
                new Machine{Capacity=1, Name="AssemblyUnit 2", Count=1, MachineGroup = assemblyUnit}
            };
            context.Machines.AddRange(machines);
            context.SaveChanges();

            var machineTools = new MachineTool[]
            {
                new MachineTool{MachineId=machines.Single(m => m.Name == "Saw 1").Id, SetupTime=1, Name="Saw blade"},
                new MachineTool{MachineId=machines.Single(m => m.Name == "Drill 1").Id, SetupTime=1, Name="M6 head"},
            };
            context.MachineTools.AddRange(machineTools);
            context.SaveChanges();

            // Articles
            var articles = new Article[]
            {
                // Final Product
                new Article{Name="Dump-Truck",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"},
                new Article{Name="Race-Truck",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/06_Race-Truck_final.jpg"},
                new Article{Name="Skeleton",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id,  CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/01_Bodenplatte.jpg"},
                new Article{Name="Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/03_Ladefläche.jpg"},
                new Article{Name="Chassis Type: Dump", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/02_Gehäuse.jpg"},
                new Article{Name="Chassis Type: Race", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 20.00, ToPurchase = false, ToBuild = true,  PictureUrl ="/images/Product/08_Race-Truck_Chassie.jpg"},
                new Article{Name="Race Wing", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToPurchase = false, ToBuild = true, PictureUrl ="/images/Product/07_Race-Wing.jpg"},
                
                // Chassies
                new Article{Name="Cabin", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.75, ToPurchase = false, ToBuild = true},
                new Article{Name="Engine-Block", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00, ToPurchase = false, ToBuild = true},
                
                // Truck Bed
                new Article{Name="Side wall long", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.35, ToPurchase = false, ToBuild = true},
                new Article{Name="Side wall short", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = false, ToBuild = true},
                new Article{Name="Base plate Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.40, ToPurchase = false, ToBuild = true},
                new Article{Name="Dump Joint" /*Kippgelenk*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.90, ToPurchase = true, ToBuild = false},
                
                // Engine Extension and Race Wing
                new Article{Name="Engine Race Extension", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.50, ToPurchase = false, ToBuild = true},
                // Skeleton
                new Article{Name="Wheel", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00, ToPurchase = true, ToBuild = false},
                new Article{Name="Base plate", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.80, ToPurchase = false, ToBuild = true},
                new Article{Name="Semitrailer" /*Aufleger*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.10, ToPurchase = true, ToBuild = false},
                new Article{Name="Washer", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10,UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.02, ToPurchase = true, ToBuild = false},

                // base Materials
                new Article{Name="Timber Plate 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.20, ToPurchase = true, ToBuild = false},
                new Article{Name="Timber Block 0,20m x 0,20m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.70, ToPurchase = true, ToBuild = false},
                new Article{Name="Glue", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Litre").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new Article{Name="Pegs", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.01, ToPurchase = true, ToBuild = false},
                new Article{Name="Pole", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.25, ToPurchase = true, ToBuild = false},
                new Article{Name="Button", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05, ToPurchase = true, ToBuild = false},
                new Article{Name="Packing", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 2.15, ToPurchase = true, ToBuild = false},
                new Article{Name="User Manual", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.50, ToPurchase = true, ToBuild = false},


            };

            context.Articles.AddRange(articles);
            context.SaveChanges();

            // create Stock Entrys for each Article
            foreach (var article in articles)
            {
                var stock = new Stock
                {
                    ArticleForeignKey = article.Id,
                    Name = "Stock: " + article.Name,
                    Min = (article.ToPurchase) ? 200 : 0,
                    Max = (article.ToPurchase) ? 1000 : 0,
                    Current = (article.ToPurchase) ? 800 : 0,
                    StartValue = (article.ToPurchase) ? 800 : 0,
                };
                context.Stocks.Add(stock);
                context.SaveChanges();
            }
            var workSchedule = new WorkSchedule[]
            {
                // assemble Truck
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Wedding", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Glue Truck-Bed", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },

                // assemble Truck
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Wedding", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Glue Race Wing", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },



                // assemble chassie
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Assemble Lamps", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Mount Engine to Cabin", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                
                // assemble chassie
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Assemble Lamps", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Mount Engine Extension", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Mount Engine to Cabin", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 30 },


                // assemble Skeleton
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "mount poles with wheels to Skeleton", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Screw wheels onto poles", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Glue Semitrailer", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 30 },
               
                // assemble Truck Bed
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Glue side walls and base plate together", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Mount hatchback", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit 1").MachineGroupId, HierarchyNumber = 20 },

                // assemble Race Wing
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Cut shape", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Drill Mount Holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // Engine Race Extension
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Cut shape", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Drill Mount Holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                
                // side Walls for Truck-bed
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Side wall long").Id,  Name = "Cut long side", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Side wall long").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Side wall short").Id,  Name = "Cut short side", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Side wall short").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id,  Name = "Cut Base plate Truck-Bed", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // engin Block
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Engine-Block").Id,  Name = "Cut Engine-Block", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Engine-Block").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // cabin 
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Cabin").Id,  Name = "Cut Cabin", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Cabin").Id,  Name = "Drill mount holes", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },
                // Base Plate
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Base plate").Id,  Name = "Cut Base plate", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Base plate").Id, Name = "drill holes for axis mount", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill 1").MachineGroupId, HierarchyNumber = 20 },

            };
            context.WorkSchedules.AddRange(workSchedule);
            context.SaveChanges();



            var articleBom = new List<ArticleBom>
            {
                // Final Products 
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Dump-Truck" },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Race-Truck").Id, Name = "Race-Truck" },
                // Bom For DumpTruck
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis Type: Dump").Id, Name = "Chassis Type: Dump", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Truck-Bed", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },

                // Bom For Race Truck
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis Type: Race").Id, Name = "Chassis Type: Race", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Race Wing").Id, Name = "Race Wing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Race-Truck").Id },
                

                // Bom for Skeleton
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Base plate").Id, Name = "Base plate", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Wheel").Id, Name = "Wheel", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Semitrailer").Id, Name = "Semitrailer", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=5, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Washer").Id, Name = "Washer", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                
                // Bom For Chassis Dump
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Dump").Id },

                // Bom For Chassis Race
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Cabin").Id, Name = "Cabin", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine-Block").Id, Name = "Engine-Block", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Engine Race Extension").Id, Name = "Engine Race Extension", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Chassis Type: Race").Id },


                // Bom for Truck-Bed
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Side wall long").Id, Name = "Side wall long", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Side wall short").Id, Name = "Side wall short", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id, Name = "Base plate Truck-Bed", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dump Joint").Id, Name = "Dump Joint", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=10, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Button").Id, Name = "Knop", Quantity=2, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Glue").Id, Name = "Glue", Quantity=7, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pole").Id, Name = "Pole", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Truck-Bed").Id },

                // Bom for some Assemblies
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Side wall long").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Side wall short").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Base plate Truck-Bed").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Base plate").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Plate 1,5m x 3,0m").Id, Name = "Timber Plate 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Race Wing").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Cabin").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Engine-Block").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber Block 0,20m x 0,20m").Id, Name = "Timber Block 0,20m x 0,20m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Engine Race Extension").Id },

            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new BusinessPartner(){ Debitor = true,Kreditor = false,Name = "Toys'R'us toy department" };
            var businessPartner2 = new BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new ArticleToBusinessPartner[]
            {
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Skeleton").Id,PackSize = 10,Price = 20.00, DueTime = 2880},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Truck-Bed").Id,PackSize = 10,Price = 20.00, DueTime = 2880},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Chassis Type: Dump").Id, PackSize = 10,Price = 20.00, DueTime = 2880},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Chassis Type: Race").Id, PackSize = 10,Price = 25.00, DueTime = 2880},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Cabin").Id,PackSize = 10,Price = 1.75, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Engine-Block").Id, PackSize = 10,Price = 0.40, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Engine Race Extension").Id, PackSize = 10,Price = 1.00, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Side wall long").Id,PackSize = 10,Price = 0.55, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Side wall short").Id,PackSize = 10,Price = 0.45, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Base plate Truck-Bed").Id, PackSize = 10,Price = 0.40, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Dump Joint").Id /*Kippgelenk*/,PackSize = 50,Price = 0.90, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Wheel").Id, PackSize = 150,Price = 0.35, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Base plate").Id, PackSize = 10,Price = 0.80, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Semitrailer" /*Aufleger*/).Id, PackSize = 25,Price = 0.10, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Race Wing").Id, PackSize = 10,Price = 1.50, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Washer").Id,PackSize = 150,Price = 0.02, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Timber Plate 1,5m x 3,0m").Id, PackSize = 100,Price = 0.20, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Timber Block 0,20m x 0,20m").Id, PackSize = 100,Price = 0.20, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Glue").Id, PackSize = 1000,Price = 0.01, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Pegs").Id, PackSize = 200,Price = 0.01, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Pole").Id, PackSize = 200,Price = 0.25, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Button").Id, PackSize = 500,Price = 0.05, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="Packing").Id, PackSize = 50,Price = 2.50, DueTime = 1440},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name=="User Manual").Id, PackSize = 50,Price = 0.20, DueTime = 1440},

            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

            var order1 = new Order
            {
                BusinessPartnerId = businessPartner.Id,
                DueTime = 1540,
                Name = "Race-Truck",
                CreationTime = 0
            };
            var order2 = new Order
            {
                BusinessPartnerId = businessPartner.Id,
                DueTime = 1690,
                Name = "Dump-Truck",
                CreationTime = 0
            };
            //create order
            var orders = new List<Order>() { 
                order1, order2
            };
            context.Orders.AddRange(orders);
            context.SaveChanges();

            //create orderParts
            var orderParts = new List<OrderPart>()
            {
                new OrderPart(){Quantity = 1, ArticleId = articles.Single(a => a.Name == "Race-Truck").Id, OrderId = order1.Id, IsPlanned = false},
                new OrderPart(){Quantity = 1, ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, OrderId = order2.Id, IsPlanned = false},
            };
            context.OrderParts.AddRange(orderParts);
            context.SaveChanges();

            var simConfig = new SimulationConfiguration
            {
                Name = "Test config",
                Lotsize = 1,
                MaxCalculationTime = 3000, // test  // 10080, // 7 days
                OrderQuantity = 12,
                Seed = 1337,
                TimeSpanForOrders = 1,
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 7190,
                DecentralRuns = 0,
                CentralRuns = 0
            };
            context.SimulationConfigurations.Add(simConfig);
            context.SaveChanges();
        }
    }
}
