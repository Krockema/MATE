using System.Collections.Generic;
using Mate.Production.Core.Environment.Abstractions;
using Mate.Production.Core.Environment.Options;

namespace Mate.Production.CLI
{
    public class Commands : List<ICommand>
    {
        private static readonly Commands self = new Commands
        {
            new ResultsDbConnectionString()
            , new DebugAgents()
            , new TimeToAdvance()
            , new DebugSystem()
            , new EstimatedThroughPut()
            , new KpiTimeSpan()
            , new OrderArrivalRate()
            , new OrderQuantity()
            , new SaveToDB()
            , new Core.Environment.Options.Seed()
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
            , new CreateQualityData()
            , new PriorityRule()
        };


        public static List<ICommand> GetAllValidCommands => self;

        public static int CountRequiredCommands => self.Count;

    }
}
