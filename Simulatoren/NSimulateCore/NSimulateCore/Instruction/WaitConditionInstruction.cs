using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// Instruction used to wait until a condition is met
	/// </summary>
	public class WaitConditionInstruction : InstructionBase
	{
		public WaitConditionInstruction (Func<bool> condition)
		{
			Condition = condition;
		}

		protected Func<bool> Condition {
			get;
			set;
		}

		public override bool CanComplete(SimulationContext context, out long? skipFurtherChecksUntilTimePeriod){
			skipFurtherChecksUntilTimePeriod = null;

			return Condition();
		}
	}
}

