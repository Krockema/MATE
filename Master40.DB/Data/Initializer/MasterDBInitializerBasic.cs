using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.DB.Enums;

namespace Master40.DB.Data.Initializer
{
    public static class MasterDBInitializerBasic
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

            // Erstelle MachinenTools mit SetupTimes
            var machineTools = new MachineTool[]
            {
                // Add MachineToolsToMachineGroup
                // Backup    new MachineTool{MachineToolId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=10, Name="Sägeblatt 1mm Zahnabstant"},
                // Für jedes Werkzeug,z.B. Saw1 für 1mm und Saw2 für 2mm
                new MachineTool{Name = "Saw1", SetupTime=10, Discription="Sägeblatt 1mm Zahnabstant"},
                new MachineTool{Name = "Drillhead1", SetupTime=5, Discription="M6 Bohrkopf"},
                new MachineTool{Name = "Montage", SetupTime=2, Discription="Greifarm"},
            };

            context.MachineTools.AddRange(machineTools);
            context.SaveChanges();

            //map MachineTools to Machines
            var machineGroupToolsCut = new List<MachineTool>
            {
                machineTools.Single(a => a.Name == "Saw1")
            };

            var machineGroupToolsDrill = new List<MachineTool>
            {
                machineTools.Single(a => a.Name == "MachineTool")
            };

            var machineGroupToolsCutAndDrill = new List<MachineTool>() {
                machineTools.Single(a => a.Name == "Saw1"),
                machineTools.Single(a => a.Name == "Drillhead1")
            };

            var machineGroupToolsMontage = new List<MachineTool>
            {
                machineTools.Single(a => a.Name == "Montage")
            };

            var machinegroups = new MachineGroup[] {

               new MachineGroup{ Name = "MachineGroupCut", MachineTools = machineGroupToolsCut},
               new MachineGroup{ Name = "MachineGroupDrill", MachineTools = machineGroupToolsDrill },
               new MachineGroup{ Name = "MachineGroupCutAndDrill", MachineTools = machineGroupToolsCutAndDrill },
               new MachineGroup{ Name = "MachineGroupMontage", MachineTools = machineGroupToolsMontage},

            };

            context.MachineGroups.AddRange(machinegroups);
            context.SaveChanges();

            var machines = new Machine[] {
                //new Machine{Capacity=1, Name="Säge", Count = 1, MachineGroup = new MachineGroup{ Name = "Zuschnitt" } },
                //new Machine{Capacity=1, Name="Bohrer", Count = 1, MachineGroup = new MachineGroup{ Name = "Bohrwerk" } },
                //new Machine{Capacity=1, Name ="Machine Allrounder", Count=1, MachineGroup = new MachineGroup{ Name = "MachineGroupAllrounder" }},
                new Machine{Capacity=1, Name="Machine Allrounder", Count=1, MachineGroup = machinegroups.Single(a => a.Name =="MachineGroupCutAndDrill")},
                //new Machine{Capacity=1, Name="Machine Cut", Count=1, MachineGroup = machinegroups.Single(a => a.Name =="MachineGroupCut")},
                //new Machine{Capacity=1, Name="Machine Drill", Count=1, MachineGroup = machinegroups.Single(a => a.Name == "MachineGroupDrill" )},
                new Machine{Capacity=1, Name="Machine Montage", Count=1, MachineGroup = machinegroups.Single(a => a.Name == "MachineGroupMontage")},

            };
            context.Machines.AddRange(machines);
            context.SaveChanges();

