using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to deactivate a process
	/// </summary>
	public class DeactivateInstruction : InstructionBase
	{
		/// <summary>
		/// The process to be deactivated
		/// </summary>
		private Process _process;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.DeactivateInstruction"/> class.
		/// </summary>
		/// <param name='process'>
		/// Process.
		/// </param>
		public DeactivateInstruction(Process process)
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
		/// Complete the instruction.  For this instruction type, deactivates the specified process
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			// de-activate the process
			if (_process.SimulationState.IsActive){
				_process.SimulationState.IsActive = false;
			}

			if (context.ActiveProcesses.Contains(_process)){
				context.ActiveProcesses.Remove(_process);
			}

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

