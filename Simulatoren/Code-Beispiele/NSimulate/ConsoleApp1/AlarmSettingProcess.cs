using System;
using NSimulate;
using NSimulate.Instruction;
using System.Collections.Generic;

namespace NSimulate.Example4
{
	public class AlarmSettingProcess : Process
	{
		/// <summary>
		/// Simulate the process.
		/// </summary>
		public override IEnumerator<InstructionBase> Simulate()
		{
			// set the alarm activity to occur 8 time periods from now
			yield return new ScheduleActivityInstruction(new AlarmActivity(), 8);

			// and another alarm activity to occur 9 time periods from now
			yield return new ScheduleActivityInstruction(new AlarmActivity(), 9);
		}
	}
}

