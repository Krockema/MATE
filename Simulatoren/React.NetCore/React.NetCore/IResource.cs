//=============================================================================
//=  $Id: IResource.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// An object that can be used as a resource in the simulation.
	/// <seealso cref="Resource"/>
	/// </summary>
	/// <remarks>
    /// <para>
	/// In reality, an <see cref="IResource"/> represents a <em>set</em> or
	/// <em>pool</em> of resources that may be acquired and released by
	/// <see cref="Task"/>s during the course of a simulation run.  Each time
	/// a resource is acquired, there is one less resource available to
	/// dispense to other <see cref="Task"/>s.  Each time a resource is
	/// released by a <see cref="Task"/>, there is one more resource available
	/// to dispense.  When all resources have been acquired, the next
	/// <see cref="Task"/> to call <see cref="Acquire"/> will be blocked until
	/// such time as another <see cref="Task"/> puts a resource back into the
	/// available pool by calling <see cref="Release"/> or
	/// <see cref="Transfer"/>.
    /// </para>
	/// <para>
	/// This interface represents the minimum functionality required to act as
	/// a resource pool in a React.NET simulation.  Most applications can use
	/// one of the <b>Create</b> methods on the <see cref="Resource"/> class to
	/// create a resource pool.
	/// </para>
	/// </remarks>
	public interface IResource
	{
		/// <summary>
		/// Gets the total number of resources in the pool.
		/// </summary>
		/// <remarks>
        /// <para>
		/// The total number of resources in the pool is defined as
        /// </para>
		/// <code>Count = Free + InUse + OutOfService</code>
		/// </remarks>
		/// <value>
		/// The total number of resources in the pool as an <see cref="int"/>.
		/// </value>
		int Count {get;}

		/// <summary>
		/// Gets the number of resources that are not currently in use.
		/// </summary>
		/// <remarks>
        /// <para>
		/// The number of free resources available in the pool is defined as
        /// </para>
		/// <code>Free = Count - (InUse + OutOfService)</code>
		/// </remarks>
		/// <value>
		/// The number of resources that are not currently in use as an
		/// <see cref="int"/>.
		/// </value>
		int Free {get;}

		/// <summary>
		/// Gets the number of resources that are currently in use.
		/// </summary>
		/// <value>
		/// The number of resources that are currently in use as an
		/// <see cref="int"/>.
		/// </value>
		int InUse {get;}

		/// <summary>
		/// Gets or sets the number of resources that are out of service.
		/// </summary>
		/// <remarks>
        /// <para>
		/// Out-of-service resources may not be acquired from the pool.  If
		/// <b>OutOfService</b> is set to a value greater or equal to
		/// <see cref="Count"/>, then all resources are out of service and
		/// all subsequent calls to <see cref="Acquire"/> will block.
        /// </para>
        /// <para>
        /// Decreasing the number of out-of-service resources has the potential
        /// side-effect of resuming one or more waiting <see cref="Task"/>s.
        /// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If an attempt is made to set this property to a value less than
		/// zero (0).
		/// </exception>
		/// <value>
		/// The number of resources currently out of service as an
		/// <see cref="int"/>.
		/// </value>
		int OutOfService {get; set;}

		/// <summary>
		/// Attempt to acquire a resource from the pool.
		/// </summary>
		/// <param name="requestor">
		/// The <see cref="Task"/> that is requesting to acquire a resource
		/// from the pool.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/> which will acquire a resource from the pool
		/// on behalf of <paramref name="requestor"/>.
		/// </returns>
		[BlockingMethod]
		Task Acquire(Task requestor);

		/// <summary>
		/// Releases a previously acquired resource back to the pool.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="owner"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="owner"/> is not an owner of one of the
		/// resources in this pool.
		/// </exception>
		/// <param name="owner">
		/// The <see cref="Task"/> that is releasing a resource.
		/// </param>
		/// <returns>
		/// The <see cref="Task"/> which will release the resource on behalf
		/// of <paramref name="owner"/>.
		/// </returns>
		[BlockingMethod]
		Task Release(Task owner);

		/// <summary>
		/// Transfer ownership of a previously acquired resource from one
		/// <see cref="Task"/> to another.
		/// </summary>
		/// <remarks>
        /// <para>
		/// Ownership of a resource must be transferred from one
		/// <see cref="Task"/> to another if one task is supposed to acquire
		/// the resource, but another task will release it.
        /// </para>
        /// <para>
        /// It is important to note that <paramref name="receiver"/> is
        /// <b>not</b> resumed if it was waiting to acquire the resource.
        /// In the case where <paramref name="receiver"/> is blocking on
        /// the resource, <paramref name="owner"/> should interrupt
        /// <paramref name="receiver"/> after making the transfer.
        /// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If <paramref name="owner"/> is the same as
		/// <paramref name="receiver"/> or if <paramref name="owner"/> is not
		/// an owner of one of the resources in this pool.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="owner"/> or <paramref name="receiver"/> is
		/// <see langword="null"/>.
		/// </exception>
		/// <param name="owner">
		/// The <see cref="Task"/> that owns the resource.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="Task"/> which will receive the resource from
		/// <paramref name="owner"/>.
		/// </param>
        /// <returns>
        /// The <see cref="Task"/> which will transfer the resource to
        /// <paramref name="receiver"/> on behalf of <paramref name="owner"/>.
        /// </returns>
        [BlockingMethod]
		Task Transfer(Task owner, Task receiver);
	}
}
