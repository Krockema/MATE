using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Nominal;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class Confirmation
    {
        public ResourceDefinition ResourceDefinition { get; private set; }

        // State as Enum = Ganttplan confirmation_type =  1 und 16 (beendet)
        public GanttConfirmationState State { get; set; }

        public bool IsFinished => State == GanttConfirmationState.Finished;

        public Confirmation(ResourceDefinition resourceDefinition)
        {
            ResourceDefinition = resourceDefinition;
            State = GanttConfirmationState.Started;
        }

    }
}
