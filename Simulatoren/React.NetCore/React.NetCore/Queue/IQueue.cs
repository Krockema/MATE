//=============================================================================
//=  $Id: IQueue.cs 181 2006-10-14 17:50:23Z eroe $
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace React.Queue
{
	/// <summary>
	/// A generic collection that supports queuing semantics.
	/// </summary>
    /// <remarks>
    /// React.NET contains this interface because the .NET Framework
    /// <see cref="Queue&lt;T&gt;"/> class is strictly first-in, first-out
    /// or FIFO.  The classes in the <b>React.Queue</b> namespace, which
    /// implement this interface, support FIFO as well as LIFO (last-in,
    /// first-out) and priority queues.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of object to store in the <see cref="IQueue&lt;T&gt;"/>.
    /// </typeparam>

    [
        SuppressMessage("Microsoft.Naming",
            "CA1710:IdentifiersShouldHaveCorrectSuffix"),
        SuppressMessage("Microsoft.Naming",
            "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")
    ]
    public interface IQueue<T> : ICollection<T>
	{
		/// <summary>
		/// Adds the specified item to the <see cref="IQueue&lt;T&gt;"/>.
		/// </summary>
		/// <param name="item">The item to add to the queue.</param>
		void Enqueue(T item);

		/// <summary>
		/// Removes the next available item from the <see cref="IQueue&lt;T&gt;"/>.
		/// </summary>
		/// <returns>
		/// The item removed from the <see cref="IQueue&lt;T&gt;"/>.
		/// </returns>
		T Dequeue();

		/// <summary>
		/// Returns a reference to the next item on the <see cref="IQueue&lt;T&gt;"/>
		/// without removing it.
		/// </summary>
		/// <returns>
		/// The next item on the <see cref="IQueue&lt;T&gt;"/>.
		/// </returns>
        T Peek();
	}
}
