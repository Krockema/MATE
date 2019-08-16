using System;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using MathNet.Numerics.Distributions;

namespace Master40.SimulationCore.DistributionProvider
{
    public class WorkTimeGenerator
    {
        public static WorkTimeGenerator Create(Configuration configuration)
        {
            return new WorkTimeGenerator(
                seed: configuration.GetOption<Seed>().Value
                , deviation: configuration.GetOption<WorkTimeDeviation>().Value
                , simNumber: configuration.GetOption<SimulationNumber>().Value);
        }
        public WorkTimeGenerator(int seed, double deviation, int simNumber)
        {
            var source = new Random(Seed: seed + simNumber);
            _distribution = new LogNormal(mu: 0, sigma: deviation, randomSource: source);
        }

        private readonly LogNormal _distribution;
        /// <summary>
        /// Returns LogNormal-distributed worktime.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public int GetRandomWorkTime(int duration)
        {
            int newDuration;
            while (true)
            {
                newDuration = (int)Math.Round(value: duration * _distribution.Sample(), mode: MidpointRounding.AwayFromZero);
                if (newDuration <= 3 * duration) break;
            }
            return newDuration > 0 ? newDuration : 0;
        }
    }
}
