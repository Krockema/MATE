﻿using Akka.Actor;
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
                case Hub.Instruction.Default.AddResourceToHub msg: AddResourceToHub(resourceInformation: msg.GetObjectFromMessage); break;
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
            _bucketManager.Replace(bucket);
            
            Agent.DebugMessage($"{((FBucket)jobConfirmation.Job).Name} has been set fix, but can still add more operations.", CustomLogger.JOB, LogLevel.Warn);

            Agent.Send(Job.Instruction.BucketIsFixed.Create(jobConfirmation.JobAgentRef));
        }

        private void SendFinalBucket(Guid bucketKey)
        {
            var jobConfirmation = BucketCleanup(bucketKey);
            var bucket = jobConfirmation.Job as FBucket;

            bucket.Operations.ForEach(operation =>
            {
                operation.Operation.RandomizedDuration =
                    _workTimeGenerator.GetRandomWorkTime(operation.Operation.Duration);

                Agent.DebugMessage($"{operation.Operation.Name} {operation.Key} from {((FBucket)jobConfirmation.Job).Name} has new work time of {operation.Operation.RandomizedDuration}.", CustomLogger.JOB, LogLevel.Warn);

                //RemoveJobFromBucketManager(operation.Key);
            });

            jobConfirmation.Job = bucket;
            Agent.DebugMessage($"Send finalized {jobConfirmation.Job.Name} with {((FBucket)jobConfirmation.Job).Operations.Count()} operations to Job Agent.", CustomLogger.JOB, LogLevel.Warn);

            Agent.Send(Job.Instruction.FinalBucket.Create(jobConfirmation.ToImmutable(), jobConfirmation.JobAgentRef));

        }

        private void DissolveBucket(Guid bucketKey)
        {
            var bucket = _bucketManager.GetBucketByBucketKey(bucketKey);

            if (bucket == null)
                return;

            var successRemove = _bucketManager.Remove(bucket.Key);
            
            if (successRemove)
            {
                Agent.DebugMessage(msg: $"Dissolve {bucket.Name} succeeded",CustomLogger.JOB, LogLevel.Warn);
                
                _proposalManager.Remove(bucket.Key);
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

            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} {operation.Key}" + 
                                    $"| with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}" +
                                    $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {operation.StartConditions.PreCondition}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            _bucketManager.AddOrUpdateBucketSize(job.RequiredCapability, job.Duration);

            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation fOperation)
        {

            Agent.DebugMessage(msg: $"Original FOperation before add to Bucket : {fOperation.Operation.Name} {fOperation.Key}" +
                                    $"| with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}" +
                                    $"| ArticleProvided: {fOperation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {fOperation.StartConditions.PreCondition}");

            var operationInBucket = _bucketManager.GetBucketByOperationKey(fOperation.Key);
            
            if (operationInBucket != null)
            {
                Agent.DebugMessage($"{fOperation.Operation.Name} {fOperation.Key} is already in bucket", CustomLogger.ENQUEUE, LogLevel.Warn);
                return;
            }

            
            var bucket = _bucketManager.AddToBucket(fOperation);
            if (bucket != null)
            {
                var operationInsideBucket = bucket.Operations.Single(x => x.Key.Equals(fOperation.Key));
                
                Agent.DebugMessage(msg: $"FOperation inside Bucket {operationInsideBucket.Operation.Name} {operationInsideBucket.Key}" +
                                        $"| with start condition: {operationInsideBucket.StartConditions.Satisfied} with Id: {operationInsideBucket.Key}" +
                                        $"| ArticleProvided: {operationInsideBucket.StartConditions.ArticlesProvided} " +
                                        $"| PreCondition: {operationInsideBucket.StartConditions.PreCondition}");

                Agent.DebugMessage($"Operation {fOperation.Operation.Name} {fOperation.Key} was added to {bucket.Name}", CustomLogger.ENQUEUE, LogLevel.Warn);

                return;//if no bucket to add exists create a new one

            }
            
            var jobConfirmation = _bucketManager.CreateBucket(fOperation: fOperation, Agent);
            EnqueueBucket(jobConfirmation.Job.Key);
             
            Agent.DebugMessage($"New {jobConfirmation.Job.Name} has been created for Operation {fOperation.Operation.Name} {fOperation.Key}", CustomLogger.ENQUEUE, LogLevel.Warn);

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
                    if (bucket.IsRequestedToDissolve)
                        continue;

                    bucket.IsRequestedToDissolve = true;
                    Agent.DebugMessage(msg: $"Asking Job Agent to start dissolve {bucket.Job.Key}");
                    Agent.Send(Job.Instruction.RequestDissolve.Create(bucket.JobAgentRef));
                }
            }
        }

        internal void EnqueueBucket(Guid bucketKey)
        {
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

            Agent.DebugMessage($"{((FBucket)jobConfirmation.Job).Name} start new proporsal request", CustomLogger.PROPOSAL, LogLevel.Warn);

            jobConfirmation.ResetConfirmation();
            _proposalManager.Add(jobConfirmation.Job.Key, _capabilityManager.GetAllCapabilityProvider(jobConfirmation.Job.RequiredCapability));
            

            
            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(jobConfirmation.Job.RequiredCapability);

            foreach (var capabilityProvider in capabilityDefinition.GetAllCapabilityProvider())
            {
                foreach (var setup in capabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical))
                {
                    var resourceRef = setup.Resource.IResourceRef as IActorRef;
                    Agent.DebugMessage(msg: $"Ask for proposal at resource {resourceRef.Path.Name} with {jobConfirmation.Job.Name}", CustomLogger.PROPOSAL, LogLevel.Warn);
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
            Agent.DebugMessage(msg: $"Proposal({propSet.ReceivedProposals} of {propSet.RequiredProposals}) " +
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
                var setups = jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical).ToList();
                foreach (var setup in setups)
                {
                    var resourceRef = setup.Resource.IResourceRef as IActorRef;
                    jobResourceConfirmation.ScopeConfirmations.Add(resourceRef,
                        possiblePosition._queuingDictionary.Single(x => x.Key.Equals(setup.Resource.IResourceRef)).Value);

                }
                Agent.Send(Job.Instruction.AcknowledgeJob.Create(jobResourceConfirmation, jobConfirmation.JobAgentRef));
                Agent.DebugMessage($"Send Acknwoledge Job for {jobResourceConfirmation.JobConfirmation.Job.Name}"
                                   , CustomLogger.PROPOSAL, LogLevel.Warn);
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
            var jobConfirmation = _bucketManager.GetAndRemoveJob(bucketKey);
            
            if (jobConfirmation != null)
            {
                Agent.DebugMessage($"Start finalize {jobConfirmation.Job.Name} with {((FBucket)jobConfirmation.Job).Operations.Count} operations left from {((FBucket)jobConfirmation.Job).Name}", CustomLogger.JOB, LogLevel.Warn);

                var beforeCount = ((FBucket)jobConfirmation.Job).Operations.Count;
                var removedCount = 0;

                //Step 1. Clear Bucket from unsatisfied operations
                Agent.DebugMessage($"Start clearing {jobConfirmation.Job.Name}", CustomLogger.JOB, LogLevel.Warn);

                var (bucket, notSatisfiedOperations) = _bucketManager.RemoveNotSatisfiedOperations(jobConfirmation.Job as FBucket, Agent);
                jobConfirmation.Job = bucket;

                if (notSatisfiedOperations.Count > 0)
                {
                    removedCount = notSatisfiedOperations.Count;
                    Agent.DebugMessage($"Clearing {removedCount} operations from {jobConfirmation.Job.Name}", CustomLogger.JOB, LogLevel.Warn);

                    //Setp 2. Requeue all unsatisfied operations
                    RequeueOperations(notSatisfiedOperations);
                }

                var afterCount = ((FBucket)jobConfirmation.Job).Operations.Count;

                Agent.DebugMessage($"Clearing finished for {jobConfirmation.Job.Name} with {afterCount} operations left and {removedCount} operations requeued, startet with {beforeCount}", CustomLogger.JOB, LogLevel.Warn);
            }
            else
            {
                throw new Exception("No bucket found. Sad times....");
            }

            return jobConfirmation;
        }

        internal override void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            Agent.DebugMessage(msg: $"Received: Update and forward start condition for {startCondition.OperationKey}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} ");

            var bucket = _bucketManager.GetBucketByOperationKey(startCondition.OperationKey);
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucketKey: bucket.Key);

            _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);

            if (jobConfirmation.IsRequestedToDissolve) return;

            Agent.DebugMessage(msg: $"Found Bucket Update and forward start condition: {startCondition.OperationKey} in {bucket.Name} with key  {bucket.Key}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} " +
                                    $"to job agent {jobConfirmation.JobAgentRef}");

            Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: jobConfirmation.JobAgentRef));
        }
        
        internal void RequeueOperations(List<FOperation> operations)
        {
            var i = 0;
            var remember = operations.Count;
            Agent.DebugMessage(msg: $"Start Requeue operation of {operations.Count} operations", CustomLogger.ENQUEUE, LogLevel.Warn);
            foreach (var operation in operations.OrderBy(x => x.ForwardStart).ToList())
            {
                Agent.DebugMessage(msg: $"Now Requeue operation {operation.Operation.Name} {operation.Key}", CustomLogger.ENQUEUE, LogLevel.Warn);
                EnqueueOperation(operation);
                i++;
            }

            Agent.DebugMessage($"Requeue {i} of {remember} operations finished", CustomLogger.ENQUEUE, LogLevel.Warn);
        }
    }
}
