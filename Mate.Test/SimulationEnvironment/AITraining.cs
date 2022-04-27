using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using Mate.Production.AI;

namespace Mate.Test.SimulationEnvironment
{
    public class AITraining
    {

        private ContinuousPrediction continousPrediction = new();

        [Fact]
        public void Train_Model_ProductionData()
        {
            //For test only
            var referenceDataPoint = GenerateRandomDataPoints(1).First();

            var datapoints = GenerateRandomDataPoints(800);
            continousPrediction.RetrainModel(datapoints);
            var score = continousPrediction.Predict(referenceDataPoint);

            var listOfScores = new List<float>();
            //Retrain 
            for (var i = 0; i < 10; i++)
            {
                var newScore = RetrainModel(referenceDataPoint);
                System.Diagnostics.Debug.WriteLine($"++++++++++++++++++++++ {score.Score} to {newScore}+++++++++++++++++++++");
                listOfScores.Add(newScore);
            }

            foreach (var newScores in listOfScores)
            {
                Assert.NotEqual(score.Score, newScores);
            }

        }

        [Fact]
        public void Train_Model()
        {
            //For test only
            var referenceDataPoint = GenerateRandomDataPoints(1).First();
            
            var datapoints = GenerateRandomDataPoints(800);
            continousPrediction.RetrainModel(datapoints);
            var score = continousPrediction.Predict(referenceDataPoint);

            var listOfScores = new List<float>();
            //Retrain 
            for(var i = 0; i < 10; i++)
            {
                var newScore = RetrainModel(referenceDataPoint);
                System.Diagnostics.Debug.WriteLine($"++++++++++++++++++++++ {score.Score} to {newScore}+++++++++++++++++++++");
                listOfScores.Add(newScore);  
            }
            
            foreach(var newScores in listOfScores)
            {
                Assert.NotEqual(score.Score, newScores);
            }

        }

        public float RetrainModel(DataPoint referenceDataPoint)
        {
            var newdatapoints = GenerateRandomDataPoints(800);

            continousPrediction.RetrainModel(newdatapoints);
            continousPrediction.UpdatePredictionEngine();
            var score_retrained = continousPrediction.Predict(referenceDataPoint);

            return score_retrained.Score;
        }


        private IEnumerable<DataPoint> GenerateRandomDataPoints(int count,int seed = 0)
        {
            var random = new Random(seed);
            for (int i = 0; i < count; i++)
            {
                float label = (float)random.NextDouble();
                yield return new DataPoint
                {
                    Label = label,
                    // Create random features that are correlated with the label.
                    Features = Enumerable.Repeat(label, 3).Select(
                        x => x + (float)random.NextDouble()).ToArray()
                };
            }
        }




    }
}
