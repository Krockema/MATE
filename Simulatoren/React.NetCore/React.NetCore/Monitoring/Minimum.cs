//=============================================================================
//=  $Id: Minimum.cs 158 2006-08-30 11:49:06Z eroe $
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
    /// A <see cref="Statistic"/> that computes the running minimum value of a
    /// series of observations.
    /// <seealso cref="Maximum"/>
    /// </summary>
    public class Minimum : Statistic
    {
        /// <summary>The current minimum value.</summary>
        private double _min;

        /// <overloads>
        /// Create and initialize a Minimum statictic.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="Minimum"/> statistic.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Minimum"/> instance using this constructor, the returned
        /// value is <see cref="Double.NaN"/>.
        /// </remarks>
        public Minimum() : this(Double.NaN)
        {
        }

        /// <summary>
        /// Create a new <see cref="Minimum"/> statistic specifying the initial
        /// observation.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="Minimum"/> instance using this constructor, the returned
        /// value is <paramref name="initialObservation"/>.
        /// </remarks>
        /// <param name="initialObservation">
        /// The value of the initial observation.
        /// </param>
        public Minimum(double initialObservation)
        {
            _min = initialObservation;
            if (!Double.IsNaN(_min))
                Observations++;
        }

        /// <summary>
        /// Gets the current value of the <see cref="Minimum"/> statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current minimum observed value as a <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get { return _min; }
        }

        /// <summary>
        /// Record an observation of the specified value.
        /// </summary>
        /// <remarks>
        /// Observed values are not stored by the <see cref="Minimum"/>
        /// instance, they are only used to compute the running minimum.
        /// </remarks>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public override void Observe(double value)
        {
            base.Observe(value);
            if (Double.IsNaN(_min))
                _min = value;
            else
                _min = Math.Min(_min, value);
        }
    }
}
