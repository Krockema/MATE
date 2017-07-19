//=============================================================================
//=  $Id: Resource.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections;
using System.Collections.Generic;
using React.Queue;
using React.Tasking;

namespace React
{
	/// <summary>
	/// Abstract class used as a base for creating <see cref="IResource"/>
    /// implementations.
	/// </summary>
    /// <remarks>
    /// In most cases, <see cref="IResource"/> implementators should use this
    /// class as a base.  There are already two concrete <see cref="Resource"/>
    /// subclasses available: <see cref="AnonymousResource"/> and
    /// <see cref="TrackedResource"/>.  In addition, <see cref="Resource"/>
    /// offers several factory methods that can be used to instantiate
    /// <see cref="IResource"/> objects.
    /// </remarks>
	public abstract class Resource : Blocking<AcquireResource>, IResource
	{
        /// <summary>
        /// Whether or not a <see cref="Task"/> may own more than one resource
        /// item.
        /// </summary>
		private bool _ownMany;
        /// <summary>
        /// An <see cref="IDictionary"/> used to track resource item owners.
        /// </summary>
		private IDictionary<Task, ResourceEntry> _owners;
        /// <summary>
        /// The wait queue used to block <see cref="AcquireResource"/> tasks.
        /// </summary>
        private IQueue<AcquireResource> _waitQ;
        /// <summary>
        /// The number of reserved resources.
        /// </summary>
        private int _nReserved;

		/// <overloads>
		/// Create and initialize a new Resource instance.
		/// </overloads>
		/// <summary>
		/// Create a new <see cref="Resource"/> instance that has no name.
		/// </summary>
        protected Resource() : this(null)
		{
		}

		/// <summary>
		/// Create a new <see cref="Resource"/> instance with the specified
		/// name.
		/// </summary>
		/// <param name="name">The name of the <see cref="Resource"/>.</param>
        protected Resource(string name) : base(name)
		{
		}

		/// <summary>
		/// Gets or sets whether multiple resources from the pool may be
		/// owned by the same <see cref="Task"/>.
		/// </summary>
		/// <remarks>
		/// If a <see cref="Resource"/> does not allow ownership of multiple
		/// resources by a single <see cref="Task"/>, an exception will be
		/// thrown if an owning <see cref="Task"/> calls <see cref="Acquire"/>
		/// before first calling <see cref="Release"/>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the same <see cref="Task"/> may own multiple
		/// resource from the pool; or <b>false</b> if at most one resource
		/// from the pool may be owned by each <see cref="Task"/>. 
		/// </value>
		public bool AllowOwnMany
		{
			get {return _ownMany;}
			set {_ownMany = value;}
		}

        /// <summary>
        /// Gets the number of resources that have been reserved for use by
        /// requesting <see cref="Task"/>s but not yet actually allocated to
        /// those <see cref="Task"/>s.
        /// </summary>
        /// <value>
        /// The number of reserved resources as an <see cref="int"/>.
        /// </value>
        public int Reserved
        {
            get { return _nReserved; }
        }

		#region IResource Implementation

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
        public virtual int Count
		{
			get {return Free + InUse + OutOfService;}
		}

        /// <summary>
        /// Gets the number of resources that are not currently in use.
        /// </summary>
        /// <value>
        /// The number of resources that are not currently in use as an
        /// <see cref="int"/>.
        /// </value>
        public abstract int Free { get; }

        /// <summary>
        /// Gets the number of resources that are currently in use.
        /// </summary>
        /// <value>
        /// The number of resources that are currently in use as an
        /// <see cref="int"/>.
        /// </value>
        public abstract int InUse { get; }

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
        public abstract int OutOfService { get; set; }

        /// <summary>
        /// Attempt to acquire a resource from the pool.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="requestor"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="requestor"/> is already an owner of a resource
        /// from this pool and <see cref="AllowOwnMany"/> is <b>false</b>.
        /// </exception>
        /// <param name="requestor">
        /// The <see cref="Task"/> that is requesting to acquire a resource
        /// from the pool.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> which will acquire a resource from the pool
        /// on behalf of <paramref name="requestor"/>.
        /// </returns>
        [BlockingMethod]
		public virtual Task Acquire(Task requestor)
		{
            if (requestor == null)
                throw new ArgumentNullException("requestor", "cannot be null");

			if (!AllowOwnMany && IsOwner(requestor))
			{
				throw new InvalidOperationException(
					"'requestor' already owns a resource from this pool.");
			}
			return new AcquireResource(requestor.Simulation, this);
		}

