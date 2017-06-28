using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DB.Models;

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
            foreach (ArticleType at in articleTypes)
            {
                context.ArticleTypes.Add(at);
            }
            context.SaveChanges();

            // Units
            var units = new Unit[]
            {
                new Unit {Name = "Kilo"},
                new Unit {Name = "Litre"},
                new Unit {Name = "Pieces"}
            };
            foreach (Unit u in units)
            {
                context.Units.Add(u);
            }
            context.SaveChanges();

            var machines = new Machine[] {
                new Machine{Capacity=1, Name="Saw", Count = 1, MachineGroup = new MachineGroup{ Name = "Cutting" } },
                new Machine{Capacity=1, Name="Drill", Count = 1, MachineGroup = new MachineGroup{ Name = "Drills" } },
                new Machine{Capacity=1, Name="AssemblyUnit", Count=1, MachineGroup = new MachineGroup{ Name = "AssemblyUnits" }}
            };
            foreach (var m in machines)
            {
                context.Machines.Add(m);
            }
            context.SaveChanges();

            var machineTools = new MachineTool[]
            {
                new MachineTool{MachineId=machines.Single(m => m.Name == "Saw").Id, SetupTime=1, Name="Saw blade"},
                new MachineTool{MachineId=machines.Single(m => m.Name == "Drill").Id, SetupTime=1, Name="M6 head"},
            };
            foreach (var mt in machineTools)
            {
                context.MachineTools.Add(mt);
            }
            context.SaveChanges();

            // Articles
            var articles = new Article[]
            {
            new Article{Name="Dump-Truck",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00},
            new Article{Name="Skeleton",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},
            new Article{Name="Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},
            new Article{Name="Chassis", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},

            new Article{Name="Wheel", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00},
            new Article{Name="Rim", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 2, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00},
            new Article{Name="Base plate", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 4.00},
            new Article{Name="Semitrailer" /*Aufleger*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00},
            
            new Article{Name="Side wall long", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.50},
            new Article{Name="Side wall short", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 2.00},
            new Article{Name="Base plate Truck-Bed", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 4.00},
            new Article{Name="Cabin", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00},
            new Article{Name="Engine block", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00},
            new Article{Name="Axis", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.50},
            new Article{Name="Button", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05},
            new Article{Name="Dump Joint" /*Kippgelenk*/, ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1},
            new Article{Name="Timber 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5},

            new Article{Name="Washer", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10,UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05},
            new Article{Name="Glue", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 5.00},
            new Article{Name="Pegs", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 3.00},
            new Article{Name="Packing", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 7.00},
            new Article{Name="User Manual", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.50},

            };
            foreach (Article a in articles)
            {
                context.Articles.Add(a);
            }
            context.SaveChanges();

            // get the name -> id mappings
            var dbArticles = context
              .Articles
              .ToDictionary(p => p.Name, p => p.Id);


            // create Stock Entrys for each Article
            foreach (var article in dbArticles)
            {
                var Stocks = new Stock[]
                {
                    new Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == "Dump-Truck") ? 1 : 0,
                        Max = 50,
                        Current = (article.Key == "Dump-Truck") ? 1 : 0
            }
                };
                foreach (Stock s in Stocks)
                {
                    context.Stocks.Add(s);
                }
                context.SaveChanges();
            }
            var workSchedule = new WorkSchedule[]
            {
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Wedding", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Glue Truck-Bed", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit").MachineGroupId, HierarchyNumber = 20 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id,  Name = "Cut Skeleton", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Saw").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "drill holes for axis mount", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Drill").MachineGroupId, HierarchyNumber = 20 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Cut Truck-Bed", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Saw").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Screw axis with skeleton", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit").MachineGroupId, HierarchyNumber = 30 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Wheel").Id, Name = "Mount rim onto wheel", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Wheel").Id, Name = "Screw wheel with axis", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="AssemblyUnit").MachineGroupId, HierarchyNumber = 20 },

            };
            foreach (var ws in workSchedule)
            {
                context.WorkSchedules.Add(ws);
            }
            context.SaveChanges();



            var articleBom = new List<ArticleBom>
            {
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dump-Truck").Id, Name = "Dump-Truck" },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Skeleton").Id, Name = "Skeleton", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis").Id, Name = "Chassis", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Truck-Bed").Id, Name = "Truck-Bed", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Dump-Truck").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Base plate").Id, Name = "Base plate", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Axis").Id, Name = "Axis", Quantity=2,
                                ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Wheel").Id, Name = "Wheel", Quantity=4,
                                ArticleParentId = articles.Single(a => a.Name == "Skeleton").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Rim").Id, Name = "Rim", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Wheel").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Pegs").Id, Name = "Pegs", Quantity=2,
                                ArticleParentId = articles.Single(a => a.Name == "Wheel").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Timber 1,5m x 3,0m").Id, Name = "Timber 1,5m x 3,0m", Quantity=4,
                                ArticleParentId = articles.Single(a => a.Name == "Base plate").Id },
            };
            
        
            foreach (var item in articleBom)
            {
                context.ArticleBoms.Add(item);
            }
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new BusinessPartner(){Debitor = true,Kreditor = false,Name = "Toys'R'us toy department"};
            var businessPartner2 = new BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new ArticleToBusinessPartner[]
            {
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Pegs").Id , PackSize = 100, Price = 0.10, DueTime = 10},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Chassis").Id,PackSize = 1, Price = 2, DueTime = 30},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Truck-Bed").Id,PackSize = 1, Price = 3, DueTime = 10},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Axis").Id,PackSize = 10, Price = 0.50, DueTime = 10},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Timber 1,5m x 3,0m").Id,PackSize = 1, Price = 1, DueTime = 10},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Rim").Id, PackSize = 10,Price = 0.20, DueTime = 10},
            };
            foreach (var art in artToBusinessPartner)
            {
                context.ArticleToBusinessPartners.Add(art);
            }
            context.SaveChanges();

            //create order
            var orders = new List<Order>() { 
                new Order {BusinessPartnerId = businessPartner.Id, DueTime = 2880, Name = "First Truck order"},
            };
            foreach (var order in orders)
            {
                context.Orders.Add(order);
            }
            context.SaveChanges();

            //create orderParts
            var orderParts = new List<OrderPart>()
            {
                new OrderPart(){Quantity = 3, ArticleId = articles.Single(a => a.Name == "Dump-Truck").Id, OrderId = 1, IsPlanned = false},
            };
            foreach (var orderPart in orderParts)
            {
                context.OrderParts.Add(orderPart);
            }
            context.SaveChanges();


        }
    }
}
