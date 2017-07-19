//=============================================================================
//=  $Id: Delegating.cs 162 2006-09-03 01:09:52Z eroe $
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
	/// Delegate that handles task processing on behalf of a
	/// <see cref="Delegating"/> task.
	/// </summary>
	/// <remarks>
	/// If the <see cref="DelegatingTaskHandler"/> completes its processing
	/// entirely, then it must return <b>true</b>; if it re-schedules
	/// <paramref name="task"/>, then it must return <b>false</b>.
	/// Returning <b>true</b> will unblock any <see cref="Task"/> instances
	/// waiting on the completion of <paramref name="task"/>.
	/// </remarks>
    /// <param name="task">
    /// The invoking <see cref="Delegating"/> task.
    /// </param>
    /// <param name="activator">
    /// The object that activated <paramref name="task"/>.
    /// </param>
    /// <param name="data">
    /// Data passed to <paramref name="task"/> when it was activated.
    /// </param>
	public delegate bool DelegatingTaskHandler(Delegating task,
		object activator, object data);

	/// <summary>
	/// A <see cref="Task"/> that delegates processing to an
	/// <see cref="DelegatingTaskHandler"/>.
	/// </summary>
	public class Delegating : Task
	{
		/// <summary>
		/// The callback that does the actual task processing.
		/// </summary>
		private DelegatingTaskHandler _handler;

		/// <summary>
		/// Create a new <see cref="Delegating"/> task that has no
		/// <see cref="Handler"/>.
		/// </summary>
		/// <remarks>
		/// A handler must be set or this task will do nothing.
		/// </remarks>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which the task will run.
		/// </param>
		public Delegating(Simulation sim) : base(sim)
		{
		}

		/// <summary>
		/// Create a new <see cref="Delegating"/> task that delegates task
		/// processing to the given <see cref="DelegatingTaskHandler"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="handler"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which the task will run.
		/// </param>
		/// <param name="handler">
		/// The <see cref="DelegatingTaskHandler"/> that performs the actual
		/// task processing functionality.
		/// </param>
		public Delegating(Simulation sim, DelegatingTaskHandler handler)
			: base(sim)
		{
			this.Handler = handler;
		}

		/// <summary>
		/// Gets or sets the tasks <see cref="DelegatingTaskHandler"/> callback
		/// that handles the actual task processing functionality.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If an attempt is made to set the value to <see langword="null"/>.
		/// </exception>
		/// <value>
		/// The <see cref="Delegating"/> task's callback that handles the
		/// actual task processing functionality.
		/// </value>
		public DelegatingTaskHandler Handler
		{
			get {return _handler;}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				_handler = value;
			}
		}

		/// <summary>
		/// Performs task processing by invoking the associated
		/// <see cref="DelegatingTaskHandler"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="Handler"/> was never set, then this method does
		/// nothing and simply returns <b>true</b>.
		/// </remarks>
		/// <param name="activator">
		/// The object that activated this <see cref="Delegating"/> task.
		/// </param>
		/// <param name="data">
		/// Optional data for the <see cref="Delegating"/> task.
		/// </param>
		protected override void ExecuteTask(object activator, object data)
		{
			bool isComplete;

			if (_handler != null)
				isComplete = _handler(this, activator, data);
			else
				isComplete = true;

			if (isComplete)
				ResumeAll();
		}
	}
}
