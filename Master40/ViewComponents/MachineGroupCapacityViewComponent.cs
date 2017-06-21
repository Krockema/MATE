﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using ChartJSCore.Models.Bar;
using Microsoft.EntityFrameworkCore;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class MachineGroupCapacityViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public MachineGroupCapacityViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(int schedulingState)
        {
            var generateCharTask = Task.Run(() =>
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
                Chart chart = new Chart();

                // charttype
                chart.Type = "bar";

                // use available hight in Chart
                chart.Options = new Options {MaintainAspectRatio = true};
                var data = new Data { Labels = GetRangeForSchedulingType(schedulingState) };
                var machineGroups = _context.MachineGroups.Select(x => x.Id);


                var yMaxScale = 0;

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();
                if (data.Labels.Any())
                {
                    foreach (var id in machineGroups)
                    {
                        data.Datasets.Add(GetCapacityForMachineGroupById(id, Convert.ToInt32(data.Labels.First()),
                            Convert.ToInt32(data.Labels.Last()), schedulingState));
                        var tempMax = Convert.ToInt32(data.Datasets.Last().Data.Max());
                        if (yMaxScale < tempMax)
                            yMaxScale = tempMax;
                    }
                }
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() {new BarScale {Stacked = false}};
                var yAxis = new List<Scale>() { new BarScale { Stacked = false, Ticks = new Tick{ BeginAtZero = true, Min = 0, Max = yMaxScale + 1, StepSize = 1 } } };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options() {Scales = new Scales {XAxes = xAxis, YAxes = yAxis}};

                return chart;
            });
           
            // create JS to Render Chart.
            ViewData["chart"] = await generateCharTask;

            return View($"MachineGroupCapacity");

        }

        /// <summary>
        /// creates Range for given Scheduling state
        /// </summary>
        /// <param name="schedulingState"></param>
        /// 1: Backward
        /// 2: Forward
        /// 3: Default
        /// <returns></returns>
        private List<string> GetRangeForSchedulingType(int schedulingState)
        {
            List<string> labeList = new List<string>();
            int min, max;

            switch (schedulingState)
            {
                case 1:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.StartBackward < 9000).Min(x => x.StartBackward);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.EndBackward < 9000).Max(x => x.EndBackward);
                    break;
                case 2:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.StartForward < 9000).Min(x => x.StartForward);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.EndForward < 9000).Max(x => x.EndForward);
                    break;
                default:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.Start < 9000).Min(x => x.Start);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.End < 9000).Max(x => x.End);
                    break;
            }

            for (int i = min; i < max; i++)
            {
                labeList.Add(i.ToString());
            }
            return labeList;
        }



        private BarDataset GetCapacityForMachineGroupById(int machineGroupId, int minRange, int maxRange, int schedulingState)
        {
            /*
            select ts.Time, Count(ts.Id)
            from[ProductionOrderWorkSchedulesByTimeSteps] ts
                join MachineGroupProductionOrderWorkSchedules ws on ts.MachineGroupProductionOrderWorkScheduleId = ws.Id
            where Time< 9000 AND MachineGroupId = 1
            group by Time, MachineGroupId
            Order By Time
            */
            /*
            var productionOrderWorkSchedulesBy =
                _context.ProductionOrderWorkSchedulesByTimeSteps
                    .Include(x => x.MachineGroupProductionOrderWorkSchedule)
                    .Where(x => x.MachineGroupProductionOrderWorkSchedule.MachineGroupId == machineGroupId && x.Time < 9000)
                    .GroupBy(x => x.Time).Select(n => new {
                                                            MetricName = n.Key,
                                                            MetricCount = n.Count()
                                                        }).ToList();
            */

            var productionOrderWorkSchedulesBy = _context.ProductionOrderWorkSchedule.Where(x => x.MachineGroupId == machineGroupId).AsNoTracking();
            
            var data = new List<double>();
            for (var i = minRange; i < maxRange; i++)
            {
                int item;
                switch (schedulingState)
                {
                    case 1:
                        item = productionOrderWorkSchedulesBy.Count(x => x.StartBackward <= i && x.EndBackward > i);
                        break;
                    case 2:
                        item = productionOrderWorkSchedulesBy.Count(x => x.StartForward <= i && x.EndForward > i);
                        break;
                    default:
                        item = productionOrderWorkSchedulesBy.Count(x => x.Start <= i && x.End > i);
                        break;
                }
                data.Add(item);
            }

            var dataset = new BarDataset()
            {
                Label = "MachineGroup " + machineGroupId.ToString(),
                BackgroundColor = new List<string> { new ChartColor().Color[machineGroupId - 1] },
                Data = data,
            };
            
            return dataset;


            
        }
    }
}