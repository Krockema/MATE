using Master40.DB.Nominal;
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
                case ActivityStart msg: StartWork(msg.GetObjectFromMessage);
                    break;
                case ActivityFinish msg: FinishWork();
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
                                                                                        , Agent.Context.Self)
                , Agent.VirtualParent));
           return true;
        }

        public void StartWork(FCentralActivity activity)
        {
            System.Diagnostics.Debug.WriteLine("Start Activity {0} with Duration: {1}", activity.Name ,activity.Duration);
            _currentActivity = activity;
            Agent.Send(ActivityFinish.Create(null, Agent.Context.Self), activity.Duration);
        }

        
        public void FinishWork()
        {
            System.Diagnostics.Debug.WriteLine("Finish Activity {0} with Duration: {1}", _currentActivity.Name ,_currentActivity.Duration);
            Agent.Send(ActivityFinish.Create(_currentActivity, _currentActivity.Hub));
            _currentActivity = null;
        }

    }
}
