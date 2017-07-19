//=============================================================================
//=  $Id: TimeWeightedVariance.cs 158 2006-08-30 11:49:06Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2006, Eric K. Roe.  All rights reserved.
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
using System.Collections.Generic;
using System.Text;

namespace React.Monitoring
{
    /// <summary>
    /// A <see cref="TimeWeightedStatistic"/> monitor that computes the running
    /// time-weighted variance of a series of observations.
    /// <seealso cref="TimeWeightedStandardDeviation"/>
    /// </summary>
    public class TimeWeightedVariance : TimeWeightedStatistic
    {
        /// <summary>Sum of weighted observations.</summary>
        private double _weightedObs;
        /// <summary>Sum of the weight times the square of the observation.</summary>
        private double _sumOfWeightedSquares;
        /// <summary>Sum of the weights.</summary>
        private double _sumOfWeights;
        /// <summary>Bias if computing variance of a sample.</summary>
        /// <remarks>This value must be either 0.0 or 1.0.</remarks>
        private double _sampleBias;

        /// <summary>
        /// Create a new <see cref="TimeWeightedVariance"/> that will obtain
        /// the current time from the specified <see cref="Simulation"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="sim"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="sim">
        /// The <see cref="Simulation"/> from which the current simulation
        /// time can be obtained.
        /// </param>
        public TimeWeightedVariance(Simulation sim)
            : base(sim)
        {
        }

        /// <summary>
        /// Gets or sets whether the <see cref="TimeWeightedVariance"/> is computing a
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
        /// Gets the time-weighted standard deviation based on the current
        /// value of the <see cref="TimeWeightedVariance"/>.
        /// <seealso cref="React.Monitoring.StandardDeviation"/>
        /// </summary>
        /// <remarks>
        /// If there have been no observations, this property will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The time-weighted standard deviation of observed values computed
        /// using the current <see cref="Value"/> of the
        /// <see cref="TimeWeightedVariance"/>.
        /// </value>
        public double StandardDeviation
        {
            get { return Observations > 0 ? Math.Sqrt(Value) : Double.NaN; }
        }

        /// <summary>
        /// Gets the current value of the <see cref="TimeWeightedVariance"/>
        /// statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current time-weighted variance of the observations as a
        /// <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get
            {
                System.Diagnostics.Debug.Assert(
                    _sampleBias == 0.0 || _sampleBias == 1.0);

                double result;
                if (Observations > 1)
                {
                    if (_sumOfWeights > 0.0)
                    {
                        double numerator = _sumOfWeightedSquares -
                            Math.Pow(_weightedObs, 2.0) / _sumOfWeights;
                        double denominator = ((Observations - _sampleBias) / Observations) *
                            _sumOfWeights;
                        result = denominator > 0.0 ? numerator / denominator : Double.NaN;
                    }
                    else
                    {
                        result = Double.NaN;
                    }
                }
                else
                {
                    result = Observations == 0 ? Double.NaN : 0.0;
                }

                return result;
            }
        }

        /// <summary>
        /// Make an observation of a single <see cref="double"/> value at the
        /// specified simulation time.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is a <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/>, or
        /// <see cref="Double.NegativeInfinity"/>.
        /// </exception>
        /// <param name="value">
        /// The value observed at <paramref name="time"/>.
        /// </param>
        /// <param name="time">
        /// The simulation time at which <paramref name="value"/> was observed.
        /// </param>
        public override void Observe(double value, long time)
        {
            TimeValue last = LastObservation;
            base.Observe(value, time);
            if (last.IsValid)
            {
                long duration = time - last.Time;
                _sumOfWeightedSquares += duration * Math.Pow(last.Value, 2.0);
                _weightedObs += (double)duration * last.Value;
                _sumOfWeights += duration;
            }
        }
    }
}
