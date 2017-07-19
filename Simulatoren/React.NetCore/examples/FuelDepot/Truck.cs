//=============================================================================
//=  $Id: Truck.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
using React.Tasking;

class Truck : Process
{
    private static int idPool = 0;
    private int id;

    public Truck(Simulation sim) : base(sim)
    {
        idPool++;
        id = idPool;
    }

    private Consumable FuelDepot
    {
        get
        {
            Program p = (Program)Simulation;
            return p.FuelDepot;
        }
    }

    /*
    protected override IEnumerator<Task> GetProcessSteps()
    {
        Console.WriteLine(id + ": Truck process begins ...");
        yield return Delay(id);
        Console.WriteLine(id + ": Attempting to get fuel ...");
        //yield return FuelDepot.Acquire(this, 2500);
        yield return new AcquireConsumable(Simulation, FuelDepot, 2500, 300);
        if (Activator == FuelDepot)
        {
            Console.WriteLine(id + ": Got fueled @ " + Now + ", " +
                FuelDepot.Count + " gallons remain.");
        }
        else
        {
            Console.WriteLine(id + ": Gave up waiting for fuel @ " + Now);
        }
        yield return Delay(6);
        Console.WriteLine(id + ": Truck process ends @ " + Now);
        yield break;
    }
    */

    protected override IEnumerator<Task> GetProcessSteps()
    {
        Consumable depot = (Consumable)ActivationData;
        System.Diagnostics.Debug.Assert(depot != null);

        Console.WriteLine(id + ": Truck process begins TTT ...");
        yield return Delay(id);
        Console.WriteLine(id + ": Attempting to get fuel ...");
        //yield return FuelDepot.Acquire(this, 2500);
        yield return new AcquireConsumable(Simulation, depot, 2500, 300);
        if (Activator == depot)
        {
            Console.WriteLine(id + ": Got fueled @ " + Now + ", " +
                depot.Count + " gallons remain.");
        }
        else
        {
            Console.WriteLine(id + ": Gave up waiting for fuel @ " + Now);
        }
        yield return Delay(6);
        Console.WriteLine(id + ": Truck process ends @ " + Now);
        yield break;
    }
}
