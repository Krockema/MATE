using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to pass control from a process back to the simulator without performing any other action
	/// </summary>
	public class PassInstruction : InstructionBase
	{
		/// <summary>
		/// Flag used to record whether the pass instruction has been used
		/// </summary>
		bool _hasPassed = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.PassInstruction"/> class.
		/// </summary>
		public PassInstruction ()
		{
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
		public override  bool CanComplete(SimulationContext context, out long? skipFurtherChecksUntilTimePeriod){
			skipFurtherChecksUntilTimePeriod = null;

			bool canComplete = _hasPassed;

			if(!_hasPassed){
				_hasPassed = true;
			}

			return canComplete;
		}
	}
}

