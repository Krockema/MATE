//=============================================================================
//=  $Id: Simulation.cs 177 2006-10-07 16:30:59Z eroe $
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
	/// A class for running discrete-event simulations.
	/// </summary>
	/// <remarks>
    /// <para>
	/// <see cref="Simulation"/> contains mechanisms for maintaining the
    /// current simulated time as well as a collection of
    /// <see cref="ActivationEvent"/> instances scheduled to occur at some
    /// time during the simulation run.  This collection is typically
    /// referred to as the the <em>event queue</em>, <em>event calendar</em>
    /// or <em>future event set</em>.
    /// </para>
	/// <para>
	/// Fundamentally, the operation of the <see cref="Simulation"/> is
	/// quite simple.  It starts with the scheduling of one or more <em>
	/// generator</em>&#160;<see cref="Task"/> instances.  Internally, the
    /// generators are scheduled with <see cref="ActivationEvent"/>s, but since
    /// it's <see cref="Task"/>s that perform the actual simulated processing,
	/// the generators are <see cref="Task"/>s rather than
    /// <see cref="ActivationEvent"/>s.
	/// </para>
	/// <para>
	/// The generator <see cref="Task"/>s serve to "jumpstart" or "bootstrap"
	/// the simulation.  They perform some set of initialization actions, which
	/// probably result in additional <see cref="Task"/>&#160;<em>activations</em>
    /// and thus additional <see cref="ActivationEvent"/>s being scheduled.  As
    /// each <see cref="ActivationEvent"/> is fired it runs a
    /// <see cref="Task"/>, and each <see cref="Task"/> might activate other
    /// <see cref="Task"/>s (or itself).  Each activation places an
    /// <see cref="ActivationEvent"/> on the event queue.  This process
    /// continues until the event queue is emptied or the
    /// <see cref="Simulation"/> is ordered to stop. 
	/// </para>
	/// </remarks>
	public class Simulation
	{
        /// <summary>
        /// Event raised when the <see cref="SimulationState"/> has changed.
        /// </summary>
        /// <remarks>
        /// The new <see cref="SimulationState"/> is not passed to the
        /// delegate method, rather each handler must query the sender (e.g.
        /// the <see cref="Simulation"/>) to obtain its current state.
        /// </remarks>
        public event EventHandler StateChanged;

		/// <summary>The current simulation time.</summary>
		private long _currentTime;
		/// <summary>The time the simulation will stop.</summary>
		private long _stopTime = System.Int64.MaxValue;
		/// <summary>The number of discardable Tasks scheduled.</summary>
		private int _nDiscardableTasks;
		/// <summary>The event calendar (future event set).</summary>
        private IQueue<ActivationEvent> _eventQueue;
        /// <summary>The current <see cref="SimulationState"/>.</summary>
        private SimulationState _state = SimulationState.Ready;

		/// <summary>
		/// Create and initialize a new <see cref="Simulation"/>.
		/// </summary>
		public Simulation()
		{
            PriorityQueue<ActivationEvent> queue =
                new PriorityQueue<ActivationEvent>();

            // Create a prioritizer used to sort ActivationEvents in the
            // event queue.  Events are sorted on time and then priority.
            queue.Prioritizer = delegate(ActivationEvent e1, ActivationEvent e2)
            {
                if (e1.Time < e2.Time)
                    return 1;
                if (e1.Time > e2.Time)
                    return -1;

                if (e1.Priority > e2.Priority)
                    return 1;
                if (e1.Priority < e2.Priority)
                    return -1;

                return 0;
            };

            _eventQueue = queue;
		}

		/// <summary>
		/// Gets the current simulation time.
		/// </summary>
		/// <remarks>
		/// The current time will never be less than zero (0).  The simulation
		/// time at the beginning of a simulation run is zero.
		/// </remarks>
		/// <value>
		/// The current simulation time as an <see cref="long"/>.
		/// </value>
		public long Now
		{
			get {return _currentTime;}
		}

        /// <summary>
        /// Gets or sets the current <see cref="SimulationState"/>.
        /// </summary>
        /// <remarks>
        /// Setting the current simulation state will raise the
        /// <see cref="StateChanged"/> event.
        /// </remarks>
        public SimulationState State
        {
            get { return _state; }
            protected set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged();
                }
            }
        }

		/// <summary>
		/// Gets the simulation time when the simulation stopped.
		/// </summary>
		/// <remarks>
		/// The <see cref="Simulation"/> will stop its run loop when
		/// the current simulation time equals the stop time.  Any events
		/// scheduled at the stop time will be fired.
		/// </remarks>
		/// <value>
		/// The simulation stop time as an <see cref="long"/>.
		/// </value>
		public long StopTime
		{
			get {return _stopTime;}
		}

		/// <summary>
        /// Adds the specified <see cref="ActivationEvent"/> to the event queue
        /// (future event set).
		/// </summary>
		/// <remarks>
        /// <para>
		/// Once <paramref name="evt"/> is scheduled, it cannot be removed from
		/// the event queue, but it may be canceled.  Canceled
        /// <see cref="ActivationEvent"/>s remain in the queue, but are simply
        /// discarded rather than fired when they are removed from the queue.
        /// </para>
        /// <para>
        /// Most client code will not need to call this method, rather one of
        /// the <b>Activate</b> methods of the <see cref="Task"/> class should
        /// be used to schedule <see cref="Task"/>s to run.
        /// </para>
        /// <para>
        /// <b>Important:</b> If the <see cref="Simulation"/> is in the
        /// <see cref="React.SimulationState.Stopping"/> state,
        /// <paramref name="evt"/> is silently ignored, <b>it is not
        /// scheduled</b>.
        /// </para>
		/// </remarks>
		/// <exception cref="BackClockingException">
		/// If <paramref name="evt"/> has an event time earlier than the
		/// current simulation time, <see cref="Now"/>.
		/// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the simulation <see cref="State"/> is either
        /// <see cref="React.SimulationState.Completed"/> or
        /// <see cref="React.SimulationState.Failed"/>.
        /// </exception>
		/// <param name="evt">
        /// The <see cref="ActivationEvent"/> to schedule (add to the event queue).
		/// </param>
        public virtual void ScheduleEvent(ActivationEvent evt)
		{
			if (evt.Time < Now)
				throw new BackClockingException(this, evt.Time);

            if (State == SimulationState.Completed ||
                State == SimulationState.Failed)
            {
                throw new InvalidOperationException(
                    "Simulation state is " + State.ToString());
            }

            if (State != SimulationState.Stopping)
            {
                _eventQueue.Enqueue(evt);
                if (evt.Priority == TaskPriority.Discardable)
                    _nDiscardableTasks++;
            }
		}

		/// <overloads>Run the simulation.</overloads>
		/// <summary>
		/// Run the <see cref="Simulation"/>.
		/// </summary>
		/// <remarks>
		/// When this version of <see cref="Run()"/> is used, one or more
		/// initial <see cref="Task"/> instance must have already been
		/// activated or the <see cref="Simulation"/> will immediately stop.
		/// </remarks>
		/// <returns>
        /// The number of <see cref="ActivationEvent"/> instances that remained
        /// in the event queue at the time the <see cref="Simulation"/>
        /// stopped.
		/// </returns>
		public int Run()
		{
			return Run(new Task [] {});
		}

		/// <summary>
		/// Run the <see cref="Simulation"/> using the given generator
		/// <see cref="Task"/> instance.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="generator"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="generator">
		/// The <see cref="Task"/> which will serve as the sole generator.
		/// </param>
		/// <returns>
        /// The number of <see cref="ActivationEvent"/> instances that
        /// remained in the event queue at the time the
        /// <see cref="Simulation"/> stopped.
		/// </returns>
		public int Run(Task generator)
		{
			if (generator == null)
			{
				throw new ArgumentNullException("generator");
			}
			return Run(new Task [] {generator});
		}

		/// <summary>
		/// Run the <see cref="Simulation"/> using the provided generator
		/// <see cref="Task"/> instances.
		/// </summary>
		/// <remarks>
		/// The array of generators must contain zero (0) or more
		/// <see cref="Task"/> instances; it cannot be <see langword="null"/>.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="generators"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="generators">
		/// An array of generator <see cref="Task"/> instances.
		/// </param>
		/// <returns>
        /// The number of <see cref="ActivationEvent"/> instances that
        /// remained in the event queue at the time the
        /// <see cref="Simulation"/> stopped.
		/// </returns>
		public virtual int Run(Task [] generators)
		{
			// Reset from a previous run.
			_currentTime = 0L;
			_stopTime = System.Int64.MaxValue;

			ActivateGenerators(generators);

            State = SimulationState.Running;

			while (_stopTime >= _currentTime && _eventQueue.Count > 0 &&
                _nDiscardableTasks < _eventQueue.Count)
			{
                ActivationEvent evt = _eventQueue.Dequeue();
				_currentTime = evt.Time;

                if (evt.Priority == TaskPriority.Discardable)
                    _nDiscardableTasks--;

				if (_currentTime > _stopTime)
				{
					// Time to stop.  Put the event back on the event queue so
					// it's propertly counted as one of the events not fired.
					_eventQueue.Enqueue(evt);
				}
				else if (evt.IsPending)
				{
					evt.Fire(this);
				}
			}

			// Make some final time updates.
			if (_currentTime < _stopTime)
				_stopTime = _currentTime;
			else
				_currentTime = _stopTime;

			int nNotRun = _eventQueue.Count;
			_eventQueue.Clear();

            State = SimulationState.Completed;

			return nNotRun;
		}

		/// <summary>
		/// Stop the <see cref="Simulation"/> at the current simulation time.
		/// </summary>
		/// <remarks>
		/// This method is the equivalent of <c>sim.Stop(sim.Now)</c>.  If the
		/// <see cref="Simulation"/> is not running, invoking this method has
		/// no effect.
		/// </remarks>
		public void Stop()
		{
			Stop(Now);
		}

		/// <summary>
		/// Stop the <see cref="Simulation"/> at the specified simulation time.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Simulation"/> is not running, this method has no
		/// effect.
		/// </remarks>
		/// <param name="absTime">
		/// The absolute simulation time when the <see cref="Simulation"/>
		/// should stop running.  If <paramref name="absTime"/> is less than
		/// <see cref="Now"/>, the simulation will stop at the current time.
		/// </param>
		public virtual void Stop(long absTime)
		{
            State = SimulationState.Stopping;

			if (absTime < Now)
				_stopTime = Now;
			else
				_stopTime = absTime;
		}

        /// <summary>
        /// Invoked after the <see cref="SimulationState"/> has changed.
        /// </summary>
        /// <remarks>
        /// The default implementation raises the <see cref="StateChanged"/>
        /// event.
        /// </remarks>
        protected virtual void OnStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
        }

		/// <summary>
		/// Activate all the generator <see cref="Task"/> instances in the
		/// given array.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="generators"/> is <see langword="null"/>.
		/// </exception>
		/// <param name="generators">
		/// An array containing zero or more <see cref="Task"/>s that will
		/// serve as generators for the <see cref="Simulation"/>.
		/// </param>
		private void ActivateGenerators(Task [] generators)
		{
			if (generators == null)
			{
				throw new ArgumentNullException("'generators' cannot be null.");
			}

            State = SimulationState.Initializing;

			foreach (Task task in generators)
			{
				task.Activate(this);
			}
		}
	}
}
