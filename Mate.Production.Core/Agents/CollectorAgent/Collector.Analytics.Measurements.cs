using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Actors;
using Mate.DataCore.Data.Context;
using Mate.DataCore.ReportingModel;
using Mate.PiWebApi;
using Mate.PiWebApi.Interfaces;
using Mate.Production.Core.Agents.ResourceAgent;
using Mate.Production.Core.Agents.ResourceAgent.Types;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Types;
using static Mate.Production.Core.Agents.CollectorAgent.Collector.Instruction;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public class CollectorAnalyticsMeasurements : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticsMeasurements() : base() { }

        public Collector Collector { get; set; }

        private List<IPiWebMeasurement> simulationMeasurement { get; } = new List<IPiWebMeasurement>();

        public static CollectorAnalyticsMeasurements Get()
        {
            return new CollectorAnalyticsMeasurements();
        }

        internal static List<Type> GetStreamTypes()
        {
            return new List<Type>
            {
                typeof(Measurements),
                typeof(Resource.Instruction.Default.CreateMeasurements),
                typeof(UpdateLiveFeed),
            };
        }

        public override bool Action(object message) => throw new Exception(message: "Please use EventHandle method to process Messages");

        public bool EventHandle(MessageMonitor simulationMonitor, object message)
        {
            switch (message)
            {
                case Measurements m: CreateEntry(measurements: m); break;
                case UpdateLiveFeed m: UpdateFeed(writeResultsToDB: m.GetObjectFromMessage); break; 
                //case Resource.Instruction.BucketScope.CreateMeasurements msg: Test(fJobInformation: msg.GetObjectFromMessage); break;
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
                measure.SimulationType = (int)Collector.simulationKind.Value;
                simulationMeasurement.Add(measure);
                
                ZeissConnector.TransferMeasurementsToPiWeb(measure);
            }
        }

        private void UpdateFeed(bool writeResultsToDB)
        {
            //var open = openOrderParts.GroupBy(x => x.Article).Select(y => new { Article =  y.Key, Count = y.Sum(z => z.Quantity)} );
            //Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Update Feed from Contracts");
            if (Collector.saveToDB.Value && writeResultsToDB)
            {
                using (var ctx = MateResultDb.GetContext(resultCon: Collector.Config.GetOption<ResultsDbConnectionString>().Value))
                {
                    ctx.SimulationMeasurements.AddRange(entities: simulationMeasurement.Cast<SimulationMeasurement>());
                    ctx.SaveChanges();
                    ctx.Dispose();
                }
            }


            Collector.Context.Sender.Tell(message: true, sender: Collector.Context.Self);
            Collector.messageHub.SendToAllClients(msg: "(" + Collector.Time + ") Finished Update Feed from Measurements");
        }
    }
}
