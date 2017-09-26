using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;
using Master40.DB.Data.Helper;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeBoxPlotViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public ProductLeadTimeBoxPlotViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpi = _context.Kpis.Where(x => x.KpiType == KpiType.LeadTime
                                    && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                    && x.SimulationType == simType);

            var generateChartTask = Task.Run(() =>
            {
                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }
                var chart = new List<BoxPlot>
                {
                    new BoxPlot{ HeigestSample=379, UpperQartile=220, Median=180, LowerQuartile= 150, LowestSample = 95, Name="Dump-Truck", Color = "rgba(255,136,51," },
                    new BoxPlot{ HeigestSample=337, UpperQartile=195, Median=163, LowerQuartile= 136, LowestSample = 73, Name="Race-Truck", Color = "rgba(0,102,255," }
                };
                return chart;
            });
           
            // create JS to Render Chart.
            var boxPlot = await generateChartTask;
            ViewData["BoxPlot"] = boxPlot;
            ViewData["Type"] = paramsList[1];
            ViewData["Data"] = kpi.ToList();
            ViewData["Max"] = Math.Ceiling((double)boxPlot.Max(x => x.HeigestSample)/100)*100;
            return View($"ProductLeadTimeBoxPlot");
        }
    }
}
