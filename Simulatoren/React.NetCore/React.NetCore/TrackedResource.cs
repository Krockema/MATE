//=============================================================================
//=  $Id: TrackedResource.cs 174 2006-09-10 22:13:37Z eroe $
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
using System.Collections;
using React.Queue;
//using Hashtable = System.Collections.Generic.Dictionary<object, object>;

namespace React
{
    /// <summary>
    /// A <see cref="Resource"/> that tracks its resource items as
    /// actual objects.
    /// <seealso cref="AnonymousResource"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="TrackedResource"/> can be created directly or via
    /// the <see cref="Resource.Create(IEnumerable)"/> factory method.
    /// </para>
    /// <para>
    /// <see cref="TrackedResource"/>s are used when there is a need to
    /// track individual resource items using references to actual objects.
    /// In other words, a <see cref="TrackedResource"/> represents a pool
    /// of CLR <see cref="object"/>s that are tracked by their references.
    /// </para>
    /// <para>
    /// Items acquired from a <see cref="TrackedResource"/> will always
    /// provide a reference through task activation data (the <c>data</c>
    /// parameter of the <see cref="Task.ExecuteTask"/> mehtod) or the
    /// <see cref="Process.ActivationData"/> property.  When a
    /// <see cref="TrackedResource"/> is acquired, these will always be
    /// reference to a live CLR <see cref="object"/>.
    /// </para>
    /// </remarks>
    public class TrackedResource : Resource
	{
        /// <summary>
        /// The objects (resource items) that are tracked by this resource.
        /// </summary>
		private IDictionary _members = new Hashtable();
        /// <summary>
        /// The resource items available to be allocated.
        /// </summary>
		private IQueue<object> _free = new FifoQueue<object>();
        /// <summary>
        /// The number of resource items that are out of service.
        /// </summary>
		private int _outOfService;
        /// <summary>
        /// The number of resource items that are in use.
        /// </summary>
		private int _inUse;
        /// <summary>
        /// Whether or not the resource item to release will be automatically
        /// selected.
        /// </summary>
        private bool _autoSelect = true;

        /// <summary>
        /// Create a new, unnamed <see cref="TrackedResource"/> that
        /// contains the given resource items.
        /// </summary>
        /// <param name="items">
        /// The resource items.  None of the objects in
        /// <paramref name="items"/> may be <see langword="null"/>.
        /// </param>
		public TrackedResource(IEnumerable items) : this(null, items)
		{
		}

        /// <summary>
        /// Create a new <see cref="TrackedResource"/> having the given
        /// name and containing the given resource items.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="items">
        /// The resource items.  None of the objects in
        /// <paramref name="items"/> may be <see langword="null"/>.
        /// </param>
        public TrackedResource(string name, IEnumerable items)
			: base(name)
		{
			foreach (object obj in items)
			{
				_members.Add(obj, null);
				_free.Enqueue(obj);
			}
		}

        /// <summary>
        /// Gets or sets whether this <see cref="TrackedResource"/> will
        /// automatically select the resource item to release.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property is <b>true</b>.  It should be set to
        /// <b>false</b> if the client program needs to exactly control which
        /// resource items are released and in which order.
        /// </para>
        /// <para>
        /// Setting <see cref="AutoSelect"/> to <b>false</b> is normally only
        /// useful when acquiring multiple resource items from a
        /// <see cref="TrackedResource"/>.
        /// </para>
        /// </remarks>
        /// <value>
        /// <b>true</b> to automatically select the resource item to release;
        /// or <b>false</b> to require the client program to explicitly specify
        /// the item to release.
        /// </value>
        public bool AutoSelect
        {
            get {return _autoSelect;}
            set {_autoSelect = value;}
        }

		#region IResource Implementation

        /// <summary>
        /// Gets the total number of resources in the pool.
        /// </summary>
        /// <value>
        /// The total number of resources in the pool as an <see cref="int"/>.
        /// </value>
        public override int Count
		{
			get {return _members.Count;}
		}

