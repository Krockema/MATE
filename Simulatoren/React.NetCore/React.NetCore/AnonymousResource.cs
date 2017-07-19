//=============================================================================
//=  $Id: AnonymousResource.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React
{
	/// <summary>
	/// A <see cref="Resource"/> that does not track its resource items as
    /// actual objects.
    /// <seealso cref="TrackedResource"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="AnonymousResource"/> can be created directly or via
    /// the <see cref="Resource.Create(int)"/> factory method.
    /// </para>
    /// <para>
    /// <see cref="AnonymousResource"/>s are used when there is no need to
    /// track individual resource items using references to actual objects.
    /// In other words, an <see cref="AnonymousResource"/> represents a pool
    /// of "things" that are tracked solely by the number of such things in
    /// the pool.
    /// </para>
    /// <para>
    /// Items acquired from an <see cref="AnonymousResource"/> will never
    /// provide a reference through task activation data (the <c>data</c>
    /// parameter of the <see cref="Task.ExecuteTask"/> mehtod) or the
    /// <see cref="Process.ActivationData"/> property.  When an
    /// <see cref="AnonymousResource"/> is acquired, these will always be
    /// <see langword="null"/>.
    /// </para>
    /// </remarks>
	public class AnonymousResource : Resource
	{
        /// <summary>
        /// The total number of items in the pool.
        /// </summary>
		private int _count;
        /// <summary>
        /// The number of items currently in use.
        /// </summary>
		private int _inUse;
        /// <summary>
        /// The number of items that are not in service.
        /// </summary>
		private int _outOfService;

        /// <overloads>
        /// Create and initialize a new AnonymousResource.
        /// </overloads>
        /// <summary>
        /// Create a new, unnamed <see cref="AnonymousResource"/> that contains
        /// one (1) item.
        /// </summary>
		public AnonymousResource() : this(1)
		{
		}

        /// <summary>
        /// Create a new, unnamed <see cref="AnonymousResource"/> that contains
        /// the specified number of items.
        /// </summary>
        /// <param name="count">
        /// The number of items in the <see cref="AnonymousResource"/>.
        /// </param>
		public AnonymousResource(int count) : this(null, count)
		{
		}

        /// <summary>
        /// Create a new <see cref="AnonymousResource"/> having the specified
        /// name and containing the given number of items.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="count">
        /// The number of items in the <see cref="AnonymousResource"/>.
        /// </param>
        public AnonymousResource(string name, int count)
            : base(name)
		{
			if (count < 1)
			{
				throw new ArgumentException(
					"'count' cannot be less than one (1).");
			}
			_count = count;
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
			get {return _count;}
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
			get {return _count - (_inUse + _outOfService + Reserved);}
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
        /// Calling this method will increment the in-use count by one.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If there are no free resources (i.e. <see cref="Free"/> is less
        /// than one).
        /// </exception>
        /// <returns>
        /// Always returns <see langword="null"/>.
        /// </returns>
		protected override object AllocateResource()
		{
			if (Free + Reserved < 1)
			{
				throw new InvalidOperationException("No free resources.");
			}

			_inUse++;

			return null;  // Anonymous, so just return null.
		}

        /// <summary>
        /// Deallocate a resource item.
        /// </summary>
        /// <remarks>
        /// Calling this method will decrement the in-use count by one.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If there are no resource in use (i.e. <see cref="InUse"/> is less
        /// than one).
        /// </exception>
        /// <param name="item">Not used.</param>
		protected override void DeallocateResource(object item)
		{
			if (_inUse < 1)
			{
				throw new InvalidOperationException("No resources in use.");
			}

			_inUse--;
		}

        /// <summary>
        /// Select and return a particular resource item to release.
        /// </summary>
        /// <remarks>
        /// This method always returns <see langword="null"/> as an
        /// <see cref="AnonymousResource"/> does not keep track of
        /// individual resource items.
        /// </remarks>
        /// <param name="owner">
        /// The <see cref="Task"/> that is the actual resource owner.
        /// </param>
        /// <param name="items">
        /// An immutable <see cref="IList"/> of resource items owned by
        /// <paramref name="owner"/>.  This will be <see langword="null"/>
        /// if the <see cref="Resource"/> is not tracking individual items.
        /// </param>
        /// <returns>Always returns <see langword="null"/>.</returns>
        protected override object SelectItemToRelease(Task owner, IList items)
        {
            return null;
        }
	}
}
