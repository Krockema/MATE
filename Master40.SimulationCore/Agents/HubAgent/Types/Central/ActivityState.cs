using Master40.DB.GanttPlanModel;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Nominal;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ActivityState
    {
        public GptblProductionorderOperationActivity Activity { get; private set; }

        public List<Confirmation> Confirmations { get; private set; } = new List<Confirmation>();

        public bool ActivityIsFinished => Confirmations.TrueForAll(x => x.IsFinished);

        public ActivityState(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition)
        {
            var confirmation = new Confirmation(resourceDefinition);
            Confirmations.Add(confirmation);
            Activity = activity;
        }

        public void AddResource(ResourceDefinition resourceDefinition)
        {
            var confirmation = new Confirmation(resourceDefinition);
            Confirmations.Add(confirmation);
        }

        public void FinishForResource(string resourceId)
        {
            var confirmation = Confirmations.Single(x => x.ResourceDefinition.Id.Equals(resourceId));
            confirmation.State = GanttState.Finished;
        }
    }
}