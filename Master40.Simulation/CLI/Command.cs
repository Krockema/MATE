using System;
using System.Collections.Generic;
using Master40.Simulation.CLI.Arguments;

namespace Master40.Simulation.CLI
{
    public class Commands : List<ICommand>
    {
        public Commands()
        {
            this.Add(item: new Help());
            this.Add(item: new DBConnectionString());
            this.Add(item: new DebugAgents());
            this.Add(item: new DebugSystem());
            this.Add(item: new EstimatedThroughPut());
            this.Add(item: new KpiTimeSpan());
            this.Add(item: new OrderArrivalRate());
            this.Add(item: new OrderQuantity());
            this.Add(item: new SaveToDB());
            this.Add(item: new Seed());
            this.Add(item: new SettlingStart());
            this.Add(item: new SimulationEnd());
            this.Add(item: new WorkTimeDeviation());
            this.Add(item: new SimulationId());
            this.Add(item: new SimulationKind());
            this.Add(item: new SimulationNumber());
        }


    }
}
