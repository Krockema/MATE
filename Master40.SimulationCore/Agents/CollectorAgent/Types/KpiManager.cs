using Master40.DB.DataModel;
using Master40.DB.ReportingModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.ReportingModel.Interface;
using Remotion.Linq.Clauses;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class KpiManager
    {

        public void GetTotalLoadFromLastInterval(long currentTime, long lastIntervalStart)
        {

        }

        internal long GetTotalTimeForInterval(List<M_Resource> resources, List<ISimulationResourceData> simulationResourceData, long startInterval, long endInterval)
        {
            var totalSetupTime = 0L;
            foreach (var resource in resources)
            {
                var setupTime = GetResourceTimeForInterval(resource, simulationResourceData, startInterval, endInterval);
                totalSetupTime += setupTime;
            }

            return totalSetupTime;
        }

        private long GetResourceTimeForInterval(M_Resource resource, List<ISimulationResourceData> simulationResourceData, long startInterval, long endInterval)
        {
            var time = 0L;

            var lower_borders = simulationResourceData.Where(x => x.Start < startInterval
                                                                  && x.End > startInterval
                                                                  && x.Resource == resource.Name).ToList()
                                                                    .Sum(y => y.End - startInterval);

            var upper_borders = simulationResourceData.Where(x => x.Start < endInterval
                                                                  && x.End > endInterval
                                                                  && x.Resource == resource.Name).ToList()
                                                                    .Sum(y => endInterval - y.Start);

            var between = simulationResourceData.Where(x => x.Start >= startInterval
                                                            && x.End <= endInterval
                                                            && x.Resource == resource.Name).ToList()
                                                                    .Sum(y => y.End - y.Start);

            time = between + lower_borders + upper_borders;

            return time;

        }

    }
}
