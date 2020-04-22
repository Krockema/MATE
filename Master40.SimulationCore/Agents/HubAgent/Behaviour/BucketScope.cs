using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.JobAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Helper.DistributionProvider;
using static FBuckets;
using static FJobResourceConfirmations;
using static FOperations;
using static FProposals;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static FScopeConfirmations;
using static FUpdateStartConditions;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(long maxBucketSize, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.BucketScope)
            : base(simulationType: simulationType)
        {
            _bucketManager = new BucketManager(maxBucketSize: maxBucketSize);
            _workTimeGenerator = workTimeGenerator;
        }
        private List<FOperation> _unassigendOperations { get; } = new List<FOperation>();
        private BucketManager _bucketManager { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                //case Hub.Instruction.BucketScope.EnqueueOperation msg: EnqueueOperation(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetBucketFix msg: SetBucketFix(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.RequestFinalBucket msg: SendFinalBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.DissolveBucket msg: DissolveBucket(msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void SetBucketFix(Guid bucketKey)
        {
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;
            //refuse bucket if not exits anymore
            if (bucket == null) return;
            
            bucket = bucket.SetFixPlanned;
            _bucketManager.SetBucketSatisfied(bucket);

            Agent.Send(Job.Instruction.BucketIsFixed.Create(Agent.Sender));
        }

        private void SendFinalBucket(Guid bucketKey)
        {
            var jobConfirmation = BucketCleanup(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;

            // Remove all Information from Hub
            bucket.Operations.ForEach(operation =>
            {
                operation.Operation.RandomizedDuration =
                    _workTimeGenerator.GetRandomWorkTime(operation.Operation.Duration);
                RemoveJobFromBucketManager(operation.Key);
            });
            RemoveJobFromBucketManager(jobConfirmation.Job.Key);

            Agent.Send(Job.Instruction.FinalBucket.Create(jobConfirmation.ToImmutable(), jobConfirmation.JobAgentRef));
        }

        private void DissolveBucket(Guid bucketKey)
        {
            var bucket = _bucketManager.GetBucketByBucketKey(bucketKey);

            if (bucket == null)
                return;

            var successRemove = _bucketManager.Remove(bucket.Key, Agent);
            
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

            _bucketManager.AddOrUpdateBucketSize(job.RequiredCapability, job.Duration);

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

            var bucket = _bucketManager.AddToBucket(fOperation);
            if (bucket != null) return;//if no bucket to add exists create a new one
            
            var jobConfirmation = _bucketManager.CreateBucket(fOperation: fOperation, Agent);
            _unassigendOperations.Remove(fOperation);
            EnqueueBucket(jobConfirmation.Job.Key);

            //after creating new bucket, modify subsequent buckets
            RequestDissolveBucket(fOperation);
        }
        
        /// <summary>
        /// TODO:
        /// Message to JobAgent with order to Dissolve
        /// JobAgent withdraws, Proposals/Slots
        /// JobAgent sends Operations to Requeue
        /// JobAgent Dissolves
        /// </summary>
        /// <param name="operation"></param>
        private void RequestDissolveBucket(FOperation operation)
        {
            var bucketsToDissolve = _bucketManager.FindAllBucketsLaterForwardStart(operation);

            if (bucketsToDissolve.Count > 0)
            {
                foreach (var bucket in bucketsToDissolve)
                {
                    if (bucket.IsConfirmed)
                    {

                        Agent.Send(Job.Instruction.RequestDissolve.Create(bucket.JobAgentRef));
                        
                    }
                    else
                    {
                        DissolveBucket(bucket.Job.Key);
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
                _proposalManager.Remove(bucketKey);
                return;
            }
            if (jobConfirmation.IsFixPlanned)
            {
                return;
            }


            jobConfirmation.ResetConfirmation();
            _proposalManager.Add(jobConfirmation.Job.Key, _capabilityManager.GetAllCapabilityProvider(jobConfirmation.Job.RequiredCapability));
            

            Agent.DebugMessage($"Enqueue {jobConfirmation.Job.Name} with {((FBucket)jobConfirmation.Job).Operations.Count} operations", CustomLogger.PROPOSAL, LogLevel.Warn);
            
            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(jobConfirmation.Job.RequiredCapability);

            foreach (var capabilityProvider in capabilityDefinition.GetAllCapabilityProvider())
            {
                foreach (var setup in capabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical))
                {
                    var resourceRef = setup.Resource.IResourceRef as IActorRef;
                    Agent.DebugMessage(msg: $"Ask for proposal at resource {resourceRef.Path.Name} with {jobConfirmation.Job.Key}", CustomLogger.PROPOSAL, LogLevel.Warn);
                    Agent.Send(instruction: Resource.Instruction.Default.RequestProposal
                        .Create(new FRequestProposalForCapabilityProvider(jobConfirmation.Job
                                                                                , capabilityProviderId : capabilityProvider.Id)
                                                                                , target: resourceRef));
                }
            }

        }

        internal override void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(fProposal.JobKey);

            if (jobConfirmation == null) return;

            var bucket = jobConfirmation.Job as FBucket;
            var resourceAgent = fProposal.ResourceAgent as IActorRef;
            var required = _proposalManager.AddProposal(fProposal);
            if (required == null) return;
            var schedules = fProposal.PossibleSchedule as List<FQueueingScope>;
            var propSet = _proposalManager.GetProposalForSetupDefinitionSet(fProposal.JobKey);
            Agent.DebugMessage(msg: $"Proposal({propSet.ReceivedProposals}of{propSet.RequiredProposals}) " +
                                    $"for {bucket.Name} {bucket.Key} with Schedule: {schedules.First().Scope.Start} " +
                                    $"JobKey: {fProposal.JobKey} from: {resourceAgent.Path.Name}!", CustomLogger.PROPOSAL, LogLevel.Warn);

            // if all resources replied 
            if (propSet.AllProposalsReceived)
            {
                // item Postponed by All resources ? -> requeue after given amount of time.
                var proposalForCapabilityProvider = propSet.GetValidProposal();
                if (proposalForCapabilityProvider.Count() == 0)
                {
                    var postponedFor = propSet.PostponedUntil; // TODO: Naming Until != For

                    _proposalManager.RemoveAllProposalsFor(bucket.Key);

                    Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(bucket.Key, target: Agent.Context.Self), waitFor: postponedFor);
                    Agent.DebugMessage($"{bucket.Name} {bucket.Key} has been postponed for {postponedFor}", CustomLogger.PROPOSAL, LogLevel.Warn);
                    return;

                }


                List<PossibleProcessingPosition> possibleProcessingPositions = _proposalManager.CreatePossibleProcessingPositions(proposalForCapabilityProvider, bucket);

                var possiblePosition = possibleProcessingPositions.OrderBy(x => x._processingPosition).First();

                jobConfirmation.CapabilityProvider = possiblePosition.ResourceCapabilityProvider;

                var jobResourceConfirmation = new FJobResourceConfirmation(jobConfirmation.ToImmutable(), new Dictionary<IActorRef, FScopeConfirmation>());
                foreach (var setup in jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical))
                {
                    var resourceRef = setup.Resource.IResourceRef as IActorRef;
                    jobResourceConfirmation.ScopeConfirmations.Add(resourceRef,
                        possiblePosition._queuingDictionary.Single(x => x.Key.Equals(setup.Resource.IResourceRef)).Value);

                }
                Agent.Send(Job.Instruction.AcknowledgeJob.Create(jobResourceConfirmation, jobConfirmation.JobAgentRef));
                _proposalManager.Remove(bucket.Key);
            }
        }

        /// <summary>
        /// Source: ResourceAgent
        /// Rename this one to set job fix
        /// muss bucket schon für das Setup fix gesetzt werden? --> Was wenn das Bucket zwischendurch doch noch aufgelöst wird?
        /// </summary>
        /// <param name="bucketKey"></param>
        internal JobConfirmation BucketCleanup(Guid bucketKey)
        {
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;
            //refuse bucket if not exits anymore
            if (bucket != null)
            {
                //Step 1. Clear Bucket from unsatisfied operations
                var notSatisfiedOperations = _bucketManager.GetAllNotSatifsiedOperation(bucket);
                
                if (notSatisfiedOperations.Count > 0)
                {
                    bucket = _bucketManager.RemoveOperations(bucket, notSatisfiedOperations);
                }
                //Setp 2. Requeue all unsatisfied operations
                _unassigendOperations.AddRange(notSatisfiedOperations); 
                RequeueOperations(notSatisfiedOperations);
            }
            else
            {
                throw new Exception("No bucket found. Sad times....");
            }
            return jobConfirmation;
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var operations = _unassigendOperations.Where(x => x.Key == startCondition.OperationKey);

            if (operations.Count() > 0)
            {
                operations.First().SetStartConditions(startCondition);
                return;
            }

            var bucket = _bucketManager.GetBucketByOperationKey(startCondition.OperationKey);
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey: bucket.Key);
            bucket = jobConfirmation.Job as FBucket;

            _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (!jobConfirmation.IsConfirmed || !bucket.Operations.Any(x => x.StartConditions.Satisfied))
                return;

            foreach (var setup in jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical))
            {
                var resourceRef = setup.Resource.IResourceRef as IActorRef;
                Agent.DebugMessage(msg: $"Update and forward start condition: {startCondition.OperationKey} in {bucket.Name}" +
                                        $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                        $"| PreCondition: {startCondition.PreCondition} " +
                                        $"to resource {resourceRef.Path.Name}");

                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: resourceRef));
            }
            
        }
        
        internal void RequeueOperations(List<FOperation> operations)
        {
            foreach (var operation in operations.OrderBy(x => x.ForwardStart).ToList())
            {
                Agent.DebugMessage(msg: $"Requeue operation {operation.Operation.Name} {operation.Key}");
                EnqueueOperation(operation);
            }
        }

        /// <summary>
        /// Job = Bucket
        /// </summary>
        /// <param name="jobResult"></param>
        internal void RemoveJobFromBucketManager(Guid operationKey)
        {
            var operation =_bucketManager.RemoveOperation(operationKey);

            // TODO Dynamic Lot Sizing
            _bucketManager.DecreaseBucketSize(operation.RequiredCapability.Id, operation.Operation.Duration);
            if (Agent.DebugThis)
            {
                var bucket = _bucketManager.GetBucketByOperationKey(operationKey: operation.Key);
                Agent.DebugMessage(msg: $"Operation finished: {operation.Operation.Name} {operationKey} in bucket: {bucket.Name} {bucket.Key}");
            }
        }

        internal void RemoveBucketOnStartBucketProcessing(Guid jobKey)
        {
            _bucketManager.Remove(jobKey, Agent);
        }

    }
}
