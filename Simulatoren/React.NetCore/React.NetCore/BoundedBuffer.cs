//=============================================================================
//=  $Id: BoundedBuffer.cs 183 2006-10-14 17:54:20Z eroe $
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
using React.Queue;
using React.Tasking;

namespace React
{
    /// <summary>
    /// A concrete implementation of <see cref="IBoundedBuffer"/>.
    /// </summary>
    public class BoundedBuffer : Blocking<Task>, IBoundedBuffer
    {
        /// <summary>
        /// Constant used to indicate the <see cref="BoundedBuffer"/> has no
        /// limit on its capacity.
        /// </summary>
        /// <remarks>
        /// This value is defined as <see cref="Int32.MaxValue"/>.
        /// </remarks>
        public const int Infinite = Int32.MaxValue;
        /// <summary>
        /// Queue identifier for the <em>consumer</em> wait queue.
        /// </summary>
        public const int ConsumerQueueId = 1;
        /// <summary>
        /// Queue identifier for the <em>producer</em> wait queue.
        /// </summary>
        public const int ProducerQueueId = 2;

        /// <summary>The buffer capacity (max size).</summary>
        private int _capacity;
        /// <summary>The number of items in the buffer.</summary>
        /// <remarks>
        /// Used only when <see cref="_items"/> is <see langword="null"/>;
        /// otherwise the count is obtained from <see cref="_items"/>.
        /// </remarks>
        private int _count;
        /// <summary>The items added to the buffer.</summary>
        private IQueue<object> _items;
        /// <summary>
        /// The wait queue for consumer tasks.
        /// </summary>
        private IQueue<Task> _consumerQ;
        /// <summary>
        /// The wait queue for producere tasks.
        /// </summary>
        private IQueue<Task> _producerQ;
        /// <summary>
        /// The number of items reserved by consumer tasks.
        /// </summary>
        private int _nConsumerReserved;

        /// <overloads>Create and initialize a BoundedBuffer.</overloads>
        /// <summary>
        /// Create an empty, unnamed <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="BoundedBuffer"/> will have a capacity of
        /// zero (0) items.
        /// </remarks>
        public BoundedBuffer() : this(null, 0)
        {
        }

        /// <summary>
        /// Create an empty <see cref="BoundedBuffer"/> having the specified
        /// name.
        /// </summary>
        /// <remarks>
        /// The <see cref="BoundedBuffer"/> will have a capacity of
        /// zero (0) items.
        /// </remarks>
        /// <param name="name">The name.</param>
        public BoundedBuffer(string name) : this(name, 0)
        {
        }

        /// <summary>
        /// Create an empty, unnamed <see cref="BoundedBuffer"/> having
        /// the specified capacity.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="maxSize"/> is less than zero.
        /// </exception>
        /// <param name="maxSize">The buffer capacity.</param>
        public BoundedBuffer(int maxSize) : this(null, maxSize)
        {
        }

        /// <summary>
        /// Create an empty <see cref="BoundedBuffer"/> having the specified
        /// name and capacity.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="maxSize"/> is less than zero.
        /// </exception>
        /// <param name="name">The name.</param>
        /// <param name="maxSize">The buffer capacity.</param>
        public BoundedBuffer(string name, int maxSize) : base(name)
        {
            Capacity = maxSize;
        }

        #region IBoundedBuffer Members

        /// <summary>
        /// Gets the number of items in the <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="Count"/> is zero (0), <see cref="Task"/>s
        /// returned by <see cref="Get"/> will block.  When
        /// <see cref="Count"/> is equal or greater than <see cref="Capacity"/>,
        /// <see cref="Task"/>s returned by <see cref="Put"/> will block.
        /// </remarks>
        /// <value>
        /// The number of items in the <see cref="BoundedBuffer"/> as an
        /// <see cref="int"/>.
        /// </value>
        public int Count
        {
            get
            {
                return _items != null ? _items.Count : _count;
            }
        }

