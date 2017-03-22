using System;
using NSimulate;
using NSimulate.Instruction;

namespace NSimulate.Example4
{
	public class AlarmActivity : Activity
	{
		public override System.Collections.Generic.IEnumerator<NSimulate.Instruction.InstructionBase> Simulate ()
		{
			var notification = new AlarmRingingNotification();
			yield return new RaiseNotificationInstruction<AlarmRingingNotification>(notification);
		}
	}
}

