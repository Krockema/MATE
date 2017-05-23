using Master40.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.DB.Data.Context
{
    public static class MasterDBInitializerSmall
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
                new Machine{Capacity=1, Name="Säge", Count = 1, MachineGroup = new MachineGroup{ Name = "Zuschnitt" } },
                new Machine{Capacity=1, Name="Bohrer", Count = 1, MachineGroup = new MachineGroup{ Name = "Bohrwerk" } },
                new Machine{Capacity=1, Name="MontagePlatform", Count=1, MachineGroup = new MachineGroup{ Name = "Montage" }}
            };
            foreach (var m in machines)
            {
                context.Machines.Add(m);
            }
            context.SaveChanges();

            var machineTools = new MachineTool[]
            {
                new MachineTool{MachineId=machines.Single(m => m.Name == "Säge").Id, SetupTime=1, Name="Sägeblatt 1mm Zahnabstant"},
                new MachineTool{MachineId=machines.Single(m => m.Name == "Bohrer").Id, SetupTime=1, Name="M6 Bohrkopf"},
            };
            foreach (var mt in machineTools)
            {
                context.MachineTools.Add(mt);
            }
            context.SaveChanges();

            // Articles
            var articles = new Article[]
            {
            new Article{Name="Kipper",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00},
            new Article{Name="Rahmengestell",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},
            new Article{Name="Ladebehälter", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},
            new Article{Name="Chassis", ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 15.00},

            new Article{Name="Rad", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00},
            new Article{Name="Felge", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 2, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.00},
            new Article{Name="Bodenplatte", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 4.00},
            new Article{Name="Aufliegeplatte", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00},
            
            new Article{Name="Seitenwand land", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1.50},
            new Article{Name="Seitenwand kurz", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 2.00},
            new Article{Name="Bodenplatte Behälter", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 4.00},
            new Article{Name="Fahrerhaus", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00},
            new Article{Name="Motorblock", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 3.00},
            new Article{Name="Achse", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.50},
            new Article{Name="Knopf", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05},
            new Article{Name="Kippgelenk", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1},
            new Article{Name="Holz 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5},

            new Article{Name="Unterlegscheibe", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10,UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.05},
            new Article{Name="Leim", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 5.00},
            new Article{Name="Dübel", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 3.00},
            new Article{Name="Verpackung", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 7.00},
            new Article{Name="Bedienungsanleitung", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitId = units.Single( s => s.Name == "Kilo").Id, Price = 0.50},

            };
            foreach (Article a in articles)
            {
                context.Articles.Add(a);
            }
            context.SaveChanges();

            // get the name -> id mappings
            var DBArticles = context
              .Articles
              .ToDictionary(p => p.Name, p => p.Id);


            // create Stock Entrys for each Article
            foreach (var article in DBArticles)
            {
                var Stocks = new Stock[]
                {
                    new Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == "Kipper") ? 1 : 0,
                        Max = 50,
                        Current = (article.Key == "Kipper") ? 1 : 0
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
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Kipper").Id, Name = "Kipper Hochzeit", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="MontagePlatform").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Kipper").Id, Name = "Kipper Ladefläche Kleben", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="MontagePlatform").MachineGroupId, HierarchyNumber = 20 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Rahmengestell").Id,  Name = "Rahmen zuschneiden", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Säge").MachineGroupId
                                  , HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Rahmengestell").Id, Name = "Löcher für Achse in den Rahmen bohren", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Bohrer").MachineGroupId
                                  , HierarchyNumber = 20 },
                
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Bodenplatte").Id, Name = "Bodenplatte zuschneiden", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Säge").MachineGroupId
                                    , HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Rahmengestell").Id, Name = "Achse mit Rahmen Verschrauben", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="MontagePlatform").MachineGroupId, HierarchyNumber = 30 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Rad").Id, Name = "Felge auf Rad Aufziehen", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="MontagePlatform").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Rad").Id, Name = "Rad mit Achse verschrauben", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="MontagePlatform").MachineGroupId, HierarchyNumber = 20 },

            };
            foreach (var ws in workSchedule)
            {
                context.WorkSchedules.Add(ws);
            }
            context.SaveChanges();



            var articleBom = new List<ArticleBom>
            {
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Kipper").Id, Name = "Kipper" },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Rahmengestell").Id, Name = "Rahmengestell", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Kipper").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Chassis").Id, Name = "Chassis", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Kipper").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Ladebehälter").Id, Name = "Ladebehälter", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Kipper").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Bodenplatte").Id, Name = "Bodenplatte", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Rahmengestell").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Achse").Id, Name = "Achse", Quantity=2,
                                ArticleParentId = articles.Single(a => a.Name == "Rahmengestell").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Rad").Id, Name = "Rad", Quantity=4,
                                ArticleParentId = articles.Single(a => a.Name == "Rahmengestell").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Felge").Id, Name = "Felge", Quantity=1,
                                ArticleParentId = articles.Single(a => a.Name == "Rad").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Dübel").Id, Name = "Dübel", Quantity=2,
                                ArticleParentId = articles.Single(a => a.Name == "Rad").Id },

                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=4,
                                ArticleParentId = articles.Single(a => a.Name == "Bodenplatte").Id },
            };
            
        
            foreach (var item in articleBom)
            {
                context.ArticleBoms.Add(item);
            }
            context.SaveChanges();


            //create Businesspartner
            var businessPartner = new BusinessPartner(){Debitor = true,Kreditor = false,Name = "Toys'R'us Spielwarenabteilung"};
            var businessPartner2 = new BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material Großhandel" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new ArticleToBusinessPartner[]
            {
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Dübel").Id , PackSize = 100, Price = 0.10, DueTime = 2},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Chassis").Id,PackSize = 1, Price = 2, DueTime = 2},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Ladebehälter").Id,PackSize = 1, Price = 3, DueTime = 2},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Achse").Id,PackSize = 10, Price = 0.50, DueTime = 2},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Holz 1,5m x 3,0m").Id,PackSize = 1, Price = 1, DueTime = 2},
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Felge").Id, PackSize = 10,Price = 0.20, DueTime = 2},
            };
            foreach (var art in artToBusinessPartner)
            {
                context.ArticleToBusinessPartners.Add(art);
            }
            context.SaveChanges();

            //create order
            var orders = new List<Order>() { 
                new Order {BusinessPartnerId = businessPartner.Id, DueTime = 40, Name = "Erste Kipperbestellung"},
            };
            foreach (var order in orders)
            {
                context.Orders.Add(order);
            }
            context.SaveChanges();

            //create orderParts
            var orderParts = new List<OrderPart>()
            {
                new OrderPart(){Quantity = 2, ArticleId = articles.Single(a => a.Name == "Kipper").Id, OrderId = 1, IsPlanned = false},
            };
            foreach (var orderPart in orderParts)
            {
                context.OrderParts.Add(orderPart);
            }
            context.SaveChanges();


        }
    }
}
