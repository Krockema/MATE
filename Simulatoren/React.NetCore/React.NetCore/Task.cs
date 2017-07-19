//=============================================================================
//=  $Id: Task.cs 184 2006-10-14 18:46:48Z eroe $
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
using React.Queue;

namespace React
{
	/// <summary>
	/// An object that carries out some processing during the course of
    /// running a <see cref="Simulation"/>.
    /// <seealso cref="Process"/>
	/// </summary>
	public abstract class Task : Blocking<Task>
	{
		/// <summary>
		/// Schedule time returned when the <see cref="Task"/> is not
		/// scheduled to execute.
		/// </summary>
		/// <remarks>
        /// This value is identical to
        /// <see cref="ActivationEvent.NotScheduled"/>.
		/// </remarks>
        public const long NotScheduled = ActivationEvent.NotScheduled;

		/// <summary>
		/// The simulation context under which the <see cref="Task"/> runs.
		/// </summary>
		private Simulation _sim;
        /// <summary>
        /// The wait queue used to block <see cref="Task"/>s.
        /// </summary>
        private IQueue<Task> _waitQ;
		/// <summary>
		/// The Task instances upon which this task is blocked.
		/// </summary>
		private IList<Task> _blockedOn;
		/// <summary>
		/// The task priority.
		/// </summary>
		private int _priority = TaskPriority.Normal;
		/// <summary>
		/// The temporary elevated task priority.
		/// </summary>
		private int _elevated = TaskPriority.Normal;
		/// <summary>
		/// Flag indicating the task has been canceled.
		/// </summary>
		private bool _cancelFlag;
		/// <summary>
		/// Flag indicating the task has been interrupted.
		/// </summary>
		private bool _intFlag;
		/// <summary>
		/// The <see cref="ActivationEvent"/> which invoked the task.
		/// </summary>
		private ActivationEvent _actevt;

		/// <summary>
		/// Create a new <see cref="Task"/> instance that will run under under
        /// the given simulation context.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="sim"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="sim">The simulation context.</param>
        protected Task(Simulation sim)
		{
			if (sim == null)
			{
				throw new ArgumentNullException("sim");
			}

			this._sim = sim;
		}

		/// <summary>
		/// Gets the simulation context under which the <see cref="Task"/> is
		/// running.
		/// </summary>
		/// <value>
		/// The simulation context as a <see cref="Simulation"/>.
		/// </value>
		public Simulation Simulation
		{
			get {return _sim;}
		}

		/// <summary>
		/// Gets the current simulation time.
		/// </summary>
		/// <remarks>
		/// This is really just a shortcut for <c>task.Simulation.Now</c>.
		/// </remarks>
		/// <value>
		/// The current simulation time as an <see cref="long"/>.
		/// </value>
		public long Now
		{
			get {return _sim.Now;}
		}

		/// <summary>
		/// Gets whether or not the <see cref="Task"/> has been scheduled to
		/// run.
		/// </summary>
		/// <remarks>
		/// A <see cref="Task"/> that has been activated using one of the
		/// <b>Activate</b> methods will be scheduled to run.  Therefore after
		/// calling <b>Activate</b>, <see cref="IsScheduled"/> should always
		/// return <b>true</b>.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="Task"/> has been scheduled.
		/// </value>
		public bool IsScheduled
		{
			get {return _actevt != null && _actevt.IsPending;}
		}

		/// <summary>
		/// Gets the time the <see cref="Task"/> is scheduled to run.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Task"/> is not scheduled to run, this
		/// property will be <see cref="Task.NotScheduled"/>.
		/// </remarks>
		/// <value>
		/// The simulation time the <see cref="Task"/> will run as an
		/// <see cref="long"/>.
		/// </value>
		public long ScheduledTime
		{
			get 
			{
				if (IsScheduled)
					return _actevt.Time;
				else
					return Task.NotScheduled;
			}
		}

