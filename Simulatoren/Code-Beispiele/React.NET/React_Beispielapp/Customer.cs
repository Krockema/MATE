//=============================================================================
//=  $Id: Customer.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
using System.Collections.Generic;
using System.Text;
using React;

namespace BarberShop
{
    /// <summary>
    /// The customer <see cref="Process"/>.
    /// </summary>
    /// <remarks>
    /// Each <see cref="Customer"/> waits to acquire a <see cref="Barber"/> from
    /// a <see cref="TrackedResource"/>.  Once having obtained a
    /// <see cref="Barber"/>, they activate the <see cref="Barber"/> process
    /// to simulate cutting hair.
    /// </remarks>
    internal class Customer : Process
    {
        internal Customer(Simulation sim) : base(sim)
        {
        }

        protected override IEnumerator<Task> GetProcessSteps()
        {
            Resource barbers = (Resource)ActivationData;
            yield return barbers.Acquire(this);

            System.Diagnostics.Debug.Assert(barbers == Activator);
            System.Diagnostics.Debug.Assert(ActivationData != null);
            Barber barber = (Barber)ActivationData;

            WaitOnTask(barber);
            yield return Suspend();
            // HINT: The above two lines of code can be shortened to
            //          yield return barber;

            Console.WriteLine("Customer pays {0} for the haircut.",
                barber.Name);

            yield return barbers.Release(this);
        }
    }
}
