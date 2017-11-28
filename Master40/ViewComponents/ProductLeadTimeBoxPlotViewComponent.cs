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
                                        && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                        && x.SimulationType == simType).ToList();
            var max = _context.Kpis.Where(x => x.KpiType == KpiType.LeadTime
                                        && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                        && x.SimulationNumber == Convert.ToInt32(paramsList[2])).Max(m => m.Value);
            var generateChartTask = Task.Run(() =>
            {
                if (!kpi.Any())
                {
                    return null;
                }

                var chart = new List<BoxPlot>();
                var products = kpi.Select(x => x.Name).Distinct().ToList();
                var colors = new ChartColor();
                int i = 0;

                foreach (var product in products)
                {
                    var boxplotValues = kpi.Where(x => x.IsKpi == false && x.Name == product).OrderBy(x => x.Value).ToList();
                    chart.Add(new BoxPlot
                    {
                        HeigestSample = (decimal)boxplotValues.ElementAt(4).Value,
                        UpperQartile = (decimal)boxplotValues.ElementAt(3).Value,
                        Median = (decimal)boxplotValues.ElementAt(2).Value,
                        LowerQuartile = (decimal)boxplotValues.ElementAt(1).Value,
                        LowestSample = (decimal)boxplotValues.ElementAt(0).Value,
                        Name = product,
                        Color = colors.Color[i].Substring(0, colors.Color[i++].Length - 4)
                    });
                }

                
                
                    //new BoxPlot{ HeigestSample=337, UpperQartile=195, Median=163, LowerQuartile= 136, LowestSample = 73, Name="Race-Truck", Color = "rgba(0,102,255," }
                return chart;
            });
           
            // create JS to Render Chart.
            var boxPlot = await generateChartTask;
            ViewData["BoxPlot"] = boxPlot;
            ViewData["Type"] = paramsList[1];
            ViewData["Data"] = kpi.Where(x => x.IsKpi == true).ToList();
            ViewData["Max"] = Math.Ceiling(max / 100) * 100;
            //ViewData["Max"] = Math.Ceiling((double)boxPlot.Max(x => x.HeigestSample)/100)*100;
            return View($"ProductLeadTimeBoxPlot");
        }
    }
}
