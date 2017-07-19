//=============================================================================
//=  $Id: Statistic.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections.Generic;

namespace React.Monitoring
{
    /// <summary>
    /// A <see cref="Monitor"/> that computes a simple, non-weighted summary
    /// statistical value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All of the simple, non-weighted and non-time-dependent summary
    /// statistics classes in React.NET are derived from
    /// <see cref="Statistic"/>.
    /// </para>
    /// <para>
    /// <see cref="Statistic"/> instances are designed to be used to monitor
    /// value changes on properties or to be used to compute summary
    /// statistical values on arbitrary sets of observations.
    /// </para>
    /// </remarks>
    public abstract class Statistic : Monitor
    {
        /// <summary>The number of observations.</summary>
        private int _nObservations;

        /// <summary>
        /// Create a new <see cref="Statistic"/> instance.
        /// </summary>
        protected Statistic()
        {
        }

        /// <summary>
        /// Gets or sets the number of observations.
        /// </summary>
        /// <value>
        /// The number of observations as an <see cref="int"/>.
        /// </value>
        public int Observations
        {
            get { return _nObservations; }
            protected set { _nObservations = value; }
        }

        /// <summary>
        /// Make an observation of a single <see cref="double"/> value.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is a <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/>, or
        /// <see cref="Double.NegativeInfinity"/>.
        /// </exception>
        /// <param name="value">
        /// The value to observe.
        /// </param>
        public virtual void Observe(double value)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                throw new ArgumentException(
                    "Invalid observation, " + value, "value");
            }
            _nObservations++;
        }

        /// <summary>
        /// Iterate through each <see cref="double"/> value in the given
        /// <see cref="IEnumerable&lt;T&gt;"/> observing each one.
        /// </summary>
        /// <remarks>
        /// This method will invoke <see cref="Observe(double)"/> for each
        /// <see cref="double"/> in <paramref name="values"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If any of the values in <paramref name="values"/> is a
        /// <see cref="Double.NaN"/>, <see cref="Double.PositiveInfinity"/>, or
        /// <see cref="Double.NegativeInfinity"/>.
        /// </exception>
        /// <param name="values">
        /// The values to iterate over and observe.
        /// </param>
        /// <returns>
        /// The value of the <see cref="Statistic"/> after observing each
        /// <see cref="double"/> in <paramref name="values"/>.
        /// </returns>
        public double Observe(IEnumerable<double> values)
        {
            foreach (double d in values)
            {
                Observe(d);
            }

            return Value;
        }

        /// <summary>
        /// Iterate through each <see cref="double"/> value in the given
        /// <see cref="System.Collections.IEnumerable"/> observing each one.
        /// </summary>
        /// <remarks>
        /// This method will invoke <see cref="Observe(double)"/> for each
        /// object in <paramref name="values"/>.  Each object must be
        /// convertable to a <see cref="double"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If any of the values in <paramref name="values"/> is a
        /// <see cref="Double.NaN"/>, <see cref="Double.PositiveInfinity"/>, or
        /// <see cref="Double.NegativeInfinity"/>.
        /// </exception>
        /// <param name="values">
        /// The values to iterate over and observe.
        /// </param>
        /// <returns>
        /// The value of the <see cref="Statistic"/> after observing each
        /// <see cref="double"/> in <paramref name="values"/>.
        /// </returns>
        public double Observe(System.Collections.IEnumerable values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            foreach (object obj in values)
            {
                double d = Convert.ToDouble(obj);
                Observe(d);
            }

            return Value;
        }
        
        /// <summary>
        /// Gets the current value of the <see cref="Statistic"/>.
        /// </summary>
        /// <value>
        /// The current value as a <see cref="double"/>.
        /// </value>
        public abstract double Value
        {
            get;
        }

        /// <summary>
        /// Begin monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method simply invokes
        /// </para>
        /// <code>Attach(target, propertyName, OnValueChanged);</code>
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property of <paramref name="target"/> to monitor.
        /// </param>
        public override void Attach(object target, string propertyName)
        {
            Attach(target, propertyName, OnValueChanged);
        }

        /// <summary>
        /// End monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method simply invokes
        /// </para>
        /// <code>Detach(target, propertyName, OnValueChanged);</code>
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will stop being monitored.</param>
        /// <param name="propertyName">
        /// The name of a property of <paramref name="target"/> to stop
        /// monitoring.
        /// </param>
        public override void Detach(object target, string propertyName)
        {
            Detach(target, propertyName, OnValueChanged);
        }

        /// <summary>
        /// Event handler for property change notifications.
        /// </summary>
        /// <remarks>
        /// This method simply attempts to convert <c>args.NewValue</c> to
        /// a <see cref="double"/> and then call <see cref="Observe(double)"/>.
        /// </remarks>
        /// <param name="sender">
        /// The object whose property has changed.
        /// </param>
        /// <param name="args">
        /// The <see cref="ValueChangedEventArgs"/> providing additional
        /// information about the property change.  Only <c>args.NewValue</c>
        /// is used.
        /// </param>
        protected virtual void OnValueChanged(object sender,
            ValueChangedEventArgs args)
        {
            double value = Convert.ToDouble(args.NewValue);
            Observe(value);
        }
    }
}