        /// <summary>
        /// Gets the number of resources that are not currently in use.
        /// </summary>
        /// <value>
        /// The number of resources that are not currently in use as an
        /// <see cref="int"/>.
        /// </value>
        public override int Free
		{
			get {return _free.Count - (_outOfService + Reserved);}
		}

        /// <summary>
        /// Gets the number of resources that are currently in use.
        /// </summary>
        /// <value>
        /// The number of resources that are currently in use as an
        /// <see cref="int"/>.
        /// </value>
        public override int InUse
		{
			get {return _inUse;}
		}

        /// <summary>
        /// Gets or sets the number of resources that are out of service.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Out-of-service resources may not be acquired from the pool.  If
        /// <b>OutOfService</b> is set to a value greater or equal to
        /// <see cref="Count"/>, then all resources are out of service and
        /// all subsequent calls to <see cref="Resource.Acquire"/> will block.
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
        public override int OutOfService
		{
			get {return _outOfService;}
			set
			{
                if (value < 0)
                {
                    throw new ArgumentException("Value cannot be negative.");
                }
                int oldValue = _outOfService;
				_outOfService = value > Count ? Count : value;
                if (oldValue > _outOfService)
                    ResumeWaiting();
			}
		}

		#endregion

        /// <summary>
        /// Allocate a resource item.
        /// </summary>
        /// <remarks>
        /// Calling this method will increment the in-use count by one and
        /// remove an item from the free list.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If there are no free resources (i.e. <see cref="Free"/> is less
        /// than one).
        /// </exception>
        /// <returns>
        /// Returns a reference to one of the resource items originally
        /// passed to the constructor.  Will never be <see langword="null"/>.
        /// </returns>
        protected override object AllocateResource()
		{
			if (_free.Count < 1)
			{
				throw new InvalidOperationException("No free resources.");
			}

			_inUse++;

			return _free.Dequeue();
		}

        /// <summary>
        /// Deallocate a resource item.
        /// </summary>
        /// <remarks>
        /// Calling this method will decrement the in-use count by one and
        /// return <paramref name="item"/> to the free list.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If there are no resources in use (i.e. <see cref="InUse"/> is less
        /// than one).
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="item"/> is not a member of this
        /// <see cref="TrackedResource"/>.
        /// </exception>
        /// <param name="item">
        /// Must be a resource item obtained through a call to
        /// <see cref="AllocateResource"/>.
        /// </param>
        protected override void DeallocateResource(object item)
		{
			if (_inUse < 1)
			{
				throw new InvalidOperationException("No resources in use.");
			}
			if (item == null)
			{
				throw new ArgumentNullException("'item' cannot be null.");
			}
			if (!_members.Contains(item))
			{
				throw new ArgumentException("Not a member " + item.ToString());
			}

			_inUse--;
			_free.Enqueue(item);
		}

        /// <summary>
        /// Select and return a particular resource item to release.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will return <see langword="null"/> if
        /// <see cref="AutoSelect"/> is set to <b>false</b>.  If
        /// <see cref="AutoSelect"/> is <b>true</b> (the default), this method
        /// will select an resource item from <paramref name="items"/> to
        /// release. If only one item is owned by <paramref name="owner"/>,
        /// that item is selected for release.  If <paramref name="owner"/>
        /// owns multiple items belonging to this <see cref="TrackedResource"/>
        /// then the first item acquired is selected for release.
        /// </para>
        /// <para>
        /// This method must be overridden to change the selection policy.
        /// </para>
        /// </remarks>
        /// <param name="owner">
        /// The <see cref="Task"/> that is the actual resource owner.
        /// </param>
        /// <param name="items">
        /// An immutable <see cref="IList"/> of resource items owned by
        /// <paramref name="owner"/>.  This should never be will be <see langword="null"/>
        /// if the <see cref="Resource"/> is not tracking individual items.
        /// </param>
        /// <returns>A resource item to release.</returns>
        protected override object SelectItemToRelease(Task owner, IList items)
        {
            object item;

            if (AutoSelect)
            {
                System.Diagnostics.Debug.Assert(items != null);
                System.Diagnostics.Debug.Assert(items.Count > 0);
                item = items[0];
            }
            else
                item = null;

            return item;
        }
	}
}
