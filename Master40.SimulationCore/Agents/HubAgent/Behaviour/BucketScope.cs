using Master40.DB.Enums;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        internal BucketScope(SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType) { }

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }
    }
}
