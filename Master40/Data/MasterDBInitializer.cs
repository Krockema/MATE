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

            var articleBom = new ArticleBom();

            var articleBomParts = new List<ArticleBomPart>{
                new ArticleBomPart { ArticleId = articles.Single(a => a.Name == "Kipper").ArticleID, Count = 1, Name = "Kipper" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Rahmengestell").ArticleID, ParrentArticleBomPartId = 1, Count = 1, Name = "Rahmengestell" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Ladebehälter").ArticleID, ParrentArticleBomPartId = 1, Count = 1, Name = "Ladebehälter" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Chassis").ArticleID, Count = 1, ParrentArticleBomPartId = 1, Name = "Chassis" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Rad").ArticleID, Count = 4, ParrentArticleBomPartId = 2, Name = "Rad" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Bodenplatte").ArticleID, Count = 1, ParrentArticleBomPartId = 2, Name = "Bodenplatte" },
                new ArticleBomPart {ArticleId = articles.Single(a => a.Name == "Achse").ArticleID, Count = 2, ParrentArticleBomPartId = 2, Name = "Achse" },

            };

            articleBom.ArticleBomParts = articleBomParts;
            articleBom.ArticleId = articles.Single(a => a.Name == "Kipper").ArticleID;
            articleBom.Name = "Kipper";
            context.ArticleBoms.Add(articleBom);
            context.SaveChanges();

            var menu = new Menu();
            var menuItems = new List<MenuItem>{
                new MenuItem{MenuText = "Article", LinkUrl = "Index", MenuOrder = 1},
                new MenuItem{MenuText = "Order", LinkUrl = "Index", MenuOrder = 2},
                new MenuItem{MenuText = "Purchase", LinkUrl = "Index", MenuOrder = 3},
                new MenuItem{MenuText = "Purchase Position", LinkUrl = "Index", MenuOrder = 1, ParentMenuItemId = 3 },
                new MenuItem{MenuText = "Business Partner", LinkUrl = "Index", MenuOrder = 4},
                new MenuItem{MenuText = "Article", LinkUrl = "Index", MenuOrder = 1, ParentMenuItemId = 1},
                new MenuItem{MenuText = "Article BOM", LinkUrl = "Index", MenuOrder = 2, ParentMenuItemId = 1},
                new MenuItem{MenuText = "Article BOM Part", LinkUrl = "Index", MenuOrder = 3, ParentMenuItemId = 1},
                new MenuItem{MenuText = "Article Stock", LinkUrl = "Index",  MenuOrder = 1, ParentMenuItemId = 7},
                new MenuItem{MenuText = "Operation Chart", LinkUrl = "Index",  MenuOrder = 1, ParentMenuItemId = 7},
                new MenuItem{MenuText = "Operation Tools", LinkUrl = "Index",  MenuOrder = 1, ParentMenuItemId = 7},
                new MenuItem{MenuText = "Operation Machine", LinkUrl = "Index",  MenuOrder = 1, ParentMenuItemId = 7},
            };
            menu.MenuItems = menuItems;
            menu.MenuName = "Master 4.0";

            context.Menus.Add(menu);
            context.SaveChanges();


        }
    }
}
