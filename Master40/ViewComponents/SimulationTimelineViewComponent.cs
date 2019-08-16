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
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Master40.ViewComponents
{
    public class SimulationTimelineViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultContext;
        private readonly long _today;
        private int _orderId, _schedulingState, _simulationNumber, _simulationConfigurationId;
        private SimulationType _simulationType;
        private GanttContext _ganttContext;
        private int _schedulingPage = 0, _maxPage = 1, _timeSpan;

        public SimulationTimelineViewComponent(ProductionDomainContext context, ResultContext resultContext)
        {
            _context = context;
            _resultContext = resultContext;
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
            _orderId = Convert.ToInt32(value: paramsList[index: 0]);
            _schedulingState = Convert.ToInt32(value: paramsList[index: 1]);
            _schedulingPage = Convert.ToInt32(value: paramsList[index: 2]);
            _simulationConfigurationId = Convert.ToInt32(value: paramsList[index: 4]);
            _simulationNumber = Convert.ToInt32(value: paramsList[index: 5]);
            
            var folowLinks = false;
            switch (paramsList[index: 3])
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
            var orderSelection = new SelectList(items: _context.CustomerOrders, dataValueField: "Id", dataTextField: "Name", selectedValue: _orderId);
            ViewData[index: "OrderId"] = orderSelection.AddFirstItem(firstItem: new SelectListItem { Value = "-1", Text = "All" });
            ViewData[index: "SchedulingState"] = SchedulingState(selectedItem: _schedulingState);
            ViewData[index: "SimulationPage"] = _schedulingPage.ToString();
            ViewData[index: "SimulationType"] = _simulationType.ToString();
            ViewData[index: "SimulationNumber"] = _simulationNumber.ToString();
            ViewData[index: "SimulationConfiguration"] = _simulationConfigurationId.ToString();
            // - 1 caus we start with 0
            ViewData[index: "MaxPage"] = _maxPage;

            // if no data 
            if (!_resultContext.SimulationOperations.Any())
            {
                return View(viewName: "SimulationTimeline", model: _ganttContext); ;
            }


            _timeSpan = _resultContext.SimulationConfigurations.Single(predicate: x => x.Id == _simulationConfigurationId).DynamicKpiTimeSpan;
            ///// Needs some changes to Work. i.e. Reference SimulationOrder , Create SimulationOrderPart and writing it back
            // If Order is not selected.
            if (_orderId == -1)
            {   // for all Orders
                await GetSchedulesForTimeSlotListAsync(pageStart: _timeSpan * _schedulingPage, pageEnd: _timeSpan * _schedulingPage + _timeSpan, folowLinks: folowLinks);
            }
            else
            {  // for the specified Order
                // temporary fix 
                var tempType = (_simulationType == SimulationType.ForwardPlanning)
                    ? SimulationType.BackwardPlanning
                    : _simulationType;

                orders = _resultContext?.SimulationOrders.Where(predicate: x => x.OriginId == _orderId 
                                                        && x.SimulationType == tempType
                                                        && x.SimulationNumber == _simulationNumber
                                                        && x.SimulationConfigurationId == _simulationConfigurationId)
                                                        .Select(selector: x => x.OriginId).ToList();
                await GetSchedulesForOrderListAsync(ordersParts: orders);
                //orders = _context?.OrderParts.Where(x => x.OrderId == _orderId).Select(x => x.Id).ToList();
            }
            




            _ganttContext.Tasks = _ganttContext.Tasks.OrderBy(keySelector: x => x.type).ToList();
            // return schedule
            return View(viewName: "SimulationTimeline", model: _ganttContext);
        }
        
        /// <summary>
        /// Get All Relevant Production Work Schedules an dpass them for each Order To 
        /// </summary>
        private async Task GetSchedulesForOrderListAsync(IEnumerable<int> ordersParts)
        {
            foreach (var ordersPart in ordersParts)
            {
                var pows = _resultContext.SimulationOperations.Where(predicate: x => x.OrderId == "[" + ordersPart.ToString() + "]"
                                                                        && x.SimulationType == _simulationType
                                                                        && x.SimulationNumber == _simulationNumber
                                                                        && x.SimulationConfigurationId == _simulationConfigurationId)
                                                            .OrderBy(keySelector: x => x.Machine).ThenBy(keySelector: x => x.ProductionOrderId).ThenBy(keySelector: x => x.Start);


                foreach (var pow in pows)
                {
                    // check if head element is Created,
                    GanttTask timeline = GetOrCreateTimeline(pow: pow, orderId: ordersPart);
                    long start = 0, end = 0;
                    DefineStartEnd(start: ref start, end: ref end, item: pow);
                    // Add Item To Timeline
                    _ganttContext.Tasks.Add(
                        item: CreateGanttTask(
                            item: pow,
                            start: start,
                            end: end,
                            gc: timeline.color,
                            parent: timeline.id
                        )
                    );
                    var c = await _context.GetFollowerProductionOrderWorkSchedules(simulationWorkSchedule: pow, type: _simulationType, relevantItems: pows.ToList());

                    if (!c.Any()) continue;
                    // create Links for this pow
                    foreach (var link in c)
                    {
                        _ganttContext.Links.Add(
                            item: new GanttLink
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
            var pows = _resultContext.SimulationOperations.Where(predicate: x => x.Start >= pageStart && x.End <= pageEnd
                                                                    && x.SimulationType == _simulationType
                                                                    && x.SimulationNumber == _simulationNumber
                                                                    && x.SimulationConfigurationId == _simulationConfigurationId);
            _maxPage = (int)Math.Ceiling(a: (double)_resultContext.SimulationOperations.Where(predicate: x => x.SimulationType == _simulationType
                                                                                      && x.SimulationNumber == _simulationNumber
                                                                                      && x.SimulationConfigurationId ==
                                                                                      _simulationConfigurationId) .Max(selector: m => m.End) / _timeSpan);

            foreach (var pow in pows)
            {
                
                // check if head element is Created,
                GanttTask timeline = GetOrCreateTimeline(pow: pow);
                long start = 0, end = 0;
                DefineStartEnd(start: ref start, end: ref end, item: pow);
                // Add Item To Timeline
                _ganttContext.Tasks.Add(
                    item: CreateGanttTask(
                        item: pow,
                        start: start,
                        end: end,
                        gc: timeline.color,
                        parent: timeline.id
                    )
                );

                if (folowLinks)
                {
                    var c = await _context.GetFollowerProductionOrderWorkSchedules(simulationWorkSchedule: pow, type: _simulationType, relevantItems: pows.ToList());

                    if (!c.Any()) continue;
                    // create Links for this pow
                    foreach (var link in c)
                    {
                        _ganttContext.Links.Add(
                            item: new GanttLink
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
                        .Where(predicate: x => x.type == GanttType.project && x.id == "M_" + pow.Machine);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(predicate: x => x.type == GanttType.project) + 1;
                        //var mg = _context.MachineGroups.First(x => x.Id.ToString() == pow.Machine.ToString()).Name;
                        var pt = CreateProjectTask(id: "M_" + pow.Machine, name: pow.Machine, desc: "", @group: 0, gc: (GanttColors)gc);
                        _ganttContext.Tasks.Add(item: pt);
                        return pt;
                    }
                    //break;
                case 4: // Production Order Based
                    project = _ganttContext.Tasks
                        .Where(predicate: x => x.type == GanttType.project && x.id == "P" + pow.ProductionOrderId);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(predicate: x => x.type == GanttType.project) + 1;
                        var pt = CreateProjectTask(id: "P" + pow.ProductionOrderId, name: "PO Nr.: " + pow.ProductionOrderId, desc: "", @group: 0, gc: (GanttColors)gc);
                        _ganttContext.Tasks.Add(item: pt);
                        return pt;
                    }
                    //break;
                default: // back and forward
                    project = _ganttContext.Tasks
                        .Where(predicate: x => x.type == GanttType.project && x.id == "O" + orderId);
                    if (project.Any())
                    {
                        return project.First();
                    }
                    else
                    {
                        var gc = _ganttContext.Tasks.Count(predicate: x => x.type == GanttType.project) + 1;
                        var pt = CreateProjectTask(id: "O" + orderId, name: _context.CustomerOrderParts.
                                                                    Include(navigationPropertyPath: x => x.CustomerOrder)
                                                                    .FirstOrDefault(predicate: x => x.Id == orderId)
                                                                        .CustomerOrder.Name, desc: "", @group: 0, gc: (GanttColors)gc);
                        _ganttContext.Tasks.Add(item: pt);
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
                start_date = start.GetDateFromMilliseconds().ToString(format: "dd-MM-yyyy HH:mm"),
                end_date = end.GetDateFromMilliseconds().ToString(format: "dd-MM-yyyy HH:mm"),
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
            if (_resultContext.SimulationOperations.Any(predicate: x => x.SimulationType == SimulationType.ForwardPlanning && x.Start > 0))
            {
                itemList.Add(item: new SelectListItem() {Text = "Forward", Value = "2"});
            }
            itemList.Add(item: new SelectListItem() { Text = "Capacity-Planning Machinebased", Value = "3" });
            itemList.Add(item: new SelectListItem() { Text = "Capacity-Planning Productionorderbased", Value = "4" });
            return new SelectList( items: itemList, dataValueField: "Value", dataTextField: "Text", selectedValue: selectedItem);
        }

    }
}