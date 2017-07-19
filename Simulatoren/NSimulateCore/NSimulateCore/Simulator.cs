using System;
using System.Collections.Generic;
using NSimulate.Instruction;
using System.Linq;

namespace NSimulate
{
	/// <summary>
	/// A Simulator used to orchestrate the simulation of processes in the overall simulation context
	/// </summary>
	public class Simulator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Simulator"/> class.
		/// </summary>
		public Simulator ()
			: this (SimulationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Simulator"/> class.
		/// </summary>
		public Simulator (SimulationContext context)
		{
			Context = context;
		}

		protected SimulationContext Context {
			get;
			set;
		}

		/// <summary>
		/// Run the simulation
		/// </summary>
		public void Simulate()
		{
			bool complete = false;
			Context.MoveToTimePeriod(0);

			long? nextTimePeriod = null;

			while (!complete)
			{
				while(Context.ProcessesRemainingThisTimePeriod.Count > 0 && !Context.IsSimulationTerminating)
				{
					var process = Context.ProcessesRemainingThisTimePeriod.Dequeue();
				
					if (process.SimulationState.IsActive){
						if (process.SimulationState.InstructionEnumerator == null) {
							process.SimulationState.InstructionEnumerator = process.Simulate();
						}

						SimulateProcessAtTimePeriod(process,  ref nextTimePeriod);
					}
				}

				if (nextTimePeriod != null && !Context.IsSimulationTerminating)
				{
					// move to the next time period
					Context.MoveToTimePeriod(nextTimePeriod.Value);
					nextTimePeriod = null;
				}
				else
				{
					// simulation has completed
					complete = true;
				}
			}
		}

		/// <summary>
		/// Simulate a single process at the current time period
		/// </summary>
		/// <param name='process'>
		/// The process to be simulated
		/// </param>
		/// <param name='nextTimePeriod'>
		/// The next time period in which this process should be simulated (if known)
		/// </param>
		private void SimulateProcessAtTimePeriod(Process process, ref long? nextTimePeriod){
			bool shouldMoveNext = true;
			while (shouldMoveNext && !Context.IsSimulationTerminating) {
				InstructionBase currentInstruction = process.SimulationState.InstructionEnumerator.Current;
				bool didComplete = false;

				if (currentInstruction != null) {
					long? nextTimePeriodCheck = null;

					if (process.SimulationState.IsInterrupted) {
						currentInstruction.Interrupt(Context);
					}
					else if (currentInstruction.IsInterrupted || currentInstruction.IsCompleted){
						// no further processing of the instruction is needed
					}
					else if (currentInstruction.CanComplete(Context, out nextTimePeriodCheck)) {
						currentInstruction.Complete(Context);
						didComplete = true;
					}
					else {
						shouldMoveNext = false;
					}

					if (!didComplete){
						if (nextTimePeriodCheck != null && (nextTimePeriod == null || nextTimePeriodCheck < nextTimePeriod)) {
							nextTimePeriod = nextTimePeriodCheck.Value;
						}
					}
				}

				if (shouldMoveNext) {
					bool couldMoveNext = process.SimulationState.InstructionEnumerator.MoveNext();
					if (couldMoveNext){
						process.SimulationState.InstructionEnumerator.Current.RaisedInTimePeriod = Context.TimePeriod;
					}
					else {
						shouldMoveNext = false;
						process.SimulationState.IsComplete = true;
						Context.ActiveProcesses.Remove(process);
					}
				}
			}

			if (process.SimulationState.IsInterrupted)
			{
				// ensure the process is not in the interrupted state
				process.SimulationState.IsInterrupted = false;
			}
		}
	}
}

