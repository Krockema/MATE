using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class Confirmation
    {
        public ResourceDefinition ResourceDefinition { get; private set; }

        // State as Enum = Ganttplan confirmation_type =  1 und 16 (beendet)
        public GanttConfirmationState State { get; set; }
        public GptblProductionorderOperationActivity Activity {  get; set; }

        public int PlanVersion {  get; set; }
        public bool IsFinished => State == GanttConfirmationState.Finished;

        public Confirmation(GptblProductionorderOperationActivity activity, ResourceDefinition resourceDefinition, int planVersion)
        {
            Activity = activity;
            ResourceDefinition = resourceDefinition;
            State = GanttConfirmationState.Started;
            PlanVersion = planVersion;  
        }

    }
}
