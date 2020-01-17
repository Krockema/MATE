using MathNet.Numerics.Statistics;

namespace Master40.Tools
{
    public struct Quantil
    {
        /// <summary>
        ///            _
        ///          .   .
        ///         .     .
        ///       .         .   <-- tolerance
        ///    .            |  .
        ///------------------------------------
        /// </summary>
        /// <param name="tolerance"></param>
        /// <param name="z"></param>
        public Quantil(double tolerance, double z)
        {
            Tolerance = tolerance;
            Z = z;
        }
        public double Tolerance { get; }
        public double Z { get; }

    }
}