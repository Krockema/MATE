//=============================================================================
//=  $Id: MonitorUsingAttribute.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// Marks a property as <em>monitorable</em> using the specified value
    /// change event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A monitorable property has an associated value changed event, an
    /// event whose handler delegate is of the type <b>EventHandler&lt;</b>
    /// <see cref="ValueChangedEventArgs"/>&gt;.  The value change event gets
    /// raised every time the associated property's value is updated.
    /// A <see cref="Monitor"/> can be added to the event as a handler and is
    /// thereby be able to respond to changes in the monitored property.
    /// </para>
    /// <para>
    /// <see cref="MonitorUsingAttribute"/> can only be placed on a property.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MonitorUsingAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the event associated with the monitorable property.
        /// </summary>
        private string _eventName;

        /// <summary>
        /// Create a new <see cref="MonitorUsingAttribute"/> attribute
        /// instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="eventName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="eventName"/> is empty (zero length).
        /// </exception>
        /// <param name="eventName">
        /// The name of the value change event that is associated with the
        /// monitorable property.
        /// </param>
        public MonitorUsingAttribute(string eventName)
        {
            if (eventName == null)
                throw new ArgumentNullException("eventName");
            if (eventName.Length < 1)
                throw new ArgumentException("Event name was empty.",
                    "eventName");

            _eventName = eventName;
        }

        /// <summary>
        /// Gets the name of the value change event that is associated with
        /// the monitorable property.
        /// </summary>
        /// <value>
        /// The event name as a <see cref="string"/>.
        /// </value>
        public string EventName
        {
            get { return _eventName; }
        }
    }
}
