using Master40.Extensions;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Repository;
using Master40.DB.DB.Models;


namespace Master40.ViewComponents
{
    public class ProductionTimelineViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private List<MachineGantt> _machineGantts;
        private long _today;
        public ProductionTimelineViewComponent(ProductionDomainContext context)
        {
            _context = context;
            _today = DateTime.Now.GetEpochMilliseconds();
        }

        /// <summary>
        /// called from ViewComponent.
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(List<int> paramsList)
        {
            //.Definitions();
            var orders = new List<int>();
            int orderId = paramsList[0];
            int schedulingState = paramsList[1];
            /*
            int orderId = -1;
            int schedulingState = 1;
            if (Request.Method == "POST")
            {   // catch scheduling state
                schedulingState = Convert.ToInt32(Request.Form["SchedulingState"]);
                orderId = Convert.ToInt32(Request.Form["Order"]);
                // If Order is not selected.
                */
                if (orderId == -1)
                {   // for all Orders
                    orders = _context.Orders.Select(x => x.Id).ToList();
                }
                else
                {  // for the specified Order
                   orders.Add(orderId);
                }
                /*
            }
            else
            {   // default
                orders = _context.Orders.Select(x => x.Id).ToList();
            }
            */
            var schedule = (await GetSchedulesForOrderList(orders, schedulingState)).OrderBy(a => a.GroupId).ThenBy(b => b.Id);
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
            _machineGantts = new List<MachineGantt>();
            var machineGroups = _context.MachineGroups;
            var color = 0;
            foreach (var machineGroup in machineGroups)
            {
                _machineGantts.Add(new MachineGantt()
                {
                    GanttColor = (GanttColors)color,
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
                    .ToList();

            // ReSharper Linq
            var demandboms = demands.SelectMany(demand => _context.Demands.OfType<DemandProductionOrderBom>()
                                    .Where(a => a.DemandRequesterId == demand.Id)).ToList();
            /* Old  
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
            */

            // get Demand Providers for this Order
            var demandProviders = new List<DemandProviderProductionOrder>();
            foreach (var order in (_context.Demands.OfType<DemandProviderProductionOrder>().Join(demands, c => c.DemandRequesterId, d => ((IDemandToProvider) d).Id, (c, d) => c)))
            {
                demandProviders.Add(order);
            }

            var demandBomProviders = (_context.Demands.OfType<DemandProviderProductionOrder>()
                .Join(demandboms, c => c.DemandRequesterId, d => d.Id, (c, d) => c)).ToList();
           

            // get ProductionOrderWorkSchedule for 
            var powDetails = (pows.Join(demandProviders, p => p.ProductionOrderId, dp => dp.ProductionOrderId, (p, dp) => p)).ToList();
            var powBoms = (from p in pows join dbp in demandBomProviders on p.ProductionOrderId equals dbp.ProductionOrderId select p).ToList();
            
            powDetails.AddRange(powBoms);

            return await CreateTimelineForProductionOrder(schedule, powDetails, (GanttColors)n, schedulingState);
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
            List<ProductionOrderWorkSchedule> pows, GanttColors gc, int schedulingState)
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
                        var c = await _context.GetFollowerProductionOrderWorkSchedules(item);
                        if (c.Any())
                            dependencies = c.FirstOrDefault().Id.ToString();
                    }
                    schedule = AddToSchedule(schedule, schedulingState, item.MachineGroup.Name,
                        CreateProductionTimelineItem(item, start, end, gc, dependencies, schedulingState));
                }
                else
                {
                    schedule = AddToSchedule(schedule, schedulingState, "ProductionOrderId " + item.ProductionOrderId,
                        CreateProductionTimelineItem(item, start, end, GetMachineColor(item) , "", schedulingState));
                }

            }
            return schedule;
        }

        private GanttColors GetMachineColor(ProductionOrderWorkSchedule item)
        {
            return _machineGantts.Find(a => a.MachineGroupId == item.MachineGroupId).GanttColor;
        }

        /// <summary>
        /// Checks if the schedule item already exists, if not it creates a new element
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="schedulingState"></param>
        /// <param name="name"></param>
        /// <param name="productionTimelineItem"></param>
        private List<ProductionTimeline> AddToSchedule(List<ProductionTimeline> schedule, int schedulingState, string name, ProductionTimelineItem productionTimelineItem)
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
            return schedule;
        }

        private List<ProductionTimeline> CheckForStacking(ProductionTimelineItem productionTimelineItem, List<ProductionTimeline> schedule, string name)
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
                if (stacking) continue;
                rowId = row.Id;
                break;
            }
            return AddSubSchedule(productionTimelineItem, schedule, rowId, name);
        }

        private List<ProductionTimeline> AddSubSchedule(ProductionTimelineItem productionTimelineItem, List<ProductionTimeline> schedule, int rowId, string name)
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
            return schedule;
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
            
            switch (schedulingState)
            {
                case 1:
                    start = (_today + item.StartBackward * 60000).ToString();
                    end = (_today + item.EndBackward * 60000).ToString();
                    break;
                case 2:
                    start = (_today + item.StartForward * 60000).ToString();
                    end = (_today + item.EndForward * 60000).ToString();
                    break;
                default:
                    start = (_today + item.Start * 60000).ToString();
                    end = (_today + item.End * 60000).ToString();
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
        public ProductionTimelineItem CreateProductionTimelineItem(ProductionOrderWorkSchedule item, string start, string end, GanttColors gc, string dependencies, int schedulingState)
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
            public GanttColors GanttColor { get; set; }
            public int MachineGroupId { get; set; }
        }

        /// <summary>
        /// Select List for Diagrammsettings (Forward / Backward / GT)
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        private SelectList SchedulingState(int selectedItem)
        {
            var itemList = new List<SelectListItem> { new SelectListItem() { Text="Backward", Value="1"} };

            if (_context.ProductionOrderWorkSchedule.Max(x => x.StartForward) != 0)
                itemList.Add(new SelectListItem() {Text = "Forward", Value = "2"});

            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Machinebased", Value = "3" });
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Productionorderbased", Value = "4" });
            return new SelectList( itemList, "Value", "Text", selectedItem);
        }
    }
}