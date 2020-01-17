using AkkaSim;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public class CollectorAnalyticsMeasurements : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsMeasurements() : base() { }

        public Collector Collector { get; set; }

        private List<SimulationMeasurement> simulationMeasurement { get; } = new List<SimulationMeasurement>();

        public static CollectorAnalyticsMeasurements Get()
        {
            return new CollectorAnalyticsMeasurements();
        }

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type>
            {
                typeof(Measurements),
                typeof(UpdateLiveFeed),
            };
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(SimulationMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case Measurements m: CreateEntry(measurements: m); break;
                case UpdateLiveFeed m: UpdateFeed(writeResultsToDB: m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void CreateEntry(Measurements measurements)
        {
            foreach (var measure in measurements)
            {
                measure.SimulationConfigurationId = Collector.simulationId.Value;
                measure.SimulationNumber = Collector.simulationNumber.Value;
                measure.SimulationType = Collector.simulationKind.Value;
                simulationMeasurement.Add(measure);
            }
        }

        private void UpdateFeed(bool writeResultsToDB)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from Contracts");
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = ResultContext.GetContext(resultCon: Collector.Config.GetOption<DBConnectionString>().Value))
                {
                    ctx.SaveChanges();
                    ctx.SimulationMeasurements.AddRange(entities: simulationMeasurement);
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }


            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from Measurements");
        }
    }
}