        /// <summary>
        /// Releases a previously acquired resource back to the pool.
        /// </summary>
        /// <remarks>
        /// This method invokes <see cref="SelectItemToRelease"/> to
        /// select a resource item to release.  For
        /// <see cref="AnonymousResource"/> instances, the item will be
        /// <see langword="null"/>.  For <see cref="TrackedResource"/>
        /// instances, <see cref="SelectItemToRelease"/> <b>may</b>
        /// return a <see langword="null"/> or a reference to a resource
        /// item owned by <paramref name="owner"/> that should be released.
        /// This allows <see cref="Release"/> to be used to release a
        /// <see cref="TrackedResource"/> item without requiring the calling
        /// program to explicitly specify the item to release.  This
        /// behavior can be disabled on <see cref="TrackedResource"/>
        /// instance through the <see cref="TrackedResource.AutoSelect"/>
        /// property.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="owner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
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
		public virtual Task Release(Task owner)
		{
            if (owner == null)
                throw new ArgumentNullException("owner", "cannot be null");

			if (!IsOwner(owner))
			{
				throw new InvalidOperationException(
					"Task is not a resource owner.");
			}

            ResourceEntry entry = _owners[owner];
            object item = SelectItemToRelease(owner, entry.Items);
			return new ReleaseResource(owner.Simulation, this, item);
		}

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
        /// <paramref name="receiver"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="owner"/> or <paramref name="receiver"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="owner"/> does not own a resource from this
        /// pool; or <paramref name="receiver"/> already owns a resource from
        /// this pool and <see cref="AllowOwnMany"/> is <b>false</b>.
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
		public Task Transfer(Task owner, Task receiver)
		{
            if (owner == receiver)
            {
                throw new ArgumentException(
                    "'owner' was the same as 'receiver'.");
            }
            if (owner == null)
            {
                throw new ArgumentNullException("'owner' was null.");
            }
            if (receiver == null)
            {
                throw new ArgumentNullException("'receiver' was null.");
            }
            if (!IsOwner(owner))
			{
				throw new InvalidOperationException(
					"Task is not a resource owner.");
			}
			if (!AllowOwnMany && IsOwner(receiver))
			{
				throw new InvalidOperationException(
					"'receiver' already owns a resource from this pool.");
			}

            ResourceEntry entry = _owners[owner];
            object item = SelectItemToRelease(owner, entry.Items);
			return new TransferResource(owner.Simulation, this, receiver, item);
		}

		#endregion
	
		/// <summary>
		/// Checks if the given <see cref="Task"/> owns any resources in the
		/// pool.
		/// </summary>
		/// <param name="task">
		/// The <see cref="Task"/> which may own one or more resources in the
		/// resource pool.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="task"/> owns at least one resource
		/// from this resource pool.
		/// </returns>
		public bool IsOwner(Task task)
		{
			return _owners != null ? _owners.ContainsKey(task) : false;
		}
	
		/// <summary>
		/// Gets the number of resources owned by the specified
		/// <see cref="Task"/> (if any).
		/// </summary>
		/// <param name="task">
		/// The <see cref="Task"/> whose resource ownership count will be
		/// computed.
		/// </param>
		/// <returns>
		/// The number of resource from the pool owned by
		/// <paramref name="task"/>.  If <paramref name="task"/> does not
		/// own any resources from the pool, the returned value will be zero
		/// (0).
		/// </returns>
		public int OwnershipCount(Task task)
		{
			int count;
			if (IsOwner(task))
			{
				ResourceEntry entry = _owners[task];
				count = entry.Count;
				System.Diagnostics.Debug.Assert(count > 0);
			}
			else
			{
				count = 0;
			}
			return count;
		}

        /// <summary>
        /// Get the number of <see cref="Task"/> instances blocked on the
        /// specified wait queue.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="queueId"/> is not a valid queue identifier.
        /// </exception>
        /// <param name="queueId">The queue identifier.</param>
        /// <returns>
        /// The number of <see cref="Task"/> instances blocked on the
        /// queue identified by <paramref name="queueId"/>.
        /// </returns>
        public override int GetBlockCount(int queueId)
        {
            if (queueId == DefaultQueue || queueId == AllQueues)
                return _waitQ != null ? _waitQ.Count : 0;

            throw new ArgumentException("Invalid queue id: " + queueId);
        }

        /// <summary>
        /// Gets the <see cref="Task"/> instances blocking on the
        /// wait queue identified by a queue id.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="queueId"/> is not a valid queue identifier.
        /// </exception>
        /// <param name="queueId">The queue identifier.</param>
        /// <returns>
        /// An array of <see cref="Task"/> instances that are currently
        /// contained in the wait queue identified by
        /// <paramref name="queueId"/>.  The returned array will never
        /// by <see langword="null"/>.
        /// </returns>
        public override AcquireResource[] GetBlockedTasks(int queueId)
        {
            if (queueId == DefaultQueue || queueId == AllQueues)
                return GetBlockedTasks(_waitQ);

            throw new ArgumentException("Invalid queue id: " + queueId);
        }