            // Articles
            var articles = new Article[]
            {
                new Article{Name="Tisch",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Tisch-Platte",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 10.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Tisch-Bein",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Holz 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1, ToBuild = false, ToPurchase = true  },
                new Article{Name="Schrauben", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1, ToBuild = false, ToPurchase = true },
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
                var stocks = new Stock[]
                {
                    new Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = (article.Key == "Schrauben" || article.Key == "Holz 1,5m x 3,0m")? 300 : 0,
                        Max = 500,
                        Current = (article.Key == "Schrauben" || article.Key == "Holz 1,5m x 3,0m")? 300 : 0
                    }
                };
                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }

            var articleBom = new List<ArticleBom>
            {
                // Tisch
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch" },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Tisch-Platte", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Tisch-Bein", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Schrauben").Id, Name = "Schrauben", Quantity=8, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                // Tisch-Platte
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch-Platte").Id },
                // Tisch-Bein
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch-Bein").Id },

            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();

            var workSchedule = new WorkSchedule[]
            {
                // Tisch 
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch Montage", Duration=100, MachineGroupId=machines.Single(n=> n.Name=="Machine Montage").MachineGroupId, HierarchyNumber = 10, MachineToolId = machineTools.Single(a => a.Name == "Montage").Id },
                 
                // Tisch Platte
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Zuschneiden", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 10, MachineToolId =machineTools.Single(a => a.Name == "Saw1").Id },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Löcher vorbohren", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 20, MachineToolId=machineTools.Single(a => a.Name == "Drillhead1").Id },
                // Tisch Beine 
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Zuschneiden", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 10, MachineToolId=machineTools.Single(a => a.Name == "Saw1").Id },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Löcher vorbohren", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 20, MachineToolId=machineTools.Single(a => a.Name == "Drillhead1").Id },
            };

            context.WorkSchedules.AddRange(workSchedule);
            context.SaveChanges();


            //create Businesspartner
            var buisnessPartnerList = new List<BusinessPartner>();
            buisnessPartnerList.Add(new BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us Spielwarenabteilung" });
            buisnessPartnerList.Add(new BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material Großhandel" });
            foreach (BusinessPartner b in buisnessPartnerList)
            {
                context.BusinessPartners.Add(b);
            }
            context.SaveChanges();

            var artToBusinessPartner = new ArticleToBusinessPartner[]
            {
                new ArticleToBusinessPartner{ BusinessPartnerId = buisnessPartnerList[1].Id, ArticleId = articles.Single(x => x.Name == "Holz 1,5m x 3,0m").Id, PackSize = 1, Price = 1, DueTime = 2 },
                new ArticleToBusinessPartner{ BusinessPartnerId = buisnessPartnerList[1].Id, ArticleId = articles.Single(x => x.Name == "Schrauben").Id, PackSize = 25, Price = 2, DueTime = 2 },
            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

            // Sim Config
            var simConfig = new SimulationConfiguration
            {
                Name = "Test config",
                //LotsizeType = LotsizeType.LotsizeStatic, //not active
                Lotsize = 5, //not active 
                MaxCalculationTime = 120, // test  // 10080, // 7 days
                OrderQuantity = 0,
                Seed = 1338,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 2500,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 120,
                SettlingStart = 0,
                WorkTimeDeviation = 0.2 //deviation for Workschedule from 0 - 1 with, with 0.2 = 20%

            };

            context.SimulationConfigurations.Add(simConfig);
            context.SaveChanges();


            var order = new Order
            {
                BusinessPartnerId = buisnessPartnerList[0].Id,
                CreationTime = 0,
                Name = "BeispielOrder 1",
                DueTime = 350
            };
            context.Add(order);
            context.SaveChanges();
            var orderPart = new OrderPart
            {
                ArticleId = 1,
                Quantity = 2,
                OrderId = order.Id
            };
            context.Add(orderPart);
            context.SaveChanges();
            var order2 = new Order
            {
                BusinessPartnerId = buisnessPartnerList[0].Id,
                CreationTime = 300,
                Name = "BeispielOrder 2",
                DueTime = 1500
            };
            context.Add(order2);
            context.SaveChanges();
            var orderPart2 = new OrderPart
            {
                ArticleId = 1,
                Quantity = 3,
                OrderId = order2.Id
            };
            context.Add(orderPart2);
            context.SaveChanges();

        }
    }
}