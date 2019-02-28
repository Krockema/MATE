using AkkaSim;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Master40.SimulationCore.Agents.Collector.Instruction;
using Newtonsoft.Json;
using System.Reflection;
using Master40.DB.Data.Helper;
using Akka.Util.Internal;

namespace Master40.SimulationCore.Agents
{
    public class CollectorAnalyticsWorkSchedule : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsWorkSchedule() : base() { }

        private List<SimulationWorkschedule> simulationWorkschedules = new List<SimulationWorkschedule>();
        private List<Tuple<string, long>> tuples = new List<Tuple<string, long>>();
        private long lastIntervalStart = 0;

        public static CollectorAnalyticsWorkSchedule Get()
        {
            return new CollectorAnalyticsWorkSchedule();
        }

        public override bool Action(Agent agent, object message) => throw new Exception("Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case CreateSimulationWork m: CreateSimulationWorkSchedule((Collector)simulationMonitor,m); break;
                case UpdateSimulationWork m: UpdateSimulationWorkSchedule(m); break;
                case UpdateSimulationWorkProvider m: UpdateSimulationWorkItemProvider(m); break;
                case UpdateLiveFeed m: UpdateFeed((Collector)simulationMonitor, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void UpdateFeed(Collector agent, bool logToDB)
        {
            // var mbz = agent.Context.AsInstanceOf<Akka.Actor.ActorCell>().Mailbox.MessageQueue.Count;
            // Debug.WriteLine("Time " + agent.Time + ": " + agent.Context.Self.Path.Name + " Mailbox left " + mbz);
            MachineUtilisation(agent);
            ThroughPut(agent);
            lastIntervalStart = agent.Time;
            if (logToDB)
            {
                
                agent.DBContext.SimulationWorkschedules.AddRange(simulationWorkschedules);
                agent.DBContext.SaveChanges();
            }
            agent.Context.Sender.Tell(true, agent.Context.Self);

        }

        private void ThroughPut(Collector agent)
        {
            
            var art = from a in simulationWorkschedules
                      where a.ArticleType == "Product"
                          && a.CreatedForOrderId != null
                          && a.Time >= lastIntervalStart
                      group a by new { a.Article, a.OrderId } into arti
                      select new
                      {
                          Article = arti.Key.Article,
                          Order = arti.Key.OrderId
                      };

            var leadTime = from lt in simulationWorkschedules
                           group lt by lt.OrderId into so
                           select new
                           {
                               OrderID = so.Key,
                               Dlz = so.Max(x => x.End) - so.Min(x => x.Start)
                           };

            var innerJoinQuery =
                   from a in art
                   join l in leadTime on a.Order equals l.OrderID
                   select new
                   {
                       a.Article,
                       a.Order,
                       l.Dlz
                   };

            var group = from dlz in innerJoinQuery
                        group dlz by dlz.Article into agregat
                        select new
                        {
                            agregat.Key,
                            List = agregat.Select(x => x.Dlz).ToList()
                        };

            foreach (var item in group)
            {
                var thoughput = JsonConvert.SerializeObject(new { group });
                agent.messageHub.SendToClient("Throughput",  thoughput);
            }

            var v2 = simulationWorkschedules.Where(a => a.ArticleType == "Product"
                                                   && a.HierarchyNumber == 20
                                                   && a.End == 0);
            agent.messageHub.SendToClient("ContractsV2", JsonConvert.SerializeObject(new { Time = agent.Time, Processing = v2.Count().ToString() }));



        }

        private void MachineUtilisation(Collector agent)
        {
            double divisor = agent.Time - lastIntervalStart;
            agent.messageHub.SendToAllClients("(" + agent.Time + ") Update Feed from DataCollection");
            agent.messageHub.SendToAllClients("(" + agent.Time + ") Time since last Update: " + divisor + "min");

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
                                where sw.Start < agent.Time
                                   && sw.End > agent.Time
                                   && sw.Machine != null
                                select new
                                {
                                    M = sw.Machine,
                                    C = 1,
                                    W = agent.Time - sw.Start
                                };


            var from_work = from sw in simulationWorkschedules
                            where sw.Start >= lastIntervalStart 
                               && sw.End <= agent.Time
                               && sw.Machine != null
                            group sw by sw.Machine into mg
                            select new
                            {
                                M = mg.Key,
                                C = mg.Count(),
                                W = (long)mg.Sum(x => x.End - x.Start)
                            };
            var merge = from_work.Union(lower_borders).Union(upper_borders).ToList();

            var final = from m in merge
                        group m by m.M into mg
                        select new
                        {
                            M = mg.Key,
                            C = mg.Sum(x => x.C),
                            W = mg.Sum(x => x.W)
                        };


            foreach (var item in final.OrderBy(x => x.M))
            {
                var nan = Math.Round(item.W / divisor, 3).ToString().Replace(",", ".");
                if (nan == "NaN") nan = "0";
                //Debug.WriteLine(item.M + " worked " + item.W + " min of " + divisor + " min with " + item.C + " items!", "work");
                agent.messageHub.SendToClient(item.M.Replace(")", "").Replace("Machine(", ""), nan);
            }

            var totalLoad = Math.Round(final.Sum(x => x.W) / divisor / final.Count() * 100, 3).ToString().Replace(",", ".");
            if (totalLoad == "NaN")  totalLoad = "0";
            agent.messageHub.SendToClient("TotalWork", JsonConvert.SerializeObject(new { Time = agent.Time, Load = totalLoad }));
            
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
        }

        private void CreateSimulationWorkSchedule(Collector agent, CreateSimulationWork cws)
        {
            var ws = cws.WorkItem;
            var sws = new SimulationWorkschedule
            {
                WorkScheduleId = ws.Key.ToString(),
                Article = ws.WorkSchedule.Article.Name,
                WorkScheduleName = ws.WorkSchedule.Name,
                DueTime = (int)ws.DueTime,
                EstimatedEnd = (int)ws.EstimatedEnd,
                SimulationConfigurationId = -1,
                OrderId = "[" + cws.OrderId + "]",
                HierarchyNumber = ws.WorkSchedule.HierarchyNumber,
                ProductionOrderId = "[" + ws.ProductionAgent.Path.Uid.ToString() + "]",
                Parent = cws.IsHeadDemand.ToString(),
                ParentId = "[]",
                Time = (int)(agent.Time),
                ArticleType = cws.ArticleType
            };
            simulationWorkschedules.Add(sws);
        }


        private void UpdateSimulationWorkSchedule(UpdateSimulationWork uws)
        {

            var edit = simulationWorkschedules.FirstOrDefault(x => x.WorkScheduleId.Equals(uws.WorkScheduleId));
            edit.Start = (int)uws.Start;
            edit.End = (int)(uws.Start + uws.Duration + 1); // to have Time Points instead of Time Periods
            edit.Machine = uws.Machine;

            tuples.Add(new Tuple<string, long>(uws.Machine, uws.Duration));
        }

        private void UpdateSimulationId(Collector agent, int simulationId, SimulationType simluationType, int simNumber)
        {

            var simItems = simulationWorkschedules.Where(x => x.SimulationConfigurationId == -1).ToList();
            foreach (var item in simItems)
            {
                item.SimulationConfigurationId = simulationId;
                item.SimulationType = simluationType;
                item.SimulationNumber = simNumber;
            }
        }

        private void UpdateSimulationWorkItemProvider(UpdateSimulationWorkProvider uswp)
        {
            foreach (var agentId in uswp.ProductionAgents)
            {
                var items = simulationWorkschedules.Where(x => x.ProductionOrderId.Equals("[" + agentId.Path.Uid.ToString() + "]")).ToList();
                foreach (var item in items)
                {
                    item.ParentId = item.Parent.Equals(false.ToString()) ? "[" + uswp.RequestAgentId + "]" : "[]";
                    item.Parent = uswp.RequestAgentName;
                    item.CreatedForOrderId = item.OrderId;
                    item.OrderId = "[" + uswp.OrderId + "]";

                    // item.OrderId = orderId;
                }
            }

        }




    }
}
