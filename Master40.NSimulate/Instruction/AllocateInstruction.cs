using System;
using NSimulate;
using System.Linq;
using System.Collections.Generic;

namespace NSimulate.Instruction
{
	/// <summary>
	/// Instruction used to allocate resources
	/// </summary>
	public class AllocateInstruction<TResource> : InstructionBase
		where TResource : Resource
	{
		/// <summary>
		/// A function used to test whether a resource matches the request
		/// </summary>
		private Func<TResource, bool> _resourceMatchFunction  = (o)=>true;

		/// <summary>
		/// A function used to determine a priority of a resource, used to prioritise resource selections
		/// </summary>
		private Func<TResource, int> _resourcePriorityFunction = null;

		/// <summary>
		/// Gets the set of allocations made whe completing this instruction
		/// </summary>
		/// <value>
		/// The allocations made
		/// </value>
		public List<KeyValuePair<TResource, int>> Allocations{
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of resources requested.
		/// </summary>
		/// <value>
		/// The number requested.
		/// </value>
		public int NumberRequested{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the allocations associated with this instruction have been mae
		/// </summary>
		/// <value>
		/// <c>true</c> if resources are allocated; otherwise, <c>false</c>.
		/// </value>
		public bool IsAllocated{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the resources associated with this instructions have been released
		/// </summary>
		/// <value>
		/// <c>true</c> if resources are released; otherwise, <c>false</c>.
		/// </value>
		public bool IsReleased{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Instruction.AllocateInstruction`1"/> class.
		/// </summary>
		/// <param name='number'>
		/// Number of resources to allocate.
		/// </param>
		/// <param name='resourceMatchFunction'>
		/// An optional function used to match resources for allocation.
		/// </param>
		/// <param name='resourcePriorityFunction'>
		/// An optional function used to prioritise resources for allocation where multiple resources match
		/// </param>
		public AllocateInstruction (int number, Func<TResource, bool> resourceMatchFunction = null, Func<TResource, int> resourcePriorityFunction = null)
		{
			Priority = Priority.Low; // reduce priority so that allocate is processed after any releases
			NumberRequested = number;

			if (resourceMatchFunction != null){
				_resourceMatchFunction = resourceMatchFunction;
			}
			if (resourcePriorityFunction!=null){
				_resourcePriorityFunction = resourcePriorityFunction;
			}
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

			// build a set or resources for possible allocation, filtering and prioritising as appropriate
			IEnumerable<TResource> resources = null;
			if (_resourcePriorityFunction == null){
				resources = context.GetByType<TResource>()
					.Where(_resourceMatchFunction);
			}
			else{
				resources = context.GetByType<TResource>()
					.Where(_resourceMatchFunction)
					.OrderBy(_resourcePriorityFunction);
			}

			// check if the are enough resources available for allocation
			bool enoughAvailable = false;
			int available = 0;
			foreach(TResource resource in resources){
				int stillAvailable = resource.Capacity - resource.Allocated;

				if (stillAvailable > 0){
					available += stillAvailable;
				}

				if (available >= NumberRequested){
					enoughAvailable = true;
					break;
				}
			}

			// if there are currently enough resources available, the instruction can complete
			return enoughAvailable;
		}

		/// <summary>
		/// Complete the instruction.  For this instruction type, allocates the requested resources
		/// </summary>
		/// <param name='context'>
		/// Context providing state information for the current simulation.
		/// </param>
		public override void Complete (SimulationContext context)
		{
			base.Complete(context);

			IEnumerable<TResource> resources = null;

			if (_resourcePriorityFunction == null){
				resources = context.GetByType<TResource>()
					.Where(_resourceMatchFunction);
			}
			else{
				resources = context.GetByType<TResource>()
					.Where(_resourceMatchFunction)
					.OrderBy(_resourcePriorityFunction);
			}

			Allocations = new List<KeyValuePair<TResource, int>>();

			int allocated = 0;
			foreach(TResource resource in resources){
				int available = resource.Capacity - resource.Allocated;

				if (available > 0){

					int amountToAllocate = Math.Min(available, NumberRequested - allocated);

					Allocations.Add(new KeyValuePair<TResource, int>(resource, amountToAllocate));
					resource.Allocated += amountToAllocate;
					allocated += amountToAllocate;

					if (allocated == NumberRequested){
						break;
					}
				}
			}

			IsAllocated = true;
			IsReleased = false;
			 	
			CompletedAtTimePeriod = context.TimePeriod;
		}

		/// <summary>
		/// Release the resources previously allocated wen completing tis instruction.
		/// </summary>
		public void Release(){
			if (IsAllocated && !IsReleased){
				foreach(var allocation in Allocations){
					allocation.Key.Allocated -= allocation.Value;
				}

				IsAllocated = false;
				IsReleased = true;
			}
		}
	}
}

