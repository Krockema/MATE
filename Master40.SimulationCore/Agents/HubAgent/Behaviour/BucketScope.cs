using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FOperationResults;
using static FOperations;
using static FProposals;
using static FRequestProposalForSetups;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;

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

        private List<FOperation> _unassigendOperations { get; } = new List<FOperation>();
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
            var bucket = _bucketManager.GetBucketByBucketKey(bucketKey);

            var successRemove = _bucketManager.Remove(bucket.Key);
            
            if (successRemove)
            {
                _proposalManager.Remove(bucket.Key);
                _unassigendOperations.AddRange(bucket.Operations);
                RequeueOperations(bucket.Operations.ToList());
            }
            //TODO multiple reset from one setupdefinition?
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

            _unassigendOperations.Add(operation);
            
            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation fOperation)
        {
            var operationInBucket = _bucketManager.GetBucketByOperationKey(fOperation.Key);

            if (operationInBucket != null)
            {
                Agent.DebugMessage($"{fOperation.Operation.Name} {fOperation.Key} is already in bucket");
                return;
            }

            // TODO Dynamic Lot Sizing
            _bucketManager.AddOrUpdateBucketSize(fOperation.RequiredCapability, fOperation.Operation.Duration);
            /*
             * Implements the Self-Organizing Bucket Method
             */

            var bucket = _bucketManager.AddToBucket(fOperation);

            if (bucket != null)//if no bucket to add exists create a new one
            {
                return;
            }

            var jobConfirmation = _bucketManager.CreateBucket(fOperation: fOperation, Agent.Context.Self, Agent.CurrentTime);

            //Agent.DebugMessage($"Create new Bucket {jobConfirmation.Job.Name} with scope of {((FBucket)jobConfirmation.Job).Scope} from {jobConfirmation.Job.ForwardStart} to {bucket.BackwardStart}");
            
            _unassigendOperations.Remove(fOperation);
            
            EnqueueBucket(jobConfirmation.Job.Key);

            //after creating new bucket, modify subsequent buckets
            ModifyBucket(fOperation);

        }

        private void ModifyBucket(FOperation operation)
        {
            var bucketsToModify = _bucketManager.FindAllBucketsLaterForwardStart(operation);

            if (bucketsToModify.Count > 0)
            {
                foreach (var modBucket in bucketsToModify)
                {
                    if (modBucket.IsConfirmed)
                    {
                        //Send to first resource
                        Agent.Send(
                            Resource.Instruction.BucketScope.RequeueBucket
                                .Create(modBucket.Job.Key, modBucket.SetupDefinition.RequiredResources.First()));
                    }
                    else
                    {
                        ResetBucket(modBucket.Job.Key);
                    }

                }
            }
        }

        internal void EnqueueBucket(Guid bucketKey)
        {
            //TODO maybe ID

            //delete all proposals if exits
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);

            if (jobConfirmation == null)
            {
                //if bucket already deleted in BucketManager, also delete bucket in proposalmanager
                _proposalManager.Remove(jobConfirmation.Job.Key);
                return;
            }

            _proposalManager.Add(jobConfirmation.Job.Key, _capabilityManager.GetAllSetupDefinitions(jobConfirmation.Job.RequiredCapability, Agent));
            jobConfirmation.ResetConfirmation();

            Agent.DebugMessage($"Enqueue {jobConfirmation.Job.Name} with {((FBucket)jobConfirmation.Job).Operations.Count} operations");
            
            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(jobConfirmation.Job.RequiredCapability);

            foreach (var setupDefinition in capabilityDefinition.GetAllSetupDefinitions)
            {
                foreach (var resource in setupDefinition.RequiredResources)
                {
                    Agent.DebugMessage(msg: $"Ask for proposal at resource {resource.Path.Name} with {jobConfirmation.Job.Key}");
                    Agent.Send(instruction: Resource.Instruction.Default.RequestProposal
                        .Create(new FRequestProposalForSetup(jobConfirmation.Job
                                                                  , setupDefinition.SetupKey)
                              , target: resource));
                }
            }

        }

        internal override void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(fProposal.JobKey);

            if (jobConfirmation == null) return;

            var bucket = jobConfirmation.Job as FBucket;
            var required = _proposalManager.AddProposal(fProposal);
            if (required == null) return;
            var propSet = _proposalManager.GetProposalForSetupDefinitionSet(fProposal.JobKey);
            Agent.DebugMessage(msg: $"Proposal({propSet.ReceivedProposals}of{propSet.RequiredProposals}) for {bucket.Name} {bucket.Key} with Schedule: {fProposal.PossibleSchedule} " +
                                    $"JobKey: {fProposal.JobKey} from: {fProposal.ResourceAgent.Path.Name}!");

            // if all resources replied 
            if (propSet.AllProposalsReceived)
            {
                // item Postponed by All resources ? -> requeue after given amount of time.
                var proposalForSetupDefinition = propSet.GetValidProposal();
                if (proposalForSetupDefinition == null)
                {
                    var postponedFor = propSet.PostponedUntil; // TODO: Naming Until != For

                    _proposalManager.RemoveAllProposalsFor(bucket.Key);

                    Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(bucket.Key, target: Agent.Context.Self), waitFor: postponedFor);
                    return;

                }

                jobConfirmation.Schedule = proposalForSetupDefinition.EarliestStart();
                jobConfirmation.SetupDefinition = proposalForSetupDefinition.GetFSetupDefinition;

                foreach (IActorRef resource in proposalForSetupDefinition.GetFSetupDefinition.RequiredResources)
                {
                    Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {bucket.Name} {bucket.Key} on resource {resource}");
                    Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal
                        .Create(jobConfirmation.ToImutable()
                            , target: resource));
                }

                _proposalManager.Remove(bucket.Key);

            }
        }

        /// <summary>
        /// Source: ResourceAgent 
        /// </summary>
        /// <param name="bucketKey"></param>
        internal void SetBucketFix(Guid bucketKey)
        {
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;
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
                _unassigendOperations.AddRange(notSatisfiedOperations); 
                RequeueOperations(notSatisfiedOperations);
            }
            else
            {
                Agent.DebugMessage(msg: $"{bucket.Name} does not exits anymore");
                jobConfirmation.Schedule = -1;
            }

            //TODO Send only to one resource and let the resource handle all the other resources? or send to all? --> For now send to first (like requeue bucket)
            Agent.Send(Resource.Instruction.BucketScope.AcknowledgeJob.Create(jobConfirmation.ToImutable(), jobConfirmation.SetupDefinition.RequiredResources.First()));
            //Requeue all unsatisfied operations
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var operation = _unassigendOperations.SingleOrDefault(x => x.Key == startCondition.OperationKey);

            if (operation != null)
            {
                operation.SetStartConditions(startCondition);
                return;
            }

            var bucket = _bucketManager.GetBucketByOperationKey(startCondition.OperationKey);
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey: bucket.Key);

            _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (!jobConfirmation.IsConfirmed || !bucket.Operations.Any(x => x.StartConditions.Satisfied))
                return;

            foreach (var resource in jobConfirmation.SetupDefinition.RequiredResources)
            {
                Agent.DebugMessage(msg: $"Update and forward start condition: {startCondition.OperationKey} in {bucket.Name}" +
                                        $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                        $"| PreCondition: {startCondition.PreCondition} " +
                                        $"to resource {resource.Path.Name}");

                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: resource));
            }
            
        }

        internal override void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _bucketManager.GetOperationByOperationKey(operationKey);
            if (operation == null)
                throw new Exception("operation was not found in bucketManager ");
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
                // TODO Dynamic Lot Sizing
                _bucketManager.DecreaseBucketSize(operation.RequiredCapability,
                    operation.Operation.Duration);
            }

        }

        /// <summary>
        /// Job = Bucket
        /// </summary>
        /// <param name="jobResult"></param>
        internal override void FinishJob(IJobResult jobResult)
        {
            
            var operation =_bucketManager.RemoveOperation(jobResult.Key);

            // TODO Dynamic Lot Sizing
            _bucketManager.DecreaseBucketSize(operation.RequiredCapability,
                operation.Operation.Duration);
            if (Agent.DebugThis)
            {
                var bucket = _bucketManager.GetBucketByOperationKey(operationKey: operation.Key);
                Agent.DebugMessage(msg: $"Operation finished: {operation.Operation.Name} {jobResult.Key} in bucket: {bucket.Name} {bucket.Key}");
            }
            
            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: ((FOperationResult)jobResult).ProductionAgent));
            
        }

        internal void FinishBucket(IJobResult jobResult)
        {
            _bucketManager.Remove(jobResult.Key);
        }

    }
}
