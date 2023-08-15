using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Mate.Test.Online.Preparations
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

            Receive<HiveMessage.Done>(handler: x => Console.WriteLine(value: x.ToString()));

            Receive<IHiveMessage>(handler: m =>
            {
                if (m.Target != ActorRefs.NoSender)
                    m.Target.Forward(message: m);
                else
                    Sender.Tell(message: m);

                testProbe.Forward(message: m);
            });

            ReceiveAny(handler: x => Console.WriteLine(value: x.ToString()));
        }
    }

    public record TestMessage : HiveMessage
    {
        public TestMessage(IActorRef target, object msg) : base(message: msg, target: target)
        {
        }

    }
}
