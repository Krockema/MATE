using System;
using System.Linq;
using System.Collections.Generic;
using NSimulate.Instruction;

namespace NSimulate
{
	/// <summary>
	/// The simulation state for a process
	/// </summary>
	public class ProcessSimulationState
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.ProcessSimulationState"/> class.
		/// </summary>
		public ProcessSimulationState ()
		{
			IsActive = true;
		}

		/// <summary>
		/// Gets or sets the last instruction issued by the process
		/// </summary>
		/// <value>
		/// The last instruction.
		/// </value>
		public InstructionBase LastInstruction
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the process is active.
		/// </summary>
		/// <value>
		/// <c>true</c> if the process is active; otherwise, <c>false</c>.
		/// </value>
		public bool IsActive
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the process has been interrupted
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has been interrupted; otherwise, <c>false</c>.
		/// </value>
		public bool IsInterrupted
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the process has been completed.
		/// </summary>
		/// <value>
		/// <c>true</c> if the process has completed; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the enumerator used to iterate through instructions issued by this process.
		/// </summary>
		/// <value>
		/// The instruction enumerator.
		/// </value>
		public IEnumerator<InstructionBase> InstructionEnumerator
		{
			get;
			set;
		}
	}
}

