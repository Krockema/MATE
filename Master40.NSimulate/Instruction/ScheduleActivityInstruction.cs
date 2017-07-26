using System;
using NSimulate;

namespace NSimulate.Instruction
{
	public class ScheduleActivityInstruction : InstructionBase
	{
		public ScheduleActivityInstruction (Activity activity, long waitTime)
		{
			Activity = activity;
			WaitTime = waitTime;
		}

		public Activity Activity
		{
			get;
			private set;
		}

		public long WaitTime
		{
			get;
			private set;
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

			return true;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, this involves interrupting a process.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			var process = new ActivityHostProcess(Activity, WaitTime);
			context.Register(process);

			if (context.ActiveProcesses != null){
				// add it to te active process list
				context.ActiveProcesses.Add(process);
				context.ProcessesRemainingThisTimePeriod.Enqueue(process);
			}
		}
	}
}

