//=============================================================================
//=  $Id: Blocking.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React
{
    /// <summary>
    /// Base class for most objects that can block <see cref="Task"/>
    /// instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Blocking&lt;T&gt;"/> is used to enforce a common idiom for
    /// all blocking objects without allowing substantially different types of
    /// blocking objects too much visibility into each other's inner-working.
    /// For example, both <see cref="Task"/> and <see cref="Resource"/> are
    /// derivatives of <see cref="Blocking&lt;T&gt;"/>.  Because of this, each
    /// implements a common method for creating their blocking (wait) queues,
    /// getting a count of the number of blocked <see cref="Task"/>s on the
    /// wait queues, etc.  However, neither <see cref="Task"/> nor
    /// <see cref="Resource"/> directly exposes the underlying wait queues
    /// to each other.  This helps ensure that dissimilar blocking objects
    /// can't intentionally or unintentionally affect each other.
    /// </para>
    /// <para>
    /// Of course, by not exposing the wait queue even to sub-classes, it may
    /// make it difficult for certain functions to be carried out.  As a brief
    /// aside, the <see cref="Task"/> class actually does expose its wait queue
    /// to sub-classes through the <see cref="Task.WaitQueue"/> property, but
    /// this is not the norm for the other blocking objects.
    /// </para>
    /// <para>
    /// If you absolutely, positively need access to a blocking object's wait
    /// queue (or queues), it can be easily accomplished by overriding the
    /// <see cref="CreateBlockingQueue"/> method and simply hanging onto the
    /// returned <see cref="IQueue&lt;T&gt;"/> reference that you return to
    /// the caller.  One thing to bear in mind, however, is that most of the
    /// React.NET blocking objects create their wait queue(s) on demand, so
    /// you're not immediately guaranteed access to the wait queue using these
    /// steps.
    /// </para>
    /// <para>
    /// Another consideration when sub-classing <see cref="Blocking&lt;T&gt;"/>
    /// is the types of wait queues the <see cref="CreateBlockingQueue"/>
    /// will return.  This is not an issue for a blocking object that has only
    /// one wait queue, but if multiple wait queues are required, they must be
    /// of the same type.  The primary reason for this restriction is that
    /// C# generics do not allow downcasting of generic types whose type
    /// parameters differ.  For example, you can't create an
    /// <see cref="IQueue&lt;AcquireConsumable&gt;"/> and return it as an
    /// <see cref="IQueue&lt;Task&gt;"/>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The type of <see cref="Task"/> which can be blocked.
    /// </typeparam>
    public abstract class Blocking<T> where T : Task
    {
        /// <summary>
        /// Queue identifer for retrieving information about all queues.
        /// </summary>
        public const int AllQueues = -1;
        /// <summary>
        /// Queue identifier for the default wait queue.
        /// </summary>
        public const int DefaultQueue = 0;

        /// <summary>
        /// The name of the <see cref="Blocking&lt;T&gt;"/> object.
        /// </summary>
        private string _name;

        /// <summary>
        /// Create a new, unnamed <see cref="Blocking&lt;T&gt;"/> instance.
        /// </summary>
        protected Blocking() : this(null)
        {
        }

        /// <summary>
        /// Create a new <see cref="Blocking&lt;T&gt;"/> instance having the
        /// specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        protected Blocking(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets or sets the <see cref="Blocking&lt;T&gt;"/> object's name.
        /// </summary>
        /// <value>
        /// The name as a <see cref="string"/>.
        /// </value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the number of <see cref="Task"/>s blocking on this
        /// <see cref="Blocking&lt;T&gt;"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property returns the count of all blocked
        /// <see cref="Task"/>s without regard for which wait queue they
        /// might actually be blocking upon.  In most cases, there is only
        /// a single wait queue and therefore <see cref="BlockCount"/>
        /// provides adequate insight into the number of waiting tasks.
        /// When a <see cref="Blocking&lt;T&gt;"/> object has multiple wait
        /// queues, it may be necessary to query the per-queue count using the
        /// <see cref="GetBlockCount"/> method rather than this property.
        /// </para>
        /// <para>
        /// This property simply calls <c>GetBlockCount(AllQueues)</c>.
        /// </para>
        /// </remarks>
        /// <value>
        /// The number of blocked <see cref="Task"/>s as an
        /// <see cref="int"/>.
        /// </value>
        public int BlockCount
        {
            get { return GetBlockCount(AllQueues); }
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
        public abstract int GetBlockCount(int queueId);

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
        public abstract T[] GetBlockedTasks(int queueId);

        /// <summary>
        /// Extract the <see cref="Task"/> instances contained in the specified
        /// wait queue.
        /// </summary>
        /// <param name="waitQueue">The wait queue.</param>
        /// <returns>
        /// An array of <see cref="Task"/>s contained in
        /// <paramref name="waitQueue"/>.
        /// </returns>
        protected static T[] GetBlockedTasks(IQueue<T> waitQueue)
        {
            T[] tasks;
            if (waitQueue != null)
            {
                tasks = new T[waitQueue.Count];
                if (tasks.Length > 0)
                {
                    waitQueue.CopyTo(tasks, 0);
                }
            }
            else
            {
                tasks = new T[0];
            }

            return tasks;
        }

        /// <summary>
        /// Extract a consolidated array of <see cref="Task"/> instances
        /// contained in each of the given wait queues.
        /// </summary>
        /// <param name="waitQueues">
        /// An array of <see cref="IQueue&lt;T&gt;"/> instances being used as
        /// wait queues.
        /// </param>
        /// <returns>
        /// An array of <see cref="Task"/>s contained in each
        /// <see cref="IQueue&lt;T&gt;"/> in <paramref name="waitQueues"/>
        /// </returns>
        protected static T[] GetBlockedTasks(IQueue<T>[] waitQueues)
        {
            T[] tasks;
            if (waitQueues != null && waitQueues.Length > 0)
            {
                int i;
                int nTasks = 0;
                for (i = 0; i < waitQueues.Length; i++)
                {
                    nTasks = waitQueues[i].Count;
                }

                tasks = new T[nTasks];

                int ndx = 0;
                for (i = 0; i < waitQueues.Length; i++)
                {
                    waitQueues[i].CopyTo(tasks, ndx);
                    ndx += waitQueues[i].Count;
                }
            }
            else
            {
                tasks = new T[0];
            }

            return tasks;
        }

        /// <summary>
        /// Create the specified wait queue.
        /// </summary>
        /// <remarks>
        /// This method allows a <see cref="Blocking&lt;T&gt;"/> object to
        /// support one or more wait queues with the creation of each queue
        /// taking place in a centralized location.  By default, this method
        /// supports only the <see cref="Blocking&lt;T&gt;.DefaultQueue"/>
        /// queue identifier.  The default queue is created as a
        /// <see cref="FifoQueue&lt;T&gt;"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="queueId"/> is not a valid queue identifier.
        /// </exception>
        /// <param name="queueId">
        /// The queue identifier.  Must not be <see cref="AllQueues"/>.
        /// </param>
        /// <returns>
        /// The wait queue identified by <paramref name="queueId"/>.
        /// </returns>
        protected virtual IQueue<T> CreateBlockingQueue(int queueId)
        {
            if (queueId != DefaultQueue)
                throw new ArgumentException("Invalid queue id: " + queueId);

            return new FifoQueue<T>();
        }
    }
}
