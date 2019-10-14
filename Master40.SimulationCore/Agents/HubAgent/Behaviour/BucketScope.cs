using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using static FBuckets;
using static FBucketScopes;
using static FOperations;
using static IJobs;
using static FStartConditions;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType) { }

        private BucketManager _bucketManager { get; } = new BucketManager();

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueOperation msg: EnqueueOperation(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetBucketFix msg: SetBucketFix(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }
        private void AssignJob(IJob job)
        {
            var operation = (FOperation)job;

            System.Diagnostics.Debug.WriteLine($"Enqueue Operation {operation.Operation.Name} {operation.Key} ");
            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} | with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation operation)
        {
            System.Diagnostics.Debug.WriteLine($"Modify Buckets");

            var operationsToModify = _bucketManager.ModifyBucket(operation);

            if (operationsToModify != null)
            {
                operationsToModify.Add(operation);
                RequeueOperations(operationsToModify);
                return;
            }

            //if no bucket has to be modified try to add
            System.Diagnostics.Debug.WriteLine($"Add To Buckets");
            var bucket = _bucketManager.AddToBucket(operation, Agent.Context.Self, Agent.CurrentTime);

            if (bucket != null)
            {

                System.Diagnostics.Debug.WriteLine($"Add {operation.Operation.Name} to {bucket.Name}");
                if (bucket.ResourceAgent != null)
                {
                    FBucketScope updateBucketScope = new FBucketScope(bucketKey: bucket.Key
                                                                     , start: 0
                                                                     , end: 0
                                                                     , duration: ((IJob)bucket).Duration);

                    Agent.Send(instruction: Resource.Instruction.BucketScope.UpdateBucketScope.Create(updateBucketScope, bucket.ResourceAgent));

                }
                return;
            }

            //if no bucket to add exists create a new one
            bucket = _bucketManager.CreateBucket(fOperation: operation, Agent.Context.Self, Agent.CurrentTime);
            System.Diagnostics.Debug.WriteLine($"Create {bucket.Name}");
            EnqueueBucket(bucket);

        }

        internal void EnqueueBucket(FBucket bucket)
        {
            System.Diagnostics.Debug.WriteLine($"Enqueue {bucket.Name}");
            var resourceToRequest = _resourceManager.GetResourceByTool(bucket.Tool);

            foreach (var actorRef in resourceToRequest)
            {
                Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name}");
                Agent.Send(instruction: Resource.Instruction.BucketScope.RequestProposalForBucketScope.Create(message: bucket, target: actorRef));
            }

        }

        internal void RequeueOperations(List<FOperation> operations)
        {
            foreach (var operation in operations.OrderBy(x => x.ForwardStart).ToList())
            {
                EnqueueOperation(operation);
            }

        }

        /// <summary>
        /// Source: ResourceAgent 
        /// </summary>
        /// <param name="bucketKey"></param>
        internal void SetBucketFix(Guid bucketKey)
        {
            var bucket = _bucketManager.SetBucketFix(bucketKey);
            var notSatisfiedOperations = _bucketManager.RemoveAllNotSatisfiedOperations(bucket);
            bucket = _bucketManager.GetBucketById(bucketKey);

            //Send fix bucket
            Agent.Send(Resource.Instruction.BucketScope.EnqueueBucket.Create(bucket, bucket.ResourceAgent));

            //Requeue all unsatisfied operations
            RequeueOperations(notSatisfiedOperations);
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var bucket = _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (bucket.ResourceAgent.IsNobody())
                return;
            
            Agent.DebugMessage(msg: $"Update and forward start condition: {startCondition.OperationKey} in {bucket.Name}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} " +
                                    $"to resource {bucket.ResourceAgent}");

            Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: bucket.ResourceAgent));

        }
    }
}
