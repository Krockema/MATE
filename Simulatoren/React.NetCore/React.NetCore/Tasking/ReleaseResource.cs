//=============================================================================
//=  $Id: ReleaseResource.cs 184 2006-10-14 18:46:48Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
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
	/// A <see cref="Task"/> used to return a resource item back to its
    /// <see cref="Resource"/>.
	/// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="IResource.Release"/>
    /// method which will return the appropriate <see cref="Task"/> for
    /// releasing a resource item on behalf of a client <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="Resource"/>.
    /// </para>
    /// </remarks>
	public class ReleaseResource : ProxyTask<Resource>
	{
        /// <summary>
        /// The item to return to the <see cref="Resource"/>.
        /// </summary>
        /// <remarks>
        /// This will be <see langword="null"/> for un-tracked (anonymous)
        /// resources.
        /// </remarks>
        private object _itemToRelease;

        /// <summary>
        /// Create a new <see cref="ReleaseResource"/> instance that will
        /// release to the specified <see cref="Resource"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="resource">
        /// The <see cref="Resource"/> to release.
        /// </param>
		public ReleaseResource(Simulation sim, Resource resource)
			: this(sim, resource, null)
		{
		}

        /// <summary>
        /// Create a new <see cref="ReleaseResource"/> instance that will
        /// release a specified item to the specified <see cref="Resource"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="resource">
        /// The <see cref="Resource"/> to which <paramref name="item"/> is
        /// returned.
        /// </param>
        /// <param name="item">
        /// The resource item originally obtained from
        /// <paramref name="resource"/>.
        /// </param>
        public ReleaseResource(Simulation sim, Resource resource, object item)
            : base(sim, resource)
        {
            _itemToRelease = item;
        }

        /// <summary>
        /// Attempt to release an item to a <see cref="Resource"/> on behalf
        /// of some client <see cref="Task"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="activator"/> is not a <see cref="Task"/>
        /// instance.
        /// </exception>
        /// <param name="activator">
        /// The object that activated this <see cref="ReleaseResource"/> task.
        /// </param>
        /// <param name="data">Not used.</param>
        protected override void ExecuteTask(object activator, object data)
		{
			Task owner = activator as Task;
			if (owner == null)
			{
				throw new ArgumentException("'activator' not a Task instance.");
			}
			Blocker.ReturnResource(owner, _itemToRelease);
            ResumeAll(Blocker, null);
		}
	}
}
