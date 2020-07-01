using Master40.SimulationCore.Environment;
using System;

namespace Master40.Simulation.CLI
{
    public interface ICommand
    {
        string ArgLong { get; }
        string ArgShort { get; }
        bool HasProperty { get; }
        string Description { get; }
        /// <summary>
        /// Input is { Configuration, ConfigElementName, ConfigElementValue }
        /// </summary>
        Action<Configuration, string> Action { get; }
    }
}
