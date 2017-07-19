//=============================================================================
//=  $Id: Normal.cs 185 2006-10-14 18:53:40Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
//=
//=  React.NET is free software; you can redistribute it and/or modify it
//=  under the terms of the GNU General Public License as published by the
//=  Free Software Foundation; either version 2 of the License, or (at your
//=  option) any later version.
//=
//=  React.NET is distributed in the hope that it will be useful, but WITHOUT
//=  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
//=  FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
//=  more details.
//=
//=  You should have received a copy of the GNU General Public License along
//=  with React.NET; if not, write to the Free Software Foundation, Inc.,
//=  51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//=============================================================================

using System;

namespace React.Distribution
{
    /// <summary>
    /// Generates random values according to a <em>normal</em>
    /// distribution.
    /// </summary>
    /// <remarks>
    /// A normal distribution is sometimes called a <em>gaussian</em>
    /// distribution.
    /// </remarks>
    public class Normal : NonUniform
    {
        /// <summary>
        /// The default mean value.
        /// </summary>
        public const double DefaultMean = 0.0;
        /// <summary>
        /// The default standard deviation.
        /// </summary>
        public const double DefaultStandardDeviation = 1.0;

        /// <summary>
        /// The mean.  Also often referred to as <em>mu</em>.
        /// </summary>
        private double _mean;   // mu
        /// <summary>
        /// The standard deviation.  Also often referred to as <em>sigma</em>.
        /// </summary>
        private double _stdev;  // sigma
        /// <summary>
        /// The cached next random value.
        /// </summary>
        private double _next = Double.NaN;

        /// <overloads>
        /// Create and initialize a Normal random number generator.
        /// </overloads>
        /// <summary>
        /// Create a <see cref="Normal"/> random number generator.
        /// </summary>
        /// <remarks>
        /// The <see cref="Mean"/> and <see cref="StandardDeviation"/> for the
        /// <see cref="Normal"/> are set to <see cref="DefaultMean"/> and
        /// <see cref="DefaultStandardDeviation"/> respectively.  The
        /// underlying <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        public Normal()
            : this(DefaultMean, DefaultStandardDeviation)
        {
        }

        /// <summary>
        /// Create a <see cref="Normal"/> random number generator that
        /// has the given mean and standard deviation.
        /// </summary>
        /// <remarks>
        /// The underlying <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        /// <param name="mean">
        /// The mean.  This value is often referred to as <em>mu</em>.
        /// </param>
        /// <param name="stddev">
        /// The standard deviation.  This value is often referred to as
        /// <em>sigma</em>.
        /// </param>
        public Normal(double mean, double stddev)
        {
            this.Mean = mean;
            this.StandardDeviation = stddev;
        }

        /// <summary>
        /// Create a <see cref="Normal"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Mean"/> and <see cref="StandardDeviation"/> for the
        /// <see cref="Normal"/> are set to <see cref="DefaultMean"/> and
        /// <see cref="DefaultStandardDeviation"/> respectively.
        /// </remarks>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Exponential"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        public Normal(IUniformSource source)
            : this(source, DefaultMean, DefaultStandardDeviation)
        {
        }

        /// <summary>
        /// Create a <see cref="Normal"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/> and has the given mean and
        /// standard deviation.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Exponential"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        /// <param name="mean">
        /// The mean.  This value is often referred to as <em>mu</em>.
        /// </param>
        /// <param name="stddev">
        /// The standard deviation.  This value is often referred to as
        /// <em>sigma</em>.
        /// </param>
        public Normal(IUniformSource source, double mean, double stddev)
            : base(source)
        {
            this.Mean = mean;
            this.StandardDeviation = stddev;
        }

        /// <summary>
        /// Gets or sets the mean (average) for the <see cref="Normal"/>
        /// distribution.
        /// </summary>
        /// <remarks>
        /// This property is often referred to as <em>mu</em>.
        /// </remarks>
        /// <value>
        /// The mean as a <see cref="double"/>.
        /// </value>
        public double Mean
        {
            get { return _mean; }
            set { _mean = value; }
        }

        /// <summary>
        /// Gets or sets the standard deviation for the <see cref="Normal"/>
        /// distribution.
        /// </summary>
        /// <remarks>
        /// This property is often referred to as <em>sigma</em>.
        /// </remarks>
        /// <value>
        /// The standard deviation as a <see cref="double"/>.
        /// </value>
        public double StandardDeviation
        {
            get { return _stdev; }
            set { _stdev = value; }
        }

        /// <summary>
        /// Generates the next random value according to a normal
        /// distribution.
        /// </summary>
        /// <returns>
        /// The next random value.
        /// </returns>
        public override double NextDouble()
        {
            return Normal.Generate(GetUniform(), Mean, StandardDeviation, ref _next);
        }

        /// <summary>
        /// Static method used by both <see cref="Normal"/> and
        /// <see cref="Lognormal"/> to generate normally distributed pseudo
        /// random values.
        /// </summary>
        /// <param name="u">The uniform generator.</param>
        /// <param name="mu">The mean.</param>
        /// <param name="sigma">The standard deviation.</param>
        /// <param name="next">The cached next value.</param>
        /// <returns>
        /// A pseudo random value that is normally distributed.
        /// </returns>
        internal static double Generate(IUniform u, double mu, double sigma,
            ref double next)
        {
            double z = next;
            if (Double.IsNaN(z))
            {
                double x2pi = u.NextDouble() * Math.PI * 2.0;
                z = 1.0 - u.NextDouble();
                double g2rad = Math.Sqrt(-2.0 * Math.Log(z));
                z = Math.Cos(x2pi) * g2rad;
                next = Math.Sin(x2pi) * g2rad;
            }
            else
            {
                next = Double.NaN;
            }

            return mu + z * sigma;
        }
    }
}
