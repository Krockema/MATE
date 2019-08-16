using Master40.SimulationCore.Environment;
using System;
namespace Master40.Simulation.CLI.Arguments
{
    public class Help : ICommand
    {
        public string ArgLong => "Help";
        public string ArgShort => "h";
        public bool HasProperty => false;
        public string Description => " -h, -help, -?, /? : Display this!";
        public Action<Configuration, string> Action { get; }
        public Help()
        {
            Action = (result, str) => { 
                new Commands().ForEach(action: x => { Console.WriteLine(value: x.Description); });
            };
        }
    }
}
