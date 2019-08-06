using System;
using System.Collections.Generic;
using Master40.Simulation.CLI.Arguments;

namespace Master40.Simulation.CLI
{
    public class Commands : List<ICommand>
    {
        public Commands()
        {
            this.Add(new Help());
            this.Add(new DBConnectionString());
            this.Add(new DebugAgents());
            this.Add(new DebugSystem());
            this.Add(new EstimatedThroughPut());
            this.Add(new KpiTimeSpan());
            this.Add(new OrderArrivalRate());
            this.Add(new OrderQuantity());
            this.Add(new SaveToDB());
            this.Add(new Seed());
            this.Add(new SettlingStart());
            this.Add(new SimulationEnd());
            this.Add(new WorkTimeDeviation());
            this.Add(new SimulationId());
            this.Add(new SimulationKind());
            this.Add(new SimulationNumber());
        }


    }
}
