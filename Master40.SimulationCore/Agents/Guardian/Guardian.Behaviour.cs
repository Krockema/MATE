using System;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;

namespace Master40.SimulationCore.Agents.Guardian
{
    public class GuardianBehaviour : Behaviour
    {
        internal GuardianBehaviour(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker, SimulationType simulationType) 
            : base(childMaker: childMaker
                 , simulationType: simulationType) { }

        public static GuardianBehaviour Get(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker, SimulationType simulationType)
        {
            return new GuardianBehaviour(childMaker, simulationType);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Guardian.Instruction.CreateChild m: CreateChild(agent, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal void CreateChild(Agent agent, AgentSetup setup)
        {
            var childRef = agent.Behaviour.ChildMaker(agent.Context, setup);
            agent.Send(BasicInstruction.Initialize.Create(childRef, setup.Behaviour));
        }

        
    }
}
