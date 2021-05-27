using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.CLI.Options
{
    /// <summary>
    /// Defines whether Agent.Debug is also logged to console
    /// Require: Agent.Debug.Equal(true)
    /// </summary>
    public class DebugToConsole : Option<bool>
    {
        public DebugToConsole(bool value)
        {
            _value = value;
        }
    }
}
