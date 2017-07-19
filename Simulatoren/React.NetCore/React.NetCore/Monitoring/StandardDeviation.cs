//=============================================================================
//=  $Id: StandardDeviation.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// A <see cref="Statistic"/> that computes the running standard
    /// deviation of a series of observations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="StandardDeviation"/> simply makes use of an internal
    /// <see cref="Variance"/> instance to perform all calculations.
    /// </para>
    /// <para>
    /// The <see cref="StandardDeviation"/> can compute both the
    /// <em>population</em> and <em>sample</em> standard deviation.  By default
    /// it computes the population standard deviation.  Use the
    /// <see cref="IsSample"/> property to switch to computing the sample
    /// standard deviation.
    /// </para>
    /// </remarks>
    public class StandardDeviation : Statistic
    {
        /// <summary>
        /// <see cref="Variance"/> instance used to do all the computing.
        /// </summary>
        private Variance _variance;

        /// <overloads>
        /// Create and initialize a StandardDeviation statistic.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="StandardDeviation"/> statistic.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="StandardDeviation"/> instance using this constructor,
        /// the returned value is <see cref="Double.NaN"/>.
        /// </remarks>
        public StandardDeviation()
        {
            _variance = new Variance();
        }

        /// <summary>
        /// Create a new <see cref="StandardDeviation"/> statistic specifying
        /// the initial observation.
        /// </summary>
        /// <remarks>
        /// If <see cref="Value"/> is invoked immediately after instantiating a
        /// <see cref="StandardDeviation"/> instance using this constructor, the
        /// returned value is zero (0.0).
        /// </remarks>
        /// <param name="initialObservation">
        /// The value of the initial observation.
        /// </param>
        public StandardDeviation(double initialObservation)
        {
            _variance = new Variance(initialObservation);
            Observations = _variance.Observations;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="StandardDeviation"/> is
        /// computing a sample standard deviation.
        /// </summary>
        /// <value>
        /// <b>true</b> when computing the sample standard deviation; or
        /// <b>false</b> when computing the population standard deviation.
        /// </value>
        public bool IsSample
        {
            get { return _variance.IsSample; }
            set { _variance.IsSample = value; }
        }

        /// <summary>
        /// Gets the variance based on the current value of the internal
        /// <see cref="React.Monitoring.Variance"/> instance used to perform
        /// all the computation for the <see cref="StandardDeviation"/>.
        /// <seealso cref="React.Monitoring.Variance.Value"/>
        /// </summary>
        /// <remarks>
        /// If there have been no observations, this property will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The variance of observed values.
        /// </value>
        public double Variance
        {
            get { return _variance.Value; }
        }

        /// <summary>
        /// Gets the current value of the <see cref="StandardDeviation"/>
        /// statistic.
        /// </summary>
        /// <remarks>
        /// If there have been no observations, <see cref="Value"/> will be
        /// <see cref="Double.NaN"/>.
        /// </remarks>
        /// <value>
        /// The current standard deviation observed value as a
        /// <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get { return _variance.StandardDeviation; }
        }

        /// <summary>
        /// Record an observation of the specified value.
        /// </summary>
        /// <remarks>
        /// Observed values are not stored by the
        /// <see cref="StandardDeviation"/> instance, they are only used to
        /// compute the running standard deviation.
        /// </remarks>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public override void Observe(double value)
        {
            base.Observe(value);
            _variance.Observe(value);
        }
    }
}
