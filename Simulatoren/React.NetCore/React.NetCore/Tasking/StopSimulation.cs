//=============================================================================
//=  $Id: StopSimulation.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// A <see cref="Task"/> that stops a running <see cref="Simulation"/>.
	/// </summary>
	/// <remarks>
	/// A <see cref="StopSimulation"/> task can be used to schedule a
	/// <see cref="Simulation"/>'s stop time <b>before</b> it has been
	/// started.  Per the documentation for <see cref="Simulation.Stop()"/>,
	/// those methods do nothing if the <see cref="Simulation"/> is not
	/// running.  Therefore, to "pre-schedule" the stop time, this task may be
	/// used — simply create a new <see cref="StopSimulation"/> and activate
	/// (schedule) it to occur at the time you wish the
	/// <see cref="Simulation"/> to end.
	/// </remarks>
	public class StopSimulation : Task
	{

		/// <summary>
		/// Create and initialize a new <see cref="StopSimulation"/> task.
		/// </summary>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which this task will run.
		/// </param>
		public StopSimulation(Simulation sim) : base(sim)
		{
		}

		/// <summary>
		/// Schedules a <see cref="StopSimulation"/> task at some time
		/// in the future.
		/// </summary>
		/// <remarks>
        /// <para>
		/// This is a convenience method for stopping a running
		/// <see cref="Simulation"/>.  All it does is the following
        /// </para>
		/// <para><code>
		/// Task task = new StopSimulation(sim);
		/// task.Activate(null, relTime);</code></para>
		/// </remarks>
		/// <param name="sim">
		/// The <see cref="Simulation"/> under which the task will run.
		/// </param>
		/// <param name="relTime">
		/// The time, relative to the current simulation time, when the
		/// task should run.
		/// </param>
		public static void Stop(Simulation sim, long relTime)
		{
			Task task = new StopSimulation(sim);
			task.Activate(null, relTime);
		}

		/// <summary>
		/// Stops the associated <see cref="Simulation"/> by invoking its
		/// <see cref="Simulation.Stop()"/> method.
		/// </summary>
		/// <param name="activator">Not used.</param>
		/// <param name="data">Not used.</param>
		protected override void ExecuteTask(object activator, object data)
		{
			Simulation.Stop();
			// Don't need to resume blocked tasks here, becuse sim is done.
		}
	}
}
