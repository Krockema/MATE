using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to interrupt another process.
	/// </summary>
	public class InterruptInstruction : InstructionBase
	{
		/// <summary>
		/// The process that will be interrupted.
		/// </summary>
		private Process _process;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.InterruptInstruction"/> class.
		/// </summary>
		/// <param name='process'>
		/// Process.
		/// </param>
		public InterruptInstruction(Process process)
		{
			_process = process;
		}

		/// <summary>
		/// Determines whether this instruction can complete in the current time period
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance can complete.
		/// </returns>
		/// <param name='context'>
		/// Context providing state information for the current simulation
		/// </param>
		/// <param name='skipFurtherChecksUntilTimePeriod'>
		/// Output parameter used to specify a time period at which this instruction should be checked again.  This should be left null if it is not possible to determine when this instruction can complete.
		/// </param>
		public override bool CanComplete(SimulationContext context, out long? skipFurtherChecksUntilTimePeriod){
			skipFurtherChecksUntilTimePeriod = null;

			return true;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, this involves interrupting a process.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			// interrupt the process
			_process.SimulationState.IsInterrupted = true;
			if (_process.SimulationState.InstructionEnumerator != null
			    && _process.SimulationState.InstructionEnumerator.Current != null)
			{
				_process.SimulationState.InstructionEnumerator.Current.Interrupt(context);
			}

			if (_process.SimulationState.IsActive){
				if (!context.ProcessesRemainingThisTimePeriod.Contains(_process)){
					context.ProcessesRemainingThisTimePeriod.Enqueue(_process);
				}
			}

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

