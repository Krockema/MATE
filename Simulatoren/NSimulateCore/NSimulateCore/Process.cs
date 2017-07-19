using System;
using System.Linq;
using System.Collections.Generic;
using NSimulate.Instruction;

namespace NSimulate
{
	/// <summary>
	/// Base class for all processes
	/// </summary>
	public class Process : SimulationElement
	{
		public static int nextInstanceIndex = 1;

		/// <summary>
		/// Backing field for the Priority property
		/// </summary>
		private Priority priority = Priority.Medium;

		/// <summary>
		/// Backing field for the instance index property
		/// </summary>
		private int instanceIndex = nextInstanceIndex++;

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Process"/> class.
		/// </summary>
		public Process ()
		{
			SimulationState = new ProcessSimulationState();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Process"/> class.
		/// </summary>
		public Process (object key)
			: base(key)
		{
			SimulationState = new ProcessSimulationState();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Process"/> class.
		/// </summary>
		public Process (SimulationContext context)
			: base(context)
		{
			SimulationState = new ProcessSimulationState();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Process"/> class.
		/// </summary>
		public Process (SimulationContext context, object key)
			: base(context, key)
		{
			SimulationState = new ProcessSimulationState();
		}

		/// <summary>
		/// Gets or sets the simulation state associated with this process
		/// </summary>
		public ProcessSimulationState SimulationState
		{
			get;
			set;
		}

		public Priority Priority
		{
			get{
				return priority;
			}
			set{
				priority = value;
			}
		}

		public int InstanceIndex
		{
			get{
				return instanceIndex;
			}
		}

		/// <summary>
		/// Simulate the process.
		/// </summary>
		public virtual IEnumerator<InstructionBase> Simulate()
		{
			return new List<InstructionBase>().GetEnumerator();
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize ();
		}
	}
}