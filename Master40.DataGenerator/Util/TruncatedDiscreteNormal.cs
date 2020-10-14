using System;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Util
{
    public class TruncatedDiscreteNormal
    {
        private readonly int? _lowerBound, _upperBound;
        private readonly Normal _normal;

        public TruncatedDiscreteNormal(int? lowerBound, int? upperBound, Normal normal)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            _normal = normal;
        }

        public int Sample()
        {
            int result;
            do
            {
                result = (int) Math.Round(_normal.Sample());
            } while ((_lowerBound != null && result < _lowerBound) || (_upperBound != null && result > _upperBound));
            return result;
        }
    }
}