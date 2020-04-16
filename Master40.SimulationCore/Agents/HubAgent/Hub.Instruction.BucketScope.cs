using Akka.Actor;
using AkkaSim.Definitions;
using System;

namespace Master40.SimulationCore.Agents.HubAgent
{
    partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {
            /// <summary>
            /// Implements the classes for BucketScope
            /// </summary>
            public class BucketScope
            {
                public class SetBucketFix : SimulationMessage
                {
                    public static SetBucketFix Create(Guid key, IActorRef target)
                    {
                        return new SetBucketFix(message: key, target: target);
                    }

                    private SetBucketFix(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public Guid GetObjectFromMessage
                    {
                        get => (Guid) Message;
                    }
                }

                public class EnqueueOperation : SimulationMessage
                {
                    public static EnqueueOperation Create(FOperations.FOperation operation, IActorRef target)
                    {
                        return new EnqueueOperation(message: operation, target: target);
                    }

                    private EnqueueOperation(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public FOperations.FOperation GetObjectFromMessage
                    {
                        get => Message as FOperations.FOperation;
                    }
                }

                public class EnqueueBucket : SimulationMessage
                {
                    public static EnqueueBucket Create(Guid bucketKey, IActorRef target)
                    {
                        return new EnqueueBucket(message: bucketKey, target: target);
                    }

                    private EnqueueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public Guid GetObjectFromMessage
                    {
                        get => (Guid) Message;
                    }
                }

                public class ResponseRequeueBucket : SimulationMessage
                {
                    public static ResponseRequeueBucket Create(FRequestToRequeues.FRequestToRequeue message,
                        IActorRef target)
                    {
                        return new ResponseRequeueBucket(message: message, target: target);
                    }

                    private ResponseRequeueBucket(object message, IActorRef target) : base(message: message,
                        target: target)
                    {

                    }

                    public FRequestToRequeues.FRequestToRequeue GetObjectFromMessage
                    {
                        get => Message as FRequestToRequeues.FRequestToRequeue;
                    }
                }


                public class FinishBucket : SimulationMessage
                {
                    public static FinishBucket Create(IJobResults.IJobResult jobResult, IActorRef target)
                    {
                        return new FinishBucket(message: jobResult, target: target);
                    }

                    private FinishBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public IJobResults.IJobResult GetObjectFromMessage
                    {
                        get => Message as IJobResults.IJobResult;
                    }
                }
            }

        }
    }
}