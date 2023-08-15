using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Akka.Hive.Actors;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.DataCore.ReportingModel;
using Mate.DataCore.ReportingModel.Interface;
using Mate.Production.Core.Agents.CollectorAgent.Types;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Environment.Records.Reporting;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;
using Newtonsoft.Json;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public class CollectorAnalyticResource : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticResource(ResourceDictionary resources,DateTime startTime) : base()
        {
            _resources = resources;
            lastIntervalStart = startTime;
            _kpiManager = new KpiManager();
            _taskItems.Add(JobType.OPERATION, new List<ISimulationTask>());
            _taskItems.Add(JobType.SETUP, new List<ISimulationTask>());

            _taskArchive.Add(JobType.OPERATION, new List<ISimulationTask>());
            _taskArchive.Add(JobType.SETUP, new List<ISimulationTask>());
        }

        private Dictionary<string, List<ISimulationTask>> _taskItems { get; } = new Dictionary<string, List<ISimulationTask>>();
        public Dictionary<string, List<ISimulationTask>> _taskArchive { get; private set; } = new Dictionary<string, List<ISimulationTask>>();

        private ResourceDictionary _resources { get; set; } = new ResourceDictionary();
        private DateTime lastIntervalStart { get; set; }
        public Collector Collector { get; set; }
        private KpiManager _kpiManager { get; }
        /// <summary>
        /// Required to get Number output with . instead of ,
        /// </summary>
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");
        
        internal static List<Type> GetStreamTypes()
        {
            return new List<Type>
            {
                typeof(CreateTaskItemRecord),
                typeof(UpdateLiveFeed)
            };
        }

        public static CollectorAnalyticResource Get(ResourceDictionary resources, DateTime startTime)
        {
            return new CollectorAnalyticResource(resources: resources, startTime: startTime);
        }

        public override bool Action(object message) =>
            throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(MessageMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case CreateTaskItemRecord m: CreateTaskItem(m); break;
                case UpdateLiveFeed m: UpdateFeed(finalCall: m.GetObjectFromMessage); break;
                default: return false;
            }

            // Collector.messageHub.SendToAllClients(msg: $"Just finished {message.GetType().Name}");
            return true;
        }

        private void CreateTaskItem(CreateTaskItemRecord task)
        {
            var taskItem = new TaskItem
            {
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                Type = task.Type,
                Resource = task.Resource,
                ResourceType =  _resources.Single(x => x.Key.Equals(task.ResourceId)).Value.ResourceType,
                Start = task.Start,
                End = task.End,
                CapabilityName = task.Capability,
                Operation = task.Operation,
                GroupId = task.GroupId
            };
            if(_taskItems.TryGetValue(task.Type, out var list))
                list.Add(taskItem);
            else
                _taskItems.Add(taskItem.Type, new List<ISimulationTask> { taskItem });

            Collector.messageHub.SendToClient(listener: "ganttChart", msg: JsonConvert.SerializeObject(value: taskItem));

            CreateAndSendArElement(task);

        }

        private void UpdateFeed(bool finalCall)
        {
            //JobWorkingTimes for interval
            var operationTasks = _taskItems.GetValueOrDefault(JobType.OPERATION);
            var setupTasks = _taskItems.GetValueOrDefault(JobType.SETUP);
            var archiveOperationTask = _taskArchive.GetValueOrDefault(JobType.OPERATION);
            var archiveSetupTask = _taskArchive.GetValueOrDefault(JobType.SETUP);
            
            //Save last Timespan
            var tempOperationTasks = operationTasks.Where(x => x.End > lastIntervalStart).ToList();
            var tempSetupTasks = setupTasks.Where(x => x.End > lastIntervalStart).ToList();

            //Remove All Saved for next Round.
            operationTasks.RemoveAll(op => op.End < lastIntervalStart);
            setupTasks.RemoveAll(op => op.End < lastIntervalStart);
            
            if (finalCall)
            {
                tempOperationTasks.AddRange(archiveOperationTask);
                tempSetupTasks.AddRange(archiveSetupTask);
                lastIntervalStart = Collector.Config.GetOption<SimulationStartTime>().Value +  Collector.Config.GetOption<SettlingStart>().Value;
            }

            ResourceUtilization(finalCall, tempOperationTasks, tempSetupTasks);

            var resourceDictionary = new ResourceDictionary();
            foreach (var resource in _resources)
            {
                if (resource.Value.ResourceType == ResourceType.Workcenter)
                {
                    resourceDictionary.Add(resource.Key,resource.Value);
                }
            }

            var OEE = OverallEquipmentEffectiveness(resources: resourceDictionary, lastIntervalStart, Collector.Time.Value, tempOperationTasks, tempSetupTasks);
            Collector.CreateKpi(Collector, OEE, "OEE", KpiType.Ooe, finalCall);

            archiveOperationTask.AddRange(tempOperationTasks);
            archiveSetupTask.AddRange(tempSetupTasks);

            LogToDB(writeResultsToDB: finalCall);

            lastIntervalStart = Collector.Time.Value;
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from WorkSchedule");
        }


        /// <summary>
        /// OEE for dashboard
        /// </summary>
        private string OverallEquipmentEffectiveness(ResourceDictionary resources, DateTime startInterval, DateTime endInterval, List<ISimulationTask> operationTasks, List<ISimulationTask> setupTasks)
        {
            /* ------------- Total Production Time --------------------*/
            var totalInterval = endInterval - startInterval;
            var totalProductionTime = totalInterval * resources.Count;

            /* ------------- RunTime --------------------*/
            var totalPlannedDowntime = TimeSpan.Zero;
            var totalBreakTime = TimeSpan.Zero;
            TimeSpan runTime = totalProductionTime - (totalPlannedDowntime - totalBreakTime);

            /* ------------- WorkTime --------------------*/ //TODO add unplanned breakdown
            var breakDown = TimeSpan.Zero;


            var setupTime = _kpiManager.GetTotalTimeForInterval(resources, setupTasks, startInterval, endInterval);
            
            var totalUnplannedDowntime = breakDown + setupTime;

            var workTime = runTime - totalUnplannedDowntime;

            /* ------------- PerformanceTime --------------------*/
            var jobTime = _kpiManager.GetTotalTimeForInterval(resources, operationTasks, startInterval, endInterval);

            var idleTime = workTime - jobTime;

            // var reducedSpeed = 0L; //TODO if this is implemented the GetTotalTimeForInterval must change. to reflect speed div.

            var performanceTime = jobTime;

            /* ------------- zeroToleranceTime --------------------*/

            //TODO Feature: Branch QualityManagement
            var goodGoods = 35L;
            var badGoods = 0L;
            var totalGoods = goodGoods + badGoods;

            TimeSpan zeroToleranceTime = performanceTime / totalGoods * goodGoods;

            //1.Parameter Availability calculation
            double availability = workTime / runTime;

            //2. Parameter Performance calculation
            double performance = performanceTime / workTime;

            //3. Parameter Quality calculation
            double quality = zeroToleranceTime / performanceTime;

            //Total OEE
            var totalOEE = availability * performance * quality;

            var totalOEEString = Math.Round(totalOEE * 100, 2).ToString();
            if (totalOEEString == "NaN") totalOEEString = "0";

            Collector.messageHub.SendToClient(listener: "oeeListener", msg: totalOEEString);

            return totalOEEString;

            //TODO Feature: vX.0 Enhance GUI with details about OEE

        }

        private void LogToDB(bool writeResultsToDB)
        {
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = MateResultDb.GetContext(resultCon: Collector.Config.GetOption<ResultsDbConnectionString>().Value))
                {
                    ctx.TaskItems.AddRange(entities:  _taskArchive.Select(x => x.Value).ToArray()[0].ToList().Cast<TaskItem>());
                    ctx.TaskItems.AddRange(entities: _taskArchive.Select(x => x.Value).ToArray()[1].ToList().Cast<TaskItem>());
                    ctx.SaveChanges();
                    ctx.Kpis.AddRange(entities: Collector.Kpis);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }

        private void ResourceUtilization(bool finalCall, List<ISimulationTask> operationTasks, IList<ISimulationTask> setupTasks)
        {
            
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time.Value + ") Update Feed from DataCollection");

            var setupKpiType = KpiType.ResourceSetup;
            var utilKpiType = KpiType.ResourceUtilization;

            if (finalCall)
            {
                // reset LastIntervallStart
                lastIntervalStart = Collector.Config.GetOption<SimulationStartTime>().Value + Collector.Config.GetOption<SettlingStart>().Value;
                setupKpiType = KpiType.ResourceSetupTotal;
                utilKpiType = KpiType.ResourceUtilizationTotal;
            }

            double divisor = (Collector.Time.Value - lastIntervalStart).TotalMinutes;
            var tupleList = new List<Tuple<string, string>>();

            //resource to ensure entries, even the resource it not used in interval
            var resourceList = _resources.Select(x => new Tuple<string, TimeSpan>(x.Value.Name.Replace(" ", ""), TimeSpan.Zero));
            
            var lower_borders = from sw in operationTasks
                                where sw.Start < lastIntervalStart
                                      && sw.End > lastIntervalStart
                                      && sw.Mapping != null
                                group sw by sw.Mapping
                        into rs
                                select new Tuple<string, TimeSpan>(rs.Key,
                                    rs.Sum(x => x.End - lastIntervalStart));


            var upper_borders = from sw in operationTasks
                                where sw.Start < Collector.Time.Value
                                   && sw.End > Collector.Time.Value
                                   && sw.Mapping != null
                                group sw by sw.Mapping
                                into rs
                                select new Tuple<string, TimeSpan>(rs.Key,
                                    rs.Sum(x => Collector.Time.Value - x.Start));


            var from_work = from sw in operationTasks
                            where sw.Start >= lastIntervalStart
                               && sw.End <= Collector.Time.Value
                               && sw.Mapping != null
                            group sw by sw.Mapping
                            into rs
                            select new Tuple<string, TimeSpan>(rs.Key,
                                rs.Sum(x => x.End - x.Start));

            var merge = from_work.Union(second: lower_borders).Union(second: upper_borders).Union(second: resourceList).ToList();

            var final = from m in merge
                        group m by m.Item1
                into mg
                        select new Tuple<string, TimeSpan>(mg.Key, mg.Sum(x => x.Item2));

            foreach (var item in final.OrderBy(x => x.Item1))
            {
                var value = Math.Round(item.Item2.TotalMinutes / divisor, digits: 3).ToString(provider: _cultureInfo);
                if (value == "NaN" || value == "Infinity") value = "0";
                tupleList.Add(new Tuple<string, string>(item.Item1, value));
                Collector.CreateKpi(agent: Collector, value: value.Replace(".", ","), name: item.Item1, kpiType: utilKpiType, finalCall);
            }


            var totalLoad = Math.Round(final.Sum(x => x.Item2).TotalMinutes / divisor / final.Count() * 100, digits: 3).ToString(provider: _cultureInfo);
            if (totalLoad == "NaN" || totalLoad == "Infinity") totalLoad = "0";
            Collector.CreateKpi(agent: Collector, value: totalLoad.Replace(".", ","), name: "TotalWork", kpiType: utilKpiType, finalCall);

            //ResourceSetupTimes for interval
            var setups_lower_borders = from sw in setupTasks
                                       where sw.Start < lastIntervalStart
                                             && sw.End > lastIntervalStart
                                             && sw.Mapping != null
                                       group sw by sw.Mapping
                into rs
                                       select new Tuple<string, TimeSpan>(rs.Key,
                                           rs.Sum(x => x.End - lastIntervalStart));

            var setups_upper_borders = from sw in setupTasks
                                       where sw.Start < Collector.Time.Value
                                             && sw.End > Collector.Time.Value
                                             && sw.Mapping != null
                                       group sw by sw.Mapping
                      into rs
                                       select new Tuple<string, TimeSpan>(rs.Key,
                                           rs.Sum(x => Collector.Time.Value - x.Start));

            var totalSetups = from m in setupTasks
                              where m.Start >= lastIntervalStart
                                 && m.End <= Collector.Time.Value
                              group m by m.Mapping
                              into rs
                              select new Tuple<string, TimeSpan>(rs.Key,
                                                              rs.Sum(x => x.End - x.Start));

            var union = totalSetups.Union(setups_lower_borders).Union(setups_upper_borders).Union(resourceList).ToList();

            var finalSetup = from m in union
                             group m by m.Item1
                into mg
                             select new Tuple<string, TimeSpan>(mg.Key, mg.Sum(x => x.Item2));

            // Black magic is happening (Backend Value preperation for Frontend Chart)

            foreach (var resource in finalSetup.OrderBy(keySelector: x => x.Item1))
            {
                var value = Math.Round(value: resource.Item2.TotalMinutes / divisor, digits: 3).ToString(provider: _cultureInfo);
                if (value == "NaN" || value == "Infinity") value = "0";
                var machine = resource.Item1.Replace(oldValue: ")", newValue: "").Replace(oldValue: "Resource(", newValue: "").Replace(oldValue: " ", newValue: "");
                var workValue = tupleList.Single(x => x.Item1 == machine).Item2;
                var all = workValue + " " + value;
                Collector.messageHub.SendToClient(listener: machine, msg: all);
                Collector.CreateKpi(agent: Collector, value: value.Replace(".", ","), name: resource.Item1, kpiType: setupKpiType, finalCall);
            }

            var totalSetup = Math.Round(value: finalSetup.Where(x => !x.Item1.Contains("Operator")).Sum(selector: x => x.Item2.TotalMinutes) / divisor / finalSetup.Count() * 100, digits: 3).ToString(provider: _cultureInfo);
            if (totalSetup == "NaN" || totalSetup == "Infinity") totalSetup = "0";
            Collector.CreateKpi(agent: Collector, value: totalSetup.Replace(".", ","), name: "TotalSetup", kpiType: setupKpiType, finalCall);

            Collector.messageHub.SendToClient(listener: "TotalTimes", msg: JsonConvert.SerializeObject(value:
                new {
                    Time = Collector.Time,
                    Load = new { Work = totalLoad, Setup = totalSetup }
                }));
        }

        private void CreateAndSendArElement(CreateTaskItemRecord taskItem)
        {
            var resourceType = _resources.Single(x => x.Key.Equals(taskItem.ResourceId)).Value.ResourceType;

            if (taskItem.Type == "Setup" || resourceType != ResourceType.Workcenter) return;

            var arSimulationElement = new ArSimElement(
                stuffId: Guid.NewGuid().ToString(),
                targetMachineId: taskItem.ResourceId.ToString(),
                processDuration: (long)((taskItem.End - taskItem.Start).TotalMilliseconds), // convert to milliseconds
                transportDurRatio: 40
            );
            

            Collector.messageHub.SendToClient("SIM", System.Text.Json.JsonSerializer.Serialize(arSimulationElement));
        }

    }
}

