﻿using System;
using MathNet.Numerics.Distributions;

namespace Master40.SimulationCore.DistributionProvider
{
    public class WorkTimeGenerator
    {
        public WorkTimeGenerator(int seed, double deviation, int simNumber)
        {
            var source = new Random(seed + simNumber);
            _distribution = new LogNormal(0, deviation, source);
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
                newDuration = (int)Math.Round(duration * _distribution.Sample(), MidpointRounding.AwayFromZero);
                if (newDuration <= 3 * duration) break;
            }
            return newDuration > 0 ? newDuration : 0;
        }
    }
}
