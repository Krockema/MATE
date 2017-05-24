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


            var pows = _context.ProductionOrderWorkSchedule
                                .Include(m => m.MachineGroup)
                                .Include(a => a.ProductionOrder)
                                    .ThenInclude(p => p.ProductionOrderBoms)
                                .Include(a => a.ProductionOrder)
                                    .ThenInclude(x => x.DemandProviderProductionOrders)
                                .OrderBy(a => a.MachineGroup).ToList();
            //.ToList();
            var schedule = new List<ProductionTimeline>();
            int orderId = -1;
            int planningState = 1;
            int n = 1;
            if (Request.Method == "POST")
            {

                planningState = Convert.ToInt32(Request.Form["State"]);
                if (Request.Form["Order"] == -1)
                {
                    orderId = Convert.ToInt32(Request.Form["Order"]);

                    var demand = _context.Demands.OfType<DemandOrderPart>()
                                                .Include(x => x.OrderPart)
                                            .Where(o => o.OrderPart.OrderId == orderId).ToList();


                    var demandProviders = (from c in _context.Demands.OfType<DemandProviderProductionOrder>()
                                           join d in demand on c.DemandRequesterId equals d.Id
                                           select c).ToList();

                    pows = (from p in pows
                            join dp in demandProviders on p.ProductionOrderId equals dp.ProductionOrderId
                            select p).ToList();
                    schedule.AddRange(await GetDataForProductionOrderTimeline(pows, n++, orderId, planningState));
                }
            }
            else
            {
                var orderParts = _context.Orders.ToList();

                foreach (var item in orderParts)
                {
                    schedule.AddRange(await GetDataForProductionOrderTimeline(pows, n++, item.Id, planningState));
                }
            }

            var orderSelection = new SelectList(_context.Orders, "Id", "Name", orderId);
            ViewData["OrderId"] = orderSelection.AddFirstItem(new SelectListItem { Value = "-1", Text = "All" });

            return View("ProductionTimeline", JsonConvert.SerializeObject(schedule));
        }

        private async Task<List<ProductionTimeline>> GetDataForProductionOrderTimeline(List<ProductionOrderWorkSchedule> pows, int n, int orderId, int planningState)
        {
            var demand = _context.Demands.OfType<DemandOrderPart>()
                    .Include(x => x.OrderPart)
                    .Where(o => o.OrderPart.OrderId == orderId).ToList();

            var demandProviders = (from c in _context.Demands.OfType<DemandProviderProductionOrder>()
                                   join d in demand on c.DemandRequesterId equals d.Id
                                   select c).ToList();

            var powDetails = (from p in pows
                              join dp in demandProviders on p.ProductionOrderId equals dp.ProductionOrderId
                              select p).ToList();

            
            return await CreateTimelineForProductionOrder(powDetails, (ganttColors)n, planningState);
        }

        private async Task<List<ProductionTimeline>> CreateTimelineForProductionOrder(List<ProductionOrderWorkSchedule> pows, ganttColors gc, int planningState)
        {
            var schedule = new List<ProductionTimeline>();
            var today = DateTime.Now.GetEpochMilliseconds();
            foreach (var item in pows)
            {
                var c = await MasterDbHelper.GetPriorProductionOrderWorkSchedules(_context, item);
                var dependencies = "";
                if (c.Count() > 0)
                {
                    dependencies = c.FirstOrDefault().Id.ToString();
                };

                // chose planning method.
                long start, end;
                switch (planningState)
                {
                    case 1:
                        start = item.StartBackward;
                        end = item.EndBackward;
                        break;
                    case 2:
                        start = item.StartForward;
                        end = item.EndForward;
                        break;
                    default:
                        start = item.Start;
                        end = item.End;
                        break;
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
                           From = "/Date(" + (today + start * 3600000).ToString() + ")/",
                           To =  "/Date(" + (today + end * 3600000).ToString() + ")/",
                           CustomClass =  gc.ToString(), Dep = "" + dependencies
                        },
                    }
                });
            }
            return schedule;
        }

        public enum ganttColors
        {
            ganttRed,
            ganttBlue,
            ganttOrange,
            ganttGreen,
            ganttGray
        }

        public async Task<IViewComponentResult> InvokeAsyncOld(int productionOrderId)
        {
            var pows = _context.ProductionOrderWorkSchedule
                                .Include(m => m.MachineGroup)
                                .Include(a => a.ProductionOrder)
                                .ThenInclude(a => a.Article).OrderBy(a => a.MachineGroup).ToList();
            if (pows.Count == 0)
            {
                return View("ProductionTimeline", new string[4]); ;
            }

            var tl = new string[4];
            var maxEnd = pows.OrderBy(x => x.EndBackward).Last().EndBackward;
            var maxEndForward = pows.OrderBy(x => x.EndForward).Last().EndForward;
            if (maxEnd < maxEndForward)
                maxEnd = maxEndForward;
                
            var minStart = pows.OrderBy(x => x.StartBackward).First().StartBackward;
            var minStartForward = pows.OrderBy(x => x.StartForward).First().StartForward;
            if (minStart > minStartForward)
                minStart = minStartForward;
            tl[2] = minStart.ToString();
            tl[3] = maxEnd.ToString();

            // coloring the timeline - maybe later
            var color = Extensions.ExtensionMethods.GetGradients(
                new Color { R = 0, A = 0, B = 0, G = 0 },
                new Color { R = 255, A = 255, B = 0, G = 0 },
                pows.Count()
                );

            string t = "[[{ id: '" + pows.First().MachineGroup.Name + "', start: " + (Convert.ToInt32(minStart) - 1).ToString() + ", end: " + minStart + ", className: 'machineName' }";
            string group = pows.First().MachineGroup.Name;
            foreach (var item in pows)
            {
                //if (group != item.MachineGroup.Name)
                //{
                    group = item.MachineGroup.Name;
                    t = t + "],[{ id: '" + item.MachineGroup.Name + "', start: " + (Convert.ToInt32(minStart) - 1).ToString() + ", end: " + minStart + ", className: 'machineName' }";
                //}
                t = t + ",{ id: '" + item.Name + "', start: " + item.StartBackward.ToString() + ", end: " + item.EndBackward.ToString() + ", className: 'styleA'}";

            }
            foreach (var item in pows)
            {
                //if (group != item.MachineGroup.Name)
                //{
                group = item.MachineGroup.Name;
                t = t + "],[{ id: '" + item.MachineGroup.Name + "', start: " + (Convert.ToInt32(minStart) - 1).ToString() + ", end: " + minStart + ", className: 'machineName' }";
                //}
                t = t + ",{ id: '" + item.Name + "', start: " + item.StartForward.ToString() + ", end: " + item.EndForward.ToString() + ", className: 'styleA'}";

            }
            t = t + "]]";
            tl[0] = t;
            tl[1] = "{ start: " + minStart + ", end: " + maxEnd + ", indicatorsEvery: 1, share: .3  }";

           /*
            var data = [

                 [{ id: 'Säge', start: -1, end: 0, className: 'machineName' },
					{ id: 'PO 1', start: 1, end: 4, className: 'styleA' },
					{ id: 'PO 2', start: 6, end: 8, className: 'styleB' }],


                [{ id: 'Bohrer', start: -1, end: 0, className: 'machineName' },
					{ id: 'PO 1', start: 5, end: 9, className: 'styleA' },
					{ id: 'PO 2', start: 9, end: 13, className: 'styleB' }],

				[{ id: '1. Assembly', start: -1, end: 0, className: 'machineName' },
					{ id: 'PO 1', start: 10, end: 13, className: 'styleB', popup_html: 'I am <b>ALLMOST</b> finished!' }],

				[{ id: '2. Assembly', start: -1, end: 0, className: 'machineName' },
					{ id: 'PO 3', start: 14, end: 17, className: 'styleC' }]
			];
    */

            return View("ProductionTimeline", tl);
        }
    }

}