using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mate.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mate.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly Menu _menu;


        public MenuViewComponent()
        {
            _menu = new Menu();
            _menu.MenuItems = new List<MenuItem>();
            _menu.MenuItems.Add(item: new MenuItem { MenuItemId = 1, MenuId = 1, MenuText = "Article", LinkUrl = "#", MenuOrder = 1, Action = "Index", Symbol = "fas fa-th-list" , Children =
                new List<MenuItem> {
                    new MenuItem { MenuItemId = 5, MenuId = 1, MenuText = "Article", LinkUrl = "Articles", MenuOrder = 1, ParentMenuItemId = 1, Action = "Index", Symbol = "fas fa-archive" },
                    new MenuItem { MenuItemId = 6, MenuId = 1, MenuText = "Factory", LinkUrl = "#", MenuOrder = 2, ParentMenuItemId = 1, Action = "Index", Symbol = "fas fa-th-list", Children=
                    new List<MenuItem> {
                        new MenuItem { MenuItemId = 9, MenuId = 1, MenuText = "Operations", LinkUrl = "OperationCharts", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fas fa-tasks" },
                        //new MenuItem { MenuItemId = 10, MenuId = 1, MenuText = "Machine Tools", LinkUrl = "OperationTools", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fa-wrench" },
                        new MenuItem { MenuItemId = 11, MenuId = 1, MenuText = "Resources", LinkUrl = "Resources", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fas fa-cogs" }
                    } },
                    new MenuItem { MenuItemId = 7, MenuId = 1, MenuText = "Article BOM", LinkUrl = "ArticleBoms", MenuOrder = 2, ParentMenuItemId = 1, Action = "Index", Symbol = "fas fa-sitemap" },
                    new MenuItem { MenuItemId = 8, MenuId = 1, MenuText = "Article Stock", LinkUrl = "Stocks", MenuOrder = 1, ParentMenuItemId = 1, Action = "Index", Symbol = "fas fa-box-open" } }
            });

            _menu.MenuItems.Add(item: new MenuItem { MenuItemId = 2, MenuId = 1, MenuText = "Order", LinkUrl = "Orders", MenuOrder = 2, Action = "Index", Symbol = "fas fa-archive" });
            _menu.MenuItems.Add(item: new MenuItem { MenuItemId = 3, MenuId = 1, MenuText = "Purchase", LinkUrl = "Purchases", MenuOrder = 3, Action = "Index", Symbol = "fas fa-shopping-cart" });
            _menu.MenuItems.Add(item: new MenuItem { MenuItemId = 4, MenuId = 1, MenuText = "Business Partner", LinkUrl = "BusinessPartners", MenuOrder = 4, Action = "Index", Symbol = "fas fa-users" });

            _menu.MenuItems.Add(item: new MenuItem { MenuItemId = 12, MenuId = 1, MenuText = "Planning & Simulations", LinkUrl = "#", MenuOrder = 4, Action = "Index", Symbol = "fas fa-spinner", Children =
                new List<MenuItem>
                {
                    new MenuItem{MenuItemId = 13, MenuId = 1, MenuText = "Configuration", LinkUrl = "ConfigurationItems",  MenuOrder = 5, ParentMenuItemId = 12, Action="Index", Symbol="fas fa-wrench"},
                    new MenuItem{MenuItemId = 13, MenuId = 1, MenuText = "MRP", LinkUrl = "Mrp",  MenuOrder = 6, ParentMenuItemId = 12, Action="Index", Symbol="fas fa-magic"},
                    new MenuItem{MenuItemId = 13, MenuId = 1, MenuText = "Agent Based", LinkUrl = "AgentLive",  MenuOrder = 7, ParentMenuItemId = 12, Action="Index", Symbol="fab fa-ravelry"}
                }
            });

            _menu.MenuId = 1;
            _menu.MenuName = "Master 4.0";
        }

        public async Task<IViewComponentResult> InvokeAsync(int menueId)
        {
            var menu = await GetItemsAsync(id: menueId);
            return View(viewName: $"Menu", model: menu);
        }

        private async Task<ICollection<MenuItem>> GetItemsAsync(int id)
        {
            return await
                Task.Run(() => 
                _menu.MenuItems
                .Where(predicate: m => m.MenuId == id).OrderBy(keySelector: x => x.MenuOrder).ToList()
                .Where(predicate: m => m.ParentMenuItemId == null).OrderBy(keySelector: x => x.MenuOrder).ToList());

        }
    }
}
