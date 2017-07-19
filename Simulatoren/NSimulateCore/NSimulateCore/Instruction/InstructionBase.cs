using System;

namespace NSimulate.Instruction
{
	/// <summary>
	/// Base class for all instructions
	/// </summary>
	public abstract class InstructionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.InstructionBase"/> class.
		/// </summary>
		public InstructionBase()
		{
			Priority = Priority.Medium;
		}

		/// <summary>
		/// Gets or sets the time period that this instruction was raised in.
		/// </summary>
		/// <value>
		/// The raised in time period.
		/// </value>
		public long RaisedInTimePeriod
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instruction has been interrupted.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is interrupted; otherwise, <c>false</c>.
		/// </value>
		public bool IsInterrupted
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this instruction has completed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is completed; otherwise, <c>false</c>.
		/// </value>
		public bool IsCompleted
		{
			get {
				return CompletedAtTimePeriod != null;
			}
		}

		/// <summary>
		/// Gets or sets the time period in which this instruction completed.
		/// </summary>
		/// <value>
		/// The time period in wich the instruction completed.
		/// </value>
		public long? CompletedAtTimePeriod
		{
			get;
			protected set;
		}

		public virtual Priority Priority
		{
			get;
			protected set;
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
		public virtual bool CanComplete(SimulationContext context, out long? skipFurtherChecksUntilTimePeriod){
			skipFurtherChecksUntilTimePeriod = null;
			return false;
		}

		/// <summary>
		/// Complete the instruction.
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public virtual void Complete(SimulationContext context){
			CompletedAtTimePeriod = context.TimePeriod;
		}

		/// <summary>
		/// Interrupt the instruction
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public virtual void Interrupt(SimulationContext context){
			IsInterrupted = true;
		}
	}
}