		/// <summary>
		/// Immediately allocate a resource from the pool and return the
		/// associated resource item.
		/// </summary>
		/// <remarks>
		/// Anonymous <see cref="Resource"/> implementations will always
		/// return <see langword="null"/>.  Tracked <see cref="Resource"/>
		/// implementation must return a valid, non-null <see cref="object"/>.
		/// </remarks>
		/// <returns>
		/// The resource item associated with the allocated resource or
		/// <see langword="null"/> if resources are not assocatiated with
		/// arbitrary objects.
		/// </returns>
		protected abstract object AllocateResource();

		/// <summary>
		/// Immediately free (deallocate) a resource and return it to the
		/// pool.
		/// </summary>
		/// <param name="item">
		/// The resource item being freed.  This will be <see langword="null"/>
		/// for anonymous <see cref="Resource"/> implementations.
		/// </param>
		protected abstract void DeallocateResource(object item);

        /// <summary>
        /// Resume as many blocked <see cref="Task"/>s as there are free
        /// resource items.
        /// </summary>
        protected virtual void ResumeWaiting()
        {
            while (Free > 0 && BlockCount > 0)
            {
                AcquireResource task = _waitQ.Dequeue();
                if (!task.Canceled && task.Client != null)
                {
                    RequestResource(task);
                }
            }
        }

        /// <summary>
        /// Select and return a particular resource item to release.
        /// </summary>
        /// <remarks>
        /// This method may simply return <see langword="null"/> if the
        /// <see cref="Resource"/> implementation does not track individual
        /// resource items; otherwise it should select an item from the
        /// <see cref="IList"/> to release.
        /// </remarks>
        /// <param name="owner">
        /// The <see cref="Task"/> that is the actual resource owner.
        /// </param>
        /// <param name="items">
        /// An immutable <see cref="IList"/> of resource items owned by
        /// <paramref name="owner"/>.  This will be <see langword="null"/>
        /// if the <see cref="Resource"/> is not tracking individual items.
        /// </param>
        /// <returns>A resource item to release.</returns>
        protected abstract object SelectItemToRelease(Task owner, IList items);

		//====================================================================
		//====                 Static Factory Methods                     ====
		//====================================================================

		/// <summary>
		/// Create an anonymous <see cref="Resource"/> containing the given
		/// number of in-service resources.
		/// </summary>
		/// <param name="count">
		/// The total number of resources in the pool, all of which are 
		/// in-service.
		/// </param>
		/// <returns>
		/// An anonymous <see cref="Resource"/> containing
		/// <paramref name="count"/> resources, all of which are in-service.
		/// </returns>
		public static Resource Create(int count)
		{
			return Resource.Create(count, 0);
		}

		/// <summary>
		/// Create an anonymous <see cref="Resource"/> containing the
		/// specified number of in-service and out-of-service resources.
		/// </summary>
		/// <remarks>
		/// The total number of resources in the pool is given by
		/// <c>inService + outOfService</c>.
		/// </remarks>
		/// <param name="inService">
		/// The number of resources in the pool that are in-service.
		/// </param>
		/// <param name="outOfService">
		/// The number of resources in the pool that are out-of-service.
		/// </param>
		/// <returns>
		/// An anonymous <see cref="Resource"/> containing
		/// <paramref name="inService"/> in-service resources and
		/// <paramref name="outOfService"/> out-of-service resources.
		/// </returns>
		public static Resource Create(int inService, int outOfService)
		{
			int count = inService + outOfService;
			Resource resource = new AnonymousResource(count);
			resource.OutOfService = outOfService;
			return resource;
		}

		/// <summary>
		/// Create a tracked <see cref="Resource"/> containing the given
		/// objects.
		/// </summary>
		/// <remarks>
		/// Each object in <paramref name="items"/> must be of the same
		/// type.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If <paramref name="items"/> contains objects having differing
		/// types or is empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="items"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="items">
		/// An <see cref="IEnumerable"/> that contains one or more objects
		/// that will be dispensed as resources.
		/// </param>
		/// <returns>
		/// A tracked <see cref="Resource"/> containing the given items.
		/// </returns>
		public static Resource Create(IEnumerable items)
		{
			return new TrackedResource(items);
		}

		//====================================================================
		//====              Internal/Private Implementation               ====
		//====================================================================

