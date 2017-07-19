//=============================================================================
//=  $Id: Delay.cs 163 2006-09-03 15:15:33Z eroe $
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

namespace React.Tasking
{
	/// <summary>
	/// An <see cref="Task"/> that implements a delay.
	/// </summary>
	/// <remarks>
	/// This <see cref="Task"/> is used to simulate a delay for a period of
	/// time.  The delay can represent a wait time or a processing time.
	/// </remarks>
	public class Delay : Task
	{
		/// <summary>The delay time period.</summary>
		private long _delay;
		/// <summary>
		/// Flag indicating whether or not the delay is complete.
		/// </summary>
		private bool _complete;

		/// <overloads>Create and initialize a new Delay task.</overloads>
		/// <summary>
		/// Create and initialize a new, zero-length <see cref="Delay"/>
		/// task.
		/// </summary>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which the task will run.
		/// </param>
		public Delay(Simulation sim) : this(sim, 0L)
		{
		}

		/// <summary>
		/// Create and initialize a new <see cref="Delay"/> which will delay
		/// for the specified time.
		/// </summary>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which the task will run.
		/// </param>
		/// <param name="relTime">
		/// The delay time relative to the current simulation time.
		/// </param>
		public Delay(Simulation sim, long relTime) : base(sim)
		{
			Time = relTime;
		}

		/// <summary>
		/// Gets or sets the delay time.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// If an attempt is made to set <see cref="Time"/> to a value less
		/// than zero (0).
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// If an attempt is made to change the delay time after the
        /// task has been activated.
        /// </exception>
		/// <value>
		/// The delay time relative to the current simulation time as an
		/// <see cref="long"/>.
		/// </value>
		public long Time
		{
			get {return _delay;}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException(
						"Delay time cannot be negative.");
				}
                if (IsScheduled)
                {
                    throw new InvalidOperationException(
                        "Cannot change delay time once activated.");
                }
				_delay = value;
			}
		}

		/// <summary>
		/// Perform a simulation delay.
		/// </summary>
		/// <remarks>
		/// This method will be invoked twice.  The first invocation, schedules
		/// the <see cref="Delay"/> task to run again after the delay time.
		/// The second invocation marks the <see cref="Delay"/> as complete and
		/// reactivates any <see cref="Task"/> instances blocking on this
		/// task.
		/// </remarks>
		/// <param name="activator">
		/// The object that activated this <see cref="Delay"/>.
		/// </param>
		/// <param name="data">
		/// Optional data for the <see cref="Delay"/>.
		/// </param>
		protected override void ExecuteTask(object activator, object data)
		{
			if (!_complete)
			{
				Activate(null, Time);
				_complete = true;
			}
			else 
			{
				ResumeAll();
			}
		}
	}
}
