using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Mate.DataCore.ReportingModel;
using Mate.DataCore.Nominal;

namespace Mate.Production.Core.Agents.CollectorAgent.Types
{
    public sealed class StabilityManager
    {
        private static readonly Lazy<StabilityManager> lazyStabilityManager
            = new Lazy<StabilityManager>(() => new StabilityManager());

        public static StabilityManager Instance => lazyStabilityManager.Value;

        private Dictionary<string, List<OperationPosition>> OperationDictionary = new Dictionary<string, List<OperationPosition>>();

        public bool HasEntries => OperationDictionary.Count > 0;

        public void AddEntryForBucket(List<string> keys, long time, int position, string resource, long start, string process)
        {
            foreach (var key in keys)
            {
                AddEntryForOperation(time, key, position, resource, start, process);
            }
        }

        public void AddEntryForOperation(long time, string operationKey, int position, string resource, long start, string process)
        {
            OperationPosition operationPosition = new(time, position, resource, start, process);

            if (OperationDictionary.TryGetValue(operationKey, out var operationPositionList))
            {
                operationPositionList.Add(operationPosition);
            }
            else
            {
                OperationDictionary.Add(operationKey, new List<OperationPosition>() { operationPosition });
            }
        }



        public void WriteFile(int simNum)
        {
            string fileName = $"F:\\Work\\Data\\OperationDictionary{simNum}.json";
            string jsonString = JsonSerializer.Serialize(OperationDictionary);
            File.WriteAllText(fileName, jsonString);
        }

        public void ReadFile()
        {
            string fileName = "F:\\Work\\Data\\OperationDictionary.json";
            string jsonString = File.ReadAllText(fileName);
            OperationDictionary = JsonSerializer.Deserialize<Dictionary<string, List<OperationPosition>>>(jsonString)!;

            DoSomeStatistics();
        }

        public List<Kpi> DoSomeStatistics(int simNum=1, SimulationType simType = SimulationType.Default)
        {
            List<Kpi> keyValuePairs = new List<Kpi>();
            int result = 0;
            int counter = 0;

            List<long> averageList = new List<long>();

            Dictionary<int, int> averagePosition = new();

            Dictionary<int, List<double>> averageDistanctPosition = new();

            //Central

            foreach (KeyValuePair<string, List<OperationPosition>> entry in OperationDictionary)
            {
                var data = entry.Value.OrderBy(x => x.RealTime).ToList();

                for(int i = 0; i < data.Count-1; i++)
                {
                    var actual = data[i];
                    var next = data[i + 1];
                    if (actual.Process != Process.Dequeue.ToString() || next.Process != Process.Enqueue.ToString())
                        continue;


                    result++;
                    var distance = Math.Abs(actual.Start - next.Start);
                    averageList.Add(distance);

                    if(averagePosition.TryGetValue(actual.Position, out var amount))
                    {
                        averagePosition[actual.Position] = amount+1;
                        averageDistanctPosition[actual.Position].Add(distance);
                    }
                    else
                    {
                        averagePosition.Add(actual.Position, 1);
                        averageDistanctPosition.Add(actual.Position, new List<double>() { distance});
                    }


                    if ((actual.Resource.Equals(String.Empty) || next.Resource.Equals(String.Empty)) || next.Position.Equals(actual.Position) && next.Resource.Equals(actual.Resource))
                        counter++;

                    // wieviele Jobs haben tatsächlich die Maschine gewechselt?
                    // wieviele Jobs haben "nur" die Position in der Warteschlange 

                }
            }

            

            keyValuePairs.Add(CreateKpi("RatioUselessReplan", Math.Round(((double)counter / (double)result) * 100, 2), simNum, simType));
            keyValuePairs.Add(CreateKpi("AverageTimeSpan", averageList.Average(), simNum, simType));
            System.Diagnostics.Debug.WriteLine($"{counter} entries of {result}: {Math.Round(((double)counter/(double)result)*100,2)}%");
            System.Diagnostics.Debug.WriteLine($"Average timespan change {averageList.Average()}");

            foreach (var positon in averagePosition.OrderBy(x => x.Key))
            {
                keyValuePairs.Add((CreateKpi($"Position: {positon.Key}", positon.Value, simNum, simType)));
                //System.Diagnostics.Debug.WriteLine($"Position: {positon.Key} | {positon.Value}");
            }

            foreach (var distance in averageDistanctPosition.OrderBy(x => x.Key))
            {
                keyValuePairs.Add((CreateKpi($"DistancePosition: {distance.Key}", distance.Value.Average(), simNum, simType)));
                //System.Diagnostics.Debug.WriteLine($"DIstancePosition: {distance.Key} | {distance.Value.Average()}");
            }

            return keyValuePairs;
        }


        public Kpi CreateKpi(string name, double value, int simNumber, SimulationType simulationType)
        {
            return new Kpi
            {
                Name = name,
                Value = value,
                Time = 1,
                KpiType = DataCore.Nominal.KpiType.Stability ,
                SimulationConfigurationId = 1,
                SimulationNumber = simNumber,
                IsFinal = true,
                IsKpi = true,
                SimulationType = simulationType,
                ValueMin = 0,
                ValueMax = 0
            };
        }
    }
}
