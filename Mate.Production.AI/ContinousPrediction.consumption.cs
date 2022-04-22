using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.AI
{
    public partial class ContinuousPrediction
    {
        public Lazy<PredictionEngine<DataPoint, ModelOutput>> PredictEngine = null;
        public ContinuousPrediction()
        {
            PredictEngine = new (() => CreatePredictEngine(), true);
        }

        /// <summary>
        /// model output class for Predict_IdleTimeWithDirectRelease.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            public float Score { get; set; }
        }
        #endregion


        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public ModelOutput Predict(DataPoint input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        public static PredictionEngine<DataPoint, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(ModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<DataPoint, ModelOutput>(mlModel);
        }

        public void UpdatePredictionEngine()
        {
            PredictEngine = new(() => CreatePredictEngine(), true);
        }


    }
}
