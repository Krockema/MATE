using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.GanttPlanModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Types;
using Master40.Tools.SignalR;
using static FSetEstimatedThroughputTimes;

namespace Master40.SimulationCore.Agents.SupervisorAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Default(ProductionDomainContext productionDomainContext
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Default(productionDomainContext, messageHub, configuration, estimatedThroughputTimes);

        }
        public static IBehaviour Central(GanttPlanDBContext ganttContext
            , ProductionDomainContext productionDomainContext
            , IMessageHub messageHub
            , Configuration configuration
            , List<FSetEstimatedThroughputTime> estimatedThroughputTimes)
        {
            return new Central(ganttContext, productionDomainContext, messageHub, configuration, estimatedThroughputTimes);
        }
    }
}
