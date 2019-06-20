using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Models;

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

            var machines = new Machine[] {
                //new Machine{Capacity=1, Name="Säge", Count = 1, MachineGroup = new MachineGroup{ Name = "Zuschnitt" } },
                //new Machine{Capacity=1, Name="Bohrer", Count = 1, MachineGroup = new MachineGroup{ Name = "Bohrwerk" } },
                new Machine{Capacity=1, Name="Montage 1", Count=1, MachineGroup = new MachineGroup{ Name = "Montage" }},
                new Machine{Capacity=1, Name="Montage 2", Count=1, MachineGroup = new MachineGroup{ Name = "Montage" }}
            };
            context.Machines.AddRange(machines);
            context.SaveChanges();

            var machineTools = new MachineTool[]
            {
                new MachineTool{MachineId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=1, Name="Sägeblatt 1mm Zahnabstant"},
                new MachineTool{MachineId=machines.Single(m => m.Name == "Montage 1").Id, SetupTime=1, Name="M6 Bohrkopf"},
            };
            context.MachineTools.AddRange(machineTools);
            context.SaveChanges();

            // Articles
            var articles = new Article[]
            {
                new Article{Name="Tisch",  ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 45.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Tisch-Platte",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 10.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Tisch-Bein",ArticleTypeId = articleTypes.Single( s => s.Name == "Assembly").Id, DeliveryPeriod = 10, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5.00, ToBuild = true, ToPurchase = false },
                new Article{Name="Holz 1,5m x 3,0m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5, ToBuild = false, ToPurchase = true  },
                new Article{Name="Schrauben", ArticleTypeId = articleTypes.Single( s => s.Name == "Consumable").Id, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 0.05, ToBuild = false, ToPurchase = true },
                new Article{Name="Holz 0,8m x 0,1m x 0,1m", ArticleTypeId = articleTypes.Single( s => s.Name == "Material").Id, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 5, UnitId = units.Single( s => s.Name == "Pieces").Id, Price = 5, ToBuild = false, ToPurchase = true  },
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
                        Min = (article.Key == "Schrauben")? 50 : 0,
                        Max = 100,
                        Current = (article.Key == "Schrauben")? 100 : 0
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
                new ArticleBom { ArticleChildId = articles.Single(a => a.Name == "Holz 0,8m x 0,1m x 0,1m").Id, Name = "Holz 0,8m x 0,1m x 0,1m", Quantity=1, ArticleParentId = articles.Single(a => a.Name == "Tisch-Bein").Id },
                

            };
            context.ArticleBoms.AddRange(articleBom);
            context.SaveChanges();

            var workSchedule = new WorkSchedule[]
            {
                // Tisch 
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch").Id, Name = "Tisch Montage", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                 
                // Tisch Platte
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Zuschneiden", Duration=15, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Platte").Id, Name = "Löcher vorbohren", Duration=10, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 20 },
                // Tisch Beine 
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Zuschneiden", Duration=5, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 10 },
                new WorkSchedule{ ArticleId = articles.Single(a => a.Name == "Tisch-Bein").Id, Name = "Löcher vorbohren", Duration=2, MachineGroupId=machines.Single(n=> n.Name=="Montage 1").MachineGroupId, HierarchyNumber = 20 },

            };
            context.WorkSchedules.AddRange(workSchedule);
            context.SaveChanges();



            //create Businesspartner
            var businessPartner = new BusinessPartner() { Debitor = true,Kreditor = false,Name = "Toys'R'us Spielwarenabteilung" };
            var businessPartner2 = new BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material Großhandel" };
            context.BusinessPartners.Add(businessPartner);
            context.BusinessPartners.Add(businessPartner2);
            context.SaveChanges();

            var artToBusinessPartner = new ArticleToBusinessPartner[]
            {
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Holz 1,5m x 3,0m").Id, PackSize = 1, Price = 1, DueTime = 2 },
                new ArticleToBusinessPartner{ BusinessPartnerId = businessPartner2.Id, ArticleId = articles.Single(x => x.Name == "Holz 0,8m x 0,1m x 0,1m").Id, PackSize = 1, Price = 1, DueTime = 2 },
            };
            context.ArticleToBusinessPartners.AddRange(artToBusinessPartner);
            context.SaveChanges();

            // Sim Config
            var simConfig = new SimulationConfiguration
            {
                Name = "Test config",
                Lotsize = 1,
                MaxCalculationTime = 120, // test  // 10080, // 7 days
                OrderQuantity = 0,
                Seed = 1338,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 1000,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 120,
                SettlingStart = 0,
                WorkTimeDeviation = 0

            };

            context.SimulationConfigurations.Add(simConfig);
            context.SaveChanges();


            var order = new Order
            {
                BusinessPartnerId = businessPartner.Id,
                CreationTime = 0,
                Name = "BeispielOrder 1",
                DueTime = 30
            };
            context.Add(order);
            context.SaveChanges();
            var orderPart = new OrderPart
            {
                ArticleId = 1,
                Quantity = 1,
                OrderId = order.Id
            };
            context.Add(orderPart);
            context.SaveChanges();
            var order2 = new Order
            {
                BusinessPartnerId = businessPartner.Id,
                CreationTime = 0,
                Name = "BeispielOrder 2",
                DueTime = 50
            };
            context.Add(order2);
            context.SaveChanges();
            var orderPart2 = new OrderPart
            {
                ArticleId = 1,
                Quantity = 1,
                OrderId = order2.Id
            };
            context.Add(orderPart2);
            context.SaveChanges();

            var articleMappings = new Mapping[]
            {
                new Mapping { From = "Articles.Id", To = "Material.MaterialId", ConversionFunc = "IntToString" },
                new Mapping { From = "Articles.Name", To = "Material.Name" },
                new Mapping { From = "Articles.ToPurchase", To = "Material.InhouseProduction", ConversionFunc = "BoolToLong", ConversionArgs = "true" },
                new Mapping { From = "Articles.ToBuild", To = "Material.InhouseProduction", ConversionFunc = "BoolToLong", ConversionArgs = "false" },
                new Mapping { From = "Articles.UnitId", To = "Material.QuantityUnitId", ConversionFunc = "IntToString" },
                new Mapping { From = "Articles.Price", To = "Material.ValuePurchase"},
                new Mapping { From = "Articles.Price", To = "Material.ValueSales" },
                new Mapping { From = "Articles.Price", To = "Material.ValueProduction" }
            };
            context.AddRange(articleMappings);
            context.SaveChanges();
            var unitMappings = new Mapping[]
            {
                new Mapping { From = "Units.Id", To = "Unit.UnitId", ConversionFunc = "IntToString"},
                new Mapping { From = "Units.Name", To = "Unit.Name"}
            };
            context.AddRange(unitMappings);
            context.SaveChanges();

            var machinegroupMappings = new Mapping[]
            {
                new Mapping { From = "MachineGroups.Id", To = "Workcentergroup.WorkcentergroupId", ConversionFunc = "IntToString" },
                new Mapping { From = "MachineGroups.Name", To = "Workcentergroup.Name" },
                new Mapping { From = "MachineGroups.none", To = "Workcentergroup.ParallelAllocationCriteria", ConversionFunc = "SetIntVal", ConversionArgs = "0" },
                new Mapping { From = "MachineGroups.none", To = "Workcentergroup.ParallelSchedulingType", ConversionFunc = "SetIntVal", ConversionArgs = "1" }
            };
            context.AddRange(machinegroupMappings);
            context.SaveChanges();

            var machineMappings = new Mapping[]
            {
                new Mapping { From = "Machines.Id", To = "Workcenter.WorkcenterId", ConversionFunc = "IntToString" },
                new Mapping { From = "Machines.Name", To = "Workcenter.Name" },
                new Mapping { From = "Machines.Capacity", To = "Workcenter.AllocationMax", ConversionFunc = "IntToDouble", ConversionArgs = "100" },
                new Mapping { From = "Machines.none", To = "Workcenter.ParallelAllocationCriteria", ConversionFunc = "SetIntVal", ConversionArgs = "1" }
                //workcenter.parallel_scheduling_type			-> (immer 1) // sowieso default
            };
            context.AddRange(machineMappings);
            context.SaveChanges();

            var articlebomsMappings = new Mapping[]
            {
            new Mapping { From = "ArticleBoms.Id", To = "Bom.BomId", ConversionFunc = "IntToString" },
            new Mapping { From = "ArticleBoms.Name", To = "Bom.Name" },
            //new Mapping { From = "ArticleBoms.Quantity", To = "BomItem.Quantity", ConversionFunc = "DecimalToDouble" },
            //Decimal = "ArticleBoms.Quantity", Double = "BomItem.Quantity"
            };
            context.AddRange(articlebomsMappings);
            context.SaveChanges();

            //var articlebomsitemsMappings = new Mapping[]
            //{
            //new Mapping { From = "ArticleBoms.Id", To = "BomItem.BomId", ConversionFunc = "IntToString" },
            //new Mapping { From = "ArticleBoms.ArticleChildId", To = "BomItem.ItemId", ConversionFunc = "IntToString" },
            //new Mapping { From = "ArticleBoms.none", To = "BomItem.AlternativeId", ConversionFunc = "SetStringVal", ConversionArgs = "0" },
            ////new Mapping { From = "ArticleBoms.ArticleChild.Name", To = "BomItem.Name" },
            //new Mapping { From = "ArticleBoms.Quantity", To = "BomItem.Quantity", ConversionFunc = "DecimalToDouble"},
            ////new Mapping { From = "ArticleBoms.ArticleChild.UnitId", To = "BomItem.QuantityUnitId", ConversionFunc = "IntToString" },
            //new Mapping { From = "ArticleBoms.ArticleChildId", To = "BomItem.MaterialId", ConversionFunc = "IntToString" },
            //};
            //context.AddRange(articlebomsitemsMappings);
            //context.SaveChanges();

            //var routingMappings = new Mapping[]
            //{
            //new Mapping { From = "WorkSchedule.Id", To = "Routing.RoutingId", ConversionFunc = "IntToString" },
            //new Mapping { From = "WorkSchedule.Name", To = "Routing.Name" },
            //new Mapping { From = "WorkSchedule.ArticleBoms.Id", To = "Routing.BomId", ConversionFunc = "IntToString" },
            //new Mapping { From = "WorkSchedule.HierarchyNumber ", To = "RoutingOperationActivity.ActivityId", ConversionFunc = "IntToString" },
            //new Mapping { From = "WorkSchedule.Duration", To = "Routing.BomId", ConversionFunc = "IntToString" } // ??

            ////new Mapping { From = "ProductionAgent.WorkItems.WorkSchedule.Id", To = "Routing.RoutingId", ConversionFunc = "IntToString", IsAgentData = true},
            ////new Mapping { From = "ProductionAgent.WorkItems.WorkSchedule.Name", To = "Routing.Name", IsAgentData = true},
            //};
            //context.AddRange(routingMappings);
            //context.SaveChanges();

            //var orderMappings = new Mapping[]
            //{
            //    new Mapping { From = "Orders.Id", To = "Salesorder.SalesorderId", ConversionFunc = "IntToString" },
            //    new Mapping { From = "Orders.Name", To = "Salesorder.Name" },
            //    //new Mapping { From = "Orders.State", To = "Salesorder.Status", ConversionFunc = "MasterOrderStateToGP" },
            //    //----> Order.State immer auf 0 und bei Salesorder.Status immer auf 4
            //};
            //context.AddRange(orderMappings);
            //context.SaveChanges();

            //var orderpartsMappings = new Mapping[]
            //{
            //    new Mapping { From = "OrderParts.OrderId", To = "Salesorder.SalesorderId", ConversionFunc = "IntToString" },
            //    new Mapping { From = "OrderParts.ArticleId", To = "Salesorder.MaterialId", ConversionFunc = "IntToString" },
            //    new Mapping { From = "OrderParts.Quantity", To = "Salesorder.Quantity", ConversionFunc = "IntToDouble" },
            //    //new Mapping { From = "OrderParts.State", To = "Salesorder.Status", ConversionFunc = "MasterOrderStateToGP" }
            //    //----> Order.State immer auf 0 und bei Salesorder.Status immer auf 4
            //    //salesorder.planning_type	-> (immer 1)
            //    //salesorder.value_sales      -> (immer -1)
            //    //salesorder.quantity_delivered   -> (immer 0)
            //};
            //context.AddRange(orderpartsMappings);
            //context.SaveChanges();

            //var mapping = new Mapping { From = "Stock.Min", To = "Material.SafetyStockValue", ConversionFunc = "DecimalToDouble" };
            //context.Add(mapping);
            //context.SaveChanges();

            //var contractAgentMapping = new Mapping[]
            //{
            //    new Mapping { From = "ContractAgent.requestItem.OrderId", To = "Salesorder.SalesorderId", ConversionFunc = "IntToString", IsAgentData = true},
            //    new Mapping { From = "ContractAgent.requestItem.DueTime", To = "Salesorder.Duedate", ConversionFunc = "RelativeTimeIntToDateString", IsAgentData = true},
            //    new Mapping { From = "ContractAgent.requestItem.Article.Id", To = "Salesorder.MaterialId", ConversionFunc = "IntToString", IsAgentData = true},
            //    new Mapping { From = "ContractAgent.requestItem.Quantity", To = "Salesorder.Quantity", ConversionFunc = "IntToDouble", IsAgentData = true},
            //    new Mapping { From = "ContractAgent.requestItem.Article.UnitId", To = "Salesorder.QuantityUnitId", ConversionFunc = "IntToString", IsAgentData = true},
            //    //new Mapping { From = "ContractAgent.requestItem.IDemandToProvider.State", To = "salesorder.status", ConversionFunc = "MasterStateToGPStatus", IsAgentData = true}
            //};
            //context.AddRange(contractAgentMapping);
            //context.SaveChanges();

            //var storageAgentMapping = new Mapping[]
            //{
            //    new Mapping { From = "StorageAgent.StockElement.Id", To = "Stockquantityposting.StockquantitypostingId", ConversionFunc = "IntToString", IsAgentData = true},
            //    new Mapping { From = "StorageAgent.StockElement.Article.Id", To = "Stockquantityposting.MaterialId", ConversionFunc = "IntToString", IsAgentData = true},
            //    new Mapping { From = "StorageAgent.StockElement.Current", To = "Stockquantityposting.Quantity", ConversionFunc = "DecimalToDouble", IsAgentData = true},
            //    new Mapping { From = "StorageAgent.StockElement.Article.UnitId", To = "Stockquantityposting.QuantityUnitId", ConversionFunc = "IntToString", IsAgentData = true}

            //};
            //context.AddRange(storageAgentMapping);
            //context.SaveChanges();

            var productionAgentMapping = new Mapping[]
            {
                new Mapping { From = "ProductionAgent.RequestItem.Article.Id", To = "Productionorder.MaterialId", ConversionFunc = "IntToString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.RequestItem.OrderId", To = "Productionorder.ProductionorderId", ConversionFunc = "IntToString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.RequestItem.Quantity", To = "Productionorder.QuantityGross",  ConversionFunc = "IntToDouble", IsAgentData = true},
                new Mapping { From = "ProductionAgent.RequestItem.DueTime", To = "Productionorder.Duedate", ConversionFunc = "RelativeTimeIntToDateString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.RequestItem.Article.UnitId", To = "Productionorder.QuantityUnitId", ConversionFunc = "IntToString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.RequestItem.OrderId", To = "ProductionorderOperationActivity.ProductionorderId", ConversionFunc = "IntToString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.WorkItems.WorkSchedule.Id", To = "ProductionorderOperationActivity.OperationId", ConversionFunc = "IntToString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.none", To = "ProductionorderOperationActivity.ActivityId", ConversionFunc = "SetIntVal", ConversionArgs = "3", IsAgentData = true},
                new Mapping { From = "ProductionAgent.none", To = "ProductionorderOperationActivity.AlternativeId", ConversionFunc = "SetStringVal", ConversionArgs = "", IsAgentData = true},
                new Mapping { From = "ProductionAgent.none", To = "ProductionorderOperationActivity.SplitId", ConversionFunc = "SetIntVal", ConversionArgs = "0", IsAgentData = true},
                new Mapping { From = "ProductionAgent.WorkItems.EstimatedStart", To = "ProductionorderOperationActivity.DateStart", ConversionFunc = "RelativeTimeIntToDateString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.WorkItems.EstimatedEnd", To = "ProductionorderOperationActivity.DateEnd", ConversionFunc = "RelativeTimeIntToDateString", IsAgentData = true},
                new Mapping { From = "ProductionAgent.WorkItems.WorkSchedule.Id", To = "Productionorder.RoutingId", ConversionFunc = "IntToString", IsAgentData = true},
            };
            context.AddRange(productionAgentMapping);
            context.SaveChanges();

            /*var dispoAgentMapping = new Mapping[]
            {

            };
            context.AddRange(dispoAgentMapping);
            context.SaveChanges();

            var machineAgentMapping = new Mapping[]
            {

            };
            context.AddRange(machineAgentMapping);
            context.SaveChanges();
            */
        }
    }
}
