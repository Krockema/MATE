using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Nominal;
using Microsoft.AspNetCore.Mvc;

namespace Mate.ViewComponents
{
    public class ResourceUtilizationViewComponent : ViewComponent
    {

        private readonly MateResultDb _resultContext;

        public ResourceUtilizationViewComponent(MateResultDb resultContext)
        {
            _resultContext = resultContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            Task<List<Chart>> generateChartTask;
            List<string> _paramsList = paramsList;

            if (_paramsList[3] != null)
            {
                generateChartTask = GenerateChartTaskSingle(paramsList);
                ViewData["chart"] = await generateChartTask;
            }
            else
            {
                //generateChartTask = GenerateChartTasksMulti(paramsList);
                //ViewData["chart"] = await generateChartTask;
            }

            // create JS to Render Chart.
            ViewData["Type"] = paramsList[1];
            return View($"MachineWorkQueueStatus");
        }

        private Task<List<Chart>> GenerateChartTaskSingle(List<string> paramsList)
        {
            var generateChartTask = Task.Run(() =>
            {
                List<Chart> charts = new List<Chart>();
                List<string> resources = new List<string>();
                string resourceName = Convert.ToString(paramsList[3]);

                if (!_resultContext.Kpis.Any())
                {
                    return null;
                }

                SimulationType simType = (SimulationType) Enum.Parse(typeof(SimulationType), paramsList[1]);


                var simConfig =
                    _resultContext.SimulationConfigurations.Single(x => x.Id == Convert.ToInt32(paramsList[0]));

                var workKpi = _resultContext.Kpis.Where(x =>
                        x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                        && x.SimulationType == simType
                        && x.KpiType == KpiType.ResourceUtilization
                        && x.IsKpi
                        && x.Name == resourceName
                        && x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                    .ToList();

                var resource = workKpi.FirstOrDefault();

                //Scaling of Axis
                var maxX = Convert.ToInt32(Math.Floor((decimal) simConfig.SimulationEndTime.TotalMinutes / 1000) * 1000);
                var maxY = Math.Ceiling(workKpi.Max(x => x.Value)) + 5;
                var maxStepSize = Math.Ceiling(maxY * 0.1);

                //charts.Add(createChartForResource(workKpi, maxX, maxY, maxStepSize));
                resources.Add(resourceName);
                ViewData["resources"] = resources;
                return charts;
            });

            return generateChartTask;
        }
    }
}
