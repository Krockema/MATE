using Master40.DB.Nominal;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using static FResourceInformations;


namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {

        public Central(long maxBucketSize, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
        }

            public override bool Action(object message)
            {
                var success = true;
                switch (message)
                {
                    //Initialize
                    case Hub.Instruction.Default.AddResourceToHub msg: CreateOperations(resourceInformation: msg.GetObjectFromMessage); break;

                    default: return false;
                }
                return success;
            }

        private void CreateOperations(FResourceInformation resourceInformation)
        {
            throw new NotImplementedException();
        }
    }
}
