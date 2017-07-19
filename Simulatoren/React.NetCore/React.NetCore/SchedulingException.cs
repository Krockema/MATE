//=============================================================================
//=  $Id: SchedulingException.cs 166 2006-09-04 16:53:32Z eroe $
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
using System.Runtime.Serialization;

namespace React
{
	/// <summary>
	/// An exception that signifies an error occurred while trying to
    /// schedule an <see cref="ActivationEvent"/>.
	/// </summary>
    [Serializable]
	public class SchedulingException : SimulationException
	{

        /* ================ Standard Exception Constructors ================ */

		/// <overloads>
		/// Create and initialize a new SchedulingException.
		/// </overloads>
		/// <summary>
		/// Create a new <see cref="SchedulingException"/> with no message.
		/// </summary>
		public SchedulingException()
		{
		}

        /// <summary>
        /// Create a new <see cref="SchedulingException"/> with the given
        /// message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public SchedulingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create a new <see cref="SchedulingException"/> with a specified
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
        public SchedulingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingException"/>
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
        protected SchedulingException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        /* =============== Additional Exception Constructors =============== */

		/// <summary>
		/// Create a new <see cref="SchedulingException"/> with no message, but
		/// that has the given simulation context.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		public SchedulingException(Simulation sim) : base(sim, null)
		{
		}

		/// <summary>
		/// Create a new <see cref="SchedulingException"/> having the given
		/// simulation context and error message.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="message">The error message.</param>
		public SchedulingException(Simulation sim, string message)
			: base(sim, message)
		{
		}
	}
}
