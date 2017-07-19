//=============================================================================
//=  $Id: Program.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
using React.Monitoring;

class Program : Simulation
{
    private Consumable _fuelDepot;

    private Program()
    {
        _fuelDepot = new Consumable("Fuel Depot", 5000);
    }

    public Consumable FuelDepot
    {
        get { return _fuelDepot; }
    }

    private IEnumerator<Task> ResupplyFuel(Process process, object data)
    {
        int fuel = 10000;
        while (fuel > 0)
        {
            yield return process.Delay(10);
            Console.WriteLine("** Adding fuel @ " + process.Now + ", gallons=" + FuelDepot.Count);
            yield return FuelDepot.Resupply(process, 100);
            fuel -= 100;
        }
        yield break;
    }

    /*
    static void Main(string[] args)
    {
        Program p = new Program();
        Task[] tasks = new Task[6];
        for (int i = 0; i < tasks.Length - 1; i++)
        {
            tasks[i] = new Truck(p);
        }

        Process process = new Process(p, p.ResupplyFuel);
        process.Activate(null);
        tasks[tasks.Length - 1] = process;

        p.Run(tasks);
    }
    */

    static void Main(string[] args)
    {
        Program p = new Program();
        Task[] tasks = new Task[6];
        for (int i = 0; i < 5 ; i++)
        {
            Task t = new Truck(p);
            t.Activate(null, 0L, p.FuelDepot);
        }

        Process process = new Process(p, p.ResupplyFuel);
        process.Activate(null);
//        tasks[tasks.Length - 1] = process;

        p.Run();

  //      p.Run(tasks);
    }
}
