using Mate.DataCore.Nominal;
using static FCentralActivities;
using static FCentralResourceDefinitions;
using static FCreateTaskItems;

namespace Mate.Production.Core.Agents.ResourceAgent.Behaviour
{
    class Central : Core.Types.Behaviour
    {
        private FCentralResourceDefinition _resourceDefinition;
        private FCentralActivity _currentActivity;
        public Central(FCentralResourceDefinition resourceDefinition, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
            _resourceDefinition = resourceDefinition;
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.Central.ActivityStart msg: StartWork(msg.GetObjectFromMessage);
                    break;
                case Resource.Instruction.Central.ActivityFinish msg: FinishWork();
                    break;

                default: return false;

            }
            return true;
        }

        public override bool AfterInit()
        {
            Agent.Send(DirectoryAgent.Directory.Instruction.Central.ForwardRegistrationToHub.Create(
                new FCentralResourceRegistrations.FCentralResourceRegistration(_resourceDefinition.ResourceId
                                                                                        ,_resourceDefinition.ResourceName
                                                                                        , Agent.Context.Self
                                                                                        , _resourceDefinition.ResourceGroupId
                                                                                        , _resourceDefinition.ResourceType)
                , Agent.VirtualParent));
           return true;
        }

        public void StartWork(FCentralActivity activity)
        {
            Agent.DebugMessage($"Start {activity.ProductionOrderId}|{activity.OperationId}|{activity.ActivityId} with Duration: {activity.Duration}");
            _currentActivity = activity;

            CreateTask(activity);

            Agent.Send(Resource.Instruction.Central.ActivityFinish.Create(Agent.Context.Self), activity.Duration);
        }

        
        public void FinishWork()
        {
            Agent.DebugMessage($"Finish {_currentActivity.ProductionOrderId}|{_currentActivity.OperationId}|{_currentActivity.ActivityId} with Duration: {_currentActivity.Duration}");
            Agent.Send(HubAgent.Hub.Instruction.Central.ActivityFinish.Create(_currentActivity, _currentActivity.Hub));
            _currentActivity = null;
        }

        #region Reporting

        void CreateTask(FCentralActivity activity)
        {
            var pub = new FCreateTaskItem(
                type: activity.ActivityType
                , resource: _resourceDefinition.ResourceName
                , resourceId: _resourceDefinition.ResourceId
                , readyAt: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + activity.Duration
                , capability: activity.Capability
                , operation: activity.ActivityType == JobType.SETUP ? "Setup for "+ activity.Name : activity.Name
                , groupId: int.Parse(activity.ProductionOrderId + activity.OperationId + activity.ActivityId + activity.GanttPlanningInterval));

            //TODO NO tracking
            Agent.Context.System.EventStream.Publish(@event: pub);
        }
        
        #endregion

    }
}
