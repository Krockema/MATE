﻿using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Reporting;
using Mate.Production.Core.Environment.Records.Scopes;

namespace Mate.Production.Core.Agents.ResourceAgent
{
    public partial class Resource
    {
        public partial class Instruction
        {
            public record Default
            {
                public record SetHubAgent : HiveMessage
                {
                    public static SetHubAgent Create(AgentInformationRecord message, IActorRef target)
                    {
                        return new SetHubAgent(message: message, target: target);
                    }
                    private SetHubAgent(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public AgentInformationRecord GetObjectFromMessage { get => Message as AgentInformationRecord; }
                }

                public record RequestProposal : HiveMessage
                {
                    public static RequestProposal Create(RequestProposalForCapabilityRecord message, IActorRef target)
                    {
                        return new RequestProposal(message: message, target: target);
                    }
                    private RequestProposal(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public RequestProposalForCapabilityRecord GetObjectFromMessage { get => Message as RequestProposalForCapabilityRecord; }
                }

                public record AcknowledgeProposal : HiveMessage
                {
                    public static AcknowledgeProposal Create(IConfirmation message, IActorRef target)
                    {
                        return new AcknowledgeProposal(message: message, target: target);
                    }
                    private AcknowledgeProposal(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IConfirmation GetObjectFromMessage { get => Message as IConfirmation; }
                }

                public record AcceptedProposals : HiveMessage
                {
                    public static AcceptedProposals Create(IConfirmation message, IActorRef target)
                    {
                        return new AcceptedProposals(message: message, target: target);
                    }
                    private AcceptedProposals(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IConfirmation GetObjectFromMessage { get => Message as IConfirmation; }
                }


                public record DoWork : HiveMessage
                {
                    public static DoWork Create(OperationRecord message, IActorRef target)
                    {
                        return new DoWork(message: message, target: target);
                    }
                    private DoWork(OperationRecord message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public OperationRecord GetObjectFromMessage { get => (OperationRecord)Message; }
                }

                public record TryToWork : HiveMessage
                {
                    public static TryToWork Create(IActorRef target)
                    {
                        return new TryToWork(target: target);
                    }
                    private TryToWork(IActorRef target) : base(message: null, target: target)
                    {
                    }
                }

                public record RevokeJob : HiveMessage
                {
                    public static RevokeJob Create(Guid message, IActorRef target)
                    {
                        return new RevokeJob(message: message, target: target);
                    }
                    private RevokeJob(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public record RequeueBucket : HiveMessage
                {
                    public static RequeueBucket Create(Guid message, IActorRef target)
                    {
                        return new RequeueBucket(message: message, target: target);
                    }
                    private RequeueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public record AcknowledgeJob : HiveMessage
                {
                    public static AcknowledgeJob Create(JobConfirmationRecord message, IActorRef target)
                    {
                        return new AcknowledgeJob(message: message, target: target);
                    }
                    private AcknowledgeJob(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public JobConfirmationRecord GetObjectFromMessage { get => Message as JobConfirmationRecord; }
                }

                public record FinishBucket : HiveMessage
                {
                    public static FinishBucket Create(IActorRef target)
                    {
                        return new FinishBucket(target: target);
                    }
                    private FinishBucket(IActorRef target) : base(message: null, target: target)
                    {
                    }
                }

                public record FinishTask : HiveMessage
                {
                    public static FinishTask Create(string msg, IActorRef target)
                    {
                        return new FinishTask(message: msg, target: target);
                    }
                    private FinishTask(string message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public string GetObjectFromMessage => (string)Message;
                }

                public record AskToRequeue : HiveMessage
                {
                    public static AskToRequeue Create(Guid jobKey, IActorRef target)
                    {
                        return new AskToRequeue(message: jobKey, target: target);
                    }
                    private AskToRequeue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public record DoSetup : HiveMessage
                {
                    public static DoSetup Create(Guid jobKey, IActorRef target)
                    {
                        return new DoSetup(message: jobKey, target: target);
                    }
                    private DoSetup(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public record CreateMeasurements : HiveMessage
                {
                    public static CreateMeasurements Create(MeasurementInformationRecord message, IActorRef target)
                    {
                        return new CreateMeasurements(message: message, target: target);
                    }
                    private CreateMeasurements(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public MeasurementInformationRecord GetObjectFromMessage { get => (MeasurementInformationRecord)Message; }
                }


            }
        }

    }
}