using System;
using MathNet.Numerics.Random;

namespace Master40.DataGenerator.Util
{

    public class XRandom
    {

        private readonly Random _rng;

        public XRandom(int seed)
        {
            _rng =  new Random(seed);
        }

        public int Next(int maxValue)
        {
            return _rng.Next(maxValue);
        }

        public double NextDouble()
        {
            return _rng.NextDouble();
        }


        public long NextLong(long maxValue)
        {
            var offset = 0L;
            while (maxValue > Int32.MaxValue)
            {
                var remainder = maxValue % 1;
                maxValue = maxValue / 2;
                var lowerHalf = _rng.NextBoolean();
                var assignRemainderToLowerHalf = _rng.NextBoolean();
                if (lowerHalf)
                {
                    if (assignRemainderToLowerHalf)
                    {
                        maxValue += remainder;
                    }
                }
                else
                {
                    offset += maxValue;
                    if (assignRemainderToLowerHalf)
                    {
                        offset += remainder;
                    }
                    else
                    {
                        maxValue += remainder;
                    }
                }
            }
            return offset + _rng.Next(Convert.ToInt32(maxValue));
        }

        public Random GetRng()
        {
            return _rng;
        }

    }

}