using Master40.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var pows = _context.ProductionOrderWorkSchedule
                                .Include(m => m.MachineGroup)
                                .Include(a => a.ProductionOrder)
                                .ThenInclude(a => a.Article).OrderBy(a => a.MachineGroup).ToList();
            if (pows.Count == 0)
            {
                return View("ProductionTimeline", new string[4]); ;
            }

            var tl = new string[4];
            var maxEnd = pows.OrderBy(x => x.End).Last().End;
            var minStart = pows.OrderBy(x => x.Start).First().Start;
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
                t = t + ",{ id: '" + item.Name + "', start: " + item.Start.ToString() + ", end: " + item.End.ToString() + ", className: 'styleA'}";

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