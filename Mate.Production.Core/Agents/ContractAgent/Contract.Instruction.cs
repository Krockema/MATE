﻿using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Mate.DataCore.DataModel;
using static FArticles;
using static FCentralProvideOrders;
using static FStartOrders;

namespace Mate.Production.Core.Agents.ContractAgent
{
    public partial class Contract
    {
        public class Instruction { 
            public class StartOrder : SimulationMessage
            {
                private StartOrder(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message: message, target: target, logThis: logThis, priority: priority)
                {
                }

                public static ISimulationMessage Create(FStartOrder message, IActorRef target, bool logThis = false)
                {
                    return new StartOrder(message: message, target: target, logThis: logThis);
                }
                public FStartOrder GetObjectFromMessage { get => Message as FStartOrder; }
            }

            public class TryFinishOrder : SimulationMessage
            {
                private TryFinishOrder(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message: message, target: target, logThis: logThis, priority: priority)
                {
                }

                public static ISimulationMessage Create(FCentralProvideOrder message, IActorRef target, bool logThis = false)
                {
                    return new TryFinishOrder(message: message, target: target, logThis: logThis);
                }
                public FCentralProvideOrder GetObjectFromMessage { get => Message as FCentralProvideOrder; }
            }

            

            public class Finish : SimulationMessage
            {
                private Finish(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message: message, target: target, logThis: logThis, priority: priority)
                {
                }

                public static ISimulationMessage Create(FArticle requestItem, IActorRef target, bool logThis)
                {
                    return new Finish(message: requestItem, target: target, logThis: logThis);
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }

            }
        }
    }
}
