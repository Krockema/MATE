using System;
using System.Collections.Generic;
using System.Text;
using Master40.Simulation.CLI.Arguments;

namespace Master40.Simulation.CLI
{
    public class Commands
    {
        public static List<ICommand> GetCommands()
        {
            var cmd = new List<ICommand>();
            cmd.Add(new Help());
            cmd.Add(new Config());
            cmd.Add(new SimType());
            return cmd;
        }


    }
}
