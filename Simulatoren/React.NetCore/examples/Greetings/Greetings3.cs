//=============================================================================
//=  $Id: Greetings3.cs 128 2005-12-04 20:12:00Z Eric Roe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2004, Eric K. Roe.  All rights reserved.
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
/// The third "Hello, world" example program.
/// </summary>
/// <remarks>
/// This class runs a simple simulation that prints a greeting message to
/// standard output (e.g. the console).  In this example, the main class
/// is derived from the <see cref="Simulation"/> React.NET class.  The
/// <see cref="SayHello"/> method, which prints the greeting message,
/// is used as the <see cref="ProcessSteps"/> delegate.
/// </remarks>
public class Greetings : React.Simulation
{
    private Greetings()
    {
        Process process = new Process(this, SayHello);
        process.Activate(this);
    }

    private IEnumerator<Task> SayHello(Process process, object data)
    {
        Console.WriteLine("Greetings3 says, Hello there ...");
        yield break;
    }
	
	public static void Main(string [] args)
	{
        Greetings greetings = new Greetings();
        greetings.Run();
	}
}
