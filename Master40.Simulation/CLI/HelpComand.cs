using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Simulation.CLI
{
    public static class HelpCommand
    {
        public static void PrintHelp()
        {
            var helpline = "This is CLI Interface to AkkaSim" +
                          " -h, -help, -?, /? : Display this!";
            

            Console.Write(value: helpline);
        }
    }
}
