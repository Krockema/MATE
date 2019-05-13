using System;
namespace Master40.Simulation.CLI.Arguments
{
    public class Help : ICommand
    {
        public string ArgLong => "Help";
        public string ArgShort => "h";
        public bool HasProperty => false;
        public string Description => " -h, -help, -?, /? : Display this!";
        public Action<ParseResult, string> Action { get; }
        public Help()
        {
            Action = (result, str) => Console.WriteLine(this.Description);
        }
    }
}
