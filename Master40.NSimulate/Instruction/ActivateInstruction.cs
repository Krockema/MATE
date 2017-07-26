using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// Instruction used to activate a new or inactive process
	/// </summary>
	public class ActivateInstruction : InstructionBase
	{
		/// <summary>
		/// The Process that will be activated when this instruction completes.
		/// </summary>
		private Process _process;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.ActivateInstruction"/> class.
		/// </summary>
		/// <param name='process'>
		/// Process to be activated.
		/// </param>
		public ActivateInstruction(Process process)
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
		/// Complete the instruction.  For this instruction type, activates the specified process
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			// if the active flag is currently false
			if (!_process.SimulationState.IsActive){
				// set it true now
				_process.SimulationState.IsActive = true;
			}

			// if the process is not already in the active process list
			if (!context.ActiveProcesses.Contains(_process)){
				// add it to te active procvess list
				context.ActiveProcesses.Add(_process);
				// ensure the process just activated will be processed by the simulator again in the current time period
				if (!context.ProcessesRemainingThisTimePeriod.Contains(_process)){
					context.ProcessesRemainingThisTimePeriod.Enqueue(_process);
				}
			}
		}
	}
}

