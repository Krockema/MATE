using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Helper.DistributionProvider
{
    public class MeasurementValuesGenerator
    {
        Random _source = null;
        public MeasurementValuesGenerator(int seed)
        {
            var source = new Random(Seed: seed
                                          //TODO WARUM?
                                          //+ simNumber
                                          );
           
        }

        public double GetRandomMeasurementValues(double estimatedValue, double toleranceMin, double toleranceMax, double zForPrecision)
        {
            var min = estimatedValue + toleranceMin;
            var max = estimatedValue + toleranceMax;
            var deviation = (max - min) / (2 * zForPrecision);
            return Math.Round(new Normal(estimatedValue, deviation, _source).Sample(), 3, MidpointRounding.AwayFromZero);
        }
    }
}
