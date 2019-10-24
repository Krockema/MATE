using Master40.DB.DataModel;
using Master40.DB.ReportingModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.ReportingModel.Interface;
using Master40.SimulationCore.Agents.ResourceAgent;
using Remotion.Linq.Clauses;
using System.Globalization;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class KpiManager
    {
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");

        public void GetTotalLoadFromLastInterval(long currentTime, long lastIntervalStart)
        {

        }

        internal List<ResourceSimulationData> GetSimulationDataForResources (ResourceList resources, List<ISimulationResourceData> simulationResourceData, List<ISimulationResourceData> simulationResourceSetupData, long startInterval, long
            endInterval)
        {
            List<ResourceSimulationData> resourceSimulationDataList = new List<ResourceSimulationData>();
            var divisor = endInterval - startInterval;

            foreach (var resource in resources)
            {
                ResourceSimulationData resourceSimulationData = new ResourceSimulationData(resource);

                var resourceData = simulationResourceData.Where(x => x.Resource == resource).ToList();
                var workTime = GetResourceTimeForInterval(resource, resourceData, startInterval, endInterval);
                var work = Math.Round(value: Convert.ToDouble(workTime) / Convert.ToDouble(divisor), digits: 3).ToString(provider: _cultureInfo);
                if (work == "NaN") work = "0";
                resourceSimulationData._workTime = work;

                var resourceSetupData = simulationResourceSetupData.Where(x => x.Resource == resource).ToList();
                var setupTime = GetResourceTimeForInterval(resource, resourceSetupData, startInterval, endInterval);
                var setup = Math.Round(value: Convert.ToDouble(setupTime) / Convert.ToDouble(divisor), digits: 3).ToString(provider: _cultureInfo);
                if (setup == "NaN") setup = "0";
                resourceSimulationData._setupTime = setup;

                resourceSimulationDataList.Add(resourceSimulationData);
            }
            return resourceSimulationDataList;

        }
        internal long GetTotalTimeForInterval(ResourceList resources, List<ISimulationResourceData> simulationResourceData, long startInterval, long endInterval)
        {
            var totalTime = 0L;
            foreach (var resource in resources)
            {
                var time = GetResourceTimeForInterval(resource, simulationResourceData, startInterval, endInterval);
                totalTime += time;
            }

            return totalTime;
        }

        private long GetResourceTimeForInterval(string resource, List<ISimulationResourceData> simulationResourceData, long startInterval, long endInterval)
        {
            var time = 0L;

            var lower_borders = simulationResourceData.Where(x => x.Start < startInterval
                                                                  && x.End > startInterval
                                                                  && x.Resource == resource).ToList()
                                                                    .Sum(y => y.End - startInterval);

            var upper_borders = simulationResourceData.Where(x => x.Start < endInterval
                                                                  && x.End > endInterval
                                                                  && x.Resource == resource).ToList()
                                                                    .Sum(y => endInterval - y.Start);

            var between = simulationResourceData.Where(x => x.Start >= startInterval
                                                            && x.End <= endInterval
                                                            && x.Resource == resource).ToList()
                                                                    .Sum(y => y.End - y.Start);

            time = between + lower_borders + upper_borders;

            return time;

        }

    }
}
