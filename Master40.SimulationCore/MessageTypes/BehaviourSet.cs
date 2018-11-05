using AkkaSim.Interfaces;
using Master40.SimulationCore.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.MessageTypes
{
    public class BehaviourSet
    {
        public BehaviourSet(Dictionary<Type, Action<Agent, ISimulationMessage>> actions, Dictionary<string, object> properties = null, object obj = null)
        {
            Actions = actions;
            Properties = (properties == null) ? new Dictionary<string, object>() : properties;
            Object = obj;
        }
        public IReadOnlyDictionary<Type, Action<Agent, ISimulationMessage>> Actions { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public object Object { get; }
    }
}
