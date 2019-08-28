using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Simulation.CLI
{
    public interface ICommand
    {
        string ArgLong { get; }
        string ArgShort { get; }
        bool HasProperty { get; }
        string Description { get; }
        Action<ParseResult, string> Action { get; }
    }
}
