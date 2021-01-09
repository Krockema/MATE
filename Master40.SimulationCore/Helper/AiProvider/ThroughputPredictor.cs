using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AiProvider.DataStuctures;
using Microsoft.ML;
using Microsoft.ML.FastTree;

namespace Master40.SimulationCore.Helper.AiProvider
{
    public class ThroughputPredictor
    {
        public ThroughputPredictor()
        {
        }

        private static string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        private static string ModelPath = Path.Combine(rootDir, "Helper/AiProvider/MLModel/MLModel_OLS.zip");
        //private static string ModelPath = "../Master40.SimulationCore/Helper/AiProvider/MLModel/MLModel_OLS.zip";
        private static MLContext mlContext = new MLContext();
        internal List<SimulationKpis> SimulationKpis { get; }

        public long PredictThroughput(List<SimulationKpis> valuesForPrediction)
        {
            var kpisForPredict = valuesForPrediction.Last();

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            var predEngine = mlContext.Model.CreatePredictionEngine<SimulationKpis, CycleTimePrediction>(trainedModel);

            var resultPrediction = predEngine.Predict(kpisForPredict);
            
            return (long)Math.Round(resultPrediction.CycleTime, 0);
            //return 1920;
        }
    }
}
