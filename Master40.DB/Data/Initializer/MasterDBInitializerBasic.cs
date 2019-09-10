using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
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


            var machineGroup = new M_MachineGroup { Name = "All", Stage = 1, ImageUrl = "/images/Production/saw.svg" };

            var machines = new M_Resource[] {
                new M_Resource{Capacity=1, Name="Saw 1", Count = 1, MachineGroup = machineGroup },
            };
            context.Resources.AddRange(machines);
            context.SaveChanges();

            var ressourceTools = new M_ResourceTool[]
            {
                new M_ResourceTool{Name="Saw blade"},
            };
            context.ResourceTools.AddRange(ressourceTools);
            context.SaveChanges();
            // // Erstelle MachinenTools mit SetupTimes
            // var machineTools = new MachineTool[]
            // {
            //     // Add MachineToolsToMachineGroup
            //     // Backup    new MachineTool{MachineToolId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=10, Name="Sägeblatt 1mm Zahnabstant"},
            //     // Für jedes Werkzeug,z.B. Saw1 für 1mm und Saw2 für 2mm
            //     new MachineTool{Name = "Saw1", SetupTime=10, Discription="Sägeblatt 1mm Zahnabstant"},
            //     new MachineTool{Name = "Drillhead1", SetupTime=5, Discription="M6 Bohrkopf"},
            //     new MachineTool{Name = "Montage", SetupTime=2, Discription="Greifarm"},
            // };
            // 
            // context.MachineTools.AddRange(machineTools);
            // context.SaveChanges();
            // 
            // //map MachineTools to Machines
            // var machineGroupToolsCut = new List<MachineTool>
            // {
            //     machineTools.Single(a => a.Name == "Saw1")
            // };
            // 
            // var machineGroupToolsDrill = new List<MachineTool>
            // {
            //     machineTools.Single(a => a.Name == "MachineTool")
            // };
            // 
            // var machineGroupToolsCutAndDrill = new List<MachineTool>() {
            //     machineTools.Single(a => a.Name == "Saw1"),
            //     machineTools.Single(a => a.Name == "Drillhead1")
            // };
            // 
            // var machineGroupToolsMontage = new List<MachineTool>
            // {
            //     machineTools.Single(a => a.Name == "Montage")
            // };
            // 
            // var machinegroups = new MachineGroup[] {
            // 
            //    new MachineGroup{ Name = "MachineGroupCut", MachineTools = machineGroupToolsCut},
            //    new MachineGroup{ Name = "MachineGroupDrill", MachineTools = machineGroupToolsDrill },
            //    new MachineGroup{ Name = "MachineGroupCutAndDrill", MachineTools = machineGroupToolsCutAndDrill },
            //    new MachineGroup{ Name = "MachineGroupMontage", MachineTools = machineGroupToolsMontage},
            // 
            // };
            // 
            // context.MachineGroups.AddRange(machinegroups);
            // context.SaveChanges();

            // var machines = new Machine[] {
            //     //new Machine{Capacity=1, Name="Säge", Count = 1, MachineGroup = new MachineGroup{ Name = "Zuschnitt" } },
            //     //new Machine{Capacity=1, Name="Bohrer", Count = 1, MachineGroup = new MachineGroup{ Name = "Bohrwerk" } },
            //     //new Machine{Capacity=1, Name ="Machine Allrounder", Count=1, MachineGroup = new MachineGroup{ Name = "MachineGroupAllrounder" }},
            //     new Machine{Capacity=1, Name="Machine Allrounder", Count=1, MachineGroup = machinegroups.Single(a => a.Name =="MachineGroupCutAndDrill")},
            //     //new Machine{Capacity=1, Name="Machine Cut", Count=1, MachineGroup = machinegroups.Single(a => a.Name =="MachineGroupCut")},
            //     //new Machine{Capacity=1, Name="Machine Drill", Count=1, MachineGroup = machinegroups.Single(a => a.Name == "MachineGroupDrill" )},
            //     new Machine{Capacity=1, Name="Machine Montage", Count=1, MachineGroup = machinegroups.Single(a => a.Name == "MachineGroupMontage")},
            // 
            // };
            // context.Machines.AddRange(machines);
            // context.SaveChanges();

            // Articles
            var articles = new M_Article[]
            {
                // new Article{Name="Tisch",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToBuild = true, ToPurchase = false },
                new M_Article{Name="Tisch-Platte",ArticleTypeId = articleTypes.Single( s => s.Name == "Product").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 10.00, ToBuild = true, ToPurchase = false,  },
                // new Article{Name="Tisch-Bein",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToBuild = true, ToPurchase = false },
                new M_Article{Name="Holz 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1, ToBuild = false, ToPurchase = true  },
                new M_Article{Name="Schrauben", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 1, ToBuild = false, ToPurchase = true },
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
                        Min = (article.Key == "Schrauben" || article.Key == "Holz 1,5m x 3,0m")? 300 : 0,
                        Max = 500,
                        Current = (article.Key == "Schrauben" || article.Key == "Holz 1,5m x 3,0m")? 300 : 0
                    }
                };
                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }

            var articleBom = new List<M_ArticleBom>
            {
                // Tisch
                //new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch" },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Tisch-Platte", Quantity=1, /* ArticleParentId = articles.Single(a => a.Name == "Tisch").Id */ },
                // new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Tisch-Bein", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch").Id },
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Schrauben").Id, Name = "Schrauben", Quantity=8, ArticleParentId = articles.Single(a => a.Name == "Tisch-Platte").Id },
                // Tisch-Platte
                new M_ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch-Platte").Id },
                // Tisch-Bein
                // new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 1,5m x 3,0m").Id, Name = "Holz 1,5m x 3,0m", Quantity=4, ArticleParentId = articles.Single(a => a.Name == "Tisch-Bein").Id },

            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();

            var workSchedule = new M_Operation[]
            {
                // Tisch 
                //new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch Montage", Duration=100, MachineGroupId=machines.Single(n=> n.Name=="Machine Montage").MachineGroupId, HierarchyNumber = 10, MachineToolId = machineTools.Single(a => a.Name == "Montage").Id },
                 
                // Tisch Platte
                new M_Operation{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Zuschneiden", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="Saw 1").MachineGroupId, HierarchyNumber = 10},
                // new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Löcher vorbohren", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 20, MachineToolId=machineTools.Single(a => a.Name == "Drillhead1").Id },
                // Tisch Beine 
                // new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Zuschneiden", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 10, MachineToolId=machineTools.Single(a => a.Name == "Saw1").Id },
                // new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Löcher vorbohren", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Machine Allrounder").MachineGroupId, HierarchyNumber = 20, MachineToolId=machineTools.Single(a => a.Name == "Drillhead1").Id },
            };

            context.Operations.AddRange(workSchedule);
            context.SaveChanges();


            //create Businesspartner
            var buisnessPartnerList = new List<M_BusinessPartner>();
            buisnessPartnerList.Add(new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us Spielwarenabteilung" });
            buisnessPartnerList.Add(new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material Großhandel" });
            foreach (M_BusinessPartner b in buisnessPartnerList)
            {
                context.BusinessPartners.Add(b);
            }
            context.SaveChanges();

            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner{ BusinessPartnerId = buisnessPartnerList[1].Id, ArticleId = articles.Single(x => x.Name == "Holz 1,5m x 3,0m").Id, PackSize = 1, Price = 1, DueTime = 2 },
                new M_ArticleToBusinessPartner{ BusinessPartnerId = buisnessPartnerList[1].Id, ArticleId = articles.Single(x => x.Name == "Schrauben").Id, PackSize = 25, Price = 2, DueTime = 2 },
            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

            var customerOrder = new T_CustomerOrder
            {
                BusinessPartnerId = buisnessPartnerList[0].Id,
                CreationTime = 0,
                Name = "BeispielOrder 1",
                DueTime = 10
            };
            context.Add(customerOrder);
            context.SaveChanges();
            var customerOrderPart = new T_CustomerOrderPart
            {
                ArticleId = 1,
                Quantity = 1,
                CustomerOrderId = customerOrder.Id
            };
            context.Add(customerOrderPart);
            context.SaveChanges();
            var customerOrder2 = new T_CustomerOrder
            {
                BusinessPartnerId = buisnessPartnerList[0].Id,
                CreationTime = 0,
                Name = "BeispielOrder 2",
                DueTime = 10
            };
            context.Add(customerOrder2);
            context.SaveChanges();
            var customerOrderPart2 = new T_CustomerOrderPart
            {
                ArticleId = 1,
                Quantity = 1,
                CustomerOrderId = customerOrder2.Id
            };
            context.Add(customerOrderPart2);
            context.SaveChanges();

        }
    }
}