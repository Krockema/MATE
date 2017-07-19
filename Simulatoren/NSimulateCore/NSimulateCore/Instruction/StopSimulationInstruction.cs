using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to stop the simulation.
	/// </summary>
	/// <remarks>This instruction, when completed, marks the simulation as stopping.  Individual processes are responsible for responding to this flag</remarks>
	public class StopSimulationInstruction : InstructionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.StopSimulationInstruction"/> class.
		/// </summary>
		public StopSimulationInstruction ()
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
			return true;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, this involves putting the simulation into the stopping state.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			context.IsSimulationStopping = true;

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

