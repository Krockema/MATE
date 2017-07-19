//=============================================================================
//=  $Id: TimeWeightedMean.cs 158 2006-08-30 11:49:06Z eroe $
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
    /// A <see cref="TimeWeightedStatistic"/> that computes the running
    /// time-weighted mean (average) value of a series of observations.
    /// </summary>
    public class TimeWeightedMean : TimeWeightedStatistic
    {
        /// <summary>Sum of weighted observations.</summary>
        private double _weightedObs;
        /// <summary>Sum of weights.</summary>
        private double _sumOfWeights;

        /// <summary>
        /// Create a new <see cref="TimeWeightedMean"/> that will obtain
        /// the current time from the specified <see cref="Simulation"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="sim"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="sim">
        /// The <see cref="Simulation"/> from which the current simulation
        /// time can be obtained.
        /// </param>
        public TimeWeightedMean(Simulation sim)
            : base(sim)
        {
        }

        /// <summary>
        /// Gets the current value of the <see cref="TimeWeightedMean"/>
        /// statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current time-weighted mean (average) of the observations as a
        /// <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get
            {
                if (Observations < 1 || _sumOfWeights == 0)
                {
                    return Double.NaN;
                }

                return _weightedObs / _sumOfWeights;
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
                _sumOfWeights += duration;
                _weightedObs += (double)duration * last.Value;
            }
        }
    }
}
