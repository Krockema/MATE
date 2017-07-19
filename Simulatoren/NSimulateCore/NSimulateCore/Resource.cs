using System;

namespace NSimulate
{
	/// <summary>
	/// A Resource.
	/// </summary>
	public class Resource : SimulationElement
	{
		/// <summary>
		/// Gets or sets the number (quantity) of this resource allocated
		/// </summary>
		/// <value>
		/// The number allocated.
		/// </value>
		public int Allocated
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the capacity in terms of number / quantity that can be allocated.
		/// </summary>
		/// <value>
		/// The capacity to be allocated.
		/// </value>
		public int Capacity
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Resource"/> class.
		/// </summary>
		public Resource ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Resource"/> class.
		/// </summary>
		public Resource (int capacity)
		{
			Capacity = capacity;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Resource"/> class.
		/// </summary>
		public Resource (object key, int capacity)
			: base(key)
		{
			Capacity = capacity;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Resource"/> class.
		/// </summary>
		public Resource (SimulationContext context, int capacity)
			: base(context)
		{
			Capacity = capacity;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.Resource"/> class.
		/// </summary>
		public Resource (SimulationContext context, object key, int capacity)
			: base(context, key)
		{
			Capacity = capacity;
		}
	}
}

