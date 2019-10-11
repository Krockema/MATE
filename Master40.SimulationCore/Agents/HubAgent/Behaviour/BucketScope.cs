using System;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using static FBuckets;
using static FBucketScopes;
using static FOperations;
using static FUpdateBucketScopes;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        internal BucketScope(SimulationType simulationType = SimulationType.BucketScope)
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

            var bucket = _bucketManager.FindAndAddBucket(operation, Agent.Context.Self, Agent.CurrentTime);

            if(bucket != null) { 
                    FBucketScope updateBucketScope = new FBucketScope(bucketKey: bucket.Key
                                                                     , start: 0
                                                                     , end: 0
                                                                     , duration: ((IJob)bucket).Duration);
                    //TODO ResourceAgent has to be set
                    Agent.Send(instruction: Resource.Instruction.BucketScope.UpdateBucketScope.Create(updateBucketScope, bucket.ResourceAgent));

            }
            
            //Else Create a new one 
            bucket = _bucketManager.CreateBucket(operation, Agent.Context.Self, Agent.CurrentTime);
            EnqueueBucket(bucket);

        }

        internal void EnqueueBucket(FBucket bucket)
        {
            //TODO Create the Scope and send it to the ressources
            FBucketScope bucketScope = new FBucketScope(bucketKey: bucket.Key
                                                        ,start: bucket.ForwardStart
                                                        ,end: bucket.BackwardStart
                                                        ,duration: ((IJob)bucket).Duration);

            var resourceToRequest = _resourceManager.GetResourceByTool(bucket.Tool);

            foreach (var actorRef in resourceToRequest)
            {
                Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name}");
                Agent.Send(instruction: Resource.Instruction.BucketScope.RequestProposalForBucketScope.Create(message: bucketScope, target: actorRef));
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

            Agent.Send(Resource.Instruction.BucketScope.EnqueueBucket.Create(bucket, bucket.ResourceAgent));
            
            //take all left Operations and search for new Buckets for them
            foreach (var operation in notSatisfiedOperations)
            {
                EnqueueOperation(operation);
            }
        }





    }
}
