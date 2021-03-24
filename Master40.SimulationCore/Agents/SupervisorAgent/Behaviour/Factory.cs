using Master40.DB.Data.Context;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Types;
using Master40.Tools.SignalR;
using System.Collections.Generic;
using static FSetEstimatedThroughputTimes;

namespace Master40.SimulationCore.Agents.SupervisorAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Default(string dbNameProduction
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Default(dbNameProduction, messageHub, configuration, estimatedThroughputTimes);

        }
        public static IBehaviour Central(string dbNameProduction
            , string dbNameGantt
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Central(dbNameGantt, dbNameProduction, messageHub, configuration, estimatedThroughputTimes);
        }
    }
}
