//=============================================================================
//=  $Id: TimeWeightedStatistic.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// A <see cref="Monitor"/> that computes a time-weighted summary
    /// statistical value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All of the time-weighted summary statistics classes in React.NET are
    /// derived from <see cref="TimeWeightedStatistic"/>.
    /// </para>
    /// <para>
    /// <see cref="TimeWeightedStatistic"/> instance are designed to be used
    /// to monitor value changes on properties over time or to be used to
    /// compute summary statistical values on arbitrary sets of observations.
    /// </para>
    /// <para>
    /// Note that <see cref="TimeWeightedStatistic"/> is an extension of
    /// <see cref="Statistic"/>.  Care must be used when calling certain of
    /// the <see cref="Statistic"/> methods that do not account for time. An
    /// example is <see cref="Statistic.Observe(System.Collections.IEnumerable)"/>,
    /// which, when called on a <see cref="TimeWeightedStatistic"/>, will
    /// make all observations <em>at the same time</em>.  This behavior may
    /// not be what is desired.  Keep this fact in mind when calling methods
    /// that do not take a time parameter.
    /// </para>
    /// </remarks>
    public abstract class TimeWeightedStatistic : Statistic
    {
        /// <summary>
        /// The simulation context used to obtain the current time.
        /// </summary>
        private Simulation _sim;
        /// <summary>
        /// Whether or not an observation is made when attached to a
        /// monitorable property.
        /// </summary>
        private bool _obsOnAttach = true;
        /// <summary>
        /// Whether or not an observation is made when detached from a
        /// monitorable property.
        /// </summary>
        private bool _obsOnDetach = true;
        /// <summary>
        /// The time and value of the last observation that was made.
        /// </summary>
        private TimeValue _lastObservation = TimeValue.Invalid;

        /// <summary>
        /// Create a new <see cref="TimeWeightedStatistic"/> that will obtain
        /// the current time from the specified <see cref="Simulation"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="sim"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="sim">
        /// The <see cref="Simulation"/> from which the current simulation
        /// time can be obtained.
        /// </param>
        protected TimeWeightedStatistic(Simulation sim)
        {
            if (sim == null)
                throw new ArgumentNullException(
                    "sim", "Simulation cannot be null");
            _sim = sim;
        }

        /// <summary>
        /// Gets the simulation context under which the
        /// <see cref="TimeWeightedStatistic"/> is observing value changes.
        /// </summary>
        /// <value>
        /// The simulation context as a <see cref="Simulation"/>.
        /// </value>
        public Simulation Simulation
        {
            get { return _sim; }
        }

        /// <summary>
        /// Gets or sets whether an observation is automatically made when
        /// attached to a monitorable property.
        /// </summary>
        /// <remarks>
        /// By default, this property is <b>true</b>.
        /// </remarks>
        /// <value>
        /// <b>true</b> if an observation is automatically made when attached
        /// to a monitorable property.
        /// </value>
        public bool ObserveOnAttach
        {
            get { return _obsOnAttach; }
            set { _obsOnAttach = value; }
        }

        /// <summary>
        /// Gets or sets whether an observation is automatically made when
        /// detached from a monitorable property.
        /// </summary>
        /// <remarks>
        /// By default, this property is <b>true</b>.
        /// </remarks>
        /// <value>
        /// <b>true</b> if an observation is automatically made when detached
        /// from a monitorable property.
        /// </value>
        public bool ObserveOnDetach
        {
            get { return _obsOnDetach; }
            set { _obsOnDetach = value; }
        }

        /// <summary>
        /// Gets a <see cref="TimeValue"/> describing the last observation.
        /// </summary>
        /// <remarks>
        /// If no observations have yet been made, the returned
        /// <see cref="TimeValue"/> is <see cref="TimeValue.Invalid"/>.
        /// </remarks>
        /// <value>
        /// The last observation as a <see cref="TimeValue"/>.
        /// </value>
        public TimeValue LastObservation
        {
            get { return _lastObservation; }
        }

        /// <summary>
        /// Make an observation of a single <see cref="double"/> value at the
        /// current simulation time.
        /// </summary>
        /// <remarks>
        /// This method simply invokes <see cref="Observe(double,long)"/>,
        /// obtaining the current simulation time <see cref="Simulation"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is a <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/>, or
        /// <see cref="Double.NegativeInfinity"/>.
        /// </exception>
        /// <param name="value">
        /// The value to observe at <see cref="React.Simulation.Now"/>.
        /// </param>
        public override sealed void Observe(double value)
        {
            Observe(value, _sim.Now);
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
        public virtual void Observe(double value, long time)
        {
            // All the call to the base class does is increment the number of
            // observations and check that 'value' is valid.
            base.Observe(value);
            _lastObservation = new TimeValue(time, value);
        }

        /// <summary>
        /// Event handler for property change notifications.
        /// </summary>
        /// <remarks>
        /// This method simply attempts to convert <c>args.NewValue</c> to
        /// a <see cref="double"/> and then call
        /// <see cref="Observe(double,long)"/>.  The time is obtained from
        /// <paramref name="args"/> and failing that (e.g. <c>args.Time</c> is
        /// less than zero), the time is obtained from <see cref="Simulation"/>.
        /// </remarks>
        /// <param name="sender">
        /// The object whose property has changed.
        /// </param>
        /// <param name="args">
        /// The <see cref="ValueChangedEventArgs"/> providing additional
        /// information about the property change.  Only <c>args.NewValue</c>
        /// and <c>args.Time</c> are used.
        /// </param>
        protected override void OnValueChanged(object sender,
            ValueChangedEventArgs args)
        {
            ObserveObject(args.NewValue, args.Time);
        }

        /// <summary>
        /// Begin monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method simply invokes
        /// </para>
        /// <code>Attach(target, propertyName, OnValueChanged);</code>
        /// <para>
        /// and then, if <see cref="ObserveOnAttach"/> is <b>true</b> makes an
        /// initial observation of the monitored property's value.
        /// </para>
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
            object obj = Attach(target, propertyName, OnValueChanged);
            if (ObserveOnAttach)
            {
                ObserveObject(obj, -1L);
            }
        }

        /// <summary>
        /// End monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method simply invokes
        /// </para>
        /// <code>Detach(target, propertyName, OnValueChanged);</code>
        /// <para>
        /// and then, if <see cref="ObserveOnDetach"/> is <b>true</b> makes a
        /// final observation of the monitored property's value.
        /// </para>
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
            object obj = Detach(target, propertyName, OnValueChanged);
            if (ObserveOnDetach)
            {
                ObserveObject(obj, -1L);
            }
        }

        /// <summary>
        /// Make an observation of the supplied <see cref="Object"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="Object"/> to observe.  Must be convertible to a
        /// <see cref="double"/>.
        /// </param>
        /// <param name="time">
        /// The time at which the observation is made.  If less than zero,
        /// the time is obtained from the <see cref="Simulation"/> supplied
        /// when the <see cref="TimeWeightedStatistic"/> was created.
        /// </param>
        private void ObserveObject(object value, long time)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            double d = Convert.ToDouble(value);
            Observe(d, time >= 0L ? time : _sim.Now);
        }
    }
}
