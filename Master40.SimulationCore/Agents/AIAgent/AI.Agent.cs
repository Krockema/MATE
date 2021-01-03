using System;
using System.Collections.Generic;
using System.IO;
using AIAgent.DataStuctures;
using Microsoft.ML;

namespace Master40.SimulationCore.Agents.AIAgent
{
    public class AI
    {
        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        private static string ModelPath = Path.Combine(rootDir, "MLModel.zip");
        private static MLContext mlContext = new MLContext();
        internal List<SimulationKpis> SimulationKpis { get; } //= new List<SimulationKpis>();

        public static float PredictWithSavedModel(List<SimulationKpis> simKpis, int numberOfPredictions)
        {
            float resultPrediction = 0;
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<SimulationKpis, CycleTimePrediction>(trainedModel);

            //resultPrediction = predEngine.Predict(simKpis);
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
    }
}
