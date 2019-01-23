using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class Guardian
    {
        public class Instruction
        {
            public class CreateChild : SimulationMessage
            {
                public static CreateChild Create(AgentSetup setup, IActorRef target)
                {
                    return new CreateChild(setup, target);
                }
                private CreateChild(object message, IActorRef target) : base(message, target)
                {
                }
                public AgentSetup GetObjectFromMessage { get => this.Message as AgentSetup; }
            }
        }
    }
}
