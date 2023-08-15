using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using IdentityModel;
using Mate.DataCore.Data.Context;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.DataCore.ReportingModel.Interface;
using Mate.Production.Core.Agents.CollectorAgent.Types;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Reporting;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Types;
using Newtonsoft.Json;

using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public class CollectorAnalyticJob : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticJob(ResourceDictionary resources) : base()
        {
            _stopWatch.Start();
            _resources = resources;
            _settlingStart = Collector.Config.GetOption<SimulationStartTime>().Value + Collector.Config.GetOption<SettlingStart>().Value;
        }
        private Stopwatch _stopWatch { get; } = new Stopwatch();
        List<long> _runTime { get; } = new List<long>();
        private List<Job> simulationJobs { get; } = new List<Job>();
        private List<Setup> simulationResourceSetups { get; } = new List<Setup>();
        private KpiManager kpiManager { get; } = new KpiManager();

        private DateTime lastIntervalStart { get; set; }

        private DateTime _settlingStart { get; set; }

        private List<UpdateSimulationJobRecord> _updatedSimulationJob { get; } = new ();
        private List<ThroughPutTimeRecord> _ThroughPutTimes { get; } = new ();
        private List<ComputationalTimerRecord> _ComputationalTimes { get; } = new List<ComputationalTimerRecord>();
        private ResourceDictionary _resources { get; set; } = new ResourceDictionary();
        public Collector Collector { get; set; }
        //
        IdleTime idleTime = new IdleTime();
        
        /// <summary>
        /// Required to get Number output with . instead of ,
        /// </summary>
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(CreateSimulationJobRecord), // Published by Production Agent after Creating jobs from Operations
                                     typeof(UpdateSimulationJobRecord), // 
                                     typeof(UpdateSimulationWorkProviderRecord),
                                     typeof(UpdateLiveFeed),
                                     typeof(ThroughPutTimeRecord),
                                     typeof(ComputationalTimerRecord),
                                     typeof(Hub.Instruction.Default.AddResourceToHub),
                                     typeof(BasicInstruction.ResourceBrakeDown),
                                     typeof(CreateSimulationResourceSetupRecord)

            };
        }

        public static CollectorAnalyticJob Get(ResourceDictionary resources)
        {
            return new CollectorAnalyticJob(resources: resources);
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(MessageMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case CreateSimulationJobRecord m: CreateJob(simJob: m); break;
                case UpdateSimulationJobRecord m: UpdateJob(simJob: m); break;
                case CreateResourceSetupRecord m: CreateSetup(m); break;
                case UpdateSimulationWorkProviderRecord m: UpdateProvider(uswp: m); break;
                case ThroughPutTimeRecord m: UpdateThroughputTimes(m); break;
                case ComputationalTimerRecord m: UpdateComputationalTimes(m); break;
                case UpdateLiveFeed m: UpdateFeed(finalCall: m.GetObjectFromMessage); break;
                //case Hub.Instruction.AddResourceToHub m: RecoverFromBreak(item: m.GetObjectFromMessage); break;
                //case BasicInstruction.ResourceBrakeDown m: BreakDwn(item: m.GetObjectFromMessage); break;
                default: return false;
            }
            // Collector.messageHub.SendToAllClients(msg: $"Just finished {message.GetType().Name}");
            return true;
        }


        /// <summary>
        /// collect the resourceSetups of resource, cant be updated afterward
        /// </summary>
        /// <param name="simulationResourceSetup"></param>
        private void CreateSetup(CreateResourceSetupRecord simulationResourceSetup)
        {
            var _SimulationResourceSetup = new Setup
            {
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                Time = Collector.Time.Value,
                CapabilityProvider = simulationResourceSetup.CapabilityProvider,
                CapabilityName = simulationResourceSetup.CapabilityName,
                Start = simulationResourceSetup.Start,
                End = simulationResourceSetup.Start + simulationResourceSetup.Duration,
                ExpectedDuration = simulationResourceSetup.ExpectedDuration,
                SetupId = simulationResourceSetup.SetupId
            };
            simulationResourceSetups.Add(item: _SimulationResourceSetup);

        }

        private void UpdateThroughputTimes(ThroughPutTimeRecord m)
        {
            _ThroughPutTimes.Add(m);
        }
        
        private void UpdateComputationalTimes(ComputationalTimerRecord m)
        {
            _ComputationalTimes.Add(m);
        }

        #region breakdown
        private void BreakDwn(BreakDownRecord item)
        {
            Collector.messageHub.SendToClient(listener: item.Resource + "_State", msg: "offline");
        }

        private void RecoverFromBreak(AgentInformationRecord item)
        {
            Collector.messageHub.SendToClient(listener: item.RequiredFor + "_State", msg: "online");
        }
