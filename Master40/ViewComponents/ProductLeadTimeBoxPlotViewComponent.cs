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
using Master40.DB.Models;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeBoxPlotViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private List<Tuple<int, SimulationType>> _simList;
        public ProductLeadTimeBoxPlotViewComponent(ProductionDomainContext context)
        {
            _simList = new List<Tuple<int, SimulationType>>();
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            // Determine Type and Data
            _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[0]), (paramsList[1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[6]), (paramsList[7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[4]), (paramsList[5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[2]), (paramsList[3] == "Central") ? SimulationType.Central : SimulationType.Decentral));

            var kpi = new List<Kpi>();
            // charttype
            foreach (var sim in _simList)
            {
                var trick17 = _context.Kpis.Where(x => x.KpiType == KpiType.LeadTime
                                                       && x.SimulationConfigurationId == sim.Item1
                                                       && x.SimulationNumber == 1
                                                       && x.SimulationType == sim.Item2);
                kpi.AddRange(trick17.ToList());
            }
            var max = kpi.Max(m => m.Value);
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

                foreach (var sim in _simList)
                {
                    foreach (var product in products)
                    {
                        var boxplotValues = kpi.Where(x => x.IsKpi == false && x.Name == product
                                                      && x.KpiType == KpiType.LeadTime
                                                      && x.SimulationConfigurationId == sim.Item1
                                                      && x.SimulationNumber == 1
                                                      && x.SimulationType == sim.Item2).OrderBy(x => x.Value).ToList();
                        chart.Add(new BoxPlot
                        {
                            HeigestSample = (decimal)boxplotValues.ElementAt(4).Value,
                            UpperQartile = (decimal)boxplotValues.ElementAt(3).Value,
                            Median = (decimal)boxplotValues.ElementAt(2).Value,
                            LowerQuartile = (decimal)boxplotValues.ElementAt(1).Value,
                            LowestSample = (decimal)boxplotValues.ElementAt(0).Value,
                            Name = product + " \r\n SimId:" + sim.Item1 + " " + sim.Item2,
                            Color = colors.Color[i].Substring(0, colors.Color[i++].Length - 4)
                        });
                        i++;
                    }

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
