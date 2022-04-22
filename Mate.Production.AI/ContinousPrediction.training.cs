using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.AI
{
    public partial class ContinuousPrediction
    {
        private static string ModelPath = Path.GetFullPath("model.zip");
        private static string Ogd_ModelPath = Path.GetFullPath("ogd_model.zip");
        public bool firstRun = true;

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            var options = new OnlineGradientDescentTrainer.Options
            {
                LabelColumnName = nameof(DataPoint.Label),
                FeatureColumnName = nameof(DataPoint.Features),
                // Change the loss function.
                LossFunction = new TweedieLoss(),
                // Give an extra gain to more recent updates.
                RecencyGain = 0.1f,
                // Turn off lazy updates.
                LazyUpdate = false,
                // Specify scale for initial weights.
                InitialWeightsDiameter = 0.2f
            };

            var pipeline = mlContext.Regression.Trainers.OnlineGradientDescent(options);
            
            return pipeline;
        }


        public void RetrainModel(IEnumerable<DataPoint> transitionData)
        {
            // Create MLContext
            MLContext mlContext = new MLContext();

            // Define DataViewSchema of data prep pipeline and trained model
            DataViewSchema dataPrepPipelineSchema, modelSchema;

            var trainingData = mlContext.Data.LoadFromEnumerable(transitionData);

            /*
             * Example how it could work
             * 
                // Define data preparation estimator
                IEstimator<ITransformer> dataPrepEstimator =
                    mlContext.Transforms.Concatenate("Features", new string[] { "Size", "HistoricalPrices" })
                        .Append(mlContext.Transforms.NormalizeMinMax("Features"));

                // Create data preparation transformer
                ITransformer dataPrepTransformer = dataPrepEstimator.Fit(trainingData);

                // Define StochasticDualCoordinateAscent regression algorithm estimator
                var sdcaEstimator = mlContext.Regression.Trainers.Sdca();

                // Pre-process data using data prep operations
                IDataView transformedData = dataPrepTransformer.Transform(trainingData);

                // Train regression model
                RegressionPredictionTransformer<LinearRegressionModelParameters> trainedModel = sdcaEstimator.Fit(transformedData);
            */

            if (firstRun)
            {
                var model = BuildPipeline(mlContext).Fit(trainingData);

                mlContext.Model.Save(model, trainingData.Schema, ModelPath);
                firstRun = false;
            }
            else
            {

                // Load data preparation pipeline
                ITransformer dataPrepPipeline = BuildPipeline(mlContext).Fit(trainingData);

                // Load trained model
                ITransformer trainedModel = mlContext.Model.Load(ModelPath, out modelSchema);

                // Preprocess Data
                IDataView transformedNewData = dataPrepPipeline.Transform(trainingData);

                LinearRegressionModelParameters originalModelParameters =
                       ((ISingleFeaturePredictionTransformer<object>)trainedModel).Model as LinearRegressionModelParameters;

                // Retrain model
                RegressionPredictionTransformer<LinearRegressionModelParameters> retrainedModel =
                    mlContext.Regression.Trainers.OnlineGradientDescent()
                        .Fit(transformedNewData, originalModelParameters);

                mlContext.Model.Save(retrainedModel, modelSchema, ModelPath);
            }

        }

    }




}
