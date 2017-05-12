using Master40.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var po = _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder).ThenInclude(a => a.Article);
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
            string tl = "[[{ id: 'Säge', start: -1, end: 0, className: 'machineName' },"+
					"{ id: 'PO 1', start: 1, end: 4, className: 'styleA' },"+
					"{ id: 'PO 2', start: 6, end: 8, className: 'styleB' }],"+
                    "[{ id: 'Bohrer', start: -1, end: 0, className: 'machineName' },"+
					"{ id: 'PO 1', start: 5, end: 9, className: 'styleA' },"+
					"{ id: 'PO 2', start: 9, end: 13, className: 'styleB' }],"+
                    "[{ id: '1. Assembly', start: -1, end: 0, className: 'machineName' },"+
                    "{ id: 'PO 1', start: 10, end: 13, className: 'styleB', popup_html: 'I am <b>ALLMOST</b> finished!' }],"+
                    "[{ id: '2. Assembly', start: -1, end: 0, className: 'machineName' },"+
					"{ id: 'PO 3', start: 14, end: 17, className: 'styleC' }]"+
			        "]";
            return View("ProductionTimeline", tl);
        }
    }
}