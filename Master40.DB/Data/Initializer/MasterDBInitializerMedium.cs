
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerMedium
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
                new M_ArticleType {Name = "Consumable"}
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

            var machines = new M_Machine[] {
                //new Machine{Capacity=1, Name="Säge", Count = 1, MachineGroup = new MachineGroup{ Name = "Zuschnitt" } },
                //new Machine{Capacity=1, Name="Bohrer", Count = 1, MachineGroup = new MachineGroup{ Name = "Bohrwerk" } },
                new M_Machine{Capacity=1, Name="Montage 1", Count=1, MachineGroup = new M_MachineGroup{ Name = "Montage" }},
                new M_Machine{Capacity=1, Name="Montage 2", Count=1, MachineGroup = new M_MachineGroup{ Name = "Montage" }}
            };
            context.Machines.AddRange(machines);
            context.SaveChanges();

            var machineTools = new M_MachineTool[]
            {
                new M_MachineTool{MachineId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=1, Name="Sägeblatt 1mm Zahnabstant"},
                new M_MachineTool{MachineId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=1, Name="M6 Bohrkopf"},
            };
            context.MachineTools.AddRange(machineTools);
            context.SaveChanges();

            // Articles
            var articles = new M_Article[]
            {
                new M_Article{Name="Tisch",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToBuild = true, ToPurchase = false },
                new M_Article{Name="Tisch-Platte",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 10.00, ToBuild = true, ToPurchase = false },
                new M_Article{Name="Tisch-Bein",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToBuild = true, ToPurchase = false },
                new M_Article{Name="Holz 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5, ToBuild = false, ToPurchase = true  },
                new M_Article{Name="Schrauben", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.05, ToBuild = false, ToPurchase = true },
            };
            context.Articles.AddRange(articles);
            context.SaveChanges();

            // get the name -> id mappings
            var DBArticles = context
              .Articles
              .ToDictionary(p => p.Name, p => p.Id);


            // create Stock Entrys for each Article
            foreach (var article in DBArticles)
            {
                var stocks = new M_Stock[]
                {
                    new M_Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == "Schrauben")? 50 : 0,
                        Max = 100,
                        Current = (article.Key == "Schrauben")? 100 : 0
                    }
                };
                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }

            var articleBom = new List<M_ArticleBom>
            {
                // Tisch
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch" },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Tisch-Platte", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Tisch-Bein", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Schrauben").Id, Name = "Schrauben", Quantity=8, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                // Tisch-Platte
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch-Platte").Id },
                // Tisch-Bein
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch-Bein").Id },


            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();

            var workSchedule = new M_Operation[]
            {
                // Tisch 
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch Montage", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                 
                // Tisch Platte
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Zuschneiden", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Löcher vorbohren", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 20 },
                // Tisch Beine 
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Zuschneiden", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                // new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Löcher vorbohren", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 20 },

            };
            context.Operations.AddRange(workSchedule);
            context.SaveChanges();



            //create Businesspartner
            var businessPartner = new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us Spielwarenabteilung" };
            var businessPartner2 = new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material Großhandel" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Holz 1,5m x 3,0m").Id, PackSize = 1, Price = 1, DueTime = 2 },
            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

            var order = new T_CustomerOrder
            {
                BusinessPartnerId = businessPartner.Id,
                CreationTime = 10,
                Name = "BeispielOrder 1",
                DueTime = 30
            };
            context.Add(order);
            context.SaveChanges();
            var orderPart = new T_CustomerOrderPart
            {
                ArticleId = 1,
                Quantity = 1,
                OrderId = order.Id
            };
            context.Add(orderPart);
            context.SaveChanges();
            // var order2 = new Order
            // {
            //     BusinessPartnerId = businessPartner.Id,
            //     CreationTime = 0,
            //     Name = "BeispielOrder 2",
            //     DueTime = 50
            // };
            // context.Add(order2);
            // context.SaveChanges();
            // var orderPart2 = new OrderPart
            // {
            //     ArticleId = 1,
            //     Quantity = 1,
            //     OrderId = order2.Id
            // };
            // context.Add(orderPart2);
            // context.SaveChanges();

        }
    }
}