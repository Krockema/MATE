using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Helper;

namespace Master40.XUnitTest.Agents
{
    public class SimulationContextMoc : ReceiveActor
    {
        public IActorRef TestProbe { get; set; }
        public static Props Props(IActorRef testProbe)
        {
            return Akka.Actor.Props.Create(() => new SimulationContextMoc(testProbe));
        }
        public SimulationContextMoc(IActorRef testProbe)
        {
            TestProbe = testProbe;

            Receive<ISimulationMessage>(m =>
            {
                
                if (m.Target != ActorRefs.NoSender)
                    m.Target.Forward(m);
                else if (m.TargetSelection != null)
                    m.TargetSelection.Tell(m);
                else
                    Sender.Tell(m);

                testProbe.Forward(m);
            });


        }
    }

    public class TestMessage : SimulationMessage 
    {
        public TestMessage(IActorRef target, object msg) : base(msg, target)
        {
        }

    }
}
