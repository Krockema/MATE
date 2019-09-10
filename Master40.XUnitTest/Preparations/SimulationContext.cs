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
            return Akka.Actor.Props.Create(factory: () => new SimulationContext(testProbe));
        }
        public SimulationContext(IActorRef testProbe)
        {
            TestProbe = testProbe;

            Receive<SimulationMessage.Done>(handler: x => Console.WriteLine(value: x.ToString()));

            Receive<ISimulationMessage>(handler: m =>
            {
                if (m.Target != ActorRefs.NoSender)
                    m.Target.Forward(message: m);
                else if (m.TargetSelection != null)
                    m.TargetSelection.Tell(message: m);
                else
                    Sender.Tell(message: m);

                testProbe.Forward(message: m);
            });

            ReceiveAny(handler: x => Console.WriteLine(value: x.ToString()));
        }
    }

    public class TestMessage : SimulationMessage
    {
        public TestMessage(IActorRef target, object msg) : base(message: msg, target: target)
        {
        }

    }
}
