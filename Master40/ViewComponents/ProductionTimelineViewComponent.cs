using Master40.Extensions;
using Master40.Models;
using Master40.DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;

namespace Master40.ViewComponents
{
    public class ProductionTimelineViewComponent : ViewComponent
    {
        private readonly MasterDBContext _context;

        public ProductionTimelineViewComponent(MasterDBContext context)
        {
            _context = context;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            //.Definitions();
            var orders = new List<int>();
            int orderId = -1;
            int schedulingState = 1;
            if (Request.Method == "POST")
            {   // catch scheduling state
                schedulingState = Convert.ToInt32(Request.Form["SchedulingState"]);
                orderId = Convert.ToInt32(Request.Form["Order"]);
                // If Order is not selected.
                if (orderId == -1)
                {   // for all Orders
                    orders = _context.Orders.Select(x => x.Id).ToList();
                }
                else
                {  // for the specified Order
                   orders.Add(Convert.ToInt32(Request.Form["Order"]));
                }
            }
            else
            {   // default
                orders = orders = _context.Orders.Select(x => x.Id).ToList();
            }

            var schedule = await GetSchedulesForOrderList(orders, schedulingState);
            // Fill Select Fields
            var orderSelection = new SelectList(_context.Orders, "Id", "Name", orderId);
            ViewData["OrderId"] = orderSelection.AddFirstItem(new SelectListItem { Value = "-1", Text = "All" });
            ViewData["SchedulingState"] = SchedulingState(schedulingState);

            // return schedule
            return View("ProductionTimeline", JsonConvert.SerializeObject(schedule));
        }

        private async Task<List<ProductionTimeline>> GetSchedulesForOrderList(List<int> orders, int schedulingState)
        {

            var pows = _context.ProductionOrderWorkSchedule
                                .Include(m => m.MachineGroup)
                                .Include(a => a.ProductionOrder)
                                    .ThenInclude(p => p.ProductionOrderBoms)
                                .Include(a => a.ProductionOrder)
                                    .ThenInclude(x => x.DemandProviderProductionOrders)
                                .OrderBy(a => a.MachineGroup).ToList();


            var color = 0;
            var orderSchedule = new List<ProductionTimeline>();

            foreach(var id in orders)
            {
                await GetDataForProductionOrderTimeline(orderSchedule, pows, color++, id, schedulingState);
            };
            return orderSchedule;
        }

        private async Task<List<ProductionTimeline>> GetDataForProductionOrderTimeline(List<ProductionTimeline> schedule, List<ProductionOrderWorkSchedule> pows, int n, int orderId, int schedulingState)
        {
            // get the corrosponding Order Parts to Order
            var demand = _context.Demands.OfType<DemandOrderPart>()
                    .Include(x => x.OrderPart)
                    .Where(o => o.OrderPart.OrderId == orderId).ToList();

            // get Damand Providers for this Order
            var demandProviders = (from c in _context.Demands.OfType<DemandProviderProductionOrder>()
                                   join d in demand on c.DemandRequesterId equals d.Id
                                   select c).ToList();

            // get Production OrderWorkSchedule for 
            var powDetails = (from p in pows
                              join dp in demandProviders on p.ProductionOrderId equals dp.ProductionOrderId
                              select p).ToList();

            
            return await CreateTimelineForProductionOrder(schedule, powDetails, (ganttColors)n, schedulingState);
        }

        private async Task<List<ProductionTimeline>> CreateTimelineForProductionOrder(List<ProductionTimeline> schedule,List<ProductionOrderWorkSchedule> pows, ganttColors gc, int schedulingState)
        {
            var today = DateTime.Now.GetEpochMilliseconds();
            foreach (var item in pows)
            {
                
                var dependencies = "";
                // chose planning method. and select start and end Dependend
                string start, end;
                switch (schedulingState)
                {
                    case 1:
                        start = (today + item.StartBackward * 3600000).ToString();
                        end = (today + item.EndBackward * 3600000).ToString();
                        break;
                    case 2:
                        start = (today + item.StartForward * 3600000).ToString();
                        end = (today + item.EndForward * 3600000).ToString(); 
                        break;
                    default:
                        start = (today + item.Start * 3600000).ToString();
                        end = (today + item.End * 3600000).ToString();
                        break;
                }


                if (schedulingState == 1 || schedulingState == 2 || schedule.Find(x => x.Name == item.MachineGroup.Name) == null)
                {
                    // only follow dependencies during forward / backward schedule.
                    if (schedulingState != 3)
                    {
                        var c = await MasterDbHelper.GetPriorProductionOrderWorkSchedules(_context, item);
                        if (c.Count() > 0)
                            dependencies = c.FirstOrDefault().Id.ToString();
                    }
                    schedule.Add(new ProductionTimeline
                    {
                        Name = item.MachineGroup.Name,
                        Desc = "&rarr; ",
                        Values =
                      new List<ProductionTimelineItem>
                      {
                           new ProductionTimelineItem
                           {
                               Id = item.Id.ToString(), Desc = item.Name, Label = "P.O.: " + item.ProductionOrderId.ToString(),
                               From = "/Date(" + start + ")/",
                               To =  "/Date(" + end + ")/",
                               CustomClass =  gc.ToString(),
                               Dep = "" + dependencies
                            },
                      }
                    });
                } else
                { // add only one new item.
                    schedule.Find(x => x.Name == item.MachineGroup.Name).Values.Add(new ProductionTimelineItem
                    {
                        Id = item.Id.ToString(),
                        Desc = item.Name,
                        Label = "P.O.: " + item.ProductionOrderId.ToString(),
                        From = "/Date(" + start + ")/",
                        To = "/Date(" + end + ")/",
                        CustomClass = gc.ToString(),
                        Dep = "" + dependencies
                    });
                }

         
            }
            return schedule;
        }

        public ProductionTimelineItem productionTimelineItem(ProductionOrderWorkSchedule item, string start, string end, ganttColors gc, string dependencies)
        {
            return new ProductionTimelineItem
            {
                Id = item.Id.ToString(),
                Desc = item.Name,
                Label = "P.O.: " + item.ProductionOrderId.ToString(),
                From = "/Date(" + start + ")/",
                To = "/Date(" + end + ")/",
                CustomClass = gc.ToString(),
                Dep = "" + dependencies
            };
        }

        public enum ganttColors
        {
            ganttRed,
            ganttBlue,
            ganttOrange,
            ganttGreen,
            ganttGray
        }

        private SelectList SchedulingState(int selectedItem)
        {
             return new SelectList(new List<SelectListItem> {
                new SelectListItem() { Text="Backward", Value="1"},
                new SelectListItem() { Text="Forward", Value="2"},
                new SelectListItem() { Text="Giffler-Thompson", Value="3"},
            },"Value", "Text", selectedItem);
        }

    }

}