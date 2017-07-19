//=============================================================================
//=  $Id: IConsumable.cs 174 2006-09-10 22:13:37Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2004-2005 Eric K. Roe.  All rights reserved.
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
    /// An object that can be used as a consumable in the simulation.
    /// <seealso cref="Resource"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// In reality, an <see cref="IConsumable"/> represents a <em>set</em> or
    /// <em>pool</em> of consumables that may be acquired by <see cref="Task"/>s
    /// during the course of a simulation run.  Each time a consumable unit is
    /// acquired, there is one less available to dispense to other
    /// <see cref="Task"/>s.  When all consumable units have been dispensed,
    /// the next <see cref="Task"/> to call one of the <b>Acquire</b> methods
    /// will be blocked until such time as the <see cref="IConsumable"/> has
    /// been <em>re-supplied</em>.  This is the primary difference between
    /// <see cref="IConsumable"/> and <see cref="IResource"/>:
    /// <see cref="IConsumable"/> instances are never released, rather at
    /// some point during the simulation it may become necessary to
    /// add units to the <see cref="IConsumable"/> so that it may continue
    /// to serve <see cref="Task"/> requests.
    /// </para>
    /// <para>
    /// This interface represents the minimum functionality required to act as
    /// a consumable pool in a React.NET simulation.  Most applications can use
    /// the <see cref="Consumable"/> class rather than implementing this
    /// interface.
    /// </para>
    /// </remarks>
    public interface IConsumable
    {
        /// <summary>
        /// Gets the number of consumable units currently available.
        /// </summary>
        /// <value>
        /// The number of unit available as an <see cref="int"/>.
        /// </value>
        int Count {get;}

        /// <overloads>
        /// Attempt to acquire one or more consumable units from the pool.
        /// </overloads>
        /// <summary>
        /// Attempt to acquire a single consumable unit from the pool.
        /// </summary>
        /// <param name="requestor">
        /// The <see cref="Task"/> that is requesting to acquire a consumable
        /// unit from the pool.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> which will acquire a consumable unit from
        /// the pool on behalf of <paramref name="requestor"/>.
        /// </returns>
        [BlockingMethod]
        Task Acquire(Task requestor);

        /// <summary>
        /// Attempts to acquire the specified number of consumable units from
        /// the pool.
        /// </summary>
        /// <param name="requestor">
        /// The <see cref="Task"/> that is requesting one or more consumable
        /// units  from the pool.
        /// </param>
        /// <param name="quantity">
        /// The number of units requested.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> which will acquire the given number of
        /// consumable units from the pool on behalf of
        /// <paramref name="requestor"/>.
        /// </returns>
        [BlockingMethod]
        Task Acquire(Task requestor, int quantity);

        /// <summary>
        /// Adds consumable units to the <see cref="IConsumable"/>.
        /// </summary>
        /// <param name="task">
        /// The <see cref="Task"/> requesting the <see cref="IConsumable"/>
        /// be re-supplied.
        /// </param>
        /// <param name="quantity">
        /// The number of units to add to the <see cref="IConsumable"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that will re-supply the
        /// <see cref="IConsumable"/> on behalf of <paramref name="task"/>.
        /// </returns>
        [BlockingMethod]
        Task Resupply(Task task, int quantity);
    }
}
