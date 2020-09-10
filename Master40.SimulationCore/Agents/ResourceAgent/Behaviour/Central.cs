using Master40.DB.Nominal;
using static FCentralResourceDefinitions;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private FCentralResourceDefinition _resourceDefinition;
        public Central(FCentralResourceDefinition resourceDefinition, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
            _resourceDefinition = resourceDefinition;
        }

        public override bool Action(object message)
        {
            switch (message)
            {
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

    }
}
