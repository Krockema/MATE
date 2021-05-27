using System;
using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Helper;
using Mate.Production.Core.SignalR;
using Mate.Production.Core.Types;
using static Mate.Production.Core.Agents.Guardian.Instruction;

namespace Mate.Production.Core.Agents.Guardian
{
    public class GuardianBehaviour : Behaviour
    {
        private int _counterChilds = 0;

        private IMessageHub _messageHub;

        internal GuardianBehaviour(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker,
            SimulationType simulationType, IMessageHub messageHub)
            : base(childMaker: childMaker
                , simulationType: simulationType)
        {
            _messageHub = messageHub;
        }

        public static GuardianBehaviour Get(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker, SimulationType simulationType, IMessageHub messageHub)
        {
            return new GuardianBehaviour(childMaker: childMaker, simulationType: simulationType, messageHub);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case CreateChild m: CreateChild(setup: m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal void CreateChild(AgentSetup setup)
        {
            _counterChilds++;
            //System.Diagnostics.Debug.WriteLine($"({Agent.CurrentTime}) {Agent.Context.Self.Path.Name} add child and has {counterChilds} now");

            var childRef = Agent.Behaviour.ChildMaker(arg1: Agent.Context, arg2: setup);
            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: childRef, message: setup.Behaviour));
        }

        internal void RemoveChild()
        {
            _counterChilds--;
        }


        internal void LogChildCounter()
        {
            _messageHub.GuardianState(new[] {Agent.Name, _counterChilds.ToString()});
        }
    }
}
