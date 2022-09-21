using System;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using MathNet.Numerics.Distributions;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    public class WorkTimeGenerator
    {
        Random _sourceRandom;
        double _deviation;

        public static WorkTimeGenerator Create(Configuration configuration, int salt = 0)
        {
            return new WorkTimeGenerator(
                seed: configuration.GetOption<Mate.Production.Core.Environment.Options.Seed>().Value + salt
                , deviation: configuration.GetOption<WorkTimeDeviation>().Value
                , simNumber: configuration.GetOption<SimulationNumber>().Value);
        }

        public WorkTimeGenerator(int seed, double deviation, int simNumber)
        {
            _sourceRandom = new Random(Seed: seed 
                                          //TODO WARUM?
                                          //+ simNumber
                                          );
            _deviation = deviation;

            //_distribution = new LogNormal(mu: 0, sigma: deviation, randomSource: source);
        }

        /// <summary>
        /// Returns LogNormal-distributed worktime.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public long GetRandomWorkTime(long duration)
        {
            if (_deviation == 0.0)
                return duration;

            long newDuration;
            while (true)
            {
                newDuration = (long)Math.Round(LogNormal.WithMeanVariance(duration, duration * _deviation, _sourceRandom).Sample());

                //newDuration = (long)Math.Round(value: duration * _distribution.Sample(), mode: MidpointRounding.AwayFromZero);
                if (newDuration <= 3 * duration) break;
            }
            return newDuration > 1 ? newDuration : 1;
        }
    }
}
