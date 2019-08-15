using System;
using Akka.Actor;
using Akka.Event;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Helper;

namespace Master40.XUnitTest.Preparations
{
    public class SimulationContext : ReceiveActor
    {
        public IActorRef TestProbe { get; set; }
        public static Props Props(IActorRef testProbe)
        {
            return Akka.Actor.Props.Create(() => new SimulationContext(testProbe));
        }
        public SimulationContext(IActorRef testProbe)
        {
            TestProbe = testProbe;

            Receive<SimulationMessage.Done>(x => Console.WriteLine(x.ToString()));

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

            ReceiveAny(x => Console.WriteLine(x.ToString()));
        }
    }

    public class TestMessage : SimulationMessage
    {
        public TestMessage(IActorRef target, object msg) : base(msg, target)
        {
        }

    }
}
