using Master40.Models.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Master40.Models;

namespace Master40.Data
{
    public static class MasterDBInitializer
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
            // Articles
            var articles = new Article[]
            {
            new Article{Name="Kipper",  ArticleTypeID = articleTypes.Single( s => s.Name == "Assembly").ArticleTypeID, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 20, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 45.00},
            new Article{Name="Rahmengestell",ArticleTypeID = articleTypes.Single( s => s.Name == "Assembly").ArticleTypeID, DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 15.00},
            new Article{Name="Ladebehälter", ArticleTypeID = articleTypes.Single( s => s.Name == "Assembly").ArticleTypeID, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 15.00},
            new Article{Name="Chassis", ArticleTypeID = articleTypes.Single( s => s.Name == "Assembly").ArticleTypeID, CreationDate = DateTime.Parse("2016-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 15.00},

            new Article{Name="Rad", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 1.00},
            new Article{Name="Felge", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 2, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 1.00},
            new Article{Name="Bodenplatte", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 4.00},
            new Article{Name="Aufliegeplatte", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 3.00},
            
            new Article{Name="Seitenwand land", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 1.50},
            new Article{Name="Seitenwand kurz", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 2.00},
            new Article{Name="Bodenplatte Behälter", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 4.00},
            new Article{Name="Fahrerhaus", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 5.00},
            new Article{Name="Motorblock", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 3.00},
            new Article{Name="Achse", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 0.50},
            new Article{Name="Knopf", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 0.05},
            new Article{Name="Kippgelenk", ArticleTypeID = articleTypes.Single( s => s.Name == "Material").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Pieces").UnitID, Price = 1},

            new Article{Name="Unterlegscheibe", ArticleTypeID = articleTypes.Single( s => s.Name == "Consumable").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10,UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 0.05},
            new Article{Name="Leim", ArticleTypeID = articleTypes.Single( s => s.Name == "Consumable").ArticleTypeID, CreationDate = DateTime.Parse("2002-09-01"), DeliveryPeriod = 10, UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 5.00},
            new Article{Name="Dübel", ArticleTypeID = articleTypes.Single( s => s.Name == "Consumable").ArticleTypeID, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod = 3, UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 3.00},
            new Article{Name="Verpackung", ArticleTypeID = articleTypes.Single( s => s.Name == "Consumable").ArticleTypeID, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 7.00},
            new Article{Name="Bedienungsanleitung", ArticleTypeID = articleTypes.Single( s => s.Name == "Consumable").ArticleTypeID, CreationDate = DateTime.Parse("2005-09-01"), DeliveryPeriod  = 4, UnitID = units.Single( s => s.Name == "Kilo").UnitID, Price = 0.50},

            };
            foreach (Article a in articles)
            {
                context.Articles.Add(a);
            }
            context.SaveChanges();

            // get the name -> id mappings
            var DBArticles = context
              .Articles
              .ToDictionary(p => p.Name, p => p.ArticleID);


            // create Stock Entrys for each Article
            foreach (var article in DBArticles)
            {
                var Stocks = new Stock[]
                {
                    new Stock
                    {
                        ArticleForeignKey = article.Value,
                        Name = "Stock: " + article.Key,
                        Min = 10,
                        Max = 100,
                        Current = 50
                    }
                };
                foreach (Stock s in Stocks)
                {
                    context.Stocks.Add(s);
                }
                context.SaveChanges();
            }

            var articleBom = new List<ArticleBom>
            {
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Kipper").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Rahmengestell").ArticleID, Name = "Rahmengestell", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Kipper").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Ladebehälter").ArticleID, Name = "Ladebehälter", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Kipper").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Chassis").ArticleID, Name = "Chassis", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Rahmengestell").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Bodenplatte").ArticleID, Name = "Bodenplatte", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Rahmengestell").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Achse").ArticleID, Name = "Achse", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Achse").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Rad").ArticleID, Name = "Rad", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Rad").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Felge").ArticleID, Name = "Felge", Quantity = 1 },
                new ArticleBom { ArticleParentId = articles.Single(a => a.Name == "Achse").ArticleID, ArticleChildId = articles.Single(a => a.Name == "Dübel").ArticleID, Name = "Dübel", Quantity = 1 }
            };
            foreach (var item in articleBom)
            {
                context.ArticleBoms.Add(item);
            }
            context.SaveChanges();

            var menu = new Menu();
            var menuItems = new List<MenuItem>{
                new MenuItem{MenuText = "Article", LinkUrl = "#", MenuOrder = 1, Action="Index", Symbol="fa-th-list"},
                new MenuItem{MenuText = "Order", LinkUrl = "Orders", MenuOrder = 2, Action="Index", Symbol="fa-archive"},
                new MenuItem{MenuText = "Purchase", LinkUrl = "Purchases", MenuOrder = 3, Action="Index", Symbol="fa-shopping-cart"},
                new MenuItem{MenuText = "Business Partner", LinkUrl = "BusinessPartners", MenuOrder = 4, Action="Index", Symbol="fa-group"},
                new MenuItem{MenuText = "Article", LinkUrl = "Articles", MenuOrder = 1, ParentMenuItemId = 1, Action="Index", Symbol="fa-archive"},
                new MenuItem{MenuText = "Operations", LinkUrl = "#", MenuOrder = 2, ParentMenuItemId = 1, Action="Index", Symbol="fa-th-list"},
                new MenuItem{MenuText = "Article BOM", LinkUrl = "ArticlesBoms", MenuOrder = 2, ParentMenuItemId = 1, Action="Index", Symbol="fa-sitemap"},
                new MenuItem{MenuText = "Article Stock", LinkUrl = "Stocks",  MenuOrder = 1, ParentMenuItemId = 1,  Action="Index", Symbol="fa-stackexchange"},
                new MenuItem{MenuText = "Operation Chart", LinkUrl = "OperationCharts",  MenuOrder = 1, ParentMenuItemId = 6, Action="Index", Symbol="fa-tasks"},
                new MenuItem{MenuText = "Operation Tools", LinkUrl = "OperationTools",  MenuOrder = 1, ParentMenuItemId = 6, Action="Index", Symbol="fa-wrench"},
                new MenuItem{MenuText = "Operation Machine", LinkUrl = "OperationMachines",  MenuOrder = 1, ParentMenuItemId = 6, Action="Index", Symbol="fa-gears"},
            };
            menu.MenuItems = menuItems;
            menu.MenuName = "Master 4.0";

            context.Menus.Add(menu);
            context.SaveChanges();


        }
    }
}
