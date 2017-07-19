//=============================================================================
//=  $Id: PriorityQueue.cs 184 2006-10-14 18:46:48Z eroe $
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
	/// An <see cref="IQueue&lt;T&gt;"/> implementation whose ordering is
	/// priority based.
	/// </summary>
	/// <remarks>
    /// <para>
	/// In a priority queue, items that have higher priorities are removed
	/// before items with lower priorities.
    /// </para>
	/// <para>
	/// It's important to note that <see cref="PriorityQueue&lt;T&gt;"/> is
	/// built on a heap; therefore it is <b>not</b> stable (i.e. items of
	/// equal priority may not be dequeued in the order they were enqueued).
	/// </para>
	/// </remarks>
    /// <typeparam name="T">
    /// The type of object to store in the
    /// <see cref="PriorityQueue&lt;T&gt;"/>.
    /// </typeparam>
    [
        SuppressMessage("Microsoft.Naming",
            "CA1710:IdentifiersShouldHaveCorrectSuffix"),
        SuppressMessage("Microsoft.Naming",
            "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")
    ]
    public class PriorityQueue<T> : IQueue<T>
	{
        /// <summary>
        /// The default <see cref="Comparison&lt;T&gt;"/> used to prioritize
        /// items added to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
		public static readonly Comparison<T> DefaultPrioritizer =
			new Comparison<T>(PriorityQueue<T>.CompareComparables);

        /// <summary>
        /// The container for storing queue data.
        /// </summary>
		private List<T> _backingStore = new List<T>();
        /// <summary>
        /// The <see cref="Comparison&lt;T&gt;"/> used to prioritize
        /// items added to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
		private Comparison<T> _prioritizer = DefaultPrioritizer;

		/// <summary>
        /// Create a new, empty <see cref="PriorityQueue&lt;T&gt;"/> instance.
		/// </summary>
		public PriorityQueue()
		{
		}

        /// <summary>
        /// Gets or sets the <see cref="Comparison&lt;T&gt;"/> used to prioritize
        /// items added to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If an attempt is made to set this property to <see langword="null"/>.
        /// </exception>
        /// <value>
        /// The <see cref="Comparison&lt;T&gt;"/> used to prioritize items
        /// added to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </value>
		public Comparison<T> Prioritizer
		{
			get {return _prioritizer;}
			set
			{
				if (value == null)
					throw new ArgumentNullException(
						"Prioritizer cannot be null.");
				if (!_prioritizer.Equals(value))
				{
					_prioritizer = value;
					RebuildHeap();
				}
			}
		}

		#region ICollection<T> Members

        /// <summary>
        /// Gets the number of items in the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of items in the <see cref="PriorityQueue&lt;T&gt;"/> as
        /// an <see cref="int"/>.
        /// </value>
		public int Count
		{
			get {return _backingStore.Count;}
		}

        /// <summary>
        /// Gets a value indicating whether or not the
        /// <see cref="PriorityQueue&lt;T&gt;"/> is read-only.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <see cref="PriorityQueue&lt;T&gt;"/> is
        /// read-only.
        /// </value>
		public bool IsReadOnly
		{
			get {return false;}
		}

        /// <summary>
        /// Adds an item to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// This method simply invokes <c>Enqueue(item)</c>.
        /// </remarks>
        /// <param name="item">
        /// The object to add to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </param>
		public void Add(T item)
		{
			Enqueue(item);
		}

        /// <summary>
        /// Removes all items from the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
		public void Clear()
		{
            _backingStore.Clear();
		}

        /// <summary>
        /// Determines whether the <see cref="PriorityQueue&lt;T&gt;"/>
        /// contains the specified value.
        /// </summary>
        /// <param name="item">
        /// The object to located in the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="item"/> is found in the
        /// <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
		public bool Contains(T item)
		{
            return _backingStore.Contains(item);
		}

        /// <summary>
        /// Copies the elements of the <see cref="PriorityQueue&lt;T&gt;"/> to
        /// an <see cref="Array"/>, starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of
        /// the elements copied from <see cref="PriorityQueue&lt;T&gt;"/>.
        /// <paramref name="array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying
        /// begins.
        /// </param>
		public void CopyTo(T [] array, int arrayIndex)
		{
			RebuildHeap();
            _backingStore.CopyTo(array, arrayIndex);
		}

        /// <summary>
        /// Removes the first occurrence of a specific object from the
        /// <see cref="PriorityQueue&lt;T&gt;"/>. 
        /// </summary>
        /// <remarks>
        /// Removing arbitrary objects from the
        /// <see cref="PriorityQueue&lt;T&gt;"/> will cause the heap to be
        /// re-built with each removal.  This can be a potentially expensive
        /// operation and should normally be avoided.
        /// </remarks>
        /// <param name="item">
        /// The object to remove from the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="item"/> was removed from the
        /// <see cref="PriorityQueue&lt;T&gt;"/>; otherwise <b>false</b>.  This
        /// method will also return <b>false</b> if <paramref name="item"/> was
        /// not found in the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
		public bool Remove(T item)
		{
            bool retval = _backingStore.Remove(item);
			if (retval)
				RebuildHeap();
			return retval;
		}

        /// <summary>
        /// Returns an <see cref="IEnumerator&lt;T&gt;"/> that can be used to
        /// iterate through the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator&lt;T&gt;"/> that can be used to iterate
        /// through the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			RebuildHeap();
            return _backingStore.GetEnumerator();
		}

        /// <summary>
        /// Returns an <see cref="System.Collections.IEnumerator"/> that can be
        /// used to iterate through the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> that can be used
        /// to iterate through the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            RebuildHeap();
            return _backingStore.GetEnumerator();
        }

		#endregion

		#region IQueue<T> Members

        /// <summary>
        /// Adds the specified item to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </param>
        public void Enqueue(T item)
		{
            _backingStore.Add(default(T));

            int cndx = _backingStore.Count - 1;
			int pndx = ParentIndex(cndx);

            while (cndx > 0 && Prioritizer(_backingStore[pndx], item) < 0)
			{
                _backingStore[cndx] = _backingStore[pndx];
				cndx = pndx;
				pndx = ParentIndex(cndx);
			}

            _backingStore[cndx] = item;
		}

        /// <summary>
        /// Removes the next available item from the
        /// <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the <see cref="PriorityQueue&lt;T&gt;"/> is empty.
        /// </exception>
        /// <returns>
        /// The item removed from the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
		public T Dequeue()
		{
			int last = Count - 1;
			T item = Peek();

			if (last == 0)
			{
				Clear();
			}
			else
			{
                _backingStore[0] = _backingStore[last];
                _backingStore.RemoveAt(last);
				Heapify(0);
			}
			
			return item;
		}

        /// <summary>
        /// Returns a reference to the next item on the
        /// <see cref="PriorityQueue&lt;T&gt;"/>
        /// without removing it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the <see cref="PriorityQueue&lt;T&gt;"/> is empty.
        /// </exception>
        /// <returns>
        /// The next item on the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </returns>
        public T Peek()
		{
			if (Count < 1)
				throw new InvalidOperationException("Queue is empty.");
            return _backingStore[0];
		}

		#endregion

        //====================================================================
        //====                   Private Implementation                   ====
        //====================================================================

        /// <summary>
        /// Returns the parent node index of the specified heap node.
        /// </summary>
        /// <param name="ndx">
        /// The heap node index whose parent node index is to be found.
        /// </param>
        /// <returns>
        /// The parent node index of the node at <paramref name="ndx"/>.
        /// </returns>
		private static int ParentIndex(int ndx)
		{
			return (ndx - 1) >> 1;
		}

        /// <summary>
        /// Returns the left child index of the specified heap node.
        /// </summary>
        /// <param name="ndx">
        /// The parent node index whose left child index is to be found.
        /// </param>
        /// <returns>
        /// The left child index of the node at <paramref name="ndx"/>.
        /// </returns>
		private static int LeftIndex(int ndx)
		{
			return (ndx << 1) + 1;
		}

        /// <summary>
        /// Returns the right child index of the specified heap node.
        /// </summary>
        /// <param name="ndx">
        /// The parent node index whose right child index is to be found.
        /// </param>
        /// <returns>
        /// The right child index of the node at <paramref name="ndx"/>.
        /// </returns>
        private static int RightIndex(int ndx)
		{
			return (ndx << 1) + 2;
		}

        /// <summary>
        /// Recursively restore the heap starting at the given node index.
        /// </summary>
        /// <remarks>
        /// This method does nothing if there are not at least two (2) items
        /// in the <see cref="PriorityQueue&lt;T&gt;"/>.
        /// </remarks>
        /// <param name="ndx">
        /// The node index to heapify.
        /// </param>
		private void Heapify(int ndx)
		{
			if (Count < 2)
				return;

			int max;
			int last = Count - 1;
			int left = LeftIndex(ndx);
			int right = RightIndex(ndx);

            if (left <= last &&
                Prioritizer(_backingStore[left], _backingStore[ndx]) > 0)
			{
				max = left;
			}
			else
			{
				max = ndx;
			}

            if (right <= last &&
                Prioritizer(_backingStore[right], _backingStore[max]) > 0)
			{
				max = right;
			}

			if (max != ndx)
			{
                T temp = _backingStore[max];
                _backingStore[max] = _backingStore[ndx];
                _backingStore[ndx] = temp;
				Heapify(max);
			}
		}

        /// <summary>
        /// Force a complete rebuild of the heap.
        /// </summary>
		private void RebuildHeap()
		{
            _backingStore.Sort(new Inverter(_prioritizer));
		}

        /// <summary>
        /// Method used to implement the <see cref="DefaultPrioritizer"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="a"/> or <paramref name="b"/> are
        /// <see langword="null"/>.
        /// </exception>
        /// <param name="a">
        /// The item to compare with <paramref name="b"/>.
        /// </param>
        /// <param name="b">
        /// The item to compare with <paramref name="a"/>.
        /// </param>
        /// <returns></returns>
		private static int CompareComparables(T a, T b)
		{
			if (a == null)
				throw new ArgumentNullException("Cannot compare against 'null' (a).");
			if (b == null)
				throw new ArgumentNullException("Cannot compare against 'null' (b).");

			IComparable<T> aat = a as IComparable<T>;

			if (aat != null)
				return aat.CompareTo(b);

			IComparable aao = a as IComparable;

			if (aao != null)
				return aao.CompareTo(b);

			throw new ArgumentException("Arguments are not comparable.");
		}

        /// <summary>
        /// Class used to invert the results of a
        /// <see cref="Comparison&lt;T&gt;"/>.
        /// </summary>
		private sealed class Inverter : IComparer<T>
		{
			Comparison<T> p;
			public Inverter(Comparison<T> comp)
			{
				p = comp;
			}
			public int Compare(T a, T b)
			{
				return p(a, b) * -1;
			}
		}
	}
}
