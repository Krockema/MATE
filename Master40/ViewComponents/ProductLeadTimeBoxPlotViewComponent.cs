using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;
using Master40.DB.Data.Helper;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeBoxPlotViewComponent : ViewComponent
    {
        private readonly ResultContext _context;
        private List<Tuple<int, SimulationType>> _simList;
        public ProductLeadTimeBoxPlotViewComponent(ResultContext context)
        {
            _simList = new List<Tuple<int, SimulationType>>();
            _context = context;
        }
        


        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            // Determine Type and Data
            _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 0]), item2: (paramsList[index: 1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 6]), item2: (paramsList[index: 7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 4]), item2: (paramsList[index: 5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 2]), item2: (paramsList[index: 3] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            _simList = _simList.OrderBy(keySelector: x => x.Item2).ThenBy(keySelector: x => x.Item1).ToList();
            var displayData = new List<Kpi>();
            var kpi = new List<Kpi>();
            
            // charttype
            foreach (var sim in _simList)
            {
                var trick17 = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.LeadTime
                                                       && x.SimulationConfigurationId == sim.Item1
                                                       && x.SimulationNumber == 1);
                                                       //&& x.SimulationType == sim.Item2);
                kpi.AddRange(collection: trick17.ToList());
            }
            var maxValue = kpi.Max(selector: x => x.Value);

            
            var generateChartTask = Task.Run(function: () =>
            {
                if (!kpi.Any())
                {
                    return null;
                }

                var chart = new List<BoxPlot>();
                var products = kpi.Select(selector: x => x.Name).Distinct().ToList();
                var colors = new ChartColors();
                int i = 0;

                foreach (var sim in _simList)
                {
                    displayData.AddRange(collection: kpi.Where(predicate: x => x.IsKpi 
                                                    && x.SimulationConfigurationId == sim.Item1 
                                                    && x.SimulationType == sim.Item2).OrderBy(keySelector: x => x.Value).ToList());


                    foreach (var product in products)
                    {
                        var boxplotValues = kpi.Where(predicate: x => x.IsKpi == false && x.Name == product
                                                      && x.KpiType == KpiType.LeadTime
                                                      && x.SimulationConfigurationId == sim.Item1
                                                      && x.SimulationNumber == 1
                                                      && x.IsFinal
                                                      && x.SimulationType == sim.Item2).OrderBy(keySelector: x => x.Value).ToList();
                        if(boxplotValues.Count == 0) continue;
                        chart.Add(item: new BoxPlot
                        {
                            HeigestSample = (decimal)boxplotValues.ElementAt(index: 4).Value,
                            UpperQartile = (decimal)boxplotValues.ElementAt(index: 3).Value,
                            Median = (decimal)boxplotValues.ElementAt(index: 2).Value,
                            LowerQuartile = (decimal)boxplotValues.ElementAt(index: 1).Value,
                            LowestSample = (decimal)boxplotValues.ElementAt(index: 0).Value,
                            Name = product + "<br> SimId:" + sim.Item1 + " " + sim.Item2,
                            Color = colors.Get(i).ToString()
                        });
                        if (_simList.Count() == 1)  i++;

                    }
                    i = i + 2;

                }
                
                //new BoxPlot{ HeigestSample=337, UpperQartile=195, Median=163, LowerQuartile= 136, LowestSample = 73, Name="Race-Truck", Color = "rgba(0,102,255," }
                return chart;
            });
            
            // create JS to Render Chart.
            var boxPlot = await generateChartTask;
            ViewData[index: "BoxPlot"] = boxPlot;
            ViewData[index: "Type"] = paramsList[index: 1];
            ViewData[index: "Data"] = displayData.Distinct().ToList();
            ViewData[index: "Max"] = Math.Ceiling(a: maxValue / 100) * 100;
            //ViewData["Max"] = Math.Ceiling((double)boxPlot.Max(x => x.HeigestSample)/100)*100;
            return View(viewName: $"ProductLeadTimeBoxPlot");
        }
    }
}
