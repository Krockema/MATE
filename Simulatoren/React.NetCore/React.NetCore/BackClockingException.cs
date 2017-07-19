//=============================================================================
//=  $Id: BackClockingException.cs 166 2006-09-04 16:53:32Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2004, Eric K. Roe.  All rights reserved.
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
using System.Globalization;
using System.Runtime.Serialization;

namespace React
{
	/// <summary>
	/// The exception that is thrown when an attempt is made to schedule an
    /// <see cref="ActivationEvent"/> earlier than the current simulation time.
	/// </summary>
    [Serializable]
	public sealed class BackClockingException : SchedulingException
	{
        /// <summary>
        /// The time the back-clocking attempt occurred.
        /// </summary>
		private long _schdTime = -1L;

        /* ================ Standard Exception Constructors ================ */

        /// <overloads>
        /// Create and initialize a BackClockingException.
        /// </overloads>
        /// <summary>
        /// Create a <see cref="BackClockingException"/> having no simulation
        /// context or message.
        /// </summary>
        public BackClockingException()
        {
        }

        /// <summary>
        /// Create a <see cref="BackClockingException"/> with the specified
        /// error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public BackClockingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create a new <see cref="BackClockingException"/> with a specified
        /// error message and a reference to the inner exception that is the
        /// cause of this exception. 
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the
        /// <paramref name="innerException"/> parameter is not
        /// <see langword="null"/>, the current exception is raised in a
        /// <b>catch</b> block that handles the inner exception.
        /// </param>
        public BackClockingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="BackClockingException"/>
        /// class with serialized data. 
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.
        /// </param>
        private BackClockingException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            _schdTime = info.GetUInt32("_schdTime");
        }

        /* =============== Additional Exception Constructors =============== */

        /// <overloads>
        /// Create and initialize a new BackClockingException.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="BackClockingException"/>
        /// </summary>
        /// <remarks>
        /// The time the attempted back-clocking occurred is not set.
        /// </remarks>
        /// <param name="sim">The simulation context.</param>
		public BackClockingException(Simulation sim) : this (sim, -1L)
		{
		}

        /// <summary>
        /// Create a new <see cref="BackClockingException"/> which
        /// indicates the time of the back-clocking attempt.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="time">
        /// The simulation time when the back-clocking attempt occurred.
        /// </param>
		public BackClockingException(Simulation sim, long time)
			: base(sim)
		{
			_schdTime = time;
		}

        /// <summary>
        /// Create a new <see cref="BackClockingException"/> with the
        /// specified error message.
        /// </summary>
        /// <remarks>
        /// The time the attempted back-clocking occurred is not set.
        /// </remarks>
        /// <param name="sim">The simulation context.</param>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
		public BackClockingException(Simulation sim, string message)
			: this (sim, -1L, message)
		{
		}

        /// <summary>
        /// Create a new <see cref="BackClockingException"/> with the
        /// specified error message and indicating the time when the
        /// back-clocking attempt occurred.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="time">
        /// The simulation time when the back-clocking attempt occurred.
        /// </param>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public BackClockingException(Simulation sim, long time, string message)
            :
			base(sim, message)
		{
			_schdTime = time;
		}

        /// <summary>
        /// Gets the simulation time when the back-clocking attempt was made.
        /// </summary>
        /// <value>
        /// The simulation time when the back-clocking attempt was made as an
        /// <see cref="long"/>.
        /// </value>
		public long AttemptedTime
		{
			get {return _schdTime;}
		}

        /// <summary>
        /// Gets the error message that explains the reason the
        /// <see cref="BackClockingException"/> occurred.
        /// </summary>
        /// <remarks>
        /// If no message text was given in the constructor, default message
        /// indicating the simulation time and requested time is used.
        /// </remarks>
        /// <value>
        /// The error message that explains the reason for the
        /// <see cref="BackClockingException"/>.
        /// </value>
		public override string Message
		{
			get
			{
				string msg = base.Message;
				if (String.IsNullOrEmpty(msg))
				{
					msg = String.Format(
                        CultureInfo.CurrentCulture,
						"At time {0} attempted to schedule an event at time {1}.",
						Time, AttemptedTime);
				}

				return msg;
			}
		}

        /// <summary>
        /// Populates the given <see cref="SerializationInfo"/> with the
        /// data needed to serialize the <see cref="BackClockingException"/>. 
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> to populate with data.
        /// </param>
        /// <param name="context">
        /// The destination (see <see cref="StreamingContext"/>) for this
        /// serialization.
        /// </param>
        /*
        [
            SecurityPermissionAttribute(SecurityAction.Demand,
                SerializationFormatter = true)
        ]
        public override void GetObjectData(SerializationInfo info,
            StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info", "cannot be null");

            info.AddValue("_schdTime", _schdTime);
            base.GetObjectData(info, context);
        }
        */
	}
}
