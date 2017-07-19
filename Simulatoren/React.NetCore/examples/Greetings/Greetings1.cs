//=============================================================================
//=  $Id: Greetings1.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
using React;

/// <summary>
/// The first "Hello, world" example program.
/// </summary>
/// <remarks>
/// This class runs a simple simulation that prints a greeting message to
/// standard output (e.g. the console).  In this example, none of the
/// React.NET classes are subclassed to implement the simulation.  The
/// process steps are provided via a delegate, <see cref="SayHello"/>.
/// <para>
/// This example program demonstrates how a simple simulation can be created
/// without extending any of the React.NET classes.  Most simulations will
/// probably contain subclasses of the <see cref="Process"/> class and
/// possibly of the <see cref="Simulation"/> class.
/// </para>
/// </remarks>
public class Greetings
{
    private Greetings() { }

	private IEnumerator<Task> SayHello(Process process, object data)
	{
		Console.WriteLine("Greetings1 says, Hello there ...");
        yield break;                                                    // (1)
	}

	public static void Main(string [] args)
	{
        Simulation sim = new Simulation();
        Greetings greetings = new Greetings();                          // (2)
        Process process = new Process(sim, greetings.SayHello);         // (3)
        sim.Run(process);
	}

    /*
     * (1) Remember, an iterator must contain at least one "yield" statement.
     *     It can be either a "yield return {expr}" or a "yield break".
     * 
     * (2) Since the Greetings class doesn't really do anything, it would
     *     have been possible to avoid instantiating a Greetings object if
     *     the SayHello method had been declared static.  Give it a try.
     *     [Hint: you'll need to modify a parameter passed to the Process
     *     constructor.]
     * 
     * (3) In C# 1.0, this wouldn't be valid.  Instead we would have had
     *     to have written:
     * 
     *     ... = new Process(sim, new ProcessSteps(greetings.SayHello));
     * 
     *     to have instantiated the delegate.  Of course, this syntax is
     *     still valid with C# 2.0.
     */
}
