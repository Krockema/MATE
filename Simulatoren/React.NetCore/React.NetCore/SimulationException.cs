//=============================================================================
//=  $Id: SimulationException.cs 158 2006-08-30 11:49:06Z eroe $
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
	/// Exception that signifies a simulation error.
	/// </summary>
   [Serializable]
	public class SimulationException : System.Exception
	{
		/// <summary>
		/// The <see cref="Simulation"/> where the error occured.
		/// </summary>
        [NonSerialized]
		private Simulation _sim;

        /* ================ Standard Exception Constructors ================ */

        /// <overloads>Create and initialize a SimulationException.</overloads>
        /// <summary>
        /// Create a <see cref="SimulationException"/> having no simulation
        /// context or message.
        /// </summary>
		public SimulationException()
		{
		}

        /// <summary>
        /// Create a <see cref="SimulationException"/> with the specified
        /// error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public SimulationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create a new <see cref="SimulationException"/> with a specified
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
        public SimulationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationException"/>
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
        protected SimulationException(SerializationInfo info,
            StreamingContext context) : base(info.AssemblyName)
        {
        }

        /* =============== Additional Exception Constructors =============== */

        /// <summary>
        /// Create a <see cref="SimulationException"/> having the given
        /// simulation context.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
		public SimulationException(Simulation sim) : this(sim, null)
		{
		}

        /// <summary>
        /// Create a <see cref="SimulationException"/> having the given
        /// simulation context and error message.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="message">The error message.</param>
		public SimulationException(Simulation sim, string message)
			: base(message)
		{
			_sim = sim;
		}

        /// <summary>
        /// Create a <see cref="SimulationException"/> having the given
        /// simulation context, error message, and cause.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The cause.</param>
		public SimulationException(Simulation sim, string message,
			Exception innerException) : base(message, innerException)
		{
			_sim = sim;
		}

        /// <summary>
        /// Gets the simulation context where the error occurred.
        /// </summary>
        /// <value>
        /// The simulation context as a <see cref="React.Simulation"/>.
        /// </value>
		public Simulation Simulation
		{
			get {return _sim;}
		}

		/// <summary>
		/// Gets the simulation time when the exception was thrown.
		/// </summary>
		/// <value>
		/// The simulation time as an <see cref="long"/>.  If the simulation
		/// is not running, <see cref="Time"/> will be -1L.
		/// </value>
		public long Time
		{
			get
			{
				return (_sim != null) ? _sim.Now : -1L;
			}
		}
	}
}
