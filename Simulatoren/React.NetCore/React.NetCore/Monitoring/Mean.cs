//=============================================================================
//=  $Id: Mean.cs 158 2006-08-30 11:49:06Z eroe $
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
    /// A <see cref="Statistic"/> that computes the running mean (average)
    /// value of a series of observations.
    /// </summary>
    public class Mean : Statistic
    {
        /// <summary>Sum of observations.</summary>
        private double _accum;

        /// <overloads>
        /// Create and initialize a Mean statictic.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="Mean"/> statistic.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating
        /// a <see cref="Mean"/> instance using this constructor, the returned
        /// value is <see cref="Double.NaN"/>.
        /// </remarks>
        public Mean() : this(Double.NaN)
        {
        }

        /// <summary>
        /// Create a new <see cref="Mean"/> statistic specifying the initial
        /// observation.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating
        /// a <see cref="Mean"/> instance using this constructor, the returned
        /// value is <paramref name="initialObservation"/>.
        /// </remarks>
        /// <param name="initialObservation">
        /// The value of the initial observation.
        /// </param>
        public Mean(double initialObservation)
        {
            _accum = initialObservation;
            if (!Double.IsNaN(_accum))
                Observations++;
        }

        /// <summary>
        /// Gets the current value of the <see cref="Mean"/> statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current mean (average) of the observations as a
        /// <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get
            {
                if (Observations > 0 && !Double.IsNaN(_accum))
                    return _accum / Observations;
                return Double.NaN;
            }
        }

        /// <summary>
        /// Record an observation of the specified value.
        /// </summary>
        /// <remarks>
        /// Observed values are not stored by the <see cref="Mean"/> instance,
        /// they are only used to compute the running mean.
        /// </remarks>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public override void Observe(double value)
        {
            base.Observe(value);
            if (Double.IsNaN(_accum))
                _accum = value;
            else
                _accum += value;
        }
    }
}
