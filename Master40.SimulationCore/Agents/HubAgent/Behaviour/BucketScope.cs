using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using static FOperations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        internal BucketScope(SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType) { }

        private BucketManager _bucketManager { get; } = new BucketManager();

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
        private void AssignJob(IJob job)
        {
            var operation = (FOperation)job;

            System.Diagnostics.Debug.WriteLine($"Enqueue Operation {operation.Operation.Name} {operation.Key} ");
            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} | with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

        }

    }
}
