using System.Collections.Generic;
using Master40.Simulation.CLI.Arguments.External;
using Master40.Simulation.CLI.Arguments.Simulation;

namespace Master40.Simulation.CLI
{
    public class Commands : List<ICommand>
    {
        private static Commands self = new Commands
        {
             new Help()
            , new DBConnectionString()
            , new DebugAgents()
            , new TimeToAdvance()
            , new DebugSystem()
            , new EstimatedThroughPut()
            , new KpiTimeSpan()
            , new OrderArrivalRate()
            , new OrderQuantity()
            , new SaveToDB()
            , new Seed()
            , new SettlingStart()
            , new SimulationEnd()
            , new WorkTimeDeviation()
            , new SimulationId()
            , new SimulationKind()
            , new SimulationNumber()
            , new StartHangfire()
            , new TimePeriodForThroughputCalculation()
            , new MaxBucketSize()
            , new TransitionFactor()
            , new MaxDeliveryTime()
            , new MinDeliveryTime()
            , new TimeConstraintQueueLength()
            , new TestDataId()
        };


        public static List<ICommand> GetAllValidCommands => self;

        public static int CountRequiredCommands => self.Count;

    }
}
