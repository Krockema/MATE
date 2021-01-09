using AiProvider.DataStuctures;
using Keras.Models;
using Microsoft.ML;
using Numpy;
using System;
using System.Collections.Generic;
using System.IO;

namespace Master40.SimulationCore.Helper.AiProvider
{
    public class ThroughputPredictor
    {
        public ThroughputPredictor()
        {
        }

        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        private static string ModelPath = Path.Combine(rootDir, "MLModel.zip");
        private static string kerasModelPath = Path.Combine(rootDir, "KerasModel");
        private static MLContext mlContext = new MLContext();
        //private const string PYTHON_HOME = @"C:\Users\weick\AppData\Local\Programs\Python\Python38";
        //private const string PYTHON_PATH = @"C:\Users\weick\AppData\Local\Programs\Python\Python38\lib\site-packages";
        //private const string PYTHON_DLL = @"\python38.dll";
        internal List<SimulationKpis> SimulationKpis { get; } //= new List<SimulationKpis>();

        public static float PredictWithSavedModel(List<SimulationKpis> simKpis, int numberOfPredictions)
        {
            float resultPrediction = 0;
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<SimulationKpis, CycleTimePrediction>(trainedModel);

            //resultPrediction = predEngine.Predict(simKpis);
            //resultPrediction = Math.Round(resultPrediction, 0);
            return resultPrediction;

            // --> Finde ich doch erstmal gar nicht so sinnvoll. Aufgrund der Bildung des Durchschnitts kann es sein, dass wir einen zu hohen oder zu niedrigen Wert verwenden
            // Eventuell Durchschnitt aus 2 Predictions
            // Predict i number of Values to use a mean value
            /*            for (int i = 0; i < numberOfPredictions; i++)
                        {
                            resultPrediction = predEngine.Predict(simKpis[i]);
                        }
                        return resultPrediction / numberOfPredictions;*/
        }

        public long PredictThroughput(List<SimulationKpis> valuesForPrediction)
        {
            //_estimatedThroughPuts.UpdateOrCreate(articleName, predictedThroughput);
            // Test Commit mschwrdtnr
            return 1920;
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




            NDarray array = np.array(new double[,] {{}});

            valuesForPrediction.ForEach(v =>
            {
                array.add(new NDarray(new double[,]
                    {{v.Lateness, v.Assembly, v.Total, v.Consumab, v.Material, v.InDueTotal, v.CycleTime}}));
            });

            var model = Sequential.LoadModel(kerasModelPath);
            var predictionData = model.Predict(array);


            return 1920;
        }
    }
}