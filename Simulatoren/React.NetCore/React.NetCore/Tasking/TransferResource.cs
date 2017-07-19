//=============================================================================
//=  $Id: TransferResource.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// A <see cref="Task"/> used to tranfer a resource item from its current
    /// owning <see cref="Task"/> to another <see cref="Task"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="IResource.Transfer"/>
    /// method which will return the appropriate <see cref="Task"/> for
    /// transfering a resource item on behalf of a client <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="Resource"/>.
    /// </para>
    /// </remarks>
    public class TransferResource : ProxyTask<Resource>
	{
        /// <summary>
        /// The <see cref="Task"/> which will become the new resource item
        /// owner.
        /// </summary>
		private Task _receiver;
        /// <summary>
        /// The item to transfer to <see cref="_receiver"/>.
        /// </summary>
        /// <remarks>
        /// This will be <see langword="null"/> for un-tracked (anonymous)
        /// resources.
        /// </remarks>
        private object _itemToTransfer;


        /// <summary>
        /// Create a new <see cref="TransferResource"/> task that will transfer
        /// a resource item to the specified receiver.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="resource">
        /// The <see cref="Resource"/> containing the item to transfer to
        /// <paramref name="receiver"/>.
        /// </param>
        /// <param name="receiver">
        /// The <see cref="Task"/> which will become the new owner of an item
        /// contained in <paramref name="resource"/>.
        /// </param>
		public TransferResource(Simulation sim, Resource resource, Task receiver)
			: this(sim, resource, receiver, null)
		{
		}

        /// <summary>
        /// Create a new <see cref="TransferResource"/> task that will transfer
        /// a specific resource item to the given receiver.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="resource">
        /// The <see cref="Resource"/> containing the item to transfer to
        /// <paramref name="receiver"/>.
        /// </param>
        /// <param name="receiver">
        /// The <see cref="Task"/> which will become the new owner of an item
        /// contained in <paramref name="resource"/>.
        /// </param>
        /// <param name="item">
        /// The resource item to transfer.  This should be <see langword="null"/>
        /// for anonymous resources.
        /// </param>
        public TransferResource(Simulation sim, Resource resource,
            Task receiver, object item) : base(sim, resource)
        {
            _receiver = receiver;
            _itemToTransfer = item;
        }

        /// <summary>
        /// Transfers ownership of a resource item to another <see cref="Task"/>.
        /// </summary>
        /// <param name="activator">
        /// The current owner of the resource item.
        /// </param>
        /// <param name="data">Not used.</param>
		protected override void ExecuteTask(object activator, object data)
		{
			Task owner = activator as Task;
			if (owner == null)
			{
				throw new ArgumentException("'activator' not a Task instance.");
			}
            Blocker.TransferResource(owner, _receiver, _itemToTransfer);
            ResumeAll(Blocker, null);
		}
	}
}
