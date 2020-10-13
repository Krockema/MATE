using Master40.DB.ReportingModel.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public  class KpiManager
    {
        private CultureInfo _cultureInfo { get; } = CultureInfo.GetCultureInfo(name: "en-GB");

        internal List<ResourceSimulationData> GetSimulationDataForResources (ResourceDictionary resources, List<ISimulationTask> simulationResourceData, List<ISimulationTask> simulationResourceSetupData, long startInterval, long
            endInterval)
        {
            List<ResourceSimulationData> resourceSimulationDataList = new List<ResourceSimulationData>();
            var divisor = endInterval - startInterval;

            foreach (var resource in resources)
            {
                ResourceSimulationData resourceSimulationData = new ResourceSimulationData(resource.Value.Name);

                var resourceData = simulationResourceData.Where(x => x.Mapping == resource.Value.Name).ToList();
                resourceSimulationData._totalWorkTime = GetResourceTimeForInterval(resource.Value.Name, resourceData, startInterval, endInterval);
                
                var work = Math.Round(value: Convert.ToDouble(resourceSimulationData._totalWorkTime) / Convert.ToDouble(divisor), digits: 3).ToString(provider: _cultureInfo);
                if (work == "NaN") work = "0";
                resourceSimulationData._workTime = work;

                var resourceSetupData = simulationResourceSetupData.Where(x => x.Mapping == resource.Value.Name).ToList();
                resourceSimulationData._totalSetupTime = GetResourceTimeForInterval(resource.Value.Name, resourceSetupData, startInterval, endInterval);
                var setup = Math.Round(value: Convert.ToDouble(resourceSimulationData._totalSetupTime) / Convert.ToDouble(divisor), digits: 3).ToString(provider: _cultureInfo);
                if (setup == "NaN") setup = "0";
                resourceSimulationData._setupTime = setup;

                resourceSimulationDataList.Add(resourceSimulationData);
            }
            return resourceSimulationDataList;

        }
        internal long GetTotalTimeForInterval(ResourceDictionary resources, List<ISimulationTask> simulationResourceData, long startInterval, long endInterval)
        {
            var totalTime = 0L;
            foreach (var resource in resources)
            {
                var time = GetResourceTimeForInterval(resource.Value.Name, simulationResourceData, startInterval, endInterval);
                totalTime += time;
            }

            return totalTime;
        }

        private long GetResourceTimeForInterval(string mapping, List<ISimulationTask> simulationResourceData, long startInterval, long endInterval)
        {
            var time = 0L;

            var lower_borders = simulationResourceData.Where(x => x.Start < startInterval
                                                                  && x.End > startInterval
                                                                  && x.Mapping == mapping).ToList()
                                                                    .Sum(y => y.End - startInterval);

            var upper_borders = simulationResourceData.Where(x => x.Start < endInterval
                                                                  && x.End > endInterval
                                                                  && x.Mapping == mapping).ToList()
                                                                    .Sum(y => endInterval - y.Start);

            var between = simulationResourceData.Where(x => x.Start >= startInterval
                                                            && x.End <= endInterval
                                                            && x.Mapping == mapping).ToList()
                                                                    .Sum(y => y.End - y.Start);

            time = between + lower_borders + upper_borders;

            return time;

        }

    }
}
