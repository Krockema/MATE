using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using static FCreateTaskItems;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class CollectorAnalyticResource : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticResource(ResourceList resources) : base()
        {
            _resources = resources;
            _taskItems.Add(JobType.OPERATION, new List<TaskItem>());
            _taskItems.Add(JobType.SETUP, new List<TaskItem>());
        }

        private Dictionary<string, List<TaskItem>> _taskItems { get; } = new Dictionary<string, List<TaskItem>>();
        public List<TaskItem> _taskArchive { get; private set; } = new List<TaskItem>();

        private ResourceList _resources { get; set; } = new ResourceList();
        private long lastIntervalStart { get; set; } = 0;
        public Collector Collector { get; set; }

        /// <summary>
        /// Required to get Number output with . instead of ,
        /// </summary>
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");

        private List<Kpi> Kpis { get; } = new List<Kpi>();

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type>
            {
                typeof(FCreateTaskItem),
                typeof(UpdateLiveFeed)
            };
        }

        public static CollectorAnalyticResource Get(ResourceList resources)
        {
            return new CollectorAnalyticResource(resources: resources);
        }

        public override bool Action(object message) =>
            throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FCreateTaskItem m: CreateTaskItem(m); break;
                case UpdateLiveFeed m: UpdateFeed(finalCall: m.GetObjectFromMessage); break;
                default: return false;
            }

            // Collector.messageHub.SendToAllClients(msg: $"Just finished {message.GetType().Name}");
            return true;
        }

        private void CreateTaskItem(FCreateTaskItem task)
        {
            var taskItem = new TaskItem
            {
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                Type = task.Type,
                Resource = task.Resource,
                Start = task.Start,
                End = task.End,
                Capability = task.Capability,
                Operation = task.Operation,
                GroupId = task.GroupId
            };
            if(_taskItems.TryGetValue(task.Type, out var list))
                list.Add(taskItem);
            else
                _taskItems.Add(taskItem.Type, new List<TaskItem>{ taskItem });

            Collector.messageHub.SendToClient(listener: "ganttChart", msg: JsonConvert.SerializeObject(value: taskItem));

        }

        private void UpdateFeed(bool finalCall)
        {
            ResourceUtilization();
            lastIntervalStart = Collector.Time;
          
            LogToDB(writeResultsToDB: finalCall);

            //TODO Only For Debugging
            /*if (finalCall)
            {
                var list = new List<GanttChartItem>();
                foreach (var item in _taskArchive)
                {
                    list.Add(new GanttChartItem
                    {
                        articleId = "none",
                        article = "none",
                        end =item.End.ToString(),
                        groupId = item.GroupId,
                        IsFinalized = "true",
                        IsProcessing = "true",
                        IsReady = "true",
                        IsWorking = "true",
                        operation = item.Operation,
                        operationId = item.Operation,
                        resource = item.Resource,
                        priority = "none",
                        start = item.Start.ToString()
                    });
                }
                CustomFileWriter.WriteToFile($"Logs//ResourceRunAt-{Collector.Time}.log",
                    JsonConvert.SerializeObject(list));
            }*/

            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from WorkSchedule");
        }

        private void LogToDB(bool writeResultsToDB)
        {
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using var ctx = ResultContext.GetContext(resultCon: Collector.Config.GetOption<DBConnectionString>().Value);

                //var refTask = _taskArchive.First();
                //CreateTaskItem(
                //    new FCreateTaskItem(
                //        JobType.OPERATION,
                //        refTask.Resource,
                //        refTask.Start, refTask.End, refTask.Capability, refTask.Operation, refTask.GroupId));
                //
                //ResourceUtilization();

                ctx.TaskItems.AddRange(entities: _taskArchive);
                ctx.SaveChanges();
                ctx.Kpis.AddRange(entities: Kpis);
                ctx.SaveChanges();
                ctx.Dispose();
            }
        }

        private void ResourceUtilization()
        {
            double divisor = Collector.Time - lastIntervalStart;
            var tupleList = new List<Tuple<string, string>>();
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from DataCollection");
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Time since last Update: " + divisor + "min");

            //JobWorkingTimes for interval
            var operationTasks = _taskItems.GetValueOrDefault(JobType.OPERATION);
            var setupTasks = _taskItems.GetValueOrDefault(JobType.SETUP);

            var lower_borders = from sw in operationTasks
                                where sw.Start < lastIntervalStart
                                      && sw.End > lastIntervalStart
                                      && sw.Resource != null
                                group sw by sw.Resource
                        into rs
                                select new Tuple<string, long>(rs.Key,
                                    rs.Sum(selector: x => x.End - lastIntervalStart));


            var upper_borders = from sw in operationTasks
                                where sw.Start < Collector.Time
                                   && sw.End > Collector.Time
                                   && sw.Resource != null
                                group sw by sw.Resource
                                into rs
                                select new Tuple<string, long>(rs.Key,
                                    rs.Sum(selector: x => Collector.Time - x.Start));


            var from_work = from sw in operationTasks
                            where sw.Start >= lastIntervalStart
                               && sw.End <= Collector.Time
                               && sw.Resource != null
                            group sw by sw.Resource
                            into rs
                            select new Tuple<string, long>(rs.Key,
                                rs.Sum(selector: x => x.End - x.Start));

            var reourceList = _resources.Select(selector: x => new Tuple<string, long>(x, 0));
            var merge = from_work.Union(second: lower_borders).Union(second: upper_borders).Union(second: reourceList).ToList();

            var final = from m in merge
                        group m by m.Item1
                into mg
                        select new Tuple<string, long>(mg.Key, mg.Sum(x => x.Item2));

            foreach (var item in final.OrderBy(keySelector: x => x.Item1))
            {
                var value = Math.Round(value: item.Item2 / divisor, digits: 3).ToString(provider: _cultureInfo);
                if (value == "NaN") value = "0";
                tupleList.Add(new Tuple<string, string>(item.Item1, value));
                Collector.CreateKpi(agent: Collector, value: value.Replace(".", ","), name: item.Item1, kpiType: KpiType.ResourceUtilization);
            }


            var totalLoad = Math.Round(value: final.Sum(selector: x => x.Item2) / divisor / final.Count() * 100, digits: 3).ToString(provider: _cultureInfo);
            if (totalLoad == "NaN") totalLoad = "0";
            Collector.CreateKpi(agent: Collector, value: totalLoad.Replace(".", ","), name: "TotalWork", kpiType: KpiType.ResourceUtilization);

            //ResourceSetupTimes for interval
            var setups_lower_borders = from sw in setupTasks
                                       where sw.Start < lastIntervalStart
                                             && sw.End > lastIntervalStart
                                             && sw.Resource != null
                                       group sw by sw.Resource
                into rs
                                       select new Tuple<string, long>(rs.Key,
                                           rs.Sum(selector: x => x.End - lastIntervalStart));

            var setups_upper_borders = from sw in setupTasks
                                       where sw.Start < Collector.Time
                                             && sw.End > Collector.Time
                                             && sw.Resource != null
                                       group sw by sw.Resource
                      into rs
                                       select new Tuple<string, long>(rs.Key,
                                           rs.Sum(selector: x => Collector.Time - x.Start));

            var totalSetups = from m in setupTasks
                              where m.Start >= lastIntervalStart
                                 && m.End <= Collector.Time
                              group m by m.Resource
                              into rs
                              select new Tuple<string, long>(rs.Key,
                                                              rs.Sum(selector: x => x.End - x.Start));

            var emptyResources = _resources.Select(selector: x => new Tuple<string, long>(x, 0));
            var union = totalSetups.Union(setups_lower_borders).Union(setups_upper_borders).Union(emptyResources).ToList();

            var finalSetup = from m in union
                             group m by m.Item1
                into mg
                             select new Tuple<string, long>(mg.Key, mg.Sum(x => x.Item2));

            foreach (var resource in finalSetup.OrderBy(keySelector: x => x.Item1))
            {
                var value = Math.Round(value: resource.Item2 / divisor, digits: 3).ToString(provider: _cultureInfo);
                if (value == "NaN") value = "0";
                var machine = resource.Item1.Replace(oldValue: ")", newValue: "").Replace(oldValue: "Resource(", newValue: "");
                var workValue = tupleList.Single(x => x.Item1 == resource.Item1).Item2;
                var all = workValue + " " + value;
                Collector.messageHub.SendToClient(listener: machine, msg: all);
                Collector.CreateKpi(agent: Collector, value: value.Replace(".", ","), name: resource.Item1, kpiType: KpiType.ResourceSetup);
            }

            var totalSetup = Math.Round(value: finalSetup.Where(x => !x.Item1.Contains("Operator")).Sum(selector: x => x.Item2) / divisor / finalSetup.Count() * 100, digits: 3).ToString(provider: _cultureInfo);
            if (totalSetup == "NaN") totalSetup = "0";
            Collector.CreateKpi(agent: Collector, value: totalSetup.Replace(".", ","), name: "TotalSetup", kpiType: KpiType.ResourceSetup);

            Collector.messageHub.SendToClient(listener: "TotalTimes", msg: JsonConvert.SerializeObject(value:
                new {
                    Time = Collector.Time,
                    Load = new { Work = totalLoad, Setup = totalSetup }
                }));

            //Persist Jobs
            //TODO Find another way to archive list

            var tasksToArchive = operationTasks.Where(x => x.End < lastIntervalStart).ToList();
            tasksToArchive.AddRange(setupTasks.Where(op => op.End < lastIntervalStart));
            
            operationTasks.RemoveAll(op => op.End < lastIntervalStart);
            setupTasks.RemoveAll(op => op.End < lastIntervalStart);
            
            _taskArchive.AddRange(tasksToArchive);
        }

    }
}

