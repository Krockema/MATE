using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class MachineGroupCapacityViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultContext;


        public MachineGroupCapacityViewComponent(ProductionDomainContext context, ResultContext resultContext)
        {
            _context = context;
            _resultContext = resultContext;
        }



        public async Task<IViewComponentResult> InvokeAsync(int schedulingState)
        {
            var generateCharTask = Task.Run(function: () =>
            {
                // check if it hase Scheduling state is set
                /*int schedulingState = 1;
                if (Request.Method == "POST")
                {
                    // catch scheduling state
                    schedulingState = Convert.ToInt32(Request.Form["SchedulingState"]);
                }
                */
                // Create Chart Object

                var chart = new Chart
                {
                    Type = Enums.ChartType.Bar,
                    Options = new Options {MaintainAspectRatio = true}
                };

                var simType = (schedulingState == 1) ? SimulationType.BackwardPlanning : SimulationType.ForwardPlanning;
                    if (schedulingState == 3 || schedulingState == 4) simType = SimulationType.Central;
                    

                // charttype
                var schedules =
                    _resultContext.SimulationOperations.Where(
                        predicate: x => x.SimulationConfigurationId == 1 && x.SimulationNumber == 1 && x.SimulationType == simType).ToList();

                // no data no Gant.
                if (schedules.Count == 0)
                    return null;

                // use available hight in Chart
                var data = new Data { Labels = GetRangeForSchedulingType(schedulingState: schedulingState, schedules: schedules) };
                var machineGroups = _context.ResourceSkills.Select(selector: x => x.Id);
                var yMaxScale = 0;

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();
                if (data.Labels.Any())
                {
                    foreach (var id in machineGroups)
                    {
                        data.Datasets.Add(item: GetCapacityForMachineGroupById(machineGroupId: id, minRange: Convert.ToInt32(value: data.Labels.First()),
                            maxRange: Convert.ToInt32(value: data.Labels.Last()), state: schedulingState, simulationWorkschedule: schedules));
                        var tempMax = Convert.ToInt32(value: data.Datasets.Last().Data.Max());
                        if (yMaxScale < tempMax)
                            yMaxScale = tempMax;
                    }
                }
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() {new CartesianScale {Stacked = false}};
                var yAxis = new List<Scale>() { new CartesianScale { Stacked = false, Ticks = new CartesianLinearTick{ BeginAtZero = true, Min = 0, Max = yMaxScale + 1, StepSize = 1 } } };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options() {Scales = new Scales {XAxes = xAxis, YAxes = yAxis}, MaintainAspectRatio = false, Responsive = true };

                return chart;
            });
           
            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateCharTask;

            return View(viewName: $"MachineGroupCapacity");

        }

        /// <summary>
        /// creates Range for given Scheduling state
        /// </summary>
        /// <param name="schedulingState"></param>
        /// <param name="schedules"></param>
        /// 1: Backward
        /// 2: Forward
        /// 3: Default
        /// <returns></returns>
        private List<string> GetRangeForSchedulingType(int schedulingState, List<SimulationWorkschedule> schedules)
        {
            List<string> labeList = new List<string>();
            int min, max;

            switch (schedulingState)
            {
                case 1:
                    min = schedules.Where(predicate: x => x.SimulationType == SimulationType.BackwardPlanning).Min(selector: x => x.Start);
                    max = schedules.Where(predicate: x => x.SimulationType == SimulationType.BackwardPlanning).Max(selector: x => x.End);
                    break;
                case 2:
                    min = schedules.Where(predicate: x => x.SimulationType == SimulationType.ForwardPlanning).Min(selector: x => x.Start);
                    max = schedules.Where(predicate: x => x.SimulationType == SimulationType.ForwardPlanning).Max(selector: x => x.End);
                    break;
                default:
                    min = schedules.Where(predicate: x => x.SimulationType == SimulationType.Central).Min(selector: x => x.Start);
                    max = schedules.Where(predicate: x => x.SimulationType == SimulationType.Central).Max(selector: x => x.End);
                    break;
/*
                case 1:
                    min = _context.ProductionOrderWorkSchedules.Where(x => x.StartBackward < 9000).Min(x => x.StartBackward);
                    max = _context.ProductionOrderWorkSchedules.Where(x => x.EndBackward < 9000).Max(x => x.EndBackward);
                    break;
                case 2:
                    min = _context.ProductionOrderWorkSchedules.Where(x => x.StartForward < 9000).Min(x => x.StartForward);
                    max = _context.ProductionOrderWorkSchedules.Where(x => x.EndForward < 9000).Max(x => x.EndForward);
                    break;
                default:
                    min = _context.ProductionOrderWorkSchedules.Where(x => x.Start < 9000).Min(x => x.Start);
                    max = _context.ProductionOrderWorkSchedules.Where(x => x.End < 9000).Max(x => x.End);
                    break;
                    */
            }

            for (int i = min; i < max; i++)
            {
                labeList.Add(item: i.ToString());
            }
            return labeList;
        }



        private BarDataset GetCapacityForMachineGroupById(int machineGroupId, int minRange, int maxRange,int state, List<SimulationWorkschedule> simulationWorkschedule)
        {
       
            var productionOrderWorkSchedulesBy = simulationWorkschedule.Where(predicate: x => x.Machine == machineGroupId.ToString());
            
            var data = new List<double>();
            for (var i = minRange; i < maxRange; i++)
            {
                int item;
                switch (state)
                    {
                        case 1:
                            item = productionOrderWorkSchedulesBy.Where(predicate: x => x.SimulationType == SimulationType.BackwardPlanning).Count(predicate: x => x.Start <= i && x.End > i);
                            break;
                        case 2:
                            item = productionOrderWorkSchedulesBy.Where(predicate: x => x.SimulationType == SimulationType.ForwardPlanning).Count(predicate: x => x.Start <= i && x.End > i);
                        break;
                        default:
                            item = productionOrderWorkSchedulesBy.Where(predicate: x => x.SimulationType == SimulationType.Central).Count(predicate: x => x.Start <= i && x.End > i);
                        break;
                    }
                data.Add(item: item);
            }

            var dataset = new BarDataset()
            {
                Label = "MachineGroup " + machineGroupId.ToString(),
                BackgroundColor = new List<ChartColor> { new ChartColors().Get(index: machineGroupId - 1) },
                Data = data,
            };
            
            return dataset;


            
        }
    }
}
