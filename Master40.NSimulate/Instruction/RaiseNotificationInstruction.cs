using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// An instruction used to raise an event
	/// </summary>
	public class RaiseNotificationInstruction<TNotification> : InstructionBase
	{
		public RaiseNotificationInstruction (TNotification notificationToRaise)
		{
			Notification = notificationToRaise;
		}

		public TNotification Notification{
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
		/// Complete the instruction.  For this instruction type, this involves putting the simulation into the terminating state.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			// Add the event to the wait instructions of all processes currently waiting
			if (context.ActiveProcesses != null){
				foreach(var process in context.ActiveProcesses){
					if (process.SimulationState != null 
					    && process.SimulationState.InstructionEnumerator != null
					    && process.SimulationState.InstructionEnumerator.Current != null
					    && process.SimulationState.InstructionEnumerator.Current is WaitNotificationInstruction<TNotification>) {

						var waitEventInstruction = (WaitNotificationInstruction<TNotification>)process.SimulationState.InstructionEnumerator.Current;

						if (waitEventInstruction.MatchingCondition == null || waitEventInstruction.MatchingCondition(Notification)){
							waitEventInstruction.Notifications.Add(Notification);
						}
					}
				}
			}

			CompletedAtTimePeriod = context.TimePeriod;
		}
	}
}

