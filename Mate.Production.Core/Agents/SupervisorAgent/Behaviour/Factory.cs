using System.Collections.Generic;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment;
using Mate.Production.Core.SignalR;
using Mate.Production.Core.Types;
using static FSetEstimatedThroughputTimes;

namespace Mate.Production.Core.Agents.SupervisorAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Default(string dbNameProduction
            , IMessageHub messageHub
            , IHiveConfig hiveConfig
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Default(dbNameProduction, messageHub, hiveConfig, configuration, estimatedThroughputTimes);

        }
        public static IBehaviour Central(string dbNameProduction
            , string dbNameGantt
            , IMessageHub messageHub
            , IHiveConfig hiveConfig
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Central(dbNameGantt, dbNameProduction, messageHub, hiveConfig, configuration, estimatedThroughputTimes);
        }
    }
}
