using AiProvider.DataStuctures;
using Keras.Models;
using Microsoft.ML;
using Numpy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Master40.SimulationCore.Agents;
using Newtonsoft.Json;
using NLog;

namespace Master40.SimulationCore.Helper.AiProvider
{
    public class ThroughputPredictor
    {
        public ThroughputPredictor()
        {
        }

        //private static string rootDir =
        //    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../"));
        //private static string ModelPath = Path.Combine(rootDir, "Helper/AiProvider/MLModel/MLModel_OLS.zip");
        //private static string ModelPath = "E:/VCRepos/ng-erp-4.0_forked/Master40.SimulationCore/Helper/AiProvider/MLModel/MLModel_OLS.zip";
        private static string ModelPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("Master40.XUnitTest\\bin\\Debug\\netcoreapp3.1",
            "Master40.SimulationCore\\Helper\\AiProvider\\MLModel\\MLModel_OLS.zip");
        private static MLContext mlContext = new MLContext();
        //private const string PYTHON_HOME = @"C:\Users\weick\AppData\Local\Programs\Python\Python38";
        //private const string PYTHON_PATH = @"C:\Users\weick\AppData\Local\Programs\Python\Python38\lib\site-packages";
        //private const string PYTHON_DLL = @"\python38.dll";

        private List<float[]> predictedActualThroughputList = new List<float[]>();

        public long PredictThroughput(List<SimulationKpis> valuesForPrediction, Agent agent)
        {
            var kpisForPredict = getReshapedKpisForPrediction(valuesForPrediction);

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<SimulationKpisReshaped, CycleTimePrediction>(trainedModel);

            var resultPrediction = predEngine.Predict(kpisForPredict);

            if (predictedActualThroughputList.Count == 0)
            {
                predictedActualThroughputList.Add(new float[] { valuesForPrediction.Last().Time, resultPrediction.CycleTime, 0 });
            }
            else
            {
                predictedActualThroughputList.Last()[2] =
                    valuesForPrediction.Find(v => v.Time == predictedActualThroughputList.Last()[0] + 480).CycleTime;
                agent.DebugMessage(JsonConvert.SerializeObject(predictedActualThroughputList.Last()), CustomLogger.AIPREDICTIONS, LogLevel.Info);
                var newEntry = new float[] {valuesForPrediction.Last().Time, resultPrediction.CycleTime, 0};
                predictedActualThroughputList.Add(newEntry);
            }

            return (long)Math.Round(resultPrediction.CycleTime, 0);
            //return 1920;
        }

        private SimulationKpisReshaped getReshapedKpisForPrediction(List<SimulationKpis> kpiList)
        {
            var newKpi = new SimulationKpisReshaped
            {
                Assembly_t0 = kpiList.Last().Assembly,
                Assembly_t1 = kpiList[^2].Assembly,
                Assembly_t2 = kpiList[^3].Assembly,
                Consumab_t0 = kpiList.Last().Consumab,
                Consumab_t1 = kpiList[^2].Consumab,
                Consumab_t2 = kpiList[^3].Consumab,
                CycleTime_t0 = 0,
                CycleTime_t1 = kpiList.Last().CycleTime,
                CycleTime_t2 = kpiList[^2].CycleTime,
                InDueTotal_t0 = kpiList.Last().InDueTotal,
                InDueTotal_t1 = kpiList[^2].InDueTotal,
                InDueTotal_t2 = kpiList[^3].InDueTotal,
                Lateness_t0 = kpiList.Last().Lateness,
                Lateness_t1 = kpiList[^2].Lateness,
                Lateness_t2 = kpiList[^3].Lateness,
                Material_t0 = kpiList.Last().Material,
                Material_t1 = kpiList[^2].Material,
                Material_t2 = kpiList[^3].Material,
                Total_t0 = kpiList.Last().Total,
                Total_t1 = kpiList[^2].Total,
                Total_t2 = kpiList[^3].Total
            };

            return newKpi;
        }

        public long PredictThroughputWithKeras(List<SimulationKpis> valuesForPrediction)
        {
            //System.Environment.SetEnvironmentVariable("PATH", virtualEnvPath, EnvironmentVariableTarget.Process);
            //System.Environment.SetEnvironmentVariable("PYTHON_HOME", virtualEnvPath, EnvironmentVariableTarget.Process);
            //System.Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath, EnvironmentVariableTarget.Process); System.Environment.SetEnvironmentVariable();
            ////Runtime.PythonDLL = Path.Combine(pythonHome, pythonDLL);
            //Runtime.PythonDLL = PYTHON_HOME + PYTHON_DLL;

            //var model = new Sequential();


            //using ironpython
            //try
            //{
            //    var engine = IronPython.Hosting.Python.CreateEngine();
            //    var scope = engine.CreateScope();
            //    var ops = engine.Operations;
            //
            //    engine.Execute(script, scope);
            //    var pythonType = scope.GetVariable("MyClass");
            //    dynamic instance = ops.CreateInstance(pythonType);
            //    var value = instance.go(input);
            //
            //    if (!input.Equals(value))
            //    {
            //        throw new InvalidOperationException(
            //            "Odd... The return value wasn't the same as what we input!");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Oops! There was an exception" +
            //                      " while running the script: " + ex.Message);
            //}

            //NDarray array = np.array(new double[,] {{}});
            //
            //valuesForPrediction.ForEach(v =>
            //{
            //    array.add(new NDarray(new double[,]
            //        {{v.Lateness, v.Assembly, v.Total, v.Consumab, v.Material, v.InDueTotal, v.CycleTime}}));
            //});
            //
            //var model = Sequential.LoadModel(kerasModelPath);
            //var predictionData = model.Predict(array);


            return 1920;
        }
    }
}