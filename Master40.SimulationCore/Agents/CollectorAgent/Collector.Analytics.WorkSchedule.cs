
using Akka.Actor;
using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using MathNet.Numerics.Statistics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using static FBreakDowns;
using static FCreateSimulationWorks;
using static FAgentInformations;
using static FSetEstimatedThroughputTimes;
using static FUpdateSimulationWorkProviders;
using static FUpdateSimulationWorks;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;
using static FThroughPutTimes;
using static FCreateSimulationResourceSetups;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsWorkSchedule : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsWorkSchedule(ResourceList resources) : base() {
            _resources = resources;
        }

        private List<SimulationWorkschedule> simulationWorkschedules { get; } = new List<SimulationWorkschedule>();
        //private List<Tuple<string, long>> tuples = new List<Tuple<string, long>>();
        private long lastIntervalStart { get; set; } = 0;
        private List<FUpdateSimulationWork> _updatedSimulationWork { get;  } = new List<FUpdateSimulationWork>();
        private List<FThroughPutTime> _ThroughPutTimes { get; } = new List<FThroughPutTime>();
        private ResourceList _resources { get; set; } = new ResourceList();
        public Collector Collector { get; set; }
        /// <summary>
        /// Required to get Number output with . instead of ,
        /// </summary>
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");
        private List<Kpi> Kpis { get; } = new List<Kpi>();

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type> { typeof(FCreateSimulationWork),
                                     typeof(FUpdateSimulationWork),
                                     typeof(FUpdateSimulationWorkProvider),
                                     typeof(UpdateLiveFeed),
                                     typeof(FThroughPutTime),
                                     typeof(Hub.Instruction.AddResourceToHub),
                                     typeof(BasicInstruction.ResourceBrakeDown),
                                     typeof(FCreateSimulationResourceSetup)

            };
        }

        public static CollectorAnalyticsWorkSchedule Get(ResourceList resources)
        {
            return new CollectorAnalyticsWorkSchedule(resources: resources);
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case FCreateSimulationWork m: CreateSimulationWorkSchedule(cws: m); break;
                case FUpdateSimulationWork m: UpdateSimulationWorkSchedule(uws: m); break;
                case FCreateSimulationResourceSetup m: CreateSimulationResourceSetup(m); break;
                case FUpdateSimulationWorkProvider m: UpdateSimulationWorkItemProvider(uswp: m); break;
                case FThroughPutTime m: UpdateThroughputTimes(m); break;
                case Collector.Instruction.UpdateLiveFeed m: UpdateFeed(writeResultsToDB: m.GetObjectFromMessage); break;
                //case Hub.Instruction.AddResourceToHub m: RecoverFromBreak(item: m.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown m: BreakDwn(item: m.GetObjectFromMessage); break;
                default: return false;
            }
            // Collector.messageHub.SendToAllClients(msg: $"Just finished {message.GetType().Name}");
            return true;
        }

        private void CreateSimulationResourceSetup(FCreateSimulationResourceSetup m)
        {
            Debug.WriteLine(message: $"({Collector.Time}) CreateSimulationResourceSetup not implemented yet");
        }

        private void UpdateThroughputTimes(FThroughPutTime m)
        {
            _ThroughPutTimes.Add(m);
        }

        private void BreakDwn(FBreakDown item)
        {
            Collector.messageHub.SendToClient(listener: item.Resource + "_State", msg: "offline");
        }

        private void RecoverFromBreak(FAgentInformation item)
        {
            Collector.messageHub.SendToClient(listener: item.RequiredFor + "_State", msg: "online");
        }

        private void UpdateFeed(bool writeResultsToDB)
        {
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from WorkSchedule");
            // var mbz = agent.Context.AsInstanceOf<Akka.Actor.ActorCell>().Mailbox.MessageQueue.Count;
            // Debug.WriteLine("Time " + agent.Time + ": " + agent.Context.Self.Path.Name + " Mailbox left " + mbz);
            MachineUtilization();
            ThroughPut();
            lastIntervalStart = Collector.Time;


            LogToDB(writeResultsToDB: writeResultsToDB);

            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from WorkSchedule");
        }

        private void LogToDB(bool writeResultsToDB)
        {
            if (Collector.saveToDB.Value &&  writeResultsToDB)
            {
                using (var ctx = ResultContext.GetContext(resultCon: Collector.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.SimulationOperations.AddRange(entities: simulationWorkschedules);
                    ctx.Kpis.AddRange(entities: Kpis);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }
        }

        private void ThroughPut()
        {

            var leadTime = from lt in _ThroughPutTimes
                           where Math.Abs(value: lt.End) >= Collector.Time - Collector.Config.GetOption<TimePeriodForThrougputCalculation>().Value
                           group lt by lt.ArticleName into so
                           select new
                           {
                               ArticleName = so.Key,
                               Dlz = so.Select(selector: x => (double)x.End - x.Start).ToList()
                           };


            foreach (var item in leadTime)
            {
                var thoughput = JsonConvert.SerializeObject(value: new { leadTime });
                Collector.messageHub.SendToClient(listener: "Throughput",  msg: thoughput);

                var boxPlot = item.Dlz.FiveNumberSummary();
                var upperQuartile = Convert.ToInt64(value: boxPlot[4]);
                Collector.actorPaths.SimulationContext.Ref.Tell(
                    message: SupervisorAgent.Supervisor.Instruction.SetEstimatedThroughputTime.Create(
                        message: new FSetEstimatedThroughputTime(articleId: 0, time: upperQuartile, articleName: item.ArticleName)
                        , target: Collector.actorPaths.SystemAgent.Ref
                    )
                    , sender: ActorRefs.NoSender);

                Debug.WriteLine(message: $"({Collector.Time}) Update Throughput time for article {item.ArticleName} to {upperQuartile}"); 
            }

            var v2 = simulationWorkschedules.Where(predicate: a => a.ArticleType == "Product"
                                                   && a.HierarchyNumber == 20
                                                   && a.End == 0);


            Collector.messageHub.SendToClient(listener: "ContractsV2", msg: JsonConvert.SerializeObject(value: new { Time = Collector.Time, Processing = v2.Count().ToString() }));
        }

        private void MachineUtilization()
        {
            double divisor = Collector.Time - lastIntervalStart;
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from DataCollection");
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Time since last Update: " + divisor + "min");

            //simulationWorkschedules.WriteCSV( @"C:\Users\mtko\source\output.csv");


            var lower_borders = from sw in simulationWorkschedules
                                where sw.Start < lastIntervalStart
                                   && sw.End > lastIntervalStart
                                   && sw.Machine != null
                                select new
                                {
                                    M = sw.Machine,
                                    C = 1,
                                    W = sw.End - lastIntervalStart
                                };

            var upper_borders = from sw in simulationWorkschedules
                                where sw.Start < Collector.Time
                                   && sw.End > Collector.Time
                                   && sw.Machine != null
                                select new
                                {
                                    M = sw.Machine,
                                    C = 1,
                                    W = Collector.Time - sw.Start
                                };


            var from_work = from sw in simulationWorkschedules
                            where sw.Start >= lastIntervalStart 
                               && sw.End <= Collector.Time
                               && sw.Machine != null
                            group sw by sw.Machine into mg
                            select new
                            {
                                M = mg.Key,
                                C = mg.Count(),
                                W = (long)mg.Sum(selector: x => x.End - x.Start)
                            };
            var machineList = _resources.Select(selector: x => new { M = x, C = 0, W = (long)0 });
            var merge = from_work.Union(second: lower_borders).Union(second: upper_borders).Union(second: machineList).ToList();

            var final = from m in merge
                        group m by m.M into mg
                        select new
                        {
                            M = mg.Key,
                            C = mg.Sum(selector: x => x.C),
                            W = mg.Sum(selector: x => x.W)
                        };

            foreach (var item in final.OrderBy(keySelector: x => x.M))
            {
                var value = Math.Round(value: item.W / divisor, digits: 3).ToString(provider: _cultureInfo);
                if (value == "NaN") value = "0";
                //Debug.WriteLine(item.M + " worked " + item.W + " min of " + divisor + " min with " + item.C + " items!", "work");
                var machine = item.M.Replace(oldValue: ")", newValue: "").Replace(oldValue: "Machine(", newValue: "");
                Collector.messageHub.SendToClient(listener: machine, msg: value);
                CreateKpi(agent: Collector, value: value, name: item.M, kpiType: KpiType.MachineUtilization);
            }

            var totalLoad = Math.Round(value: final.Sum(selector: x => x.W) / divisor / final.Count() * 100, digits: 3).ToString(provider: _cultureInfo);
            if (totalLoad == "NaN")  totalLoad = "0";
            Collector.messageHub.SendToClient(listener: "TotalWork", msg: JsonConvert.SerializeObject(value: new { Time = Collector.Time, Load = totalLoad }));
            CreateKpi(agent: Collector, value: totalLoad, name: "TotalWork", kpiType: KpiType.MachineUtilization);
            // // Kontrolle
            // var from_work2 = from sw in tuples
            //                  group sw by sw.Item1 into mg
            //                  select new
            //                  {
            //                      M = mg.Key,
            //                      W = (double)(mg.Sum(x => x.Item2))
            //                  };
            // 
            // foreach (var item in from_work2.OrderBy(x => x.M))
            // {
            //     Debug.WriteLine(item.M + " workload " + Math.Round(item.W / divisor, 3) + " %!", "intern");
            // }
            // tuples.Clear();

            // Cut all lower bound ?
            // TODO save removed items to somewhere
            var removed = simulationWorkschedules.RemoveAll(sw => sw.CreatedForOrderId != string.Empty
                                                               && sw.End < lastIntervalStart);
            Collector.messageHub.SendToAllClients(msg: $"({Collector.Time}) Removed {removed}");
        }

        private void CreateKpi(Collector agent, string value, string name, KpiType kpiType)
        {
            var k = new Kpi
            {
                Name = name,
                Value = Convert.ToDouble(value: value),
                Time = (int)agent.Time,
                KpiType = kpiType,
                SimulationConfigurationId = agent.simulationId.Value,
                SimulationNumber = agent.simulationNumber.Value,
                IsFinal = false,
                IsKpi = true,
                SimulationType = agent.simulationKind.Value
            };
            Kpis.Add(item: k);
        }

        private void CreateSimulationWorkSchedule(FCreateSimulationWork cws)
        {
            var ws = cws.Operation;
            var sws = new SimulationWorkschedule
            {
                CreatedForOrderId = string.Empty,
                WorkScheduleId = ws.Key.ToString(),
                Article = ws.Operation.Article.Name,
                WorkScheduleName = ws.Operation.Name,
                DueTime = (int)ws.DueTime,
                SimulationConfigurationId = Collector.simulationId.Value,
                SimulationNumber = Collector.simulationNumber.Value,
                SimulationType = Collector.simulationKind.Value,
                OrderId = "[" + cws.CustomerOrderId + "]",
                HierarchyNumber = ws.Operation.HierarchyNumber,
                // TODO this is now a fArticleKey (Guid)
                ProductionOrderId = "[" + cws.fArticleKey+ "]",
                Parent = cws.IsHeadDemand.ToString(),
                ParentId = "[]",
                Time = (int)(Collector.Time),
                ArticleType = cws.ArticleType
            };

            var edit = _updatedSimulationWork.FirstOrDefault(predicate: x => x.WorkScheduleId.Equals(value: ws.Key.ToString()));
            if (edit != null)
            {
                sws.Start = (int)edit.Start;
                sws.End = (int)(edit.Start + edit.Duration);
                sws.Machine = edit.Machine;
                _updatedSimulationWork.Remove(item: edit);
            }
            simulationWorkschedules.Add(item: sws);
        }


        private void UpdateSimulationWorkSchedule(FUpdateSimulationWork uws)
        {

            var edit = simulationWorkschedules.FirstOrDefault(predicate: x => x.WorkScheduleId.Equals(value: uws.WorkScheduleId));
            if (edit != null)
            {
                edit.Start = (int)uws.Start;
                edit.End = (int)(uws.Start + uws.Duration); // to have Time Points instead of Time Periods
                edit.Machine = uws.Machine;
                return;
            }
            _updatedSimulationWork.Add(item: uws);

            //tuples.Add(new Tuple<string, long>(uws.Machine, uws.Duration));
        }

        private void UpdateSimulationWorkItemProvider(FUpdateSimulationWorkProvider uswp)
        {
            foreach (var fpk in uswp.FArticleProviderKeys)
            {
                var items = simulationWorkschedules.Where(predicate: x => x.ProductionOrderId.Equals(value: "[" + fpk + "]")).ToList();
                foreach (var item in items)
                {
                    item.ParentId = item.Parent.Equals(value: false.ToString()) ? "[" + uswp.RequestAgentId + "]" : "[]";
                    item.Parent = uswp.RequestAgentName;
                    item.CreatedForOrderId = item.OrderId;
                    item.OrderId = "[" + uswp.CustomerOrderId + "]";

                    // item.OrderId = orderId;
                }
            }

        }




    }
}
