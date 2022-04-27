using System.Collections.Generic;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.SignalR;
using Mate.Production.Core.Types;
using static FSetEstimatedThroughputTimes;

namespace Mate.Production.Core.Agents.SupervisorAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType, 
            string dbNameProduction
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes
            , string dbNameGantt = "")
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Central:
                    behaviour = Central(simType, dbNameProduction, messageHub, configuration, estimatedThroughputTimes, dbNameGantt);
                    break;
                case SimulationType.ML:
                    behaviour = ML(simType, dbNameProduction, messageHub, configuration, estimatedThroughputTimes);
                    break;
                default:
                    behaviour = Default(simType, dbNameProduction, messageHub, configuration, estimatedThroughputTimes);
                    break;

            }
            return behaviour;
        }

        public static IBehaviour Default(SimulationType simType
            , string dbNameProduction
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Default(dbNameProduction, messageHub, configuration, estimatedThroughputTimes);

        }
        public static IBehaviour Central(SimulationType simType
            , string dbNameProduction
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes
            , string dbNameGantt)
        {
            return new Central(dbNameGantt, dbNameProduction, messageHub, configuration, estimatedThroughputTimes);
        }
        public static IBehaviour ML(SimulationType simType
           ,string dbNameProduction
           , IMessageHub messageHub
           , Configuration configuration
           , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new ML(dbNameProduction, messageHub, configuration, estimatedThroughputTimes);

        }
    }
}
