using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public static class ProductionBehaviour
    {
        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

           // actions.Add(typeof(RequestArticle), RequestArticle);
           // actions.Add(typeof(ResponseFromStock), ResponseFromStock);

            return new BehaviourSet(actions);
        }
    }
}
