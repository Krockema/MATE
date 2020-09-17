using Master40.DB.Nominal;
using Master40.SimulationCore.Types;
using static FCentralActivities;
using static FCentralResourceDefinitions;
using static Master40.SimulationCore.Agents.ResourceAgent.Resource.Instruction.Central;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
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
            Agent.Send(Resource.Instruction.Central.ActivityFinish.Create(Agent.Context.Self), activity.Duration);
        }

        
        public void FinishWork()
        {
            Agent.DebugMessage($"Finish {_currentActivity.ProductionOrderId}|{_currentActivity.OperationId}|{_currentActivity.ActivityId} with Duration: {_currentActivity.Duration}");
            Agent.Send(HubAgent.Hub.Instruction.Central.ActivityFinish.Create(_currentActivity, _currentActivity.Hub));
            _currentActivity = null;
        }

    }
}
