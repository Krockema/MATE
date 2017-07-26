using System;
using NSimulate.Instruction;
using System.Collections.Generic;

namespace NSimulate
{
	public class ActivityHostProcess : Process
	{
		public ActivityHostProcess (Activity activity, long waitTime)
		{
			WaitTime = waitTime;
			Activity = activity;
		}

		public long WaitTime{
			get;
			private set;
		}

		public Activity Activity{
			get;
			private set;
		}

		/// <summary>
		/// Simulate the process.
		/// </summary>
		public override IEnumerator<InstructionBase> Simulate()
		{
			// wait for the time the activity is to take place
			yield return new WaitInstruction(WaitTime);

			long timePeriodOfActivityStart = Context.TimePeriod;

			var enumerator = Activity.Simulate();

			while(enumerator.MoveNext()){
				yield return enumerator.Current;
			}
		}
	}
}