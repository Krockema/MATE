//=============================================================================
//=  $Id: ValueChanged.cs 164 2006-09-03 22:12:23Z eroe $
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
    /// Event data passed to a value changed handler.
    /// </summary>
    /// <remarks>
    /// The value changed handler method is invoked each time the value of a
    /// monitorable property changes.  The value change handler method must
    /// have the signature <c>void HandlerFunction(object,
    /// EventHandler&lt;ValueChangedEventArgs&gt;)</c>.  The
    /// <see cref="ValueChangedEventArgs"/> provides the handler method the
    /// old and new values of the property that changed and possibly the
    /// <see cref="Simulation"/> time when the change occurred.
    /// </remarks>
    public class ValueChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Indicates that the time the monitored property changed was not
        /// specified.
        /// </summary>
        /// <remarks>
        /// This value is defined as -1L.
        /// </remarks>
        public const long TimeNotSpecified = -1L;

        /// <summary>The old value.</summary>
        private object _oldValue;
        /// <summary>The new value.</summary>
        private object _newValue;
        /// <summary>The simulation time the value changed.</summary>
        private long _time;

        /// <overloads>
        /// Create and initialize a ValueChangedEventArgs instance.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="ValueChangedEventArgs"/> instance that
        /// describes a change from one value to another.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        public ValueChangedEventArgs(object oldValue, object newValue)
            : this(oldValue, newValue, TimeNotSpecified)
        {
        }

        /// <summary>
        /// Create a new <see cref="ValueChangedEventArgs"/> instance that
        /// describes a change from one value to another at a specific time
        /// during a <see cref="Simulation"/> run.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="time">
        /// The simulation time the value changed.  If this value is less than
        /// zero (0), the <see cref="Time"/> property will be set to
        /// <see cref="ValueChangedEventArgs.TimeNotSpecified"/>.
        /// </param>
        public ValueChangedEventArgs(object oldValue, object newValue, long time)
        {
            _oldValue = oldValue;
            _newValue = newValue;
            _time = time >= 0L ? time : TimeNotSpecified;
        }

        /// <summary>
        /// The old (previous) value of the monitored property.
        /// </summary>
        /// <value>
        /// The old value as an <see cref="object"/>.
        /// </value>
        public object OldValue
        {
            get { return _oldValue; }
        }

        /// <summary>
        /// The new (current) value of the monitored property.
        /// </summary>
        /// <value>
        /// The new value as an <see cref="object"/>.
        /// </value>
        public object NewValue
        {
            get { return _newValue; }
        }

        /// <summary>
        /// The time during a <see cref="Simulation"/> run when the monitored
        /// property changed.
        /// </summary>
        /// <remarks>
        /// If no time was specified in the constructor, this property will
        /// be -1.
        /// </remarks>
        /// <value>
        /// The simulation time when the value of the monitored property
        /// changed.
        /// </value>
        public long Time
        {
            get { return _time; }
        }
    }
}