        /// <summary>
        /// Gets whether or not the <see cref="Task"/> is blocked (that is,
        /// waiting on other <see cref="Task"/>s)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Immediately after a call to one of the <b>Activate</b> methods,
        /// this property will normally be <b>false</b> as <b>Activate</b>
        /// invokes <see cref="ClearBlocks"/>.  Subsequent calls to
        /// <see cref="WaitOnTask(Task)"/> or <see cref="WaitOnTask(Task,int)"/>
        /// will cause this property to be <b>true</b>.
        /// </para>
        /// <para>
        /// Remember <see cref="IsBlocked"/> is used to check if this
        /// <see cref="Task"/> is waiting on other <see cref="Task"/>s
        /// <b>not</b> to check if this <see cref="Task"/> is blocking other
        /// <see cref="Task"/>s (e.g. other <see cref="Task"/>s are waiting on
        /// this <see cref="Task"/>).
        /// </para>
        /// </remarks>
        /// <value>
        /// <b>true</b> if this <see cref="Task"/> is blocking on one or more
        /// <see cref="Task"/>s.
        /// </value>
        public bool IsBlocked
        {
            get { return _blockedOn != null && _blockedOn.Count > 0; }
        }

        /// <summary>
        /// Gets the <see cref="IQueue&lt;Task&gt;"/> that contains all the
        /// <see cref="Task"/>s which are blocking on this <see cref="Task"/>.
        /// </summary>
        /// <remarks>
        /// The wait queue is created on demand when this property is first
        /// accessed. The <see cref="Blocking&lt;Task&gt;.CreateBlockingQueue"/>
        /// method is used to create the wait queue.
        /// </remarks>
        /// <value>
        /// The <see cref="IQueue&lt;Task&gt;"/> that contains the
        /// <see cref="Task"/>s blocking on this <see cref="Task"/>.
        /// </value>
        protected IQueue<Task> WaitQueue
        {
            get
            {
                if (_waitQ == null)
                    _waitQ = CreateBlockingQueue(DefaultQueue);
                return _waitQ;
            }
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
        public override Task[] GetBlockedTasks(int queueId)
        {
            if (queueId == DefaultQueue || queueId == AllQueues)
                return GetBlockedTasks(_waitQ);

            throw new ArgumentException("Invalid queue id: " + queueId);
        }

		/// <summary>
		/// Clear the interrupt state.
		/// </summary>
		/// <remarks>
		/// The <see cref="Task"/> will automatically invoke this method after
		/// it's <see cref="ExecuteTask"/> method runs.
		/// </remarks>
		public void ClearInterrupt()
		{
			_intFlag = false;
		}

		/// <summary>
		/// Temporarily elevate the <see cref="Task"/>'s priority.
		/// </summary>
		/// <remarks>
		/// This method can both elevate (raise) the priority or reduce (lower)
		/// the priority.  If <paramref name="newPriority"/> is greater than
		/// <see cref="Priority"/>, the task prioritiy is raised; if
		/// <paramref name="newPriority"/> is lower than
		/// <see cref="Priority"/>, the task prioritiy is lowered.
		/// </remarks>
		/// <param name="newPriority">
		/// The new task priority.
		/// </param>
		public void ElevatePriority(int newPriority)
		{
			_elevated = newPriority;
		}

		/// <summary>
		/// Restores the <see cref="Task"/>'s priority to its non-elevated
		/// level.
		/// </summary>
		/// <returns>
		/// The non-elevated task priority.
		/// </returns>
		public int RestorePriority()
		{
			_elevated = _priority;
			return _priority;
		}

		/// <summary>
		/// Gets the current task priority.
		/// </summary>
		/// <remarks>
        /// <para>
		/// If the priority was elevated using <see cref="ElevatePriority"/>,
		/// then <see cref="Priority"/> will return the elevated task priority.
		/// The only way to get the task's default (non-elevated) priority is
		/// as follows.
        /// </para>
        /// <para>
		/// <code>
		/// // Get the current (possibly elevated) priority.
		/// int currpriority = task.Priority;
		/// // Restore the default priority which also returns the default priority.
		/// int defpriority = task.RestorePriority();
		/// // Return the priority to it's possibly elevated level.
		/// task.ElevatePriority(currpriority);</code></para>
		/// </remarks>
		/// <value>
		/// The current task priority as an <see cref="int"/>.
		/// </value>
		public int Priority
		{
			get {return _elevated;}
		}

		/// <summary>
		/// Wait the given <see cref="Task"/> while it executes.
		/// </summary>
		/// <remarks>
        /// <para>
		/// <paramref name="task"/> must not already be scheduled because this
		/// method will invoke its <see cref="Task.Activate(object, long)"/>
		/// method.
        /// </para>
		/// <para>
		/// The method is simply shorthand for
		/// <code>
		/// task.Activate(this, 0L);
        /// task.Block(this);</code>
		/// </para>
		/// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="task"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="task">
		/// The <see cref="Task"/> to wait upon while it runs.
		/// </param>
		public void WaitOnTask(Task task)
		{
            if (task == null)
                throw new ArgumentNullException("task", "cannot be null");

			task.Activate(this, 0L);
            task.Block(this);
        }

		/// <summary>
		/// Wait the given <see cref="Task"/> while it executes at the
		/// specified priority.
		/// </summary>
		/// <remarks>
        /// <para>
		/// <paramref name="task"/> must not already be scheduled because this
		/// method will invoke its <see cref="Task.Activate(object, long, int)"/>
		/// method.
        /// </para>
		/// <para>
		/// The method is simply shorthand for
		/// <code>
		/// task.Activate(this, 0L, priority);
        /// task.Block(this);</code>
		/// </para>
		/// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="task"/> is <see langword="null"/>.
        /// </exception>
		/// <param name="task">
		/// The <see cref="Task"/> to wait upon while it runs.
		/// </param>
		/// <param name="priority">
		/// The priority to activate <paramref name="task"/>.
		/// </param>
		public void WaitOnTask(Task task, int priority)
		{
            if (task == null)
                throw new ArgumentNullException("task", "cannot be null");
            
            task.Activate(this, 0L, priority);
            task.Block(this);
        }

		/// <overloads>Activates (schedules) the Task to run.</overloads>
		/// <summary>
		/// Activates the <see cref="Task"/> at the current simulation time.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="Task"/>.  May be
		/// <see langword="null"/>
		/// </param>
		public void Activate(object activator)
		{
			Activate(activator, 0L, null, Priority);
		}

		/// <summary>
		/// Activates the <see cref="Task"/> at some time in the future.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="Task"/>.  May be
		/// <see langword="null"/>
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="Task"/>
		/// should be scheduled to run.  If this value is zero (0), this
		/// method is the same as <see cref="Activate(object)"/>.
		/// </param>
		public void Activate(object activator, long relTime)
		{
			Activate(activator, relTime, null, Priority);
		}

		/// <summary>
		/// Activates the <see cref="Task"/> at some time in the future and
		/// with the given priority.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="Task"/>.  May be
		/// <see langword="null"/>.
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="Task"/>
		/// should be scheduled to run. 
		/// </param>
		/// <param name="priority">
		/// The task priority.  Higher values indicate higher priorities.
		/// </param>
		public void Activate(object activator, long relTime, int priority)
		{
			Activate(activator, relTime, null, priority);
		}

		/// <summary>
		/// Activates the <see cref="Task"/> at some time in the future and
		/// with the given client-specific data.
		/// </summary>
		/// <param name="activator">
		/// The object that is activating the <see cref="Task"/>.  May be
		/// <see langword="null"/>
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="Task"/>
		/// should be scheduled to run.
		/// </param>
		/// <param name="data">
		/// An object containing client-specific data for the
		/// <see cref="Task"/>.
		/// </param>
		public void Activate(object activator, long relTime, object data)
		{
			Activate(activator, relTime, data, Priority);
		}

		/// <summary>
		/// Activates the <see cref="Task"/> at some time in the future and
		/// specifying the task priority and client-specific task data.
		/// </summary>
		/// <remarks>
		/// <see cref="Task"/> implementations can normally treat this method
		/// as the "designated" version of the <b>Activate</b> method, which
		/// all other versions of <b>Activate</b> invoke.  That, in fact, is
		/// how the <see cref="Task"/> class implements <b>Activate</b>.
		/// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Interrupted"/> is <b>true</b>.  Before calling
        /// this method, ensure that the <see cref="Task"/> is no longer
        /// in an interrupted state.
        /// </exception>
		/// <param name="activator">
		/// The object that is activating the <see cref="Task"/>.  May be
		/// <see langword="null"/>.
		/// </param>
		/// <param name="relTime">
		/// The time relative to the current time when the <see cref="Task"/>
		/// should be scheduled to run.
		/// </param>
		/// <param name="data">
		/// An object containing client-specific data for the
		/// <see cref="Task"/>.
		/// </param>
		/// <param name="priority">
		/// The task priority.  Higher values indicate higher priorities.
		/// </param>
		public virtual void Activate(object activator, long relTime,
								     object data, int priority)
		{
            if (Interrupted)
            {
                throw new InvalidOperationException(
                    "Task is in an interrupted state.  Clear interrupt flag.");
            }
            else
            {
                CancelPending(_actevt);
                ClearBlocks();
                ElevatePriority(priority);
                ActivationEvent evt = new ActivationEvent(this, activator, relTime);
                evt.Data = data;
                Simulation.ScheduleEvent(evt);
                _actevt = evt;
            }
		}

        /// <summary>
        /// Resume the next waiting <see cref="Task"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The next waiting <see cref="Task"/> is resumed with <c>this</c>
        /// as the activator and <see langword="null"/> for the activation
        /// data.
        /// </para>
        /// <para>
        /// Calling this method is identical to calling
        /// <code>ResumeNext(this, null);</code>
        /// </para>
        /// </remarks>
        protected void ResumeNext()
        {
            ResumeNext(this, null);
        }

        /// <summary>
        /// Resume the next waiting <see cref="Task"/> with the specified
        /// activation data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The next waiting <see cref="Task"/> is resume with <c>this</c>
        /// as the activator.
        /// </para>
        /// <para>
        /// Calling this method is identical to calling
        /// <code>ResumeNext(this, data);</code>
        /// </para>
        /// </remarks>
        /// <param name="data">The activation data.</param>
        protected void ResumeNext(object data)
        {
            ResumeNext(this, data);
        }

        /// <summary>
        /// Resume the next waiting <see cref="Task"/> specifying the
        /// activator and activation data.
        /// </summary>
        /// <param name="activator">The activator.</param>
        /// <param name="data">The activation data.</param>
        protected virtual void ResumeNext(object activator, object data)
        {
            if (BlockCount > 0)
            {
                bool canceled;
                do
                {
                    Task task = WaitQueue.Dequeue();
                    canceled = task.Canceled;
                    if (!canceled)
                        ResumeTask(task, activator, data);
                } while (canceled && BlockCount > 0);
            }
        }

        /// <summary>
        /// Resume all waiting <see cref="Task"/>s.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All waiting <see cref="Task"/>s are resumed with <c>this</c> as the
        /// activator and <see langword="null"/>as the activation data.
        /// </para>
        /// <para>
        /// Calling this method is identical to calling
        /// <code>ResumeAll(this, null);</code>
        /// </para>
        /// </remarks>
        protected void ResumeAll()
        {
            ResumeAll(this, null);
        }

        /// <summary>
        /// Resume all waiting <see cref="Task"/>s passing each the specified
        /// activation data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All waiting <see cref="Task"/>s are resumed with <c>this</c> as the
        /// activator.
        /// </para>
        /// <para>
        /// Calling this method is identical to calling
        /// <code>ResumeAll(this, data);</code>
        /// </para>
        /// </remarks>
        /// <param name="data">The activation data.</param>
        protected void ResumeAll(object data)
        {
            ResumeAll(this, data);
        }

        /// <summary>
        /// Resume all waiting <see cref="Task"/>s specifying the activator and
        /// the activation data.
        /// </summary>
        /// <param name="activator">The activator.</param>
        /// <param name="data">The activation data.</param>
        protected virtual void ResumeAll(object activator, object data)
        {
            int nWaiting = BlockCount;

            if (nWaiting > 1)
            {
                Task[] blockedTasks = new Task[nWaiting];
                WaitQueue.CopyTo(blockedTasks, 0);
                WaitQueue.Clear();
                for (int i = 0; i < nWaiting; i++)
                {
                    Task task = blockedTasks[i];
                    if (!task.Canceled)
                        ResumeTask(task, activator, data);
                }
            }
            else if (nWaiting == 1)
            {
                ResumeNext(activator, data);
            }
        }

        /// <summary>
        /// Resume the specified <see cref="Task"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is invoked for each blocked <see cref="Task"/> that is
        /// to be resumed by calling one of the <b>ResumeNext</b> or
        /// <b>ResumeAll</b> methods.  Subclasses may override this method to
        /// alter the way <paramref name="task"/> is activated.
        /// </para>
        /// <para>
        /// The default implementation simply performs
        /// </para>
        /// <para><code>task.Activate(activator, 0L, data);</code></para>
        /// <para>
        /// Client code should normally never need to invoke this method
        /// directly.
        /// </para>
        /// <para>
        /// <b>By the time this method is called, <paramref name="task"/> has
        /// already been removed from <see cref="WaitQueue"/>.</b>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="task"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="task">
        /// The <see cref="Task"/> to resume (activate).
        /// </param>
        /// <param name="activator">
        /// The activator that will be passed to <paramref name="task"/> upon
        /// its activation.
        /// </param>
        /// <param name="data">
        /// Optional activation data passed to <paramref name="task"/>.
        /// </param>
        protected virtual void ResumeTask(Task task, object activator,
            object data)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            task.Activate(activator, 0L, data);
        }

