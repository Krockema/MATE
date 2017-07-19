//=============================================================================
//=  $Id: Greetings2.cs 128 2005-12-04 20:12:00Z Eric Roe $
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
/// The second "Hello, world" example program.
/// </summary>
/// <remarks>
/// This class runs a simple simulation that prints a greeting message to
/// standard output (e.g. the console).  In this example, the main class
/// is derived from the <see cref="Process"/> React.NET class.  The
/// <see cref="Process.GetProcessSteps"/> method is overridden to provide
/// the code which prints the greeting message.
/// </remarks>
public class Greetings : React.Process
{
    private Greetings(Simulation sim) : base(sim) { }

	protected override IEnumerator<Task> GetProcessSteps()
	{
		Console.WriteLine("Greetings2 says, Hello there ...");
		yield return Delay(1000);                                   // (1)
        yield break;                                                // (2)
	}
	
	public static void Main(string [] args)
	{
        Simulation sim = new Simulation();
        sim.Run(new Greetings(sim));
	}

    /*
     * (1) Just to show off how a time delay is accomplished.
     * 
     * (2) A "yield break" is not strictly needed here, but it doesn't
     *     hurt anything.  It ensures us the iterator will work even if
     *     the "yield return" statement was deleted.
     * 
     *     For the low-level code wonks out there, including the "yield
     *     break" does generate an additional MSIL branch instruction.
     *     If the "yield break" is not included, a simple NOP (no
     *     operation) instruction is used. [This is valid as of the
     *     Beta 2 version of the .NET framework v2.]
     */
}
