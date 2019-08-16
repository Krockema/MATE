using System;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using static Master40.SimulationCore.Agents.Guardian.Instruction;

namespace Master40.SimulationCore.Agents.Guardian
{
    public class GuardianBehaviour : SimulationCore.Types.Behaviour
    {
        internal GuardianBehaviour(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker, SimulationType simulationType) 
            : base(childMaker: childMaker
                 , simulationType: simulationType) { }

        public static GuardianBehaviour Get(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker, SimulationType simulationType)
        {
            return new GuardianBehaviour(childMaker, simulationType);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case CreateChild m: CreateChild(m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal void CreateChild(AgentSetup setup)
        {
            var childRef = Agent.Behaviour.ChildMaker(Agent.Context, setup);
            Agent.Send(BasicInstruction.Initialize.Create(childRef, setup.Behaviour));
        }

        
    }
}
