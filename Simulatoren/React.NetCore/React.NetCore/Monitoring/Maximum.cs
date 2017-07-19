//=============================================================================
//=  $Id: Maximum.cs 158 2006-08-30 11:49:06Z eroe $
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
    /// A <see cref="Statistic"/> that computes the running maximum value of a
    /// series of observations.
    /// <seealso cref="Minimum"/>
    /// </summary>
    public class Maximum : Statistic
    {
        /// <summary>The current maximum value.</summary>
        private double _max;

        /// <overloads>
        /// Create and initialize a Maximum statictic.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="Maximum"/> statistic.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Maximum"/> instance using this constructor, the returned
        /// value is <see cref="Double.NaN"/>.
        /// </remarks>
        public Maximum() : this(Double.NaN)
        {
        }

        /// <summary>
        /// Create a new <see cref="Maximum"/> statistic specifying the initial
        /// observation.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Maximum"/> instance using this constructor, the returned
        /// value is <paramref name="initialObservation"/>.
        /// </remarks>
        /// <param name="initialObservation">
        /// The value of the initial observation.
        /// </param>
        public Maximum(double initialObservation)
        {
            _max = initialObservation;
            if (!Double.IsNaN(_max))
                Observations++;
        }

        /// <summary>
        /// Gets the current value of the <see cref="Maximum"/> statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current maximum observed value as a <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get { return _max; }
        }

        /// <summary>
        /// Record an observation of the specified value.
        /// </summary>
        /// <remarks>
        /// Observed values are not stored by the <see cref="Maximum"/>
        /// instance, they are only used to compute the running maximum.
        /// </remarks>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public override void Observe(double value)
        {
            base.Observe(value);
            if (Double.IsNaN(_max))
                _max = value;
            else
                _max = Math.Max(_max, value);
        }
    }
}
