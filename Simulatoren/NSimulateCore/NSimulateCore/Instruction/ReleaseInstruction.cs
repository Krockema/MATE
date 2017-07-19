using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to release allocated resources
	/// </summary>
	public class ReleaseInstruction<TResource> : InstructionBase
		where TResource : Resource
	{
		private AllocateInstruction<TResource> _allocateInstruction = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.ReleaseInstruction`1"/> class.
		/// </summary>
		/// <param name='allocateInstruction'>
		/// The allocation instruction whose resources will be released.
		/// </param>
		public ReleaseInstruction (AllocateInstruction<TResource> allocateInstruction)
		{
			_allocateInstruction = allocateInstruction;
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

			// it is always possible to release
			return true;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, involves releasing allocated resources.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);
		
			if (_allocateInstruction != null 
			    && _allocateInstruction.IsAllocated 
			    && !_allocateInstruction.IsReleased) {
				_allocateInstruction.Release();
			}

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

