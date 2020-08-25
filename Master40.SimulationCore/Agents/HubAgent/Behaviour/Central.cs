using Master40.DB.Nominal;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using static FResourceInformations;


namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {

        public Central(SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
        }

            //Properties

            public override bool Action(object message)
            {
                var success = true;
                switch (message)
                {
                    //Initialize
                    case Hub.Instruction.Default.AddResourceToHub msg: CreateOperations(resourceInformation: msg.GetObjectFromMessage); break;
                    case Hub.Instruction.Default.EnqueueJob msg: EnqueueJob(); break;
                    default: return false;
                }
                return success;
            }

        private void EnqueueJob()
        {
            throw new NotImplementedException();
        }

        private void CreateOperations(FResourceInformation resourceInformation)
        {
            throw new NotImplementedException();
        }
    }
}