		/// <summary>
		/// Block the specified <see cref="Task"/> instance.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="task"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="task"/> attempts to block itself.  For
		/// example, if code like <c>this.Block(this);</c> is executed.
		/// </exception>
		/// <param name="task">
		/// The <see cref="Task"/> to block.
		/// </param>
		public virtual void Block(Task task)
		{
			if (task == null)
			{
				throw new ArgumentNullException("'task' cannot be null.");
			}
			if (task == this)
			{
				throw new ArgumentException("Task cannot block on itself.");
			}
			task.UpdateBlockingLinks(this, true);
			WaitQueue.Enqueue(task);
		}

		/// <summary>
		/// Unblock, but do not resume, the specified <see cref="Task"/>.
		/// </summary>
		/// <remarks>
        /// <para>
		/// This method is used to remove <paramref name="task"/> from the
		/// <see cref="Task"/> instance's wait list without resuming the
		/// execution of <paramref name="task"/>.  The most common use for
		/// invoking <see cref="Unblock"/> is to stop a <see cref="Task"/>
		/// from waiting after it has been resumed by another means (e.g. a
		/// different simulation object has resumed <paramref name="task"/>).
        /// </para>
		/// <para>
		/// Again, it's very important to realize that <see cref="Unblock"/>
		/// does <b>not</b> activate <paramref name="task"/>.
		/// </para>
		/// <para>
		/// This method does nothing if <paramref name="task"/> equals
		/// <c>this</c> or is <see langword="null"/>.
		/// </para>
		/// </remarks>
		/// <param name="task">
		/// The <see cref="Task"/> which will stop blocking on this
		/// <see cref="Task"/> instance.
		/// </param>
		public virtual void Unblock(Task task)
		{
            if (task != null && task != this && BlockCount > 0)
			{
				WaitQueue.Remove(task);
				task.UpdateBlockingLinks(this, false);
			}
		}

