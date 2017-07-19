using System;
using System.Collections.Generic;
using NSimulate.Instruction;
using System.Linq;

namespace NSimulate.Instruction
{
	/// <summary>
	/// A Composite instruction which groups a set of other instructions.
	/// </summary>
	public class CompositeInstruction : InstructionBase
	{
		private List<InstructionBase>  _instructions;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.CompositeInstruction"/> class.
		/// </summary>
		/// <param name='instructions'>
		/// Instructions to be included in this composite instruction
		/// </param>
		public CompositeInstruction (List<InstructionBase> instructions)
		{
			_instructions = instructions;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.CompositeInstruction"/> class.
		/// </summary>
		/// <param name='instructions'>
		/// Instructions to be included in this composite instruction
		/// </param>
		public CompositeInstruction (params InstructionBase[] instructions)
		{
			_instructions = instructions.ToList();
		}

		/// <summary>
		/// Determines whether this instruction can complete in the current time period.  The composite instruction can complete, if all contained instructions can complete
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

			bool canComplete = true;
			bool nullCheckTimePeriodEncountered = false;

			// iterate through all contained instructions, check whether they can all complete
			// the value returned in skipFurtherChecksUntilTimePeriod is the lowest value of any instrution, and null if any return null for this value
			foreach(InstructionBase instruction in _instructions){
				long? nextCheckTimePeriod = null;

				bool thisInstructionCanComplete = instruction.CanComplete(context, out nextCheckTimePeriod);

				if (!thisInstructionCanComplete){
					canComplete = false;
				
					if (nextCheckTimePeriod == null){
						nullCheckTimePeriodEncountered = true;
						nextCheckTimePeriod = null;
					}
					else if (!nullCheckTimePeriodEncountered && 
		         			(skipFurtherChecksUntilTimePeriod == null || skipFurtherChecksUntilTimePeriod > nextCheckTimePeriod)){
						skipFurtherChecksUntilTimePeriod = nextCheckTimePeriod;
					}
				}
			}

			if (canComplete){
				skipFurtherChecksUntilTimePeriod = null;
			}

			return canComplete;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, completes all contained instructions
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			foreach(InstructionBase instruction in _instructions){
				instruction.Complete(context);
			}

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

