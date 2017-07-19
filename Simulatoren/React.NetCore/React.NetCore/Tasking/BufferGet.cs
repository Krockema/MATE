//=============================================================================
//=  $Id: BufferGet.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections.Generic;
using System.Text;

namespace React.Tasking
{
    /// <summary>
    /// A <see cref="Task"/> used to remove an item from a
    /// <see cref="BoundedBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="IBoundedBuffer.Get"/>
    /// method which will return the appropriate <see cref="Task"/> for
    /// removing items from the buffer on behalf of a client
    /// <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="BoundedBuffer"/>.
    /// </para>
    /// </remarks>
    public class BufferGet : ProxyTask<BoundedBuffer>
    {
        /// <summary>
        /// Create a <see cref="BufferGet"/> task that will remove an item
        /// from the specified <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="sim">The simulation contex.</param>
        /// <param name="buffer">
        /// The <see cref="BoundedBuffer"/> from which to remove an item.
        /// </param>
        public BufferGet(Simulation sim, BoundedBuffer buffer)
            : base(sim, buffer)
        {
        }

        /// <summary>
        /// Attempt to remove an item from a <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="activator">
        /// The object that activated this <see cref="BufferGet"/> task.
        /// </param>
        /// <param name="data">
        /// Optional data for the <see cref="BufferGet"/> task.  This will
        /// normally be <see langword="null"/> or the item removed from the
        /// <see cref="BoundedBuffer"/>.
        /// </param>
        protected override void ExecuteTask(object activator, object data)
        {
            System.Diagnostics.Debug.Assert(activator != Blocker);
            Blocker.GetObject(this);
        }
    }
}
