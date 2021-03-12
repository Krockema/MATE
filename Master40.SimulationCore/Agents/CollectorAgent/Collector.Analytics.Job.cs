using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.DB.ReportingModel.Interface;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static FAgentInformations;
using static FBreakDowns;
using static FCreateSimulationJobs;
using static FCreateSimulationResourceSetups;
using static FThroughPutTimes;
using static FUpdateSimulationJobs;
using static FUpdateSimulationWorkProviders;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticJob : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticJob(ResourceDictionary resources) : base()
        {
            _resources = resources;
        }

        private List<Job> simulationJobs { get; } = new List<Job>();
        private List<Setup> simulationResourceSetups { get; } = new List<Setup>();
        private KpiManager kpiManager { get; } = new KpiManager();

        private long lastIntervalStart { get; set; } = 0;

        private List<FUpdateSimulationJob> _updatedSimulationJob { get; } = new List<FUpdateSimulationJob>();
        private List<FThroughPutTime> _ThroughPutTimes { get; } = new List<FThroughPutTime>();
        private ResourceDictionary _resources { get; set; } = new ResourceDictionary();
        public Collector Collector { get; set; }

        /// <summary>
        /// Required to get Number output with . instead of ,
        /// </summary>
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(FCreateSimulationJob), // Published by Production Agent after Creating jobs from Operations
                                     typeof(FUpdateSimulationJob), // 
                                     typeof(FUpdateSimulationWorkProvider),
                                     typeof(UpdateLiveFeed),
                                     typeof(FThroughPutTime),
                                     typeof(Hub.Instruction.Default.AddResourceToHub),
                                     typeof(BasicInstruction.ResourceBrakeDown),
                                     typeof(FCreateSimulationResourceSetup)

            };
        }

        public static CollectorAnalyticJob Get(ResourceDictionary resources)
        {
            return new CollectorAnalyticJob(resources: resources);
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FCreateSimulationJob m: CreateJob(simJob: m); break;
                case FUpdateSimulationJob m: UpdateJob(simJob: m); break;
                case FCreateSimulationResourceSetup m: CreateSetup(m); break;
                case FUpdateSimulationWorkProvider m: UpdateProvider(uswp: m); break;
                case FThroughPutTime m: UpdateThroughputTimes(m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(finalCall: m.GetObjectFromMessage); break;
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
        private void CreateSetup(FCreateSimulationResourceSetup simulationResourceSetup)
        {
            var _SimulationResourceSetup = new Setup
            {
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                Time = (int)(Collector.Time),
                CapabilityProvider = simulationResourceSetup.CapabilityProvider,
                CapabilityName = simulationResourceSetup.CapabilityName,
                Start = (int)simulationResourceSetup.Start,
                End = simulationResourceSetup.Start + simulationResourceSetup.Duration,
                ExpectedDuration = simulationResourceSetup.ExpectedDuration,
                SetupId = simulationResourceSetup.SetupId
            };
            simulationResourceSetups.Add(item: _SimulationResourceSetup);

        }

        private void UpdateThroughputTimes(FThroughPutTime m)
        {
            _ThroughPutTimes.Add(m);
        }

#region breakdown
        private void BreakDwn(FBreakDown item)
        {
            Collector.messageHub.SendToClient(listener: item.Resource + "_State", msg: "offline");
        }

        private void RecoverFromBreak(FAgentInformation item)
        {
            Collector.messageHub.SendToClient(listener: item.RequiredFor + "_State", msg: "online");
        }
#endregion
        private void UpdateFeed(bool finalCall)
        {
            // check if Update has been processed this time step.
 
            if (lastIntervalStart != Collector.Time)
            {
                //var OEE = OverallEquipmentEffectiveness(resources: _resources, Collector.Time - 1440L, Collector.Time);
                lastIntervalStart = Collector.Time;
            }
            ThroughPut(finalCall);
            CallTotal(finalCall);

            LogToDB(writeResultsToDB: finalCall);
            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from WorkSchedule");
        }

        private void CallTotal(bool finalCall)
        {
            if (finalCall)
            {
                List<ISimulationTask> allSimulationData = new List<ISimulationTask>(simulationJobs);
                List<ISimulationTask> allSimulationSetupData = new List<ISimulationTask>(simulationResourceSetups);

                var settlingStart = Collector.Config.GetOption<SettlingStart>().Value;
                var resourcesDatas = kpiManager.GetSimulationDataForResources(resources: _resources, simulationResourceData: allSimulationData, simulationResourceSetupData: allSimulationSetupData, startInterval: settlingStart, endInterval: Collector.Time);

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
                using (var ctx = ResultContext.GetContext(resultCon: Collector.Config.GetOption<DBConnectionString>().Value))
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
                           where Math.Abs(value: lt.End) >= Collector.Time - Collector.Config.GetOption<TimePeriodForThroughputCalculation>().Value
                           group lt by lt.ArticleName into so
                           select new
                           {
                               ArticleName = so.Key,
                               Dlz = so.Select(selector: x => (double)x.End - x.Start).ToList()
                           };

            var thoughput = JsonConvert.SerializeObject(value: new { leadTime });
            Collector.messageHub.SendToClient(listener: "Throughput", msg: thoughput);

            //TODO Add Leadtime for central planning
            /*
            if (finalCall)
            {
                Collector.CreateKpi(Collector, (leadTime.Average(x => x.Dlz.Average()) / leadTime.Count()).ToString() , "AverageLeadTime", KpiType.LeadTime, true);    
            }
            */
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
                                                   && a.HierarchyNumber == 20
                                                   && a.End == 0);

            Collector.messageHub.SendToClient(listener: "ContractsV2", msg: JsonConvert.SerializeObject(value: new { Time = Collector.Time, Processing = products.Count().ToString() }));
        }


        private void CreateJob(FCreateSimulationJob simJob)
        {
            //var fOperation = ((FOperation)simJob.Job);
            var simulationJob = new Job
            {
                JobId = simJob.Key,
                JobName = simJob.OperationName,
                JobType = simJob.JobType,
                DueTime = (int)simJob.DueTime,
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
                FArticleKey = simJob.fArticleKey.ToString(),
                Time = (int)(Collector.Time),
                ExpectedDuration = simJob.OperationDuration,
                ArticleType = simJob.ArticleType,
                CapabilityName = simJob.RequiredCapabilityName,
                Bucket = "",
                Start = simJob.Start,
                End =  simJob.End,
            };

            var edit = _updatedSimulationJob.FirstOrDefault(predicate: x => x.Job.Key.Equals(simJob.Key));
            if (edit != null)
            {
                simulationJob.Start = (int)edit.Start;
                simulationJob.End = (int)(edit.Start + edit.Duration);
                simulationJob.CapabilityProvider = edit.CapabilityProvider;
                simulationJob.Bucket = edit.Bucket;
                _updatedSimulationJob.Remove(item: edit);
            }
            simulationJobs.Add(item: simulationJob);
        }
        
        private void UpdateJob(FUpdateSimulationJob simJob)
        {
            var edit = simulationJobs.FirstOrDefault(predicate: x => x.JobId.Equals(value: simJob.Job.Key.ToString()));
            if (edit != null)
            {
                edit.JobType = simJob.JobType;
                edit.Start = (int)simJob.Start;
                edit.End = (int)(simJob.Start + simJob.Duration); // to have Time Points instead of Time Periods
                edit.CapabilityProvider = simJob.CapabilityProvider;
                edit.Bucket = simJob.Bucket;
                edit.SetupId = simJob.SetupId;

                return;
            }
            _updatedSimulationJob.Add(item: simJob);

            //tuples.Add(new Tuple<string, long>(uws.Machine, uws.Duration));
        }

        private void UpdateProvider(FUpdateSimulationWorkProvider uswp)
        {
            foreach (var fpk in uswp.FArticleProviderKeys)
            {
                var items = simulationJobs.Where(predicate: x => x.FArticleKey.Equals(value: fpk.ProvidesArticleKey.ToString())
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
