//=============================================================================
//=  $Id: AcquireConsumable.cs 184 2006-10-14 18:46:48Z eroe $
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
    /// A <see cref="Task"/> used to acquire one or more consumable items
    /// from a from a <see cref="Consumable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use one of the <b>IConsumable.Acquire</b>
    /// methods which will return the appropriate <see cref="Task"/> for
    /// acquiring consumable items on behalf of a client <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="Consumable"/>.
    /// </para>
    /// </remarks>
    public class AcquireConsumable : ProxyTask<Consumable>
    {
        /// <summary>
        /// The number of consumable units to request.
        /// </summary>
        private int _quantity;
        /// <summary>
        /// Set to <b>true</b> if the <see cref="AcquireConsumable"/> task
        /// has made the request from a <see cref="Consumable"/>.
        /// </summary>
        private bool _requestMade;
        /// <summary>
        /// The maximum wait time before giving up waiting on the
        /// <see cref="Consumable"/>.
        /// </summary>
        private long _maxwait;

        /// <overloads>
        /// Create and initialize an AcquireConsumable task.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="AcquireConsumable"/> task that will acquire
        /// from the specified <see cref="Consumable"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="consumable">
        /// The <see cref="Consumable"/> from which to acquire an item.
        /// </param>
        /// <param name="quantity">
        /// The number of consumable units to request.
        /// </param>
        public AcquireConsumable(Simulation sim, Consumable consumable,
                                 int quantity)
            : this(sim, consumable, quantity, 0L)
        {
        }

        /// <summary>
        /// Create a new <see cref="AcquireConsumable"/> task that will acquire
        /// from the specfied <see cref="Consumable"/> with a timeout.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="consumable">
        /// The <see cref="Consumable"/> from which to acquire an item.
        /// </param>
        /// <param name="quantity">
        /// The number of consumable units to request.
        /// </param>
        /// <param name="maxWait">
        /// The maximum time to wait for <paramref name="consumable"/>.
        /// </param>
        public AcquireConsumable(Simulation sim, Consumable consumable,
                                 int quantity, long maxWait)
            : base(sim, consumable)
        {
            if (maxWait < 0L)
            {
                throw new ArgumentException("'maxWait' cannot be negative.");
            }
            if (quantity < 0)
            {
                throw new ArgumentException("'quantity' cannot be negative.");
            }

            _quantity = quantity;
            _maxwait = maxWait;
        }

        /// <summary>
        /// Gets the maximum wait time.
        /// </summary>
        /// <remarks>
        /// The maximum wait time is relative to the current simulation time.
        /// If <see cref="MaxWait"/> is zero (0L), the
        /// <see cref="AcquireConsumable"/> task will wait for a resource
        /// indefinitely.
        /// </remarks>
        /// <value>
        /// The maximum wait time as an <see cref="long"/>.
        /// </value>
        public long MaxWait
        {
            get { return _maxwait; }
        }

        /// <summary>
        /// Gets the number of consumable units to request from a
        /// <see cref="Consumable"/>.
        /// </summary>
        /// <value>
        /// The number of consumable units to request as an
        /// <see cref="int"/>.
        /// </value>
        public int Quantity
        {
            get { return _quantity; }
        }

        /// <summary>
        /// Attempt to acquire one or more consumable items from a
        /// <see cref="Consumable"/> on behalf of a client <see cref="Task"/>.
        /// </summary>
        /// <param name="activator">
        /// The object that activated this <see cref="AcquireConsumable"/> task.
        /// </param>
        /// <param name="data">
        /// Not used.  The <paramref name="activator"/> should normally pass
        /// <see langword="null"/> for this parameter.
        /// </param>
        protected override void ExecuteTask(object activator, object data)
        {
            System.Diagnostics.Debug.Assert(activator != Blocker);
			if (!_requestMade)
			{
				if (Client != null)
				{
					_requestMade = true;
                    if (!Blocker.RemoveUnits(this) && MaxWait > 0)
                    {
                        Activate(null, MaxWait, null, Priority);
                    }
				}
			}
			else
			{
				ResumeAll();
			}
		}

        /// <summary>
        /// Return allocated consumable units back to the
        /// <see cref="Consumable"/> in the event the
        /// <see cref="ProxyTask&lt;T&gt;.Client"/> is canceled or the event
        /// that runs the <see cref="ProxyTask&lt;T&gt;.Client"/> is canceled.
        /// </summary>
        /// <remarks>
        /// This method is used as a <see cref="DeferredDataCallback"/>
        /// delegate.  The delegate is allocated by the
        /// <see cref="Consumable.RemoveUnits"/> method.
        /// </remarks>
        /// <param name="evt">
        /// The <see cref="ActivationEvent"/> making the data request.
        /// </param>
        /// <returns>Always returns <see langword="null"/>.</returns>
        internal object ReturnUnits(ActivationEvent evt)
        {
            if (!evt.IsPending)
            {
                Blocker.AddUnits(_quantity);
            }

            return null;
        }
    }
}
