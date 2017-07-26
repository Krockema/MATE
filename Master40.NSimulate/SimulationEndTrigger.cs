using System;
using NSimulate.Instruction;
using System.Linq;
using System.Collections.Generic;

namespace NSimulate
{
	/// <summary>
	/// A process that acts as a trigger for ending the simulation
	/// </summary>
	public class SimulationEndTrigger : Process
	{
		public SimulationEndTrigger (Func<bool> condition)
		{
			Condition = condition;
		}

		protected Func<bool> Condition {
			get;
			set;
		}

		/// <summary>
		/// Simulate the process.
		/// </summary>
		public override IEnumerator<InstructionBase> Simulate()
		{
			// wait for the condition to be met
			yield return new WaitConditionInstruction(Condition);

			// terminate the simulation
			yield return new TerminateSimulationInstruction();
		}
	}
}

