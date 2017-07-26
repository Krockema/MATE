using System;

namespace NSimulate
{
	/// <summary>
	/// Base class for all simulation elements
	/// </summary>
	public abstract class SimulationElement
	{
		/// <summary>
		/// Gets the key that identifies this element
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public object Key
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the context providing state information for the simulation.
		/// </summary>
		/// <value>
		/// The context.
		/// </value>
		protected SimulationContext Context
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.SimulationElement"/> class.
		/// </summary>
		public SimulationElement ()
			: this(SimulationContext.Current, Guid.NewGuid())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.SimulationElement"/> class.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		public SimulationElement (object key)
			: this(SimulationContext.Current, key)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.SimulationElement"/> class.
		/// </summary>
		/// <param name='context'>
		/// Context.
		/// </param>
		public SimulationElement (SimulationContext context)
			: this(context, Guid.NewGuid())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NSimulate.SimulationElement"/> class.
		/// </summary>
		/// <param name='context'>
		/// Context.
		/// </param>
		/// <param name='key'>
		/// Key.
		/// </param>
		public SimulationElement (SimulationContext context, object key)
		{
			Key = key;
		    context?.Register(this.GetType(), this);

		    Context = context;
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		protected virtual void Initialize()
		{
		}
	}
}

