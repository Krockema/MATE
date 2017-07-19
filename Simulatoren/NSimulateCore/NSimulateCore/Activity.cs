using System;
using NSimulate.Instruction;
using System.Collections.Generic;

namespace NSimulate
{
	public class Activity
	{
		public Activity()
		{
		}

		/// <summary>
		/// Simulate the activity.
		/// </summary>
		public virtual IEnumerator<InstructionBase> Simulate()
		{
			return new List<InstructionBase>().GetEnumerator();
		}
	}
}

