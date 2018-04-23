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
using Master40.DB.Enums;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Master40.ViewComponents
{
    public class SimulationTimelineViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private readonly long _today;
        private int _orderId, _schedulingState, _simulationNumber, _simulationConfigurationId;
        private SimulationType _simulationType;
        private GanttContext _ganttContext;
        private int _schedulingPage = 0, _maxPage = 1, _timeSpan;

        public SimulationTimelineViewComponent(ProductionDomainContext context)
        {
            _context = context;
            _today = DateTime.Now.GetEpochMilliseconds();
            _ganttContext = new GanttContext();
        }

        /// <summary>
        /// called from ViewComponent.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            //.Definitions();
            var orders = new List<int>();
            _orderId = Convert.ToInt32(paramsList[0]);
            _schedulingState = Convert.ToInt32(paramsList[1]);
            _schedulingPage = Convert.ToInt32(paramsList[2]);
            _simulationConfigurationId = Convert.ToInt32(paramsList[4]);
            _simulationNumber = Convert.ToInt32(paramsList[5]);
            
            var folowLinks = false;
            switch (paramsList[3])
            {
                case "Decentral":
                    _simulationType = SimulationType.Decentral;
                    break;
                case "BackwardPlanning":
                    _simulationType = SimulationType.BackwardPlanning;
                    folowLinks = true;
                    break;
                case "ForwardPlanning":
                    _simulationType = SimulationType.ForwardPlanning;
                    folowLinks = true;
                    break;
                default:
                    _simulationType = SimulationType.Central;
                    break;
            }
            // refill ViewData
            // Fill Select Fields
            var orderSelection = new SelectList(_context.Orders, "Id", "Name", _orderId);
            ViewData["OrderId"] = orderSelection.AddFirstItem(new SelectListItem { Value = "-1", Text = "All" });
            ViewData["SchedulingState"] = SchedulingState(_schedulingState);
            ViewData["SimulationPage"] = _schedulingPage.ToString();
            ViewData["SimulationType"] = _simulationType.ToString();
            ViewData["SimulationNumber"] = _simulationNumber.ToString();
            ViewData["SimulationConfiguration"] = _simulationConfigurationId.ToString();
            // - 1 caus we start with 0
            ViewData["MaxPage"] = _maxPage;

            // if no data 
            if (!_context.SimulationWorkschedules.Any())
            {
                return View("SimulationTimeline", _ganttContext); ;
            }


            _timeSpan = _context.SimulationConfigurations.Single(x => x.Id == _simulationConfigurationId).DynamicKpiTimeSpan;
            ///// Needs some changes to Work. i.e. Reference SimulationOrder , Create SimulationOrderPart and writing it back
            // If Order is not selected.
            if (_orderId == -1)
            {   // for all Orders
                await GetSchedulesForTimeSlotListAsync(_timeSpan * _schedulingPage, _timeSpan * _schedulingPage + _timeSpan, folowLinks);
            }
            else
            {  // for the specified Order
                // temporary fix 
                var tempType = (_simulationType == SimulationType.ForwardPlanning)
                    ? SimulationType.BackwardPlanning
                    : _simulationType;

                orders = _context?.SimulationOrders.Where(x => x.OriginId == _orderId 
                                                        && x.SimulationType == tempType
                                                        && x.SimulationNumber == _simulationNumber
                                                        && x.SimulationConfigurationId == _simulationConfigurationId)
                                                        .Select(x => x.OriginId).ToList();
                await GetSchedulesForOrderListAsync(orders);
                //orders = _context?.OrderParts.Where(x => x.OrderId == _orderId).Select(x => x.Id).ToList();
            }
            




            _ganttContext.Tasks = _ganttContext.Tasks.OrderBy(x => x.type).ToList();
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
                var pows = _context.SimulationWorkschedules.Where(x => x.OrderId == "[" + ordersPart.ToString() + "]"
                                                                        && x.SimulationType == _simulationType
                                                                        && x.SimulationNumber == _simulationNumber
                                                                        && x.SimulationConfigurationId == _simulationConfigurationId)
                                                            .OrderBy(x => x.Machine).ThenBy(x => x.ProductionOrderId).ThenBy(x => x.Start);


                foreach (var pow in pows)
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
                    var c = await _context.GetFollowerProductionOrderWorkSchedules(pow, _simulationType, pows.ToList());

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

        private async Task GetSchedulesForTimeSlotListAsync(int pageStart, int pageEnd, bool folowLinks)
        {
            var pows = _context.SimulationWorkschedules.Where(x => x.Start >= pageStart && x.End <= pageEnd
                                                                    && x.SimulationType == _simulationType
                                                                    && x.SimulationNumber == _simulationNumber
                                                                    && x.SimulationConfigurationId == _simulationConfigurationId);
            _maxPage = (int)Math.Ceiling((double)_context.SimulationWorkschedules.Where(x => x.SimulationType == _simulationType
                                                                                      && x.SimulationNumber == _simulationNumber
                                                                                      && x.SimulationConfigurationId ==
                                                                                      _simulationConfigurationId) .Max(m => m.End) / _timeSpan);

            foreach (var pow in pows)
            {
                
                // check if head element is Created,
                GanttTask timeline = GetOrCreateTimeline(pow);
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

                if (folowLinks)
                {
                    var c = await _context.GetFollowerProductionOrderWorkSchedules(pow, _simulationType, pows.ToList());

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
                }
            } // emd pows
            
        }

        /// <summary>
        /// Returns or creates corrosponding GanttTask Item with Property  type = "Project" and Returns it.
        /// -- Headline for one Project
        /// </summary>
        private GanttTask GetOrCreateTimeline(SimulationWorkschedule pow, int orderId = 0)
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
                        //var mg = _context.MachineGroups.First(x => x.Id.ToString() == pow.Machine.ToString()).Name;
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
                        var pt = CreateProjectTask("O" + orderId, _context.OrderParts.
                                                                    Include(x => x.Order)
                                                                    .FirstOrDefault(x => x.Id == orderId)
                                                                        .Order.Name, "", 0, (GanttColors)gc);
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
            // var itemList = new List<SelectListItem>();
            var itemList = new List<SelectListItem> { new SelectListItem() { Text="Backward", Value="1"} };
            if (_context.SimulationWorkschedules.Any(x => x.SimulationType == SimulationType.ForwardPlanning && x.Start > 0))
            {
                itemList.Add(new SelectListItem() {Text = "Forward", Value = "2"});
            }
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Machinebased", Value = "3" });
            itemList.Add(new SelectListItem() { Text = "Capacity-Planning Productionorderbased", Value = "4" });
            return new SelectList( itemList, "Value", "Text", selectedItem);
        }

    }
}