        /// <summary>
        /// Gets or sets the capacity of the <see cref="BoundedBuffer"/>.
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
        /// The capacity of the <see cref="BoundedBuffer"/> as an
        /// <see cref="int"/>.
        /// </value>
        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Capacity cannot be negative.");
                _capacity = value;
            }
        }

        /// <summary>
        /// Attempt to remove an item from the <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="consumer"/> must block on the returned
        /// <see cref="Task"/> in order for an item to be removed from the
        /// <see cref="BoundedBuffer"/>.  The removed item will be passed
        /// to <paramref name="consumer"/> as the activation data.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="consumer"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="consumer">
        /// The consumer <see cref="Task"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will remove an item from the
        /// <see cref="BoundedBuffer"/> on behalf of
        /// <paramref name="consumer"/>.
        /// </returns>
        [BlockingMethod]
        public Task Get(Task consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException("consumer");

            return new BufferGet(consumer.Simulation, this);
        }

        /// <summary>
        /// Attempt to add an item to the <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="producer"/> must block on the returned
        /// <see cref="Task"/> in order for <paramref name="item"/> to be
        /// added to the <see cref="BoundedBuffer"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="producer"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="producer">
        /// The producer <see cref="Task"/>.
        /// </param>
        /// <param name="item">
        /// The object to place into the <see cref="BoundedBuffer"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will add an item to the
        /// <see cref="BoundedBuffer"/> on behalf of
        /// <paramref name="producer"/>.
        /// </returns>
        [BlockingMethod]
        public Task Put(Task producer, object item)
        {
            if (producer == null)
                throw new ArgumentNullException("producer");

            return new BufferPut(producer.Simulation, this, item);
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
            int count;
            switch (queueId)
            {
                case AllQueues:
                    count = GetBlockCount(ConsumerQueueId) +
                        GetBlockCount(ProducerQueueId);
                    break;
                case ConsumerQueueId:
                    count = _consumerQ != null ? _consumerQ.Count : 0;
                    break;
                case ProducerQueueId:
                    count = _producerQ != null ? _producerQ.Count : 0;
                    break;
                default:
                    throw new ArgumentException("Invalid queue id: " + queueId);

            }
            return count;
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
        public override Task[] GetBlockedTasks(int queueId)
        {
            Task[] tasks;

            switch (queueId)
            {
                case AllQueues:
                    IQueue<Task>[] array = new IQueue<Task>[2];
                    array[0] = _consumerQ;
                    array[1] = _producerQ;
                    tasks = GetBlockedTasks(array);
                    break;
                case ConsumerQueueId:
                    tasks = GetBlockedTasks(_consumerQ);
                    break;
                case ProducerQueueId:
                    tasks = GetBlockedTasks(_producerQ);
                    break;
                default:
                    throw new ArgumentException("Invalid queue id: " + queueId);
            }

            return tasks;
        }

        /// <summary>
        /// Create the specified wait queue.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="queueId"/> is not one of
        /// <see cref="ConsumerQueueId"/> or <see cref="ProducerQueueId"/>.
        /// </exception>
        /// <param name="queueId">
        /// The queue identifier.  Must be one of <see cref="ConsumerQueueId"/>
        /// or <see cref="ProducerQueueId"/>.
        /// </param>
        /// <returns>
        /// The wait queue identified by <paramref name="queueId"/>.
        /// </returns>
        protected override IQueue<Task> CreateBlockingQueue(int queueId)
        {
            if (queueId == ConsumerQueueId)
                return new React.Queue.FifoQueue<Task>();
            if (queueId == ProducerQueueId)
                return new React.Queue.FifoQueue<Task>();

            throw new ArgumentException("Invalid queue id: " + queueId);
        }

        /// <summary>
        /// Attempt to resume as many waiting consumer <see cref="Task"/>s as
        /// possible.
        /// </summary>
        protected void ResumeWaitingConsumers()
        {
            while (Count - _nConsumerReserved > 0 &&
                GetBlockCount(ConsumerQueueId) > 0)
            {
                BufferGet task = (BufferGet)_consumerQ.Dequeue();
                if (!task.Canceled && task.Client != null)
                {
                    GetObject(task);
                }
            }
        }

        /// <summary>
        /// Attempt to resume as many waiting producer <see cref="Task"/>s as
        /// possible.
        /// </summary>
        protected void ResumeWaitingProducers()
        {
            while (Count < Capacity && GetBlockCount(ProducerQueueId) > 0)
            {
                BufferPut task = (BufferPut) _producerQ.Dequeue();
                if (!task.Canceled && task.Client != null)
                {
                    if (task.Item == null)
                        PutObject(task);
                    else
                        PutObject(task, task.Item);
                }
            }
        }

        //====================================================================
        //====                  Internal Implementation                   ====
        //====================================================================

        /// <summary>
        /// Called by the <see cref="BufferGet"/> task as well as the
        /// <see cref="ResumeWaitingConsumers"/> method to attempt to
        /// get an item from the <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="consumer">The <see cref="BufferGet"/> task.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="consumer"/> was blocked.
        /// </returns>
        internal bool GetObject(Task consumer)
        {
            bool blocked = Count - _nConsumerReserved < 1;
            if (blocked)
            {
                if (_consumerQ == null)
                    _consumerQ = CreateBlockingQueue(ConsumerQueueId);
                _consumerQ.Enqueue(consumer);
            }
            else
            {
                _nConsumerReserved++;
                consumer.Activate(this, 0L,
                    new DeferredDataCallback(ConsumerGetOrFree));
            }

            return blocked;
        }

        /// <summary>
        /// Method used as a <see cref="DeferredDataCallback"/> delegate.
        /// </summary>
        /// <remarks>
        /// This method will decrement the number of consumer reservations.
        /// It will also either: (1) remove an object from the
        /// <see cref="_items"/> queue; or (2) decrement <see cref="_count"/>.
        /// If actual CLR objects are not being put into the
        /// <see cref="BoundedBuffer"/>, the <see cref="_items"/> queue should
        /// be <see langword="null"/> and <see cref="_count"/> is decremented.
        /// </remarks>
        /// <param name="evt">
        /// The <see cref="ActivationEvent"/> requesting data.
        /// </param>
        /// <returns>
        /// An item removed from the <see cref="_items"/> queue or
        /// <see langword="null"/> if actual CLR objects are not being stored
        /// in the <see cref="BoundedBuffer"/>.
        /// </returns>
        private object ConsumerGetOrFree(ActivationEvent evt)
        {
            object obj;
            _nConsumerReserved--;
            if (evt.IsPending)
            {
                if (_items != null)
                {
                    obj = _items.Dequeue();
                }
                else
                {
                    _count--;
                    obj = null;
                }
            }
            else
            {
                obj = null;
            }
            ResumeWaitingProducers();
            return obj;
        }

        /// <summary>
        /// Called by the <see cref="BufferPut"/> task and the
        /// <see cref="ResumeWaitingProducers"/> method to put a
        /// counted item into the buffer.
        /// </summary>
        /// <param name="producer">
        /// The <see cref="BufferPut"/> task.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="producer"/> was blocked.
        /// </returns>
        internal bool PutObject(BufferPut producer)
        {
            return PutObject(producer, null, false);
        }

        /// <summary>
        /// Called by the <see cref="BufferPut"/> task and the
        /// <see cref="ResumeWaitingProducers"/> method to put an
        /// object into the buffer.
        /// </summary>
        /// <param name="producer">
        /// The <see cref="BufferPut"/> task.
        /// </param>
        /// <param name="item">
        /// The object to put into the <see cref="BoundedBuffer"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="producer"/> was blocked.
        /// </returns>
        internal bool PutObject(BufferPut producer, object item)
        {
            return PutObject(producer, item, true);
        }

        /// <summary>
        /// Attempt to put an item (either a real CLR objecr or a "virtual"
        /// item) into the <see cref="BoundedBuffer"/>.
        /// </summary>
        /// <param name="producer">
        /// The <see cref="BufferPut"/> task.
        /// </param>
        /// <param name="item">
        /// The object to put into the <see cref="BoundedBuffer"/>.
        /// </param>
        /// <param name="useItem">
        /// <b>true</b> if the <paramref name="item"/> parameter should be used.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="producer"/> was blocked.
        /// </returns>
        private bool PutObject(BufferPut producer, object item, bool useItem)
        {
            bool blocked = Count >= Capacity;
            if (blocked)
            {
                if (_producerQ == null)
                    _producerQ = CreateBlockingQueue(ProducerQueueId);
                _producerQ.Enqueue(producer);
            }
            else
            {
                if (useItem)
                {
                    if (_items == null)
                        _items = new React.Queue.FifoQueue<object>();
                    _items.Enqueue(item);
                }
                else
                {
                    _count++;
                }

                producer.Activate(this);
                ResumeWaitingConsumers();
            }

            return blocked;
        }
    }
}