#endregion
        private void UpdateFeed(bool finalCall)
        {
            // check if Update has been processed this time step.
 
            if (lastIntervalStart != Collector.Time.Value)
            {
                //var OEE = OverallEquipmentEffectiveness(resources: _resources, Collector.Time - 1440L, Collector.Time);
                lastIntervalStart = Collector.Time.Value;
            }
            ThroughPut(finalCall);
            ComputationalTimes(finalCall);
            CallTotal(finalCall);
            CallAverageIdle(finalCall);

            LogToDB(writeResultsToDB: finalCall);
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time.Value + ") Finished Update Feed from WorkSchedule");
        }

        private void ComputationalTimes(bool finalCall)
        {
            var elapsed = _stopWatch.ElapsedMilliseconds;
            _stopWatch.Restart();
            _runTime.Add(elapsed);
            Collector.CreateKpi(Collector, elapsed.ToString(), "TimeCycleExecution", KpiType.ComputationalTime, false);

            if (finalCall) {
                Collector.CreateKpi(Collector, _runTime.Sum().ToString(), "TotalTimeCycleExecution", KpiType.ComputationalTime, true);
                Collector.CreateKpi(Collector, _runTime.Average().ToString(), "AverageTimeCycleExecution", KpiType.ComputationalTime, true);

                if (Collector.Config.GetOption<SimulationKind>().Value.Equals(SimulationType.Central))
                {
                    foreach (var timeKpi in _ComputationalTimes)
                    {
                        //additional for WriteConfirmations, ExecuteGanttplan and Init
                        Collector.CreateKpi(Collector, timeKpi.Duration.ToString() , timeKpi.Timertype, KpiType.ComputationalTime, false, new Time(timeKpi.Time));
                    }

                    foreach (var type in _ComputationalTimes.GroupBy(x => x.Timertype))
                    {
                        Collector.CreateKpi(Collector, type.Sum(x => x.Duration).ToString(), "Total" + type.Key, KpiType.ComputationalTime, true);
                        Collector.CreateKpi(Collector, type.Average(x => x.Duration.TotalMinutes).ToString(), "Average" + type.Key, KpiType.ComputationalTime, true);
                    }

                }

            }

        }

        private void CallAverageIdle(bool finalCall)
        {
            var kpis = idleTime.GetKpis(Collector, finalCall);
            Collector.Kpis.AddRange(kpis);
        }

        private void CallTotal(bool finalCall)
        {
            if (finalCall)
            {
                List<ISimulationTask> allSimulationData = new List<ISimulationTask>(simulationJobs);
                List<ISimulationTask> allSimulationSetupData = new List<ISimulationTask>(simulationResourceSetups);

                var settlingStart = Collector.Config.GetOption<SettlingStart>().Value;
                var resourcesDatas = kpiManager.GetSimulationDataForResources(resources: _resources, simulationResourceData: allSimulationData, simulationResourceSetupData: allSimulationSetupData, startInterval: _settlingStart, endInterval: Collector.Time.Value);

                foreach (var resource in resourcesDatas) { 
                    var tuple = resource._workTime + " " + resource._setupTime;
                    var machine = resource._resource;
                }

                var toSend = new
                {
                    WorkTime = resourcesDatas.Sum(x => x._totalWorkTime),
                    WorkTimePercent = Math.Round((resourcesDatas.Sum(x => Convert.ToDouble(x._workTime.Replace(".", ","))) / resourcesDatas.Count * 100), 4).ToString(_cultureInfo),
                    SetupTime = resourcesDatas.Sum(x => x._totalSetupTime),
                    SetupTimePercent = Math.Round((resourcesDatas.Sum(x => Convert.ToDouble(x._setupTime.Replace(".", ","))) / resourcesDatas.Count * 100), 4).ToString(_cultureInfo)
                };
                
                Collector.messageHub.SendToClient(listener: "totalUtilizationListener", msg: JsonConvert.SerializeObject(value: toSend ));
            }
        }

        private void LogToDB(bool writeResultsToDB)
        {
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = MateResultDb.GetContext(resultCon: Collector.Config.GetOption<ResultsDbConnectionString>().Value))
                {
                    ctx.SimulationJobs.AddRange(entities: simulationJobs);
                    ctx.SaveChanges();
                    ctx.SimulationResourceSetups.AddRange(entities: simulationResourceSetups);
                    ctx.SaveChanges();
                    ctx.Kpis.AddRange(entities: Collector.Kpis);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }

        private void ThroughPut(bool finalCall)
        {

            var leadTime = from lt in _ThroughPutTimes
                           where lt.End >= Collector.Time.Value - Collector.Config.GetOption<TimePeriodForThroughputCalculation>().Value
                           group lt by lt.ArticleName into so
                           select new
                           {
                               ArticleName = so.Key.Split(new char[] { '|' })[0],
                               Dlz = so.Select(selector: x => x.End - x.Start).ToList()
                           };

            var thoughput = JsonConvert.SerializeObject(value: new { leadTime });
            Collector.messageHub.SendToClient(listener: "Throughput", msg: thoughput);

            if (finalCall && _ThroughPutTimes.Count > 0)
            {
                var finalThroughput = _ThroughPutTimes.Average(x => x.End.ToEpochTime() - x.Start.ToEpochTime()).ToString();
                Collector.CreateKpi(Collector, finalThroughput, "AverageLeadTime", KpiType.LeadTime, true);    
            }
            
            //foreach (var item in leadTime)
            //{
            //    var boxPlot = item.Dlz.FiveNumberSummary();
            //    var upperQuartile = Convert.ToInt64(value: boxPlot[4]);
            //    Collector.actorPaths.SimulationContext.Ref.Tell(
            //        message: SupervisorAgent.Supervisor.Instruction.SetEstimatedThroughputTime.Create(
            //            message: new FSetEstimatedThroughputTime(articleId: 0, time: upperQuartile, articleName: item.ArticleName)
            //            , target: Collector.actorPaths.SystemAgent.Ref
            //        )
            //        , sender: ActorRefs.NoSender);
            //    
            //    Debug.WriteLine(message: $"({Collector.Time}) Update Throughput time for article {item.ArticleName} to {upperQuartile}");
            //}


            var products = simulationJobs.Where(predicate: a => a.ArticleType == "Product"
                                                   && a.HierarchyNumber == 20 // TODO : REMOVE THIS GARBAGE
                                                   && a.End == Time.ZERO.Value);

            Collector.messageHub.SendToClient(listener: "ContractsV2", msg: JsonConvert.SerializeObject(value: new { Collector.Time, Processing = products.Count().ToString() }));
        }


        private void CreateJob(CreateSimulationJobRecord simJob)
        {
            //var fOperation = ((FOperation)simJob.Job);
            var simulationJob = new Job
            {
                JobId = simJob.Key,
                JobName = simJob.OperationName,
                JobType = simJob.JobType,
                DueTime = simJob.DueTime,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                CapabilityProvider = simJob.CapabilityProvider,
                CreatedForOrderId = string.Empty,
                Article = simJob.ArticleName,
                OrderId = "[" + simJob.CustomerOrderId + "]",
                HierarchyNumber = simJob.OperationHierarchyNumber,
                //Remember this is now a fArticleKey (Guid)
                ProductionOrderId =  simJob.ProductionAgent,
                Parent = simJob.IsHeadDemand.ToString(),
                ArticleKey = simJob.ArticleKey.ToString(),
                Time = Collector.Time.Value,
                ExpectedDuration = simJob.OperationDuration,
                ArticleType = simJob.ArticleType,
                CapabilityName = simJob.RequiredCapabilityName,
                Bucket = "",
                Start = simJob.Start,
                End =  simJob.End,
            };

            var edit = _updatedSimulationJob.FirstOrDefault(predicate: x => x.Job.Key.ToString().Equals(simJob.Key));
            if (edit != null)
            {
                simulationJob.Start = edit.Start;
                simulationJob.End = edit.Start + edit.Duration;
                simulationJob.CapabilityProvider = edit.CapabilityProvider;
                simulationJob.Bucket = edit.Bucket;
                _updatedSimulationJob.Remove(item: edit);
            }
            simulationJobs.Add(item: simulationJob);
        }
        
        private void UpdateJob(UpdateSimulationJobRecord simJob)
        {
            var edit = simulationJobs.FirstOrDefault(predicate: x => x.JobId.Equals(value: simJob.Job.Key.ToString()));
            if (edit != null)
            {
                edit.JobType = simJob.JobType;
                edit.Start = simJob.Start;
                edit.End = (simJob.Start + simJob.Duration); // to have Time Points instead of Time Periods
                edit.CapabilityProvider = simJob.CapabilityProvider;
                edit.Bucket = simJob.Bucket;
                edit.ReadyAt = simJob.ReadyAt;
                edit.SetupId = simJob.SetupId;
                idleTime.Add(edit);
                return;
            }
            _updatedSimulationJob.Add(item: simJob);



            //tuples.Add(new Tuple<string, long>(uws.Machine, uws.Duration));
        }

        private void UpdateProvider(UpdateSimulationWorkProviderRecord uswp)
        {
            foreach (var fpk in uswp.ArticleProviderRecords)
            {
                var items = simulationJobs.Where(predicate: x => x.ArticleKey.Equals(value: fpk.ProvidesArticleKey.ToString())
                                                            && x.ProductionOrderId == fpk.ProductionAgentKey); 
                foreach (var item in items)
                {
                    item.ParentId = item.Parent.Equals(value: false.ToString()) ? "[" + uswp.RequestAgentId + "]" : "[]";
                    item.Parent = uswp.RequestAgentName;
                    item.CreatedForOrderId = item.OrderId;
                    item.OrderId = "[" + uswp.CustomerOrderId + "]";
                }
            }

        }
    }
}
