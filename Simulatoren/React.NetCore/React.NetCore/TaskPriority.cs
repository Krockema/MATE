//=============================================================================
//=  $Id: TaskPriority.cs 166 2006-09-04 16:53:32Z eroe $
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

namespace React
{
	/// <summary>
	/// The built-in task priorities.
	/// </summary>
	/// <remarks>
	/// Defines the built-in task priorities used by <see cref="Task"/>s,
    /// <see cref="ActivationEvent"/>s and <see cref="Simulation"/>.  Because
    /// other priority values are possible, <see cref="TaskPriority"/> is not
    /// an <c>enum</c>, as that would limit the priorities to a well defined
	/// set.
	/// </remarks>
	public sealed class TaskPriority
	{
		/// <summary>Immediate priority.</summary>
		private const int _immediate	= System.Int32.MaxValue;
		/// <summary>Maximum priority.</summary>
		private const int _maximum		= _immediate - 1;
		/// <summary>Elevated priority.</summary>
		private const int _elevated		= _normal + 100;
		/// <summary>Normal (default) priority.</summary>
		private const int _normal		= 0;
		/// <summary>Reduced priority.</summary>
		private const int _reduced		= _normal - 100;
		/// <summary>Discardable priority.</summary>
		private const int _discardable	= System.Int32.MinValue;

		/// <summary>Private constructor to prevent instantiation.</summary>
		private TaskPriority() {}

		/// <summary>
		/// Gets the immediate task priority.
		/// </summary>
		/// <remarks>
		/// <see cref="Task"/>s which are activated with this priority are
		/// guaranteed to be executed before all other pending
		/// <see cref="Task"/>s.  To use the <see cref="Immediate"/> task
		/// priority, the <see cref="Task"/> must be scheduled at the current
		/// simulation time.  It is an error to schedule an <see cref="Task"/>
		/// in the future with <see cref="Immediate"/> priority.
		/// </remarks>
		/// <value>
		/// The immediate priority as an <see cref="int"/>.  The immediate
		/// priority is defined as <see cref="System.Int32.MaxValue"/>.
		/// </value>
		public static int Immediate
		{
			get {return _immediate;}
		}

		/// <summary>
		/// Get the maximum task priority.
		/// </summary>
		/// <remarks>
		/// This is the highest allowable priority that can be used to activate
		/// an <see cref="Task"/> beyond the current simulation time (i.e. in
		/// the future).
		/// </remarks>
		/// <value>
		/// The maximum task priority as an <see cref="int"/>.  The maximum
		/// priority is defined as <c>TaskPriority.Immediate - 1</c>.
		/// </value>
		public static int Maximum
		{
			get {return _maximum;}
		}

		/// <summary>
		/// Get the elevated task priority.
		/// </summary>
		/// <value>
		/// The elevated task priority as an <see cref="int"/>.  The elevated
		/// priority is defined as <c>TaskPriority.Normal + 100</c>.
		/// </value>
		public static int Elevated
		{
			get {return _elevated;}
		}

		/// <summary>
		/// Gets the normal (default) task priority.
		/// </summary>
		/// <remarks>
		/// Unless otherwise activated with a different priority, all
		/// <see cref="Task"/> instances should use the <see cref="Normal"/>
		/// task priority.
		/// </remarks>
		/// <value>
		/// The normal (default) task priority as an <see cref="int"/>.  The
		/// normal priority is defined as zero (0).
		/// </value>
		public static int Normal
		{
			get {return _normal;}
		}

		/// <summary>
		/// Get the reduced task priority.
		/// </summary>
		/// <value>
		/// The reduced task priority as an <see cref="int"/>.  The reduced
		/// priority is defined as <c>TaskPriority.Normal - 100</c>.
		/// </value>
		public static int Reduced
		{
			get {return _reduced;}
		}

		/// <summary>
		/// Gets the discardable task priority.
		/// </summary>
		/// <remarks>
		/// <see cref="Task"/>s that are activated with the
		/// <see cref="Discardable"/> priority can be discarded by the
		/// <see cref="Simulation"/> if no higher priority tasks are pending.
		/// Put simply, if all <see cref="Task"/>s waiting to run have a
		/// <see cref="Task.Priority"/> of <see cref="Discardable"/>, then
		/// they are all thrown away by the <see cref="Simulation"/>, which
		/// will cause the <see cref="Simulation"/> to end.  Discardable
		/// tasks are useful for interval-based data collection where the
		/// data collector tasks should stop when there are no more tasks
		/// of significance pending.
		/// </remarks>
		/// <value>
		/// The discardable task priority as an <see cref="int"/>.  The
		/// discardable priority is defined as
		/// <see cref="System.Int32.MinValue"/>.
		/// </value>
		public static int Discardable
		{
			get {return _discardable;}
		}
	}
}