        /// <summary>
        /// Gets or frees a reserved resource item.
        /// </summary>
        /// <remarks>
        /// This method decrements the reserve count and, if 
        /// <paramref name="evt"/> is pending it will actually allocate a
        /// resource item to the <see cref="Task"/> which will be run by
        /// <paramref name="evt"/>.
        /// </remarks>
        /// <param name="evt">
        /// The <see cref="ActivationEvent"/> making the data request.
        /// </param>
        /// <returns>
        /// The resource item associated with the allocated resource or
        /// <see langword="null"/> if resources are not assocatiated with
        /// arbitrary objects.  Also returns <see langword="null"/> if
        /// <paramref name="evt"/> is not pending (i.e. the
        /// <see cref="ActivationEvent.IsPending"/> method is <b>false</b>).
        /// </returns>
        private object GetOrFreeReserved(ActivationEvent evt)
        {
            object obj;
            _nReserved--;
            if (evt.IsPending)
            {
                System.Diagnostics.Debug.Assert(evt.Task != null);
                obj = AllocateResource();
                SetOwner(evt.Task, obj);
            }
            else
            {
                obj = null;
            }
            return obj;
        }

        /// <summary>
        /// Invoked by a <see cref="AcquireResource"/> task to request
        /// allocation of a resource.
        /// </summary>
        /// <remarks>
        /// If there are no free resources, <paramref name="task"/> will be
        /// placed on the wait queue.
        /// </remarks>
        /// <param name="task">
        /// The <see cref="AcquireResource"/> task making the request.
        /// </param>
        /// <returns>
        /// <b>true</b> if a resource was allocated; or <b>false</b> if no
        /// resource was available and <paramref name="task"/> was blocked.
        /// </returns>
        internal bool RequestResource(AcquireResource task)
        {
            bool mustWait = Free < 1;
            if (mustWait)
            {
                if (_waitQ == null)
                    _waitQ = CreateBlockingQueue(DefaultQueue);
                _waitQ.Enqueue(task);
            }
            else
            {
                _nReserved++;
                task.Activate(this, 0L,
                    new DeferredDataCallback(GetOrFreeReserved));
            }

            return !mustWait;
        }

		/// <summary>
		/// Invoked by a <see cref="ReleaseResource"/> task to return a
		/// resource to the pool.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="Task"/> that is the actual resource owner.  This
		/// is <b>not</b> the <see cref="ReleaseResource"/> task.
		/// </param>
		/// <param name="item">
		/// The resource item.  This will be <see langword="null"/> for
		/// anonymous resources.
		/// </param>
		internal void ReturnResource(Task owner, object item)
		{
			DeallocateResource(item);
			ClearOwner(owner, item);
            ResumeWaiting();
		}

		/// <summary>
		/// Invoked by a <see cref="TransferResource"/> task to transfer
		/// ownership of a resource from one <see cref="Task"/> to another.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="Task"/> that is the actual resource owner.  This
		/// is <b>not</b> the <see cref="TransferResource"/> task.
		/// </param>
		/// <param name="receiver">
		/// The <see cref="Task"/> that will be granted ownership of the
		/// resource.
		/// </param>
		/// <param name="item">
		/// The resource item.  This will be <see langword="null"/> for
		/// anonymous resources.
		/// </param>
		internal void TransferResource(Task owner, Task receiver, object item)
		{
			if (receiver != null)
			{
				ClearOwner(owner, item);
				SetOwner(receiver, item);
			}
			else
			{
				ReturnResource(owner, item);
			}
		}

        /// <summary>
        /// Record the given <see cref="Task"/> as a resource item owner.
        /// </summary>
        /// <param name="task">
        /// The <see cref="Task"/> that owns <paramref name="item"/>.
        /// </param>
        /// <param name="item">
        /// The reference to a resource item for tracked resources; or
        /// <see langword="null"/> for anonymous resources.
        /// </param>
		internal void SetOwner(Task task, object item)
		{
			if (task != null)
			{
				ResourceEntry entry;
                if (_owners == null)
                    _owners = new Dictionary<Task, ResourceEntry>();

				if (!IsOwner(task))
				{
					if (item == null)
						entry = new ResourceEntry();
					else
						entry = new ResourceEntry(item);
					_owners.Add(task, entry);
				}
				else
				{
					entry = _owners[task];
					if (item == null)
						entry.Add();
					else
						entry.Add(item);
				}
			}
		}

        /// <summary>
        /// Removes the specified <see cref="Task"/> as a resource item owner.
        /// </summary>
        /// <param name="owner">
        /// The <see cref="Task"/> that owns <paramref name="item"/>.
        /// </param>
        /// <param name="item">
        /// A reference to the resource item previously obtained via a call to
        /// <see cref="AllocateResource"/> (for tracked resources); or
        /// <see langword="null"/> (for anonymous resources).
        /// </param>
		private void ClearOwner(Task owner, object item)
		{
			if (owner != null && _owners != null)
			{
				ResourceEntry entry = _owners[owner];
				if (item == null)
					entry.Remove();
				else
					entry.Remove(item);
			
				if (entry.Count < 1)
					_owners.Remove(owner);
			}
		}
    }
}
