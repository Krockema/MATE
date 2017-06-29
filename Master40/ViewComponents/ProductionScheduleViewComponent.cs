using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Master40.ViewComponents
{
    public class ProductionScheduleViewComponent : ViewContext
    {
        private readonly ProductionDomainContext _context;
        private ProductionSchedule _machineGantts;
        private long _today;
        public ProductionScheduleViewComponent(ProductionDomainContext context)
        {
            _context = context;
            _today = DateTime.Now.GetEpochMilliseconds();
        }

        public async Task<IViewComponentResult> InvokeAsync(List<int> paramsList)
        {
            return null;
        }


        /// <summary>
            /// Select List for Diagrammsettings (Forward / Backward / GT)
            /// </summary>
            /// <param name="selectedItem"></param>
            /// <returns></returns>
            private SelectList SchedulingState(int selectedItem)
        {
            var itemList = new List<SelectListItem> { new SelectListItem() { Text = "Backward", Value = "1" } };

            if (_context.ProductionOrderWorkSchedule.Max(x => x.StartForward) != 0)
                itemList.Add(new SelectListItem() { Text = "Forward", Value = "2" });

            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Machinebased", Value = "3" });
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Productionorderbased", Value = "4" });
            return new SelectList(itemList, "Value", "Text", selectedItem);
        }
    }
}
