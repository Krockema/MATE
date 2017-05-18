using Master40.Data;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Master40.Extensions.ExtensionMethods;

namespace Master40.ViewComponents
{
    public class ProductionTimelineViewComponent : ViewComponent
    {
        private readonly MasterDBContext _context;

        public ProductionTimelineViewComponent(MasterDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productionOrderId)
        {
            var schedule = new List<ProductionTimeline>();
            var pows = _context.ProductionOrderWorkSchedule
                    .Include(m => m.MachineGroup)
                    .Include(a => a.ProductionOrder)
                        .ThenInclude(p => p.ProductionOrderBoms)
                    .OrderBy(a => a.MachineGroup).ToList();


            var today = DateTime.Now.GetEpochMilliseconds();

            foreach (var item in pows)
            {
                schedule.Add(new ProductionTimeline
                {
                    Name = item.MachineGroup.Name,
                    Desc = "&rarr; ",
                    Values =
                    new List<ProductionTimelineItem>
                    {
                       new ProductionTimelineItem
                       {
                           Id = item.ProductionOrderWorkScheduleId.ToString(), Desc = item.Name, Label = "P.O.: " + item.ProductionOrderId.ToString(),
                           From = "/Date(" + (today + (long)item.StartBackward * 86400000).ToString() + ")/",
                           To =  "/Date(" + (today + (long)(item.EndBackward-1) * 86400000).ToString() + ")/",
                           CustomClass =  "ganttGreen", Dep = ""
                        },
                    }
                });
            }
            


            return View("ProductionTimeline", JsonConvert.SerializeObject(schedule));
        }
        /*
        [
            {
                "name": " Step A ", "desc": "&rarr; Step B", "values": [{ "id": "b0", "from": "/Date(1320182000000)/", "to": "/Date(1320301600000)/", "desc": "Id: 0<br/>Name:   Step A", "label": " Step A", "customClass": "ganttRed", "dep": "b1" },
                { "id": "bx", "from": "/Date(1320601600000)/", "to": "/Date(1320870400000)/", "desc": "Id: 0<br/>Name:   Step A", "label": " Step A", "customClass": "ganttRed", "dep": "b1" }]
            },
            { "name": " Step B ", "desc": "&rarr; step C", "values": [{ "id": "b1", "from": "/Date(1320601600000)/", "to": "/Date(1320870400000)/", "desc": "Id: 1<br/>Name:   Step B", "label": " Step B", "customClass": "ganttOrange", "dep": "b2" }] },
            { "name": " Step C ", "desc": "&rarr; step D", "values": [{ "id": "b2", "from": "/Date(1321192000000)/", "to": "/Date(1321500400000)/", "desc": "Id: 2<br/>Name:   Step C", "label": " Step C", "customClass": "ganttGreen", "dep": "b3" }] },
            { "name": " Step D ", "desc": "&rarr; step E", "values": [{ "id": "b3", "from": "/Date(1320302400000)/", "to": "/Date(1320551600000)/", "desc": "Id: 3<br/>Name:   Step D", "label": " Step D", "dep": "b4" }] },
            { "name": " Step E ", "desc": "&mdash;", "values": [{ "id": "b4", "from": "/Date(1320802400000)/", "to": "/Date(1321994800000)/", "desc": "Id: 4<br/>Name:   Step E", "label": " Step E", "customClass": "ganttRed" }] },
            { "name": " Step F ", "desc": "&rarr; step B", "values": [{ "id": "b5", "from": "/Date(1320192000000)/", "to": "/Date(1320401600000)/", "desc": "Id: 5<br/>Name:   Step F", "label": " Step F", "customClass": "ganttOrange", "dep": "b1" }] },
            { "name": " Step G ", "desc": "&rarr; step C", "values": [{ "id": "b6", "from": "/Date(1320401600000)/", "to": "/Date(1320570400000)/", "desc": "Id: 6<br/>Name:   Step G", "label": " Step G", "customClass": "ganttGreen", "dep": "b8" }] },
            { "name": " Step H ", "desc": "&rarr; Step J", "values": [{ "id": "b7", "from": "/Date(1321192000000)/", "to": "/Date(1321500400000)/", "desc": "Id: 7<br/>Name:   Step H", "label": " Step H", "dep": "b9" }] },
            { "name": " Step I ", "desc": "&rarr; Step H", "values": [{ "id": "b8", "from": "/Date(1320302400000)/", "to": "/Date(1320551600000)/", "desc": "Id: 8<br/>Name:   Step I", "label": " Step I", "customClass": "ganttRed", "dep": "b7" }] },
            { "name": " Step J ", "desc": "&mdash;", "values": [{ "id": "b9", "from": "/Date(1320802400000)/", "to": "/Date(1321994800000)/", "desc": "Id: 9<br/>Name:   Step J", "label": " Step J", "customClass": "ganttOrange" }] }
        ];
        */
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