//=============================================================================
//=  $Id: BufferPut.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// A <see cref="Task"/> used to add an item to a
    /// <see cref="BoundedBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="IBoundedBuffer.Put"/>
    /// method which will return the appropriate <see cref="Task"/> for adding
    /// items to the buffer on behalf of a client
    /// <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="BoundedBuffer"/>.
    /// </para>
    /// </remarks>
    public class BufferPut : ProxyTask<BoundedBuffer>
    {
        /// <summary>
        /// The object to add to the <see cref="BoundedBuffer"/>.
        /// </summary>
        private object _item;

        /// <overloads>Create and initialize a BufferPut task.</overloads>
        /// <summary>
        /// Create a <see cref="BufferPut"/> task that will add an item to
        /// the specified <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="sim">The simulation contex.</param>
        /// <param name="buffer">
        /// The <see cref="BoundedBuffer"/> to which to add an item.
        /// </param>
        public BufferPut(Simulation sim, BoundedBuffer buffer)
            : this(sim, buffer, null)
        {
        }

        /// <summary>
        /// Create a <see cref="BufferPut"/> task that will add the given
        /// <see cref="object"/> to the specified <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="buffer">
        /// The <see cref="BoundedBuffer"/> to which to add
        /// <paramref name="item"/>.
        /// </param>
        /// <param name="item">
        /// The item to add to <paramref name="buffer"/>.
        /// </param>
        public BufferPut(Simulation sim, BoundedBuffer buffer, object item)
            : base(sim, buffer)
        {
            _item = item;
        }

        /// <summary>
        /// Gets or sets the item that will be added to the
        /// <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <value>
        /// The item to add to the <see cref="BoundedBuffer"/> or
        /// <see langword="null"/>.
        /// </value>
        public object Item
        {
            get { return _item; }
            set { _item = value; }
        }

        /// <summary>
        /// Attempt to add an item to a <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="activator">
        /// The object that activated this <see cref="BufferPut"/> task.
        /// </param>
        /// <param name="data">
        /// Optional data for the <see cref="BufferPut"/> task.  This will
        /// normally be <see langword="null"/> or the item to add to the
        /// <see cref="BoundedBuffer"/>.
        /// </param>
        protected override void ExecuteTask(object activator, object data)
        {
            System.Diagnostics.Debug.Assert(activator != Blocker);

            if (Client != null)
            {
                if (Item == null)
                    Blocker.PutObject(this);
                else
                    Blocker.PutObject(this, Item);
            }
            else
            {
                ResumeAll(Blocker, null);
            }
        }
    }
}
