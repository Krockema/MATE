using Master40.Extensions;
using Master40.Models;
using Master40.DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Data.Repository;
using Microsoft.CodeAnalysis;

namespace Master40.ViewComponents
{
    public class ProductionTimelineViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private List<MachineGantt> machineGantts;
        public ProductionTimelineViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }

        /// <summary>
        /// called from ViewComponent.
        /// </summary>
        /// <returns></returns>
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
                orders = _context.Orders.Select(x => x.Id).ToList();
            }

            var schedule = await GetSchedulesForOrderList(orders, schedulingState);
            // Fill Select Fields
            var orderSelection = new SelectList(_context.Orders, "Id", "Name", orderId);
            ViewData["OrderId"] = orderSelection.AddFirstItem(new SelectListItem { Value = "-1", Text = "All" });
            ViewData["SchedulingState"] = SchedulingState(schedulingState);

            // return schedule
            return View("ProductionTimeline", JsonConvert.SerializeObject(schedule));
        }

        /// <summary>
        /// Get All Relevant Production Work Schedules an dpass them for each Order To 
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="schedulingState"></param>
        /// <returns></returns>
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
            if (schedulingState == 4)
                SetMachineColor();
            foreach (var id in orders)
            {
                await GetDataForProductionOrderTimeline(orderSchedule, pows, color++, id, schedulingState);
            }
                
            return orderSchedule;
        }

        private void SetMachineColor()
        {
            machineGantts = new List<MachineGantt>();
            var machineGroups = _context.MachineGroups;
            var color = 0;
            foreach (var machineGroup in machineGroups)
            {
                machineGantts.Add(new MachineGantt()
                {
                    GanttColor = (ganttColors)color,
                    MachineGroupId = machineGroup.Id
                });
                color++;
            }
        }

        /// <summary>
        /// Reciving required Data for the Timeline and push them further to 
        /// create TimelineForProductionOrder
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="pows"></param>
        /// <param name="n"></param>
        /// <param name="orderId"></param>
        /// <param name="schedulingState"></param>
        /// <returns></returns>
        private async Task<List<ProductionTimeline>> GetDataForProductionOrderTimeline(List<ProductionTimeline> schedule, 
                                                                                       List<ProductionOrderWorkSchedule> pows,
                                                                                       int n, int orderId, int schedulingState)
        {
            // get the corresponding Order Parts to Order
            var demands = _context.Demands.OfType<DemandOrderPart>()
                    .Include(x => x.OrderPart)
                    .Where(o => o.OrderPart.OrderId == orderId)
                    //.Join(_context.Demands.OfType<DemandProductionOrderBom>(),dop => dop.Id,dpob => dpob.DemandRequesterId,(dop,dpob) => new {DemandOrderPart = dop, DemandProductionOrderBom = dpob})
                    .ToList();
            var demandboms = new List<DemandProductionOrderBom>();
            foreach (var demand in demands)
            {
                var boms =
                    _context.Demands.OfType<DemandProductionOrderBom>().Where(a => a.DemandRequesterId == demand.Id);
                foreach (var bom in boms)
                {
                   demandboms.Add(bom); 
                }
            }
            

            // get Demand Providers for this Order
            var demandProviders = (from c in _context.Demands.OfType<DemandProviderProductionOrder>()
                                   join d in demands on c.DemandRequesterId equals ((IDemandToProvider)d).Id
                                   select c).ToList();
            var demandBomProviders = (from c in _context.Demands.OfType<DemandProviderProductionOrder>()
                join d in demandboms on c.DemandRequesterId equals d.Id
                select c).ToList();


            /*var demandBomProviders = _context.Demands.OfType<DemandProviderProductionOrder>()
                .Join(demandboms, po => po.Id, bom => bom.Id,
                    (po, bom) => new {DemandProviderProductionOrder = po, DemandProductionOrderBom = bom});*/

            // get ProductionOrderWorkSchedule for 
            var powDetails = (from p in pows
                              join dp in demandProviders on p.ProductionOrderId equals dp.ProductionOrderId
                              select p).ToList();
            var powBoms = (from p in pows join dbp in demandBomProviders on p.ProductionOrderId equals dbp.ProductionOrderId select p).ToList();
            
            powDetails.AddRange(powBoms);

            return await CreateTimelineForProductionOrder(schedule, powDetails, (ganttColors)n, schedulingState);
        }

        /// <summary>
        /// Create List Of ProductionTimeline seperated by Order
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="pows"></param>
        /// <param name="gc"></param>
        /// <param name="schedulingState"></param>
        /// <returns></returns>
        private async Task<List<ProductionTimeline>> CreateTimelineForProductionOrder(List<ProductionTimeline> schedule,
            List<ProductionOrderWorkSchedule> pows, ganttColors gc, int schedulingState)
        {

            foreach (var item in pows)
            {
                // chose planning method. and select start and end Dependend
                string start = "", end = "";
                DefineStartEnd(ref start, ref end, schedulingState, item);

                if (schedulingState != 4)
                {
                    // only follow dependencies during forward / backward schedule.
                    var dependencies = "";
                    if (schedulingState == 1 || schedulingState == 2)
                    {
                        var c = await _context.GetPriorProductionOrderWorkSchedules(item);
                        if (c.Any())
                            dependencies = c.FirstOrDefault().Id.ToString();
                    }
                    AddToSchedule(schedule, schedulingState, item.MachineGroup.Name,
                        CreateProductionTimelineItem(item, start, end, gc, dependencies, schedulingState));
                }
                else
                {
                    AddToSchedule(schedule, schedulingState, "OrderId " + GetOrderId(item),
                        CreateProductionTimelineItem(item, start, end, GetMachineColor(item) , "", schedulingState));
                }

            }
            
            return schedule;
        }

        private ganttColors GetMachineColor(ProductionOrderWorkSchedule item)
        {
            return machineGantts.Find(a => a.MachineGroupId == item.MachineGroupId).GanttColor;
        }

        /// <summary>
        /// Checks if the schedule item already exists, if not it creates a new element
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="schedulingState"></param>
        /// <param name="name"></param>
        /// <param name="productionTimelineItem"></param>
        private void AddToSchedule(List<ProductionTimeline> schedule, int schedulingState, string name, ProductionTimelineItem productionTimelineItem)
        {
            if (schedule.Find(x => x.Name == name) == null || schedulingState <= 2)
            {
                // Add Timeline with first Timeline Item item
                schedule.Add(new ProductionTimeline
                {
                    Name = name,
                    Desc = "&rarr; ",
                    Values =
                        new List<ProductionTimelineItem>
                        {
                            productionTimelineItem
                        },
                    Id = schedule.Count
                });
                schedule.Last().GroupId = schedule.Last().Id;
            }
            else
            {
                CheckForStacking(productionTimelineItem, schedule, name);
            }
        }

        private void CheckForStacking(ProductionTimelineItem productionTimelineItem, List<ProductionTimeline> schedule, string name)
        {
            var firstRow = schedule.Find(x => x.Name == name);
            var rows = schedule.FindAll(x => x.GroupId == firstRow.GroupId);
            int rowId;
            rowId = -1;
            foreach (var row in rows)
            {
                var items = schedule.Find(a => a.Id == row.Id).Values;
                var stacking = false;
                foreach (var item in items)
                {
                    if ((item.IntFrom <= productionTimelineItem.IntFrom &&
                         item.IntTo > productionTimelineItem.IntFrom)
                        ||
                        (item.IntFrom < productionTimelineItem.IntTo &&
                        item.IntTo >= productionTimelineItem.IntTo)
                        ||
                        (item.IntFrom > productionTimelineItem.IntFrom &&
                         item.IntTo < productionTimelineItem.IntTo)
                        ||
                        (item.IntFrom <= productionTimelineItem.IntFrom &&
                         item.IntTo >= productionTimelineItem.IntTo))
                    {
                        stacking = true;
                    }
                }
                if (!stacking)
                {
                    rowId = row.Id;
                    break;
                }
            }
            AddSubSchedule(productionTimelineItem, schedule, rowId, name);
        }

        private void AddSubSchedule(ProductionTimelineItem productionTimelineItem, List<ProductionTimeline> schedule, int rowId, string name)
        {
            if (rowId > -1)
            {
                //add to schedule
                schedule.Find(a => a.Id == rowId).Values.Add(productionTimelineItem);
            }
            else
            {
                //create new subschedule
                schedule.Add(new ProductionTimeline()
                {
                    Desc = "&rarr; ",
                    Values =
                        new List<ProductionTimelineItem>
                        {
                            productionTimelineItem
                        },
                    GroupId = schedule.Find(a => a.Name == name).GroupId,
                    Id = schedule.Count
                });
            }
        }

        /// <summary>
        /// Defines start and end for the ganttchart
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="schedulingState"></param>
        /// <param name="item"></param>
        private void DefineStartEnd(ref string start, ref string end, int schedulingState, ProductionOrderWorkSchedule item)
        {
            var today = DateTime.Now.GetEpochMilliseconds();
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
        }

        /// <summary>
        /// returns the OrderId for the ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        private int GetOrderId(ProductionOrderWorkSchedule pow)
        {
            //call requester.requester to make sure that also the DemandProductionOrderBoms find the DemandOrderPart
            var requester = (DemandOrderPart) pow.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequester;
            var orderId = _context.OrderParts.Single(a => a.Id == requester.OrderPartId).OrderId;
            return orderId;
        }

        /// <summary>
        /// Creates new TimelineItem with a label depending on the schedulingState
        /// </summary>
        /// <param name="item"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="gc"></param>
        /// <param name="dependencies"></param>
        /// <param name="schedulingState"></param>
        /// <returns></returns>
        public ProductionTimelineItem CreateProductionTimelineItem(ProductionOrderWorkSchedule item, string start, string end, ganttColors gc, string dependencies, int schedulingState)
        {
            var timelineItem = new ProductionTimelineItem
            {
                Id = item.Id.ToString(),
                Desc = item.Name,
                From = "/Date(" + start + ")/",
                To = "/Date(" + end + ")/",
                IntFrom = Convert.ToInt64(start),
                IntTo = Convert.ToInt64(end),
                CustomClass = gc.ToString(),
                Dep = dependencies,
                Label = schedulingState == 4 ? item.MachineGroup.Name : "P.O.: " + item.ProductionOrderId
            };
            return timelineItem;
        }

        internal class MachineGantt
        {
            public ganttColors GanttColor { get; set; }
            public int MachineGroupId { get; set; }
        }

        /// <summary>
        /// All Posible gantt colors.
        /// </summary>
        public enum ganttColors
        {
            ganttRed,
            ganttBlue,
            ganttOrange,
            ganttGreen,
            ganttGray
        }


        /// <summary>
        /// Select List for Diagrammsettings (Forward / Backward / GT)
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        private SelectList SchedulingState(int selectedItem)
        {
             return new SelectList(new List<SelectListItem> {
                new SelectListItem() { Text="Backward", Value="1"},
                new SelectListItem() { Text="Forward", Value="2"},
                new SelectListItem() { Text="Giffler-Thompson Machinebased", Value="3"},
                new SelectListItem() { Text="Giffler-Thompson Orderbased", Value ="4"},
            },"Value", "Text", selectedItem);
        }

    }

}