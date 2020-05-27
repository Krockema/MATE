using AkkaSim;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using static FCreateTaskItems;
using static Master40.SimulationCore.Agents.CollectorAgent.Collector.Instruction;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class CollectorAnalyticResource : Behaviour, ICollectorBehaviour
    {
        private CollectorAnalyticResource(ResourceList resources) : base()
        {
            _resources = resources;
        }

        private List<TaskItem> _taskItems { get; } = new List<TaskItem>();
        private ResourceList _resources { get; set; } = new ResourceList();
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

        private void UpdateFeed(bool finalCall)
        {
            // Do something or just relax
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

            _taskItems.Add(taskItem);

        }


    }
}
