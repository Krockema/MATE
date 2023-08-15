using System;

namespace Mate.Production.CLI.Options
{
    public static class HelpCommand
    {
        public static void PrintHelp()
        {
            var helpline = "This is CLI Interface to AkkaHive" +
                          " -h, -help, -?, /? : Display this!";
            

            Console.Write(value: helpline);
        }
    }
}
