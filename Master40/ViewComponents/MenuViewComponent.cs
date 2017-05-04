using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;
using Master40.Models.DB;

namespace Master40.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly MasterDBContext _context;

        public MenuViewComponent(MasterDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int menueId)
        {
            var menu = GetItemsAsync(menueId);
            return View("Menu", menu);
        }

        private IEnumerable<MenuItem> GetItemsAsync(int id)
        {
            return _context.MenuItems
                .Where(m => m.MenuId == id).OrderBy(x => x.MenuOrder).ToList()
                .Where(m => m.Parent == null).OrderBy(x => x.MenuOrder).ToList();
        }
    }
}
