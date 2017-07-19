//=============================================================================
//=  $Id: LifoQueue.cs 182 2006-10-14 17:51:51Z eroe $
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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace React.Queue
{
	/// <summary>
	/// A last-in, first-out <see cref="IQueue&lt;T&gt;"/> implementation.
	/// </summary>
	/// <remarks>
	/// In a last-in, first-out queue, the last item added to the queue is
	/// the first item removed from the queue.  These queues are typically
	/// referred to as LIFO queues or stacks.
	/// </remarks>
    /// <typeparam name="T">
    /// The type of object to store in the <see cref="LifoQueue&lt;T&gt;"/>.
    /// </typeparam>
    [
        SuppressMessage("Microsoft.Naming",
            "CA1710:IdentifiersShouldHaveCorrectSuffix"),
        SuppressMessage("Microsoft.Naming",
            "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")
    ]
    public class LifoQueue<T> : Collection<T>, IQueue<T>
	{
		/// <summary>
        /// Create a new, empty <see cref="LifoQueue&lt;T&gt;"/> instance.
		/// </summary>
		public LifoQueue()
		{
		}

		#region IQueue Members

		/// <summary>
        /// Add the specified item to the front of the
        /// <see cref="LifoQueue&lt;T&gt;"/>.
		/// </summary>
		/// <param name="item">
        /// The item to add to the front of the
        /// <see cref="LifoQueue&lt;T&gt;"/>.
		/// </param>
		public void Enqueue(T item)
		{
			Insert(0, item);
		}

		/// <summary>
		/// Removes and returns the item at the front of the
        /// <see cref="LifoQueue&lt;T&gt;"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">
        /// If the <see cref="LifoQueue&lt;T&gt;"/> is empty.
		/// </exception>
		/// <returns>
        /// The item at the front of the <see cref="LifoQueue&lt;T&gt;"/>.
		/// </returns>
		public T Dequeue()
		{
			T item = Peek();
			RemoveAt(0);
			return item;
		}

		/// <summary>
        /// Gets the item at the front of the <see cref="LifoQueue&lt;T&gt;"/>.
		/// </summary>
		/// <remarks>
		/// The item at the front of the queue is returned, but is not removed
        /// from the <see cref="LifoQueue&lt;T&gt;"/>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
        /// If the <see cref="LifoQueue&lt;T&gt;"/> is empty.
		/// </exception>
		/// <returns>
        /// The item at the front of the <see cref="LifoQueue&lt;T&gt;"/>.
		/// </returns>
		public T Peek()
        {
            if (Count < 1)
                throw new InvalidOperationException("Queue is empty.");

			return Items[0];
		}

		#endregion
	}
}
