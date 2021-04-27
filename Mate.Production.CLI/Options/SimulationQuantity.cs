using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.CLI.Options
{
    public class SimulationQuantity : Option<SimulationQuantity>, ICommand
    {
        public SimulationQuantity()
        {
            Action = (config, argument) => { config.AddOption(this); };
        }
    }
}
