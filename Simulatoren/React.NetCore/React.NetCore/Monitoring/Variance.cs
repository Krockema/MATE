//=============================================================================
//=  $Id: Variance.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React.Monitoring
{
    /// <summary>
    /// A <see cref="Statistic"/> that computes the running variance
    /// of a series of observations.
    /// <seealso cref="StandardDeviation"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The variance is computed using the sum of squares method.
    /// </para>
    /// <para>
    /// The <see cref="Variance"/> can compute both <em>population</em> and
    /// <em>sample</em> variances.  By default it computes the population
    /// variance.  Use the <see cref="IsSample"/> property to switch to
    /// computing the sample variance.
    /// </para>
    /// </remarks>
    public class Variance : Statistic
    {
        /// <summary>Sum of observations.</summary>
        private double _accum;
        /// <summary>The sum of squared observations.</summary>
        private double _sumOfSquares;
        /// <summary>Bias if computing variance of a sample.</summary>
        /// <remarks>This value must be either 0.0 or 1.0.</remarks>
        private double _sampleBias;

        /// <overloads>
        /// Create and initialize a Variance statistic.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="Variance"/> statistic.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Variance"/> instance using this constructor, the returned
        /// value is <see cref="Double.NaN"/>.
        /// </remarks>
        public Variance() : this(Double.NaN)
        {
        }

        /// <summary>
        /// Create a new <see cref="Variance"/> statistic specifying the initial
        /// observation.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Variance"/> instance using this constructor, the returned
        /// value is zero (0.0).
        /// </remarks>
        /// <param name="initialObservation">
        /// The value of the initial observation.
        /// </param>
        public Variance(double initialObservation)
        {
            if (!Double.IsNaN(initialObservation))
            {
                Observations++;
                UpdateVariance(initialObservation);
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="Variance"/> is computing a
        /// sample variance.
        /// </summary>
        /// <value>
        /// <b>true</b> when computing the sample variance; or <b>false</b>
        /// when computing the population variance.
        /// </value>
        public bool IsSample
        {
            get { return _sampleBias > 0.0; }
            set { _sampleBias = value ? 1.0 : 0.0; }
        }

        /// <summary>
        /// Gets the standard deviation based on the current value of the
        /// <see cref="Variance"/>.
        /// <seealso cref="React.Monitoring.StandardDeviation"/>
        /// </summary>
        /// <remarks>
        /// If there have been no observations, this property will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The standard deviation of observed values computed using the
        /// current <see cref="Value"/> of the <see cref="Variance"/>.
        /// </value>
        public double StandardDeviation
        {
            get { return Observations > 0 ? Math.Sqrt(Value) : Double.NaN; }
        }

        /// <summary>
        /// Gets the current value of the <see cref="Variance"/> statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current variance of observed values as a <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get
            {
                double result;
                int n = Observations;
                if (n > 1)
                {
                    result = (_sumOfSquares - (_accum * _accum) / n) /
                        (n - _sampleBias);
                }
                else
                {
                    result = (n == 1) ? 0.0 : Double.NaN;
                }

                return result;
            }
        }

        /// <summary>
        /// Record an observation of the specified value.
        /// </summary>
        /// <remarks>
        /// Observed values are not stored by the <see cref="Variance"/>
        /// instance, they are only used to compute the running variance.
        /// </remarks>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public override void Observe(double value)
        {
            base.Observe(value);
            UpdateVariance(value);
        }

        /// <summary>
        /// Update the internal sum and sum-of-squares values based on the
        /// given observed value.
        /// </summary>
        /// <param name="value">The observed value.</param>
        private void UpdateVariance(double value)
        {
            System.Diagnostics.Debug.Assert(!Double.IsNaN(value));
            _accum += value;
            _sumOfSquares += (value * value);
        }
    }
}
