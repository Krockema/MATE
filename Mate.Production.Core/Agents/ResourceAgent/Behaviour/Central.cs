using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment.Records.Central;
using Mate.Production.Core.Environment.Records.Reporting;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ResourceAgent.Behaviour
{
    class Central : Core.Types.Behaviour
    {
        private CentralResourceDefinitionRecord _resourceDefinition;
        private CentralActivityRecord _currentActivity;
        public Central(CentralResourceDefinitionRecord resourceDefinition, SimulationType simulationType = SimulationType.None)
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
                new CentralResourceRegistrationRecord(_resourceDefinition.ResourceId
                                                    ,_resourceDefinition.ResourceName
                                                    , Agent.Context.Self
                                                    , _resourceDefinition.ResourceGroupId
                                                    , _resourceDefinition.ResourceType)
                , Agent.VirtualParent));
           return true;
        }

        public void StartWork(CentralActivityRecord activity)
        {
            Agent.DebugMessage($"Start {activity.ProductionOrderId}|{activity.OperationId}|{activity.ActivityId} with Duration: {activity.Duration}");
            _currentActivity = activity;

            CreateTask(activity);

            Agent.Send(Resource.Instruction.Central.ActivityFinish.Create(Agent.Context.Self), activity.Duration.ToTimeSpan());
        }

        
        public void FinishWork()
        {
            Agent.DebugMessage($"Finish {_currentActivity.ProductionOrderId}|{_currentActivity.OperationId}|{_currentActivity.ActivityId} with Duration: {_currentActivity.Duration}");
            Agent.Send(HubAgent.Hub.Instruction.Central.ActivityFinish.Create(_currentActivity, _currentActivity.Hub));
            _currentActivity = null;
        }

        #region Reporting

        void CreateTask(CentralActivityRecord activity)
        {
            var pub = new CreateTaskItemRecord(
                Type: activity.ActivityType
                , Resource: _resourceDefinition.ResourceName
                , ResourceId: _resourceDefinition.ResourceId
                , Start: Agent.CurrentTime
                , End: Agent.CurrentTime + activity.Duration
                , Capability: activity.Capability
                , Operation: activity.ActivityType == JobType.SETUP ? "Setup for "+ activity.Name : activity.Name
                , GroupId: int.Parse(activity.ProductionOrderId + activity.OperationId + activity.ActivityId + activity.GanttPlanningInterval));

            //TODO NO tracking
            Agent.Context.System.EventStream.Publish(@event: pub);
        }
        
        #endregion

    }
}
