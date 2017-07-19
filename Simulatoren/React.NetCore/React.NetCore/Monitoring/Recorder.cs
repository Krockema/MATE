//=============================================================================
//=  $Id: Recorder.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Diagnostics.CodeAnalysis;

namespace React.Monitoring
{
    /// <summary>
    /// A <see cref="Monitor"/> implementation that records (stores) all
    /// changes to the monitored property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Recorder"/> stores each property change as a
    /// <see cref="TimeValue"/>, which provides the simulation time that
    /// the property changed as well as the associated value.
    /// </para>
    /// <para>
    /// Data is stored in the <see cref="Recorder"/> in the order it was
    /// received.  Normally, this means data is ordered by simulation
    /// time.
    /// </para>
    /// </remarks>
    [
        SuppressMessage("Microsoft.Naming",
            "CA1710:IdentifiersShouldHaveCorrectSuffix")
    ]
    public class Recorder : Monitor, IEnumerable<TimeValue>
    {
        /// <summary>The backing data store.</summary>
        private ICollection<TimeValue> _data;

        /// <summary>
        /// Create an empty <see cref="Recorder"/> instance.
        /// </summary>
        /// <remarks>
        /// The <see cref="Recorder"/> will use an <see cref="List&lt;T&gt;"/>
        /// to store <see cref="TimeValue"/> instances.
        /// </remarks>
        public Recorder() : this(new List<TimeValue>())
        {
        }

        /// <summary>
        /// Create a new <see cref="Recorder"/> that stores observations in the
        /// given <see cref="ICollection&lt;T&gt;"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="dataStore"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="dataStore">
        /// The collection used to store observations.
        /// </param>
        public Recorder(ICollection<TimeValue> dataStore)
        {
            this.Data = dataStore;
        }

        /// <summary>
        /// Gets or sets the <see cref="ICollection&lt;T&gt;"/> used to store
        /// observations.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If an attempt is made to set this property to
        /// <see langword="null"/>.
        /// </exception>
        /// <value>
        /// The data store as an <b>ICollection&lt;</b>
        /// <see cref="TimeValue"/><b>&gt;</b>
        /// </value>
        public ICollection<TimeValue> Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _data = value;
            }
        }

        /// <summary>
        /// Attach the <see cref="Recorder"/> to an object's property by name.
        /// </summary>
        /// <remarks>
        /// Attaching a <see cref="Recorder"/> to a property starts the
        /// <see cref="Recorder"/> receiving <see cref="ValueChanged"/>
        /// notifications.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="target"/> to monitor.
        /// </param>
        public override void Attach(object target, string propertyName)
        {
            Attach(target, propertyName, ValueChanged);
        }

        /// <summary>
        /// Detach the <see cref="Recorder"/> from an object's property by name.
        /// </summary>
        /// <remarks>
        /// Detaching a <see cref="Recorder"/> from a property stops the
        /// <see cref="Recorder"/> receiving <see cref="ValueChanged"/>
        /// notifications.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/> is
        /// to stop being monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="obj"/> to stop
        /// monitoring.
        /// </param>
        public override void Detach(object target, string propertyName)
        {
            Detach(target, propertyName, ValueChanged);
        }

        #region IEnumerable and IEnumerable<TimeValue> Members

        /// <summary>
        /// Returns the enumerator that allows iteration over the recorded
        /// <see cref="TimeValue"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator&lt;T&gt;"/> that allows iteration over the
        /// recorded <see cref="TimeValue"/> instances.
        /// </returns>
        public IEnumerator<TimeValue> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// Returns the enumerator that allows iteration over the recorded
        /// <see cref="TimeValue"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> that allows
        /// iteration over the recorded <see cref="TimeValue"/> instances.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)_data.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Clears all recorded items from the <see cref="Recorder"/>.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// Gets the number of recorded <see cref="TimeValue"/> instances.
        /// </summary>
        /// <value>
        /// The number of recorded items as an <see cref="int"/>.
        /// </value>
        public int Count
        {
            get { return _data.Count; }
        }

        /// <summary>
        /// Delegate method that handles <see cref="ValueChanged"/>
        /// notifications.
        /// </summary>
        /// <remarks>
        /// This method creates a new <see cref="TimeValue"/> and adds it
        /// to the collection.  The <see cref="TimeValue"/> records the
        /// observation time and value.
        /// </remarks>
        /// <param name="sender">The message sender.</param>
        /// <param name="args">
        /// Information about the <see cref="ValueChanged"/> notification.
        /// </param>
        private void ValueChanged(object sender, ValueChangedEventArgs args)
        {
            double value = Convert.ToDouble(args.NewValue);
            TimeValue tv = new TimeValue(args.Time, value);
            Data.Add(tv);
        }
    }
}