		/// <summary>
		/// Stop blocking on all <see cref="Task"/>s currently being blocked
		/// upon.
		/// </summary>
		protected virtual void ClearBlocks()
		{
			if (_blockedOn != null)
			{
				IList<Task> list = _blockedOn;
				_blockedOn = null;
				foreach (Task task in list)
				{
					task.Unblock(this);
				}
				if (_blockedOn == null)
				{
					list.Clear();
					_blockedOn = list;
				}
			}
		}

		/// <summary>
		/// Update the association between this <see cref="Task"/> and the
		/// <see cref="Task"/> upon which it is blocking.
		/// </summary>
		/// <param name="blocker">
		/// The <see cref="Task"/> upon which this <see cref="Task"/> is
		/// blocking.
		/// </param>
		/// <param name="blocked">
		/// <b>true</b> if <paramref name="blocker"/> is blocking this
		/// <see cref="Task"/>; or <b>false</b> if <paramref name="blocker"/>
		/// is unblocking this <see cref="Task"/>.
		/// </param>
		private void UpdateBlockingLinks(Task blocker, bool blocked)
		{
			if (blocked)
			{
				if (_blockedOn == null)
				{
					_blockedOn = new List<Task>();
				}
				if (_blockedOn.Contains(blocker))
				{
					throw new InvalidOperationException(
						"Already blocking on specified task.");
				}
				_blockedOn.Add(blocker);
			}
			else if (_blockedOn != null)
			{
				_blockedOn.Remove(blocker);
			}
		}

