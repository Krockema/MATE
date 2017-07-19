//=============================================================================
//=  $Id: IBoundedBuffer.cs 183 2006-10-14 17:54:20Z eroe $
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

namespace React
{
    /// <summary>
    /// An object which can serve as a <em>bounded buffer</em>.
    /// <seealso cref="BoundedBuffer"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <em>bounded buffer</em> is a blocking object that has a pre-defined
    /// capacity.  <see cref="Task"/>s that use the <see cref="IBoundedBuffer"/>
    /// attempt to put items into the buffer (they are the <em>producers</em>)
    /// or take items from the buffer (they are the <em>consumers</em>).
    /// </para>
    /// <para>
    /// <see cref="Task"/>s that are acting as producers use <see cref="Put"/>
    /// to attempt to add an item to the buffer.  If the buffer has not yet
    /// reached its capacity the item can be added immediately; otherwise
    /// the <see cref="Task"/> returned by <see cref="Put"/> will block until
    /// such time as the buffer has room for another item.
    /// </para>
    /// <para>
    /// <see cref="Task"/>s that are acting as consumers use <see cref="Get"/>
    /// to attempt to remove an item from the buffer.  If the buffer is not
    /// empty an item is immediately removed; otherwise the <see cref="Task"/>
    /// returned by <see cref="Get"/> will block until such time as a
    /// producer adds an item.
    /// </para>
    /// </remarks>
    public interface IBoundedBuffer
    {
        /// <summary>
        /// Gets the number of items in the <see cref="IBoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="Count"/> is zero (0), <see cref="Task"/>s
        /// returned by <see cref="Get"/> will block.  When
        /// <see cref="Count"/> is equal or greater than <see cref="Capacity"/>,
        /// <see cref="Task"/>s returned by <see cref="Put"/> will block.
        /// </remarks>
        /// <value>
        /// The number of items in the <see cref="IBoundedBuffer"/> as an
        /// <see cref="int"/>.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets or sets the capacity of the <see cref="IBoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Capacity"/> is set to zero (0), then items may
        /// neither be added nor removed from the buffer.  This is one way of
        /// temporarily disabling the buffer.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If an attempt is made to set the property to a value less than
        /// zero (0).
        /// </exception>
        /// <value>
        /// The capacity of the <see cref="IBoundedBuffer"/> as an
        /// <see cref="int"/>.
        /// </value>
        int Capacity { get; set; }

        /// <summary>
        /// Attempt to remove an item from the <see cref="IBoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="consumer"/> must block on the returned
        /// <see cref="Task"/> in order for an item to be removed from the
        /// <see cref="IBoundedBuffer"/>.  The removed item will be passed
        /// to <paramref name="consumer"/> as the activation data.
        /// </remarks>
        /// <param name="consumer">
        /// The consumer <see cref="Task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will remove an item from the
        /// <see cref="IBoundedBuffer"/> on behalf of
        /// <paramref name="consumer"/>.
        /// </returns>
        [BlockingMethod]
        Task Get(Task consumer);

        /// <summary>
        /// Attempt to add an item to the <see cref="IBoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="producer"/> must block on the returned
        /// <see cref="Task"/> in order for <paramref name="item"/> to be
        /// added to the <see cref="IBoundedBuffer"/>.
        /// </remarks>
        /// <param name="producer">
        /// The producer <see cref="Task"/>.
        /// </param>
        /// <param name="item">
        /// The object to place into the <see cref="IBoundedBuffer"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will add an item to the
        /// <see cref="IBoundedBuffer"/> on behalf of
        /// <paramref name="producer"/>.
        /// </returns>
        [BlockingMethod]
        Task Put(Task producer, object item);
    }
}
