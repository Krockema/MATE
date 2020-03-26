using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Environment.Options;
using static FBuckets;
using static FOperations;
using static IJobs;
using static FUpdateStartConditions;
using static FProposals;
using static IJobResults;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(long maxBucketSize, SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType)
        {
            _maxBucketSize = maxBucketSize;
        }

        private static long _maxBucketSize { get; set; }

        private BucketManager _bucketManager { get; } = new BucketManager(maxBucketSize: _maxBucketSize);

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                //case Hub.Instruction.BucketScope.EnqueueOperation msg: EnqueueOperation(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.ResetBucket msg: ResetBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetBucketFix msg: SetBucketFix(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.FinishBucket msg: FinishBucket(msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void ResetBucket(Guid bucketKey)
        {
            var bucket = _bucketManager.GetBucketById(bucketKey);

            var successRemove = _bucketManager.Remove(bucket);

            if (successRemove)
            {
                _operationList.AddRange(bucket.Operations);
                RequeueOperations(bucket.Operations.ToList());
            }
            else
            {
                new Exception($"something went wrong with reset Bucket");
            }

        }

        private void AssignJob(IJob job)
        {
            var operation = (FOperation)job;

            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} {operation.Key} | with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            _operationList.Add(operation);
            
            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation operation)
        {
            var operationInBucket = _bucketManager.GetBucketByOperationKey(operation.Key);

            if (operationInBucket != null)
            {
                Agent.DebugMessage($"{operation.Operation.Name} {operation.Key} is already in bucket");
                return;
            }

            _bucketManager.AddOrUpdateBucketSize(_resourceManager.GetToolCapabilityPair(operation.RequiredCapability),
                operation.Operation.Duration);
            /*
             * Implements the Self-Organizing Bucket Method
             */

            var bucket = _bucketManager.AddToBucket(operation);

            if (bucket != null)
            {
                operation = _operationList.Single(x => x.Key == operation.Key);
                _operationList.Remove(operation);
                //System.Diagnostics.Debug.WriteLine($"{operation.Key} to existing bucket {bucket.Name}");
                Agent.DebugMessage($"Extend Bucket: {operation.Operation.Name} {operation.Key} to {bucket.Name}");
                _bucketManager.Replace(bucket);
                if (!bucket.ResourceAgent.IsNobody())
                {
                    //TODO Maybe update bucket on Resource! But pay attention to possible inconsitency
                }
                
                return;
            }

            //if no bucket to add exists create a new one
            bucket = _bucketManager.CreateBucket(fOperation: operation, Agent.Context.Self, Agent.CurrentTime);
            Agent.DebugMessage($"Create new Bucket {bucket.Name} with scope of {bucket.Scope} from {bucket.ForwardStart} to {bucket.BackwardStart}");
            
            operation = _operationList.Single(x => x.Key == operation.Key);
            _operationList.Remove(operation);
            //System.Diagnostics.Debug.WriteLine($"{operation.Key} to new bucket {bucket.Name}");
            
            EnqueueBucket(bucket);

            //after creating new bucket, modify subsequent buckets
            ModifyBucket(operation);

        }

        private void ModifyBucket(FOperation operation)
        {
            var bucketsToModify = _bucketManager.FindAllBucketsLaterForwardStart(operation);

            if (bucketsToModify.Count > 0)
            {
                Agent.DebugMessage($"{bucketsToModify.Count} buckets to modify");
                foreach (var modBucket in bucketsToModify)
                {
                    Agent.DebugMessage($"Modify Bucket: {operation.Operation.Name} {operation.Key} modifies {modBucket.Name}");
                    if (!modBucket.ResourceAgent.IsNobody())
                    {
                        Agent.Send(
                            Resource.Instruction.BucketScope.RequeueBucket.Create(modBucket.Key, modBucket.ResourceAgent));
                    }
                    else
                    {
                        ResetBucket(modBucket.Key);
                    }

                    //System.Diagnostics.Debug.WriteLine($"{operation.Key} reset {modBucket.Name}");
                }
            }
        }

        internal void EnqueueBucket(FBucket bucket)
        {
            bucket = _bucketManager.GetBucketById(bucket.Key);

            if (bucket == null) return;

            bucket.Proposals.Clear();
            bucket = bucket.UpdateResourceAgent(r: ActorRefs.NoSender);
            _bucketManager.Replace(bucket);

            Agent.DebugMessage($"Enqueue {bucket.Name} with {bucket.Operations.Count} operations");
            
            var resourceToRequest = _resourceManager.GetResourceByTool(bucket.RequiredCapability);

            foreach (var actorRef in resourceToRequest)
            {
                Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name}");
                Agent.Send(instruction: Resource.Instruction.Default.RequestProposal.Create(message: bucket, target: actorRef));
            }

        }

        internal override void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var bucket = _bucketManager.GetBucketById(fProposal.JobKey);
            
            if (bucket == null) return;

            bucket.Proposals.RemoveAll(x => x.ResourceAgent.Equals(fProposal.ResourceAgent));
            // add New Proposal
            bucket.Proposals.Add(item: fProposal);

            Agent.DebugMessage(msg: $"Proposal for {bucket.Name} with Schedule: {fProposal.PossibleSchedule} Id: {fProposal.JobKey} from: {fProposal.ResourceAgent}!");

            // if all Machines Answered
            if (bucket.Proposals.Count == _resourceManager.GetResourceByTool(bucket.RequiredCapability).Count)
            {

                // item Postponed by All Machines ? -> requeue after given amount of time.
                if (bucket.Proposals.TrueForAll(match: x => x.Postponed.IsPostponed))
                {
                    var postPonedFor = bucket.Proposals.Min(x => x.Postponed.Offset);
                    Agent.DebugMessage(msg: $"{bucket.Name} {bucket.Key} postponed to {postPonedFor}");
                    // Call Hub Agent to Requeue
                    bucket = bucket.UpdateResourceAgent(r: ActorRefs.NoSender);
                    _bucketManager.Replace(bucket);
                    Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(bucket: bucket, target: Agent.Context.Self), waitFor: postPonedFor);
                    return;
                }

                // acknowledge Machine -> therefore get Machine -> send acknowledgement
                var earliestPossibleStart = bucket.Proposals.Where(predicate: y => y.Postponed.IsPostponed == false)
                                                               .Min(selector: p => p.PossibleSchedule);

                var acknowledgement = bucket.Proposals.First(predicate: x => x.PossibleSchedule == earliestPossibleStart
                                                                        && x.Postponed.IsPostponed == false);

                bucket = ((IJob)bucket).UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent) as FBucket;

                Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {bucket.Name} {bucket.Key} on resource {acknowledgement.ResourceAgent}");

                // set Proposal Start for Machine to Requeue if time slot is closed.
                _bucketManager.Replace(bucket);
                Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal.Create(message: bucket, target: acknowledgement.ResourceAgent));
            }
        }

        /// <summary>
        /// Source: ResourceAgent 
        /// </summary>
        /// <param name="bucketKey"></param>
        internal void SetBucketFix(Guid bucketKey)
        {
            var bucket = _bucketManager.GetBucketById(bucketKey);
            
            //refuse bucket if not exits anymore
            if (bucket != null)
            {

                var notSatisfiedOperations = _bucketManager.GetAllNotSatifsiedOperation(bucket);
                
                if (notSatisfiedOperations.Count > 0)
                {
                    bucket = _bucketManager.RemoveOperations(bucket, notSatisfiedOperations);
                }


                bucket = bucket.SetFixPlanned;

                _bucketManager.SetBucketSatisfied(bucket);

                Agent.DebugMessage(msg: $"{bucket.Name} with {bucket.Operations.Count} operations has been set fix: {bucket.IsFixPlanned} and has set satisfied: {bucket.StartConditions.Satisfied}");
               
                Agent.DebugMessage(msg: $"{bucket.Name} has {notSatisfiedOperations.Count} operations to requeue");
                
                _operationList.AddRange(notSatisfiedOperations); 
                RequeueOperations(notSatisfiedOperations);
            }
            else
            {
                Agent.DebugMessage(msg: $"{bucket.Name} does not exits anymore");
            }
            //Send fix/refuse bucket
            var jobAcknowledgement = new JobAcknowledgement(bucketKey, bucket);
            Agent.Send(Resource.Instruction.BucketScope.AcknowledgeJob.Create(jobAcknowledgement, bucket.ResourceAgent));
            //Requeue all unsatisfied operations
            
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var operation = _operationList.SingleOrDefault(x => x.Key == startCondition.OperationKey);

            if (operation != null)
            {
                operation.SetStartConditions(startCondition);
                return;
            }

            var bucket = _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (bucket.ResourceAgent.IsNobody())
                return;

            if (bucket.Operations.Any(x => x.StartConditions.Satisfied))
            {
                Agent.DebugMessage(msg: $"Update and forward start condition: {startCondition.OperationKey} in {bucket.Name}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} " +
                                    $"to resource {bucket.ResourceAgent}");

                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: bucket.ResourceAgent));
            }
        }

        internal override void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _bucketManager.GetOperationByKey(operationKey);
            Agent.DebugMessage($"WithdrawRequiredArticles for operation {operation.Operation.Name} {operationKey} on {Agent.Context.Self.Path.Name}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
            .Create(message: operation.Key
                , target: operation.ProductionAgent));

        }

        internal void RequeueOperations(List<FOperation> operations)
        {
            foreach (var operation in operations.OrderBy(x => x.ForwardStart).ToList())
            {
                Agent.DebugMessage(msg: $"Requeue operation {operation.Operation.Name} {operation.Key}");
                EnqueueOperation(operation);
                _bucketManager.DecreaseBucketSize(_resourceManager.GetToolCapabilityPair(operation.RequiredCapability),
                    operation.Operation.Duration);
            }

        }

        /// <summary>
        /// Job = Bucket
        /// </summary>
        /// <param name="jobResult"></param>
        internal override void FinishJob(IJobResult jobResult)
        {
            var operation = _bucketManager.GetOperationByKey(jobResult.Key);
            var bucket = _bucketManager.GetBucketByOperationKey(operationKey: operation.Key);
            _bucketManager.RemoveOperation(operation.Key);

            _bucketManager.DecreaseBucketSize(_resourceManager.GetToolCapabilityPair(operation.RequiredCapability),
                operation.Operation.Duration);

            Agent.DebugMessage(msg: $"Operation finished: {operation.Operation.Name} {jobResult.Key} in bucket: {bucket.Name} {bucket.Key}");

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: operation.ProductionAgent));
            
        }

        internal void FinishBucket(IJobResult jobResult)
        {
            var bucket = _bucketManager.GetBucketById(jobResult.Key);

            _bucketManager.Remove(bucket);
        }

    }
}