		/// <summary>
		/// Cancel the <see cref="Task"/>.
		/// </summary>
		/// <remarks>
        /// <para>
		/// A canceled task will not be executed.  The associated
        /// <see cref="ActivationEvent"/> (if any) is also canceled.
        /// </para>
        /// <para>
        /// Callers should note that once a <see cref="Task"/> is canceled
        /// it cannot be un-canceled, and therefore can never be
        /// re-activated.
        /// </para>
		/// </remarks>
		public void Cancel()
		{
			_cancelFlag = true;
            CancelPending(_actevt);
		}

		/// <summary>
		/// Gets whether or not the <see cref="Task"/> was canceled.
		/// </summary>
		/// <remarks>
		/// This property will be <b>true</b> after the <see cref="Cancel"/>
		/// method is invoked.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="Task"/> was canceled.
		/// </value>
		public bool Canceled
		{
			get {return _cancelFlag;}
		}

		/// <summary>
		/// Interrupt a blocked <see cref="Task"/>.
		/// </summary>
		/// <remarks>
        /// <para>
		/// When an blocked <see cref="Task"/> is interrupted, it should be
		/// activated at <see cref="React.Simulation.Now"/>.  When the
		/// <see cref="Task"/> resumes running, it can check the 
		/// <see cref="Interrupted"/> property to determine how to proceed.
		/// The <paramref name="interruptor"/> is available to the
		/// <see cref="Task"/> as the <em>activator</em> parameter when
		/// <see cref="ExecuteTask"/> is invoked.
        /// </para>
        /// <para>
        /// The <see cref="Task"/> must handle the interrupt and clear the
        /// interrupt flag by calling <see cref="ClearInterrupt"/> before
        /// <see cref="Interrupt"/> or
        /// <see cref="Activate(object,long,object,int)"/> (or any of the
        /// other <b>Activate</b> methods) may be called again.
        /// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// If an <see cref="Task"/> attempts to interrupt itself.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="interruptor"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="interruptor">
		/// The object which caused the interrupt.  This should normally be
		/// the object that is invoking this method.
		/// </param>
		public void Interrupt(object interruptor)
		{
			if (interruptor == this)
			{
                throw new ArgumentException(
                    "Task cannot interrupt itself.", "interruptor");
			}
			if (interruptor == null)
			{
				throw new ArgumentNullException("interruptor");
			}
			Activate(interruptor, 0L);
			_intFlag = true;
		}

