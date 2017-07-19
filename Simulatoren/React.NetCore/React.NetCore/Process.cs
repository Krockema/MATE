//=============================================================================
//=  $Id: Process.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React
{
	/// <summary>
	/// Delegate for the generator method that yields one or more
	/// <see cref="Task"/>s that make up a process's sequence of steps.
	/// </summary>
    /// <remarks>
    /// A <see cref="ProcessSteps"/> delegate may be passed to the
    /// <see cref="Process(Simulation,ProcessSteps)"/> or
    /// <see cref="Process(Simulation,ProcessSteps,object)"/> constructor to
    /// create a <see cref="Process"/> via <em>delegation</em> rather than
    /// through <em>derivation</em> (i.e. sub-classing and overriding the
    /// <see cref="Process.GetProcessSteps"/> method).
    /// </remarks>
    /// <example>
    /// <para>
    /// This example shows how to create two <see cref="Process"/> instances
    /// using delegation via a <see cref="ProcessSteps"/> delegate.  It also
    /// shows how client data can be passed into the delegate.
    /// </para>
    /// <para><code><![CDATA[
    /// public void CreateAProcess(Simulation sim)
    /// {
    ///     Process pa = new Process(sim, StepsMethod, 1000L);
    ///     pa.Name = "A";
    ///     pa.Activate(null);
    ///
    ///     Process pb = new Process(sim, StepsMethod, 500L);
    ///     pb.Name = "B";
    ///     pb.Activate(null, 1L);
    /// 
    ///     sim.Run();
    /// }
    /// 
    /// private IEnumerator<Task> StepsMethod(Process p, object data)
    /// {
    ///     long delayTime = (long)data;
    ///     Console.WriteLine("{0}: Hello, the time is {1}", p.Name, p.Now);
    ///     yield return p.Delay(delayTime);
    ///     Console.WriteLine("{0}: And now the time is {1}", p.Name, p.Now);
    ///     yield break;
    /// }]]></code></para>
    /// <para>
    /// The output should look something like what is shown below.
    /// </para>
    /// <para><code>
    /// A: Hello, the time is 0
    /// B: Hello, the time is 1
    /// B: And now the time is 501
    /// A: And now the time is 1000</code></para>
    /// </example>
    /// <param name="process">
    /// The <see cref="Process"/> the <see cref="ProcessSteps"/> is working
    /// on behalf of.
    /// </param>
    /// <param name="data">
    /// User defined data for <see cref="ProcessSteps"/> method.  May be
    /// <see langword="null"/>.
    /// </param>
	public delegate IEnumerator<Task> ProcessSteps(Process process, object data);

	/// <summary>
	/// A <see cref="Task"/> implementation that uses an iterator method to
    /// support simulating complex or long-running processes.
	/// </summary>
	public class Process : Task
	{
		/// <summary>
		/// The delegate which can create the process step generator.
		/// </summary>
		private ProcessSteps _stepsfunc;
		/// <summary>
		/// Data passed to the <see cref="Process"/> when using a
		/// <see cref="ProcessSteps"/> delegate.
		/// </summary>
		private object _stepsdata;
		/// <summary>
		/// The active generator which yields each processing step.
		/// </summary>
		private IEnumerator<Task> _steps;
		/// <summary>
		/// The object that activated this <see cref="Process"/>.
		/// </summary>
		private object _activator;
		/// <summary>
		/// Per-activation event data.
		/// </summary>
		private object _activationData;

		/// <summary>
		/// Create a new <see cref="Process"/> instance.
		/// </summary>
		/// <remarks>
		/// This constructor is only available to derived classes.
		/// </remarks>
		/// <param name="sim">The simulation context.</param>
		protected Process(Simulation sim) : base(sim)
		{
		}

		/// <summary>
		/// Create a new <see cref="Process"/> that obtains its processing
		/// steps generator from the given delegate.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="steps">
		/// The <see cref="ProcessSteps"/> delegate that can create the
		/// generator which supplies the processing steps for the
		/// <see cref="Process"/>.
		/// </param>
		public Process(Simulation sim, ProcessSteps steps)
			: this(sim, steps, null)
		{
		}

		/// <summary>
		/// Create a new <see cref="Process"/> that obtains its processing
		/// steps generator from the given delegate and which can pass client
		/// data to the delegate.
		/// </summary>
		/// <param name="sim">The simulation context.</param>
		/// <param name="steps">
		/// The <see cref="ProcessSteps"/> delegate that can create the
		/// generator which supplies the processing steps for the
		/// <see cref="Process"/>.
		/// </param>
		/// <param name="data">
		/// Client data passed to <paramref name="steps"/> when it is invoked.
		/// May be <see langword="null"/>.
		/// </param>
		public Process(Simulation sim, ProcessSteps steps, object data)
			: base(sim)
		{
			if (steps == null)
			{
				throw new ArgumentNullException("steps");
			}

			_stepsfunc = steps;
			_stepsdata = data;
		}

		/// <summary>
		/// Gets an <see cref="IEnumerator&lt;Task&gt;"/> that yields the processing
		/// steps.
		/// </summary>
		/// <remarks>
		/// This is a generator method that must be overridden by subclasses.  It
		/// must <c>yield</c> one or more <see cref="Task"/> instances which will
		/// perform each processing step.
		/// </remarks>
        /// <returns>
        /// An <see cref="IEnumerator&lt;T&gt;"/> (iterator) capable of
        /// yielding one or more <see cref="Task"/> instances that will
        /// perform actions on behalf of the <see cref="Process"/>.
        /// </returns>
		protected virtual IEnumerator<Task> GetProcessSteps()
		{
			if (_stepsfunc == null)
			{
				throw new InvalidOperationException("No process steps defined.");
			}

			return _stepsfunc(this, _stepsdata);
		}

		/// <summary>
		/// Gets the object that activated this <see cref="Process"/>.
		/// </summary>
		/// <remarks>
        /// <para>
		/// When the <see cref="Process"/> is activated by an
		/// <see cref="IResource"/>, <see cref="ICondition"/>, or other
		/// blocking object, this property should always be non-null.
        /// </para>
		/// <para>
		/// The property is <see langword="null"/> except during the
		/// execution of the <see cref="ExecuteTask"/> method.
		/// </para>
		/// </remarks>
		/// <value>
		/// The <see cref="object"/> that activated this <see cref="Process"/>
		/// or <see langword="null"/> if self-activated or anonymously
		/// activated.
		/// </value>
		public object Activator
		{
			get {return _activator;}
		}

		/// <summary>
		/// Gets the per-activation event data specified when this
		/// <see cref="Process"/> was activated.
		/// </summary>
		/// <remarks>
		/// The property is <see langword="null"/> except during the
		/// execution of the <see cref="ExecuteTask"/> method.
		/// </remarks>
		/// <value>
		/// The per-activation event data <see cref="object"/> or
		/// <see langword="null"/> if the <see cref="Activator"/> did not
		/// specify any activation data.
		/// </value>
		public object ActivationData
		{
			get {return _activationData;}
		}

		/// <summary>
		/// Executes each process step <see cref="Task"/> obtained from the
		/// generator created by <see cref="GetProcessSteps"/>.
		/// </summary>
        /// <remarks>
        /// Normally, <see cref="Process"/> implementors will not need to
        /// override this method; override <see cref="GetProcessSteps"/>
        /// instead.
        /// </remarks>
		/// <param name="activator">
		/// The object that activated this <see cref="Process"/>.
		/// </param>
		/// <param name="data">Per-activation event data.</param>
		protected override void ExecuteTask(object activator, object data)
		{
			if (_steps == null)
				_steps = GetProcessSteps();

			_activator = activator;
            _activationData = data;

			if (_steps.MoveNext())
			{
				Task task = _steps.Current;
				if (task == null)
				{
					throw new SimulationException("Process task step was null.");
				}
				if (task != this)
				{
                    // Clear interrupt flag before calling WaitOnTask,
                    // otherwise the call to Activate in WaitOnTask will
                    // be ignored.
                    ClearInterrupt();
                    WaitOnTask(task);
				}
			}
			else
			{
				_steps = null;
			}

            _activationData = null;
			_activator = null;

			if (_steps == null)
				ResumeAll();
		}

		//====================================================================
		//====                    Core Process Actions                    ====
		//====================================================================

		/// <summary>
		/// Defer processing to allow another <see cref="Task"/> to run.
		/// </summary>
		/// <remarks>
		/// This method is used to temporarily suspend the current
		/// <see cref="Process"/> and allow another <see cref="Task"/> to
		/// run.  It performs the same function as <c>Delay(0L)</c>.
		/// </remarks>
		/// <returns>
		/// A reference to the current <see cref="Process"/>, <c>this</c>.
		/// </returns>
		[BlockingMethod]
		public Task Defer()
		{
			return Delay(0L);
		}

		/// <summary>
		/// Delay for a period of time.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// If <paramref name="relTime"/> is less than zero (0).
		/// </exception>
		/// <param name="relTime">
		/// The delay time relative to the current simulation time.
		/// </param>
		/// <returns>
		/// A reference to the current <see cref="Process"/>, <c>this</c>.
		/// </returns>
		[BlockingMethod]
		public Task Delay(long relTime)
		{
			if (relTime < 0)
			{
				throw new ArgumentException("'relTime' cannot be negative.");
			}

            Task task;

            if (IsBlocked)
            {
                task = new React.Tasking.Delay(Simulation, relTime);
            }
            else
            {
                // Need to clear interrupt flag or Activate is ignored.
                ClearInterrupt();
                Activate(null, relTime);
                task = this;
            }

            return task;
		}

		/// <summary>
		/// Suspends or passivates the <see cref="Process"/>.
		/// </summary>
        /// <remarks>
        /// Suspending a <see cref="Process"/> is different from defering a
        /// <see cref="Process"/> (see <see cref="Defer"/>).  When a
        /// <see cref="Process"/> is suspended, it requires another
        /// <see cref="Task"/> to re-activate it; a deferred
        /// <see cref="Process"/> will automatically re-activate.
        /// </remarks>
		/// <returns>
		/// A reference to the current <see cref="Process"/>, <c>this</c>.
		/// </returns>
		[BlockingMethod]
		public Task Suspend()
		{
			return this;
		}

		/*
		protected Task Acquire(IResource resource)
		{
		}

		protected Task Acquire(IResource [] resources)
		{
		}

		protected Task Acquire(IResource [] resources, bool acquireAll)
		{
		}

		protected Task Release(IResource resource)
		{
		}

		protected Task Release(IResource [] resources)
		{
		}

		protected Task WaitFor(Task blocker)
		{
		}

		protected Task WaitFor(Task [] blockers)
		{
		}
		*/
	}
}
