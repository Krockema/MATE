using Master40.Extensions;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.ViewComponents
{
    public class SimulationTimelineViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private readonly long _today;
        private int _orderId, _schedulingState;
        private GanttContext _ganttContext;
        public SimulationTimelineViewComponent(ProductionDomainContext context)
        {
            _context = context;
            _today = DateTime.Now.GetEpochMilliseconds();
            _ganttContext = new GanttContext();
        }

        /// <summary>
        /// called from ViewComponent.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(List<int> paramsList)
        {

            if (!_context.SimulationWorkschedules.Any())
            {
                return View("SimulationTimeline", _ganttContext);
            }

            //.Definitions();
            var orders = new List<int>();
            _orderId = paramsList[0];
            _schedulingState = paramsList[1];

                // If Order is not selected.
                if (_orderId == -1)
                {   // for all Orders
                    orders = _context?.OrderParts.Select(x => x.Id).ToList();
                }
                else
                {  // for the specified Order
                    orders = _context?.OrderParts.Where(x => x.OrderId == _orderId).Select(x => x.Id).ToList();
                }

            await GetSchedulesForOrderListAsync(orders);
            // Fill Select Fields
            var orderSelection = new SelectList(_context.Orders, "Id", "Name", _orderId);
            ViewData["OrderId"] = orderSelection.AddFirstItem(new SelectListItem { Value = "-1", Text = "All" });
            ViewData["SchedulingState"] = SchedulingState(_schedulingState);

            // return schedule
            return View("SimulationTimeline", _ganttContext);
        }
        
        /// <summary>
        /// Get All Relevant Production Work Schedules an dpass them for each Order To 
        /// </summary>
        private async Task GetSchedulesForOrderListAsync(IEnumerable<int> ordersParts)
        {
            foreach (var ordersPart in ordersParts)
            {
                foreach (var pow in _context.SimulationWorkschedules.Where(x => x.OrderId == ordersPart))
                {
                    // check if head element is Created,
                    GanttTask timeline = GetOrCreateTimeline(pow, ordersPart);
                    long start = 0, end = 0;
                    DefineStartEnd(ref start, ref end, pow);
                    // Add Item To Timeline
                    _ganttContext.Tasks.Add(
                        CreateGanttTask(
                            pow,
                            start,
                            end,
                            timeline.color,
                            timeline.id
                        )
                    );
                    var c = await _context.GetFollowerProductionOrderWorkSchedules(pow);

                    if (!c.Any()) continue;
                    // create Links for this pow
                    foreach (var link in c)
                    {
                        _ganttContext.Links.Add(
                            new GanttLink
                            {
                                id = Guid.NewGuid().ToString(),
                                type = LinkType.finish_to_start,
                                source = pow.Id.ToString(),
                                target = link.Id.ToString()
                            }
                        );
                    } // end foreach link 
                } // emd pows
            }
        }

        /// <summary>
        /// Returns or creates corrosponding GanttTask Item with Property  type = "Project" and Returns it.
        /// -- Headline for one Project
        /// </summary>
        private GanttTask GetOrCreateTimeline(SimulationWorkschedule pow,int orderId = 0)
        {
            IEnumerable<GanttTask> project;
            // get Timeline
            switch (_schedulingState)
            {
                case 3: // Machine Based
                    project = _ganttContext.Tasks
                        .Where(x => x.type == GanttType.project && x.id == "M_" + pow.Machine);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(x => x.type == GanttType.project) + 1;
                        var pt = CreateProjectTask("M_" + pow.Machine, pow.Machine, "", 0, (GanttColors)gc);
                        _ganttContext.Tasks.Add(pt);
                        return pt;
                    }
                    //break;
                case 4: // Production Order Based
                    project = _ganttContext.Tasks
                        .Where(x => x.type == GanttType.project && x.id == "P" + pow.ProductionOrderId);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(x => x.type == GanttType.project) + 1;
                        var pt = CreateProjectTask("P" + pow.ProductionOrderId, "PO Nr.: " + pow.ProductionOrderId, "", 0, (GanttColors)gc);
                        _ganttContext.Tasks.Add(pt);
                        return pt;
                    }
                    //break;
                default: // back and forward
                    project = _ganttContext.Tasks
                        .Where(x => x.type == GanttType.project && x.id == "O" + orderId);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(x => x.type == GanttType.project) + 1;
                        var pt = CreateProjectTask("O" + orderId, _context.Orders.FirstOrDefault(x => x.Id == orderId).Name, "", 0, (GanttColors)gc);
                        _ganttContext.Tasks.Add(pt);
                        return pt;
                    }
                   // break;
            }
        }

        /// <summary>
        /// Defines start and end for the ganttchart based on the Scheduling State
        /// </summary>
        private void DefineStartEnd(ref long start, ref long end, SimulationWorkschedule item)
        {
            start = (_today + item.Start * 60000);
            end = (_today + item.End * 60000);
        }

        /// <summary>
        /// Creates new TimelineItem with a label depending on the schedulingState
        /// </summary>
        public GanttTask CreateGanttTask(SimulationWorkschedule item, long start, long end, GanttColors gc, string parent)
        {
            var gantTask = new GanttTask()
            {
                id = item.Id.ToString(),
                type = GanttType.task,
                desc = item.WorkScheduleName,
                text = _schedulingState == 4 ? item.Machine : "P.O.: " + item.ProductionOrderId,
                start_date = start.GetDateFromMilliseconds().ToString("dd-MM-yyyy HH:mm"),
                end_date = end.GetDateFromMilliseconds().ToString("dd-MM-yyyy HH:mm"),
                IntFrom = start,
                IntTo = end,
                parent = parent,
                color = gc,
            };
            return gantTask;
        }

        private static GanttTask CreateProjectTask(string id, string name, string desc, int group, GanttColors gc)
        {
            return new GanttTask
            {
                id = id,
                text = name,
                desc = desc,
                type = GanttType.project,
                GroupId = group,
                color = gc
            };
        }

        /// <summary>
        /// Select List for Diagrammsettings (Forward / Backward / GT)
        /// </summary>
        private SelectList SchedulingState(int selectedItem)
        {
            var itemList = new List<SelectListItem> { new SelectListItem() { Text="Backward", Value="1"} };
            if (_context.ProductionOrderWorkSchedules.Any())
            {
                if (_context.ProductionOrderWorkSchedules.Max(x => x.StartForward) != 0)
                    itemList.Add(new SelectListItem() {Text = "Forward", Value = "2"});
            }
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Machinebased", Value = "3" });
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Productionorderbased", Value = "4" });
            return new SelectList( itemList, "Value", "Text", selectedItem);
        }

    }
}