//=============================================================================
//=  $Id: RecessTracked.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
using React.Distribution;
using React.Tasking;
using React.Monitoring;

public class RecessTracked : Simulation
{
    /// <summary>The "thwacker" resource.</summary>
    private Resource _thwacker;
    /// <summary>
    /// Use to generate unique ids for each PreSchooler process.
    /// </summary>
    private int _idPool = 1;

    private RecessTracked()
    {
        _thwacker = Resource.Create(
            new string[] { "RED", "BLUE", "GREEN", "YELLOW" });
    }

    private IEnumerator<Task> PreSchooler(Process process, object data)
    {
        int id = _idPool++;
        IUniform rnd = UniformStreams.DefaultStreams[0];
        long delayDuration = (long)(rnd.NextDouble() * 100.0);

        Console.WriteLine(id + ": Attempting to get a thwacker @ " + Now);
        yield return _thwacker.Acquire(process);                    // (1)

        string color = process.ActivationData as string;

        Console.WriteLine(id + ": Got the " + color + " thwacker @ " + Now);
        yield return process.Delay(delayDuration);

        Console.WriteLine(id + ": Releasing the " + color + " thwacker @ " + Now);
        yield return _thwacker.Release(process);                    // (2)

        Console.WriteLine(id + ": PreSchooler process ends @ " + Now);
        yield break;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Recess begins ...");
        RecessTracked p = new RecessTracked();
        Task[] tasks = new Task[25];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Process(p, p.PreSchooler);
        }

        p.Run(tasks);
        Console.WriteLine("Recess has ended.");
    }

    /*
     * (1) It would also be possible to acquire a thwacker using
     * 
     *        yield return new AcquireResource(this, _thwacker);
     * 
     * (2) It would also be possible to release a thwacker using
     * 
     *         yield return new ReleaseResource(this, _thwacker, color);
     * 
     *     Note that when using the ReleaseResource task directly,
     *     the actual resource item ('color' in this case) MUST be
     *     specified.
     */
}