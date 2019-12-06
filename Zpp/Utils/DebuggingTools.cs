using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Helper.Types;
using Zpp.DataLayer;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util.Graph.impl;
using Zpp.Utils;

namespace Zpp.Util
{
    public static class DebuggingTools
    {
        private static readonly string SimulationFolder = $"../../../Test/Ordergraphs/Simulation/";
        private static readonly string performanceLogFileName = "performance";
        private static bool _isCleanedUp = false;
        private static string defaultFileExtension = ".log";

        public static void PrintStateToFiles(IDbTransactionData dbTransactionData, bool forcePrint)
        {
            // interval and end is not used anyway, so the second 1 is here a random number
            PrintStateToFiles(new SimulationInterval(0, 1), dbTransactionData, "", false, forcePrint);
        }

        /**
         * includes demandToProviderGraph, OrderOperationGraph and dbTransactionData
         * forcePrint: force printing even if in performanceMode
         */
        public static void PrintStateToFiles(SimulationInterval simulationInterval,
            IDbTransactionData dbTransactionData, string stageName, bool includeOrderOperationGraph,
            bool forcePrint = false)
        {
            if (Constants.IsWindows == false || ZppConfiguration.IsInPerformanceMode)
            {
                // skip this in the cloud/IsInPerformanceMode
                if (forcePrint == false)
                {
                    return;
                }
            }

            CleanupOldFiles();

            WriteToFile(
                $"{SimulationFolder}dbTransactionData_interval_{simulationInterval.StartAt}_{stageName}.txt",
                dbTransactionData.ToString());
            WriteToFile(
                $"{SimulationFolder}dbTransactionDataArchive_interval_{simulationInterval.StartAt}_{stageName}.txt",
                ZppConfiguration.CacheManager.GetDbTransactionDataArchive().ToString());
            DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();
            WriteToFile(
                $"{SimulationFolder}demandToProviderGraph_interval_{simulationInterval.StartAt}_{stageName}.txt",
                demandToProviderGraph.ToString());
            if (includeOrderOperationGraph)
            {
                OrderOperationGraph orderOperationGraph = new OrderOperationGraph();    
                WriteToFile(
                    $"orderOperationGraph_interval_{simulationInterval.StartAt}_{stageName}.log",
                    orderOperationGraph.ToString());
            }
        }

        private static void CleanupOldFiles()
        {
            if (_isCleanedUp == false)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(SimulationFolder);
                if (directoryInfo.Exists)
                {
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }

                _isCleanedUp = true;
            }
        }

        public static void WriteToFile(string fileName, string content)
        {
            Directory.CreateDirectory(SimulationFolder);
            File.WriteAllText($"{SimulationFolder}{fileName}", content, Encoding.UTF8);
        }

        public static void WritePerformanceLog(string content, string type="")
        {
            ICentralPlanningConfiguration testConfiguration =
                ZppConfiguration.CacheManager.GetTestConfiguration();
            int cycles = testConfiguration.SimulationMaximumDuration /
                         testConfiguration.SimulationInterval;
            if (type.Equals(""))
            {
                type = $"_{testConfiguration.Name}_cycles_{cycles}_COs_{testConfiguration.CustomerOrderPartQuantity}";    
            }
            
            WriteToFile($"{performanceLogFileName}{type}{defaultFileExtension}", content);
            
        }

        public static string Prettify(long value)
        {
            string valueAsString = value.ToString();
            string newValue = "";
            int length = valueAsString.Length;
            int count = 0;
            for (int i = length - 1; i >= 0; i--, count++)
            {
                if (count > 0 && count % 3 == 0)
                {
                    newValue = "," + newValue;
                }

                newValue = valueAsString[i] + newValue;
            }

            return newValue;
        }

        private static string ReadFile(string pathToFile)
        {
            return File.ReadAllText($"{pathToFile}{defaultFileExtension}", Encoding.UTF8);
        }

        public static string ReadPerformanceLog()
        {
            return ReadFile($"{SimulationFolder}{performanceLogFileName}");
        }
    }
}