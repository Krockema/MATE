using System;
using NSimulate;
using NSimulate.Instruction;

namespace NSimulate.Example4
{
	public class SleepingProcess : Process
	{
		public override System.Collections.Generic.IEnumerator<NSimulate.Instruction.InstructionBase> Simulate() 
		{
			Console.WriteLine(string.Format("Going to sleep at time period {0}", Context.TimePeriod));

			// wait till the alarm rings
			yield return new WaitNotificationInstruction<AlarmRingingNotification>();

			Console.WriteLine(string.Format("Alarm ringing at time period {0}", Context.TimePeriod));
			Console.WriteLine(string.Format("Going back to sleep at time period {0}", Context.TimePeriod));

			// go back to sleep and wait till it rings again
			yield return new WaitNotificationInstruction<AlarmRingingNotification>();

			Console.WriteLine(string.Format("Alarm ringing again..waking up at time period {0}", Context.TimePeriod));

			// notify now awake
			var notification = new AwakeNotification();
			yield return new RaiseNotificationInstruction<AwakeNotification>(notification);
		}
	}
}

