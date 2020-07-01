using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
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
using static FOperations;
using static FThroughPutTimes;
using static FUpdateSimulationJobs;
using static FUpdateSimulationWorkProviders;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticJob : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticJob(ResourceList resources) : base()
        {
            _resources = resources;
        }

        private List<Job> simulationJobs { get; } = new List<Job>();
        private List<Job> simulationJobsForDb { get; } = new List<Job>();
        private List<Setup> simulationResourceSetups { get; } = new List<Setup>();
        private List<Setup> simulationResourceSetupsForDb { get; } = new List<Setup>();
        
        private KpiManager kpiManager { get; } = new KpiManager();

        private long lastIntervalStart { get; set; } = 0;

        private List<FUpdateSimulationJob> _updatedSimulationJob { get; } = new List<FUpdateSimulationJob>();
        private List<FThroughPutTime> _ThroughPutTimes { get; } = new List<FThroughPutTime>();
        private ResourceList _resources { get; set; } = new ResourceList();
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

        public static CollectorAnalyticJob Get(ResourceList resources)
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
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from WorkSchedule");
            // var mbz = agent.Context.AsInstanceOf<Akka.Actor.ActorCell>().Mailbox.MessageQueue.Count;
            // Debug.WriteLine("Time " + agent.Time + ": " + agent.Context.Self.Path.Name + " Mailbox left " + mbz);
            // check if Update has been processed this time step.
            

            if (lastIntervalStart != Collector.Time)
            {
                var OEE = OverallEquipmentEffectiveness(resources: _resources, Collector.Time - 1440L, Collector.Time);
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
                List<ISimulationResourceData> allSimulationData = new List<ISimulationResourceData>();
                allSimulationData.AddRange(simulationJobs);
                allSimulationData.AddRange(simulationJobsForDb);

                List<ISimulationResourceData> allSimulationSetupData = new List<ISimulationResourceData>();
                allSimulationSetupData.AddRange(simulationResourceSetups);
                allSimulationSetupData.AddRange(simulationResourceSetupsForDb);

                var settlingStart = Collector.Config.GetOption<SettlingStart>().Value;
                var resourcesDatas = kpiManager.GetSimulationDataForResources(resources: _resources, simulationResourceData: allSimulationData, simulationResourceSetupData: allSimulationSetupData, startInterval: settlingStart, endInterval: Collector.Time);

                foreach (var resource in resourcesDatas) { 
                    var tuple = resource._workTime + " " + resource._setupTime;
                    var machine = resource._resource;
                    //Collector.messageHub.SendToClient(listener: machine, msg: tuple);
                    //Collector.CreateKpi(Collector, resource._workTime.Replace(".",","), machine, KpiType.ResourceUtilizationTotal, true);
                    //Collector.CreateKpi(Collector, resource._setupTime.Replace(".", ","), machine, KpiType.ResourceSetupTotal, true);
                }

                var toSend = new
                {
                    WorkTime = resourcesDatas.Sum(x => x._totalWorkTime),
                    WorkTimePercent = Math.Round((resourcesDatas.Sum(x => Convert.ToDouble(x._workTime.Replace(".", ","))) / resourcesDatas.Count * 100), 4).ToString(_cultureInfo),
                    SetupTime = resourcesDatas.Sum(x => x._totalSetupTime),
                    SetupTimePercent = Math.Round((resourcesDatas.Sum(x => Convert.ToDouble(x._setupTime.Replace(".", ","))) / resourcesDatas.Count * 100), 4).ToString(_cultureInfo)
                };

                // Einschwingzeit - Ende der Simulation
                var OEE = OverallEquipmentEffectiveness(resources: _resources, 0 + Collector.Config.GetOption<TimePeriodForThroughputCalculation>().Value, Collector.Time);
                //Collector.CreateKpi(Collector, OEE, "OEE", KpiType.Ooe, true);

                Collector.messageHub.SendToClient(listener: "totalUtilizationListener", msg: JsonConvert.SerializeObject(value: toSend ));
            }
        }

        private void LogToDB(bool writeResultsToDB)
        {
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = ResultContext.GetContext(resultCon: Collector.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.SimulationJobs.AddRange(entities: simulationJobsForDb);
                    ctx.SaveChanges();
                    ctx.SimulationResourceSetups.AddRange(entities: simulationResourceSetupsForDb);
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

            if (finalCall)
            {
                //Collector.CreateKpi(Collector, (leadTime.Average(x => x.Dlz.Average()) / leadTime.Count()).ToString() , "AverageLeadTime", KpiType.LeadTime, true);    
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
                                                   && a.HierarchyNumber == 20
                                                   && a.End == 0);

            Collector.messageHub.SendToClient(listener: "ContractsV2", msg: JsonConvert.SerializeObject(value: new { Time = Collector.Time, Processing = products.Count().ToString() }));
        }

        /// <summary>
        /// OEE for dashboard
        /// </summary>
        private string OverallEquipmentEffectiveness(ResourceList resources, long startInterval, long endInterval)
        {
            /* ------------- Total Production Time --------------------*/
            var totalInterval = endInterval - startInterval;
            var totalProductionTime = totalInterval * resources.Count;

            /* ------------- RunTime --------------------*/
            var totalPlannedDowntime = 0L;
            var totalBreakTime = 0L;
            double runTime = totalProductionTime - (totalPlannedDowntime - totalBreakTime);

            /* ------------- WorkTime --------------------*/ //TODO add unplanned breakdown
            var breakDown = 0L;
            
            List<ISimulationResourceData> allSimulationResourceSetups = new List<ISimulationResourceData>();
            allSimulationResourceSetups.AddRange(simulationResourceSetups.Where(x => x.Start >= startInterval + 50).ToList());
            allSimulationResourceSetups.AddRange(simulationResourceSetupsForDb.Where(x => x.Start >= startInterval + 50).ToList());

            var setupTime = kpiManager.GetTotalTimeForInterval(resources, allSimulationResourceSetups, startInterval, endInterval);

            var totalUnplannedDowntime = breakDown + setupTime;

            double workTime = runTime - totalUnplannedDowntime;

            /* ------------- PerformanceTime --------------------*/
            List<ISimulationResourceData> allSimulationResourceJobs = new List<ISimulationResourceData>();
            allSimulationResourceJobs.AddRange(simulationJobs.Where(x => x.Start >= startInterval + 50).ToList());
            allSimulationResourceJobs.AddRange(simulationJobsForDb.Where(x => x.Start >= startInterval + 50).ToList());
            var jobTime = kpiManager.GetTotalTimeForInterval(resources, allSimulationResourceJobs, startInterval, endInterval);

            var idleTime = workTime - jobTime;

            // var reducedSpeed = 0L; //TODO if this is implemented the GetTotalTimeForInterval must change. to reflect speed div.

            double performanceTime = jobTime;

            /* ------------- zeroToleranceTime --------------------*/

            //TODO Feature: Branch QualityManagement
            var goodGoods = 35L;
            var badGoods = 0L;
            var totalGoods = goodGoods + badGoods;

            double zeroToleranceTime = performanceTime / totalGoods * goodGoods;

            //1.Parameter Availability calculation
            double availability =  workTime /runTime;

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

        private void CreateJob(FCreateSimulationJob simJob)
        {
            var fOperation = ((FOperation)simJob.Job);
            var simulationJob = new Job
            {
                JobId = simJob.Job.Key.ToString(),
                JobName = fOperation.Operation.Name,
                JobType = simJob.JobType,
                DueTime = (int)simJob.Job.DueTime,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                CreatedForOrderId = string.Empty,
                Article = fOperation.Operation.Article.Name,
                OrderId = "[" + simJob.CustomerOrderId + "]",
                HierarchyNumber = fOperation.Operation.HierarchyNumber,
                //Remember this is now a fArticleKey (Guid)
                ProductionOrderId =  simJob.ProductionAgent,
                Parent = simJob.IsHeadDemand.ToString(),
                FArticleKey = simJob.fArticleKey.ToString(),
                Time = (int)(Collector.Time),
                ExpectedDuration = fOperation.Operation.Duration,
                ArticleType = simJob.ArticleType,
                CapabilityName = fOperation.RequiredCapability.Name
            };

            var edit = _updatedSimulationJob.FirstOrDefault(predicate: x => x.Job.Key.Equals(fOperation.Key));
            if (edit != null)
            {
                simulationJob.Start = (int)edit.Start;
                simulationJob.End = (int)(edit.Start + edit.Duration);
                simulationJob.CapabilityProvider = edit.CapabilityProvider;
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
