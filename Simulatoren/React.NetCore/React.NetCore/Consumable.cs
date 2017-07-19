//=============================================================================
//=  $Id: Consumable.cs 168 2006-09-04 17:31:20Z eroe $
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
using React.Queue;
using React.Tasking;

namespace React
{
	/// <summary>
	/// A concrete implementation of <see cref="IConsumable"/>.
	/// </summary>
	public class Consumable : Blocking<AcquireConsumable>, IConsumable
	{
        /// <summary>
        /// The number of available consumable units.
        /// </summary>
        private int _count;
        /// <summary>
        /// The wait queue used to block <see cref="AcquireConsumable"/> tasks.
        /// </summary>
        private IQueue<AcquireConsumable> _waitQ;

        /// <overloads>Create and initialize a new Consumable.</overloads>
        /// <summary>
        /// Create an unnamed <see cref="Consumable"/> which contains
        /// no consumable units.
        /// </summary>
		public Consumable() : this(null, 0)
		{
		}

        /// <summary>
        /// Create a <see cref="Consumable"/> having the given name and
        /// containing no consumable units.
        /// </summary>
        /// <param name="name">The name.</param>
        public Consumable(string name) : this(name, 0)
        {
        }

        /// <summary>
        /// Create an unnamed <see cref="Consumable"/> that contains the
        /// specified number of consumable units.
        /// </summary>
        /// <param name="quantity">The number of consumable units.</param>
        public Consumable(int quantity) : this(null, quantity)
        {
        }

        /// <summary>
        /// Create a <see cref="Consumable"/> having the given name
        /// and containing the specified number of consumable units.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="quantity">The number of consumable units.</param>
        public Consumable(string name, int quantity) : base(name)
        {
            if (quantity < 0)
                throw new ArgumentException("quantity was negative.");
            _count = quantity;
        }

        #region IConsumable Members

        /// <summary>
        /// Gets the number of consumable units currently available.
        /// </summary>
        /// <value>
        /// The number of unit available as an <see cref="int"/>.
        /// </value>
        public int Count
        {
            get { return _count; }
        }

        /// <overloads>
        /// Attempt to acquire one or more consumable units from the pool.
        /// </overloads>
        /// <summary>
        /// Attempt to acquire a single consumable unit from the pool.
        /// </summary>
        /// <param name="requestor">
        /// The <see cref="Task"/> that is requesting to acquire a consumable
        /// unit from the pool.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> which will acquire a consumable unit from
        /// the pool on behalf of <paramref name="requestor"/>.
        /// </returns>
        [BlockingMethod]
        public Task Acquire(Task requestor)
        {
            return Acquire(requestor, 1);
        }

        /// <summary>
        /// Attempts to acquire the specified number of consumable units from
        /// the pool.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="requestor"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="requestor">
        /// The <see cref="Task"/> that is requesting one or more consumable
        /// units  from the pool.
        /// </param>
        /// <param name="quantity">
        /// The number of units requested.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> which will acquire the given number of
        /// consumable units from the pool on behalf of
        /// <paramref name="requestor"/>.
        /// </returns>
        [BlockingMethod]
        public Task Acquire(Task requestor, int quantity)
        {
            if (requestor == null)
                throw new ArgumentNullException("requestor");

            return new AcquireConsumable(requestor.Simulation, this, quantity);
        }

        /// <summary>
        /// Adds consumable units to the <see cref="IConsumable"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="task"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="task">
        /// The <see cref="Task"/> requesting the <see cref="IConsumable"/>
        /// be re-supplied.
        /// </param>
        /// <param name="quantity">
        /// The number of units to add to the <see cref="IConsumable"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that will re-supply the
        /// <see cref="IConsumable"/> on behalf of <paramref name="task"/>.
        /// </returns>
        [BlockingMethod]
        public Task Resupply(Task task, int quantity)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            return new ResupplyConsumable(task.Simulation, this, quantity);
        }

        #endregion

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
        public override AcquireConsumable[] GetBlockedTasks(int queueId)
        {
            if (queueId == DefaultQueue || queueId == AllQueues)
                return GetBlockedTasks(_waitQ);

            throw new ArgumentException("Invalid queue id: " + queueId);
        }

        /// <summary>
        /// Resume as many blocked <see cref="Task"/>s as there are available
        /// consumable items to satisfy requests.
        /// </summary>
        protected virtual void ResumeWaiting()
        {
            bool enough = Count > 0;
            while (enough && BlockCount > 0)
            {
                AcquireConsumable task = _waitQ.Peek();
                if (!task.Canceled && task.Client != null)
                {
                    enough = task.Quantity <= Count;
                    if (enough)
                    {
                        _waitQ.Dequeue();
                        RemoveUnits(task);
                    }
                }
                else
                {
                    // Throw away canceled tasks.
                    _waitQ.Dequeue();
                }
            }
        }

        //====================================================================
        //====                  Internal Implementation                   ====
        //====================================================================

        /// <summary>
        /// Removes consumable units from the <see cref="Consumable"/>.
        /// </summary>
        /// <param name="task">
        /// The <see cref="AcquireConsumable"/> task that has requested one
        /// or more consumable units.
        /// </param>
        /// <returns>
        /// <b>true</b> if the requested number of units were available; or
        /// <b>false</b> if the request could not be immediately satisfied and
        /// <paramref name="task"/> was blocked.
        /// </returns>
        internal bool RemoveUnits(AcquireConsumable task)
        {
            bool mustWait = Count < task.Quantity;

            if (mustWait)
            {
                if (_waitQ == null)
                    _waitQ = CreateBlockingQueue(DefaultQueue);
                _waitQ.Enqueue(task);
            }
            else
            {
                _count -= task.Quantity;
                task.Activate(this, 0L,
                    new DeferredDataCallback(task.ReturnUnits));
            }

            return !mustWait;
        }

        /// <summary>
        /// Adds consumable units to the <see cref="Consumable"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="quantity"/> is negative.
        /// </exception>
        /// <param name="quantity">
        /// The number of units to add to the <see cref="Consumable"/>.
        /// </param>
        internal void AddUnits(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("'quantity' was negative.");
            _count += quantity;
            ResumeWaiting();
        }
    }
}
