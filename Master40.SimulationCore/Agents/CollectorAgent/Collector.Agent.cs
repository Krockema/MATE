using Akka.Actor;
using AkkaSim;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.Tools.SignalR;
using System;
using System.Collections.Generic;
using Master40.DB.Nominal;
using Master40.DB.ReportingModel;
using Newtonsoft.Json;
using NLog;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public partial class Collector : SimulationMonitor
    {
        internal long Time => this._Time;
        private ICollectorBehaviour Behaviour;
        internal IMessageHub messageHub { get; }
        internal Configuration Config;
        internal ActorPaths actorPaths;
        internal SimulationId simulationId;
        internal SimulationNumber simulationNumber;
        internal SimulationKind simulationKind;
        internal SaveToDB saveToDB;
        internal long maxTime;
        internal List<Kpi> Kpis { get; } = new List<Kpi>();
        internal new IUntypedActorContext Context => UntypedActor.Context;
        /// <summary>
        /// Collector Agent for Life data Aquesition
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>
        public Collector(ActorPaths paths
            , ICollectorBehaviour collectorBehaviour
            , IMessageHub msgHub
            , Configuration configuration
            , long time
            , List<Type> streamTypes)
            : base(time: time, channels: streamTypes)
        {
            collectorBehaviour.Collector = this;
            Behaviour = collectorBehaviour;
            messageHub = msgHub;
            Config = configuration;
            actorPaths = paths;
            simulationId = Config.GetOption<SimulationId>();
            simulationNumber = Config.GetOption<SimulationNumber>();
            simulationKind = Config.GetOption<SimulationKind>();
            saveToDB = Config.GetOption<SaveToDB>();
            maxTime = Config.GetOption<SimulationEnd>().Value;
            messageHub.SendToAllClients(msg: "Collector initialized: " + Self.Path.ToStringWithAddress());
        }

        public static Props Props(ActorPaths actorPaths
            , ICollectorBehaviour collectorBehaviour
            , IMessageHub msgHub
            , Configuration configuration
            , long time
            , bool debug
            , List<Type> streamTypes)
        {
            return Akka.Actor.Props.Create(
                factory: () => new Collector(actorPaths, collectorBehaviour, msgHub, configuration, time, streamTypes));
        }

        protected override void EventHandle(object o)
        {
            Behaviour.EventHandle(simulationMonitor: this, message: o);
        }

        internal void CreateKpi(Collector agent, string value, string name, KpiType kpiType, bool isFinal = false)
        {
            var k = new Kpi
            {
                Name = name,
                Value = Math.Round(Convert.ToDouble(value: value), 2),
                Time = (int)agent.Time,
                KpiType = kpiType,
                SimulationConfigurationId = agent.simulationId.Value,
                SimulationNumber = agent.simulationNumber.Value,
                IsFinal = isFinal,
                IsKpi = true,
                SimulationType = agent.simulationKind.Value,
                ValueMin = 0,
                ValueMax = 0
            };
            Kpis.Add(item: k);

        }

        internal void SendKpis(List<Kpi> kpis)
        {

            this.actorPaths.SimulationContext.Ref.Tell(
                message: SupervisorAgent.Supervisor.Instruction.AddKpi.Create(
                    message: kpis
                    , target: this.actorPaths.SystemAgent.Ref
                )
                , sender: ActorRefs.NoSender);
        }
    }
}
