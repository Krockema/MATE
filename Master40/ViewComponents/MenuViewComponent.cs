using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Models;

namespace Master40.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly Menu _menu;


        public MenuViewComponent()
        {
            _menu = new Menu();
            _menu.MenuItems = new List<MenuItem>();
            _menu.MenuItems.Add(new MenuItem { MenuItemId = 1, MenuId = 1, MenuText = "Article", LinkUrl = "#", MenuOrder = 1, Action = "Index", Symbol = "fa-th-list" , Children =
                new List<MenuItem> {
                    new MenuItem { MenuItemId = 5, MenuId = 1, MenuText = "Article", LinkUrl = "Articles", MenuOrder = 1, ParentMenuItemId = 1, Action = "Index", Symbol = "fa-archive" },
                    new MenuItem { MenuItemId = 6, MenuId = 1, MenuText = "Operations", LinkUrl = "#", MenuOrder = 2, ParentMenuItemId = 1, Action = "Index", Symbol = "fa-th-list", Children=
                    new List<MenuItem> {
                        new MenuItem { MenuItemId = 9, MenuId = 1, MenuText = "Operation Chart", LinkUrl = "OperationCharts", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fa-tasks" },
                        new MenuItem { MenuItemId = 10, MenuId = 1, MenuText = "Operation Tools", LinkUrl = "OperationTools", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fa-wrench" },
                        new MenuItem { MenuItemId = 11, MenuId = 1, MenuText = "Operation Machine", LinkUrl = "OperationMachines", MenuOrder = 1, ParentMenuItemId = 6, Action = "Index", Symbol = "fa-gears" }
                    } },
                    new MenuItem { MenuItemId = 7, MenuId = 1, MenuText = "Article BOM", LinkUrl = "ArticleBoms", MenuOrder = 2, ParentMenuItemId = 1, Action = "Index", Symbol = "fa-sitemap" },
                    new MenuItem { MenuItemId = 8, MenuId = 1, MenuText = "Article Stock", LinkUrl = "Stocks", MenuOrder = 1, ParentMenuItemId = 1, Action = "Index", Symbol = "fa-dropbox" } }
            });

            _menu.MenuItems.Add(new MenuItem { MenuItemId = 2, MenuId = 1, MenuText = "Order", LinkUrl = "Orders", MenuOrder = 2, Action = "Index", Symbol = "fa-archive" });
            _menu.MenuItems.Add(new MenuItem { MenuItemId = 3, MenuId = 1, MenuText = "Purchase", LinkUrl = "Purchases", MenuOrder = 3, Action = "Index", Symbol = "fa-shopping-cart" });
            _menu.MenuItems.Add(new MenuItem { MenuItemId = 4, MenuId = 1, MenuText = "Business Partner", LinkUrl = "BusinessPartners", MenuOrder = 4, Action = "Index", Symbol = "fa-group" });

            _menu.MenuItems.Add(new MenuItem { MenuItemId = 12, MenuId = 1, MenuText = "Planning & Simulations", LinkUrl = "#", MenuOrder = 4, Action = "Index", Symbol = "fa-spinner", Children =
                new List<MenuItem>
                {
                    new MenuItem{MenuItemId = 13, MenuId = 1, MenuText = "MRP", LinkUrl = "Mrp",  MenuOrder = 5, ParentMenuItemId = 12, Action="Index", Symbol="fa-magic"}
                }
            });


            _menu.MenuId = 1;
            _menu.MenuName = "Master 4.0";
        }

        public async Task<IViewComponentResult> InvokeAsync(int menueId)
        {
            var menu = await GetItemsAsync(menueId);
            return View($"Menu", menu);
        }

        private async Task<ICollection<MenuItem>> GetItemsAsync(int id)
        {
            return await _menu.MenuItems
                .Where(m => m.MenuId == id).OrderBy(x => x.MenuOrder).ToAsyncEnumerable()
                .Where(m => m.ParentMenuItemId == null).OrderBy(x => x.MenuOrder).ToList();

        }
    }
}
