using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Simulation.CLI
{
    public static class HelpCommand
    {
        public static void PrintHelp()
        {
            Console.Write(@"This is CLI Interface to AkkaSim" +
                          " -h, -help, -?, /? : Display this!" +
                          " -simtype <Type> : Specify simulation Type <Central/Decentral>" +
                          " -config <id> : Specify the simulationId to run with.");
        }
    }
}
