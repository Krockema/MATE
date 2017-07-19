//=============================================================================
//=  $Id: ICondition.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// A general condition upon which <see cref="Task"/>s may block.
    /// <seealso cref="Condition"/>
	/// </summary>
	public interface ICondition
	{
		/// <summary>
		/// Block (wait) on the <see cref="ICondition"/> until it becomes
		/// signalled.
		/// </summary>
		/// <param name="task">
		/// The <see cref="Task"/> that will block on this
		/// <see cref="ICondition"/> until it is signalled.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/> which will waits on the condition on
		/// behalf of <paramref name="task"/>.
		/// </returns>
		[BlockingMethod]
		Task Block(Task task);

		/// <summary>
		/// Gets whether or not the <see cref="ICondition"/> automatically
		/// resets to an unsignalled state after invoking <see cref="Signal"/>.
		/// </summary>
		/// <remarks>
		/// <see cref="ICondition"/> instances that do not auto-reset, will
		/// remain in the signalled state until the <see cref="Reset"/> method
		/// is invoked.  While signalled, the <see cref="ICondition"/> will not
		/// block any <see cref="Task"/>s.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="ICondition"/> automatically resets;
		/// or <b>false</b> if it must be manually reset by calling the
		/// <see cref="Reset"/> method.
		/// </value>
		bool AutoReset {get;}

		/// <summary>
		/// Gets whether or not the <see cref="ICondition"/> is signalled.
		/// </summary>
		/// <remarks>
		/// When this property is <b>true</b>, the <see cref="ICondition"/>
		/// will not block an <see cref="Task"/> during a call to
		/// <see cref="Block"/>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="ICondition"/> is signalled; or
		/// <b>false</b> if it is reset.
		/// </value>
		bool Signalled {get;}

		/// <summary>
		/// Place the <see cref="ICondition"/> into a <em>signalled</em> state.
		/// </summary>
		/// <remarks>
        /// <para>
		/// One or more of the <see cref="Task"/>s blocking on the
		/// <see cref="ICondition"/> are activated.  It is up to the actual
		/// implementation to decide how many of the blocked
		/// <see cref="Task"/>s to activate.
        /// </para>
		/// <para>
		/// If there are no <see cref="Task"/>s blocking on this
		/// <see cref="ICondition"/> calling this method does nothing
		/// except set <see cref="Signalled"/> to <b>true</b>.  Even that
		/// change will be short-lived if <see cref="AutoReset"/> is
		/// <b>true</b>.
		/// </para>
		/// </remarks>
		void Signal();

		/// <summary>
		/// Place the <see cref="ICondition"/> into a <em>reset</em> state.
		/// </summary>
		/// <remarks>
		/// Subsequent calls to <see cref="Block"/> will block the
		/// <see cref="Task"/>.  Also, <see cref="Signalled"/> will be
		/// set to <b>false</b>.
		/// </remarks>
		void Reset();
	}
}
