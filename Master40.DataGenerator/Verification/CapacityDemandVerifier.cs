using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;

namespace Master40.DataGenerator.Verification
{
    public class CapacityDemandVerifier
    {
        private readonly double _setupTimeFactor;
        private readonly Dictionary<int, double> _calculatedCapacityDemandByArticleId = new Dictionary<int, double>();

        public CapacityDemandVerifier(double setupTimeFactor)
        {
            _setupTimeFactor = setupTimeFactor;
        }

        public void Verify(ProductStructure productStructure)
        {
            var sumCapacity = 0.0;
            foreach (var endProduct in productStructure.NodesPerLevel[0])
            {
                sumCapacity += CalculateCapacityDemandForArticle(endProduct.Value);
            }

            var averageCapacityDemandPerEndProduct = sumCapacity / productStructure.NodesPerLevel[0].LongCount();
            System.Diagnostics.Debug.WriteLine("################################# Actual average capacity demand per end product is " + averageCapacityDemandPerEndProduct);
        }

        private double CalculateCapacityDemandForArticle(Node article)
        {
            if (_calculatedCapacityDemandByArticleId.ContainsKey(article.Article.Id))
            {
                return _calculatedCapacityDemandByArticleId[article.Article.Id];
            }

            var timeToProduce = 0.0;
            foreach (var op in article.Operations)
            {
                timeToProduce += op.MOperation.Duration + (op.SetupTimeOfCapability * _setupTimeFactor);
            }

            var sumCapacity = 0.0;
            foreach (var incEdge in article.IncomingEdges)
            {
                if (incEdge.Start.Article.ToBuild)
                {
                    sumCapacity += incEdge.Weight * CalculateCapacityDemandForArticle(incEdge.Start);
                }
            }

            var result = timeToProduce + sumCapacity;
            _calculatedCapacityDemandByArticleId.Add(article.Article.Id, result);
            return result;
        }

    }
}