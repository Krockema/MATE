//=============================================================================
//=  $Id: ResupplyConsumable.cs 184 2006-10-14 18:46:48Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
//=
//=  React.NET is free software; you can redistribute it and/or modify it
//=  under the terms of the GNU General Public License as published by the
//=  Free Software Foundation; either version 2 of the License, or (at your
//=  option) any later version.
//=
//=  React.NET is distributed in the hope that it will be useful, but WITHOUT
//=  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
//=  FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
//=  more details.
//=
//=  You should have received a copy of the GNU General Public License along
//=  with React.NET; if not, write to the Free Software Foundation, Inc.,
//=  51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//=============================================================================

using System;

namespace React.Tasking
{
    /// <summary>
    /// A <see cref="Task"/> used to add consumable items to a 
    /// <see cref="Consumable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="IConsumable.Resupply"/>
    /// method which will return the appropriate <see cref="Task"/> for
    /// adding consumable items to a <see cref="Consumable"/> on behalf of
    /// a client <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="Consumable"/>.
    /// </para>
    /// </remarks>
    public class ResupplyConsumable : ProxyTask<Consumable>
    {
        /// <summary>
        /// The number of consumable items to add to a
        /// <see cref="Consumable"/>.
        /// </summary>
        private int _quantity;

        /// <summary>
        /// Create a <see cref="ResupplyConsumable"/> that will add one (1)
        /// consumable item to the specified <see cref="Consumable"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="consumable">
        /// The <see cref="Consumable"/> to re-supply.
        /// </param>
        public ResupplyConsumable(Simulation sim, Consumable consumable)
            : this(sim, consumable, 1)
        {
        }

        /// <summary>
        /// Create a <see cref="ResupplyConsumable"/> that will add the
        /// specified number of consumable items to a <see cref="Consumable"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="consumable">
        /// The <see cref="Consumable"/> to re-supply.
        /// </param>
        /// <param name="quantity">
        /// The number of consumable items to add to
        /// <paramref name="consumable"/>.
        /// </param>
        public ResupplyConsumable(Simulation sim, Consumable consumable, int quantity)
            : base(sim, consumable)
        {
            Quantity = quantity;
        }

        /// <summary>
        /// Gets or sets the number of consumable items that will be added to
        /// the <see cref="Consumable"/>.
        /// </summary>
        /// <value>
        /// The number of consumable items to add as an <see cref="int"/>.
        /// </value>
        /// <exception cref="ArgumentException">
        /// If an attempt is made set this property to a negative value.
        /// </exception>
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Quantity cannot be negative.");
                }
                _quantity = value;
            }
        }

        /// <summary>
        /// Resupply a <see cref="Consumable"/> on behalf of some client
        /// <see cref="Task"/>.
        /// </summary>
        /// <param name="activator">Not used.</param>
        /// <param name="data">Not used.</param>
        protected override void ExecuteTask(object activator, object data)
        {
            Blocker.AddUnits(Quantity);
            ResumeAll(Blocker, null);
        }
    }
}