		/// <summary>
		/// Gets whether or not the <see cref="Task"/> was interrupted.
		/// </summary>
		/// <remarks>
		/// This value is automatically reset to <b>false</b> after the
		/// <see cref="Task"/> executes.
		/// </remarks>
		/// <value>
		/// <b>true</b> if the <see cref="Task"/> was interrupted.
		/// </value>
		public bool Interrupted
		{
			get {return _intFlag;}
		}

		/// <summary>
		/// Perform the task actions.
		/// </summary>
        /// <remarks>
        /// This method is invoked by the <see cref="Task"/>'s associated
        /// <see cref="ActivationEvent"/> when the
        /// <see cref="ActivationEvent"/> is fired.  Normally this method
        /// should not be called by client code.
        /// </remarks>
		/// <param name="activator">
		/// The object that activated this <see cref="Task"/>.
		/// </param>
		/// <param name="data">
		/// Optional data for the <see cref="Task"/>.
		/// </param>
		protected abstract void ExecuteTask(object activator, object data);

		//=====================================================================
		//====                  INTERNAL IMPLEMENTATION                    ====
		//=====================================================================

        /// <summary>
        /// Cancel the pending <see cref="ActivationEvent"/>.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="ActivationEvent"/> to cancel.
        /// </param>
		internal void CancelPending(ActivationEvent evt)
		{
			if (_actevt != null)
			{
				// Internal consistency check
				if (_actevt != evt)
				{
					throw new InvalidOperationException("Event mis-match.");
				}
				_actevt = null;
				evt.Cancel();
			}
		}

        /// <summary>
        /// Invoked by an <see cref="ActivationEvent"/> to execute the
        /// <see cref="Task"/>.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="ActivationEvent"/> that triggered this
        /// <see cref="Task"/> to execute.
        /// </param>
		internal void RunFromActivationEvent(ActivationEvent evt)
		{
			// Internal consistency check
			if (_actevt != evt)
			{
				throw new InvalidOperationException("Event mis-match.");
			}
			RestorePriority();
			_actevt = null;
			ExecuteTask(evt.Activator, evt.Data);
			ClearInterrupt();
		}
	}
}
