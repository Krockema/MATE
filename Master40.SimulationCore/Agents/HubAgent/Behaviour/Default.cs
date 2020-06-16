using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.JobAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.Tools.ExtensionMethods;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobResourceConfirmations;
using static FOperations;
using static FProposals;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static FResourceInformations;
using static FScopeConfirmations;
using static FUpdateStartConditions;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(long maxBucketSize, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Default) : base(childMaker: null, simulationType: simulationType)
        {
            _bucketManager = new BucketManager(maxBucketSize: maxBucketSize);
            _workTimeGenerator = workTimeGenerator;
        }
        internal OperationManager _operations { get; set; } = new OperationManager();
        internal CapabilityManager _capabilityManager { get; set; } = new CapabilityManager();
        internal ProposalManager _proposalManager { get; set; } = new ProposalManager();
        private BucketManager _bucketManager { get; }
        private WorkTimeGenerator _workTimeGenerator { get; }
        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                //Initialize
                case Hub.Instruction.Default.AddResourceToHub msg: AddResourceToHub(resourceInformation: msg.GetObjectFromMessage); break;

                //Jobs
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.DissolveBucket msg: DissolveBucket(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;

                //Manage Work for Resource
                case Hub.Instruction.Default.ProposalFromResource msg: ProposalFromResource(fProposal: msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetBucketFix msg: SetBucketFix(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.RequestFinalBucket msg: SendFinalBucket(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                default:
                     throw new Exception("No message exists");
                    break;
            }
            return success;
        }

        internal void AddResourceToHub(FResourceInformation resourceInformation)
        {
            foreach (var capabilityProvider in resourceInformation.ResourceCapabilityProvider)
            {
                var capabilityDefinition = _capabilityManager.GetCapabilityDefinition(capabilityProvider.ResourceCapability);

                capabilityDefinition.AddResourceRef(resourceId: resourceInformation.ResourceId, resourceRef: resourceInformation.Ref);

                System.Diagnostics.Debug.WriteLine($"Create capability provider at {Agent.Name}" +
                                                   $" with capability provider {capabilityProvider.Name} " +
                                                   $" from {Agent.Context.Sender.Path.Name}" +
                                                   $" with capability {capabilityDefinition.ResourceCapability.Name}", CustomLogger.INITIALIZE, LogLevel.Warn);

            }
            // Added Machine Agent To Machine Pool
            Agent.DebugMessage(msg: "Added Resource Agent " + resourceInformation.Ref.Path.Name + " to Resource Pool: " + resourceInformation.RequiredFor, CustomLogger.INITIALIZE, LogLevel.Warn);
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

                Agent.Send(Job.Instruction.TerminateJob.Create(Agent.Sender));
            }
            //TODO multiple reset from one setupdefinition?
            else
            {
                new Exception($"Something went wrong while reset bucket with {bucketKey}");
            }

        }

        private void AssignJob(IJob job)
        {
            var operation = (FOperation)job;

            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} {operation.Key}" + 
                                    $"| with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}" +
                                    $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {operation.StartConditions.PreCondition}", CustomLogger.JOB, LogLevel.Warn);

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            _bucketManager.AddOrUpdateBucketSize(job.RequiredCapability, job.Duration);

            EnqueueOperation(operation);

        }

        internal void EnqueueOperation(FOperation fOperation)
        {

            Agent.DebugMessage(msg: $"Original FOperation before add to Bucket : {fOperation.Operation.Name} {fOperation.Key}" +
                                    $"| with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}" +
                                    $"| ArticleProvided: {fOperation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {fOperation.StartConditions.PreCondition}", CustomLogger.JOB, LogLevel.Warn);

            var operationInBucket = _bucketManager.GetBucketByOperationKey(fOperation.Key);
            
            if (operationInBucket != null)
            {
                Agent.DebugMessage($"{fOperation.Operation.Name} {fOperation.Key} is already in bucket", CustomLogger.ENQUEUE, LogLevel.Warn);
                return;
            }

            
            var jobConfirmation = _bucketManager.AddToBucket(fOperation, Agent.CurrentTime);
            if (jobConfirmation != null)
            {
                var bucket = ((FBucket) jobConfirmation.Job);
                var operationInsideBucket = bucket.Operations.Single(x => x.Key.Equals(fOperation.Key));
                
                Agent.DebugMessage(msg: $"FOperation {operationInsideBucket.Operation.Name} inside Bucket {bucket.Name} {bucket.Key}" +
                                        $"| with start condition: {operationInsideBucket.StartConditions.Satisfied} with Id: {operationInsideBucket.Key}" +
                                        $"| ArticleProvided: {operationInsideBucket.StartConditions.ArticlesProvided} " +
                                        $"| PreCondition: {operationInsideBucket.StartConditions.PreCondition}", CustomLogger.JOB, LogLevel.Warn);

                Agent.DebugMessage($"Operation {fOperation.Operation.Name} {fOperation.Key} was added to {bucket.Name} {bucket.Key} Send to {jobConfirmation.JobAgentRef.Path.Name}", CustomLogger.ENQUEUE, LogLevel.Warn);

                Agent.Send(instruction: BasicInstruction.UpdateJob.Create(message: bucket, target: jobConfirmation.JobAgentRef));

                return;//if no bucket to add exists create a new one

            }
            
            jobConfirmation = _bucketManager.CreateBucket(fOperation: fOperation, Agent);
            EnqueueBucket(jobConfirmation.Job.Key);
             
            Agent.DebugMessage($"New {jobConfirmation.Job.Name} has been created for Operation {fOperation.Operation.Name} {fOperation.Key}", CustomLogger.ENQUEUE, LogLevel.Warn);

            //after creating new bucket, modify subsequent buckets
            RequestDissolveBucket(fOperation);
        }
        
        internal void RequestDissolveBucket(FOperation operation)
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
                Agent.DebugMessage($"{((FBucket)jobConfirmation.Job).Name} has no corresponding jobConfirmation.", CustomLogger.PROPOSAL, LogLevel.Warn);
                _proposalManager.Remove(bucketKey);
                return;
            }
            if (jobConfirmation.IsFixPlanned)
            {
                Agent.DebugMessage($"{((FBucket)jobConfirmation.Job).Name} is Fix Planned can not Enqueue it Again.", CustomLogger.PROPOSAL, LogLevel.Warn);
                return;
            }

            Agent.DebugMessage($"{((FBucket)jobConfirmation.Job).Name} start new proposal request", CustomLogger.PROPOSAL, LogLevel.Warn);

            jobConfirmation.ResetConfirmation();
            _proposalManager.Add(jobConfirmation.Job.Key, _capabilityManager.GetAllCapabilityProvider(jobConfirmation.Job.RequiredCapability));
            
            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(jobConfirmation.Job.RequiredCapability);
            var distinctResources = new HashSet<IActorRef>();

            foreach (var capabilityProvider in capabilityDefinition.GetAllCapabilityProvider())
            {
                foreach (var setup in capabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical))
                {
                    distinctResources.Add(setup.Resource.IResourceRef as IActorRef);
                }
            }

            distinctResources.ForEach(resourceRef =>
            {
                Agent.DebugMessage(msg: $"Ask for proposal at resource {resourceRef.Path.Name} with {jobConfirmation.Job.Name}", CustomLogger.PROPOSAL, LogLevel.Warn);
                Agent.Send(instruction: Resource.Instruction.Default.RequestProposal
                    .Create(new FRequestProposalForCapability(jobConfirmation.Job
                            , capabilityId: jobConfirmation.Job.RequiredCapability.Id)
                            , target: resourceRef));

            });
        }

        internal void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(fProposal.JobKey);

            if (jobConfirmation == null) return;

            var bucket = jobConfirmation.Job as FBucket;
            var resourceAgent = fProposal.ResourceAgent as IActorRef;
            var required = _proposalManager.AddProposal(fProposal, Agent.Sender);
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
                
                if (possibleProcessingPositions.Count == 0)
                {
                    _proposalManager.RemoveAllProposalsFor(bucket.Key);
                    // var postponedFor = (long) (bucket.MaxBucketSize * 0.8);
                    var postponedFor = (long) (bucket.MaxBucketSize * 0.5);
                    Agent.Send(instruction: Hub.Instruction.BucketScope.EnqueueBucket.Create(bucket.Key, target: Agent.Context.Self), waitFor: postponedFor);
                    return;
                }


                var possiblePosition = possibleProcessingPositions.OrderBy(x => x._processingPosition).First();

                jobConfirmation.CapabilityProvider = possiblePosition.ResourceCapabilityProvider;

                var jobResourceConfirmation = new FJobResourceConfirmation(jobConfirmation.ToImmutable(), new Dictionary<IActorRef, FScopeConfirmation>());
                var setups = jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical).ToList();
                foreach (var setup in setups)
                {
                    var resourceRef = setup.Resource.IResourceRef as IActorRef;
                    var (actorRef, pos) = possiblePosition._queuingDictionary.FirstOrDefault(x => x.Key.Equals(setup.Resource.IResourceRef));
                    if (pos == null)
                        continue;
                    jobResourceConfirmation.ScopeConfirmations.Add(resourceRef, pos);

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

                Agent.DebugMessage($"Clearing finished for {jobConfirmation.Job.Name} with {afterCount} operations left and {removedCount} operations requeued, started with {beforeCount}", CustomLogger.JOB, LogLevel.Warn);
            }
            else
            {
                throw new Exception("No bucket found. Sad times....");
            }

            return jobConfirmation;
        }

        internal void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            Agent.DebugMessage(msg: $"Received: Update and forward start condition for {startCondition.OperationKey}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} ");
            
            var bucket = _bucketManager.SetOperationStartCondition(startCondition.OperationKey, startCondition);
            if (bucket.IsNull())
            {
                Agent.DebugMessage(msg: $"No Bucket found and should be at Work");
                return;
            }

            var jobConfirmation = _bucketManager.GetConfirmationByBucketKey(bucket.Key);
            
            Agent.DebugMessage(msg: $"Found Bucket Update and forward start condition: {startCondition.OperationKey} in {bucket.Name} with key  {bucket.Key}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} " +
                                    $"to job agent {jobConfirmation.JobAgentRef}");

            Agent.Send(instruction: BasicInstruction.UpdateJob.Create(message: bucket, target: jobConfirmation.JobAgentRef));
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
        internal virtual void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _operations.GetJobBy(operationKey) as FOperation;

            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
                .Create(message: operation.Key
                    , target: operation.ProductionAgent));
        }

        #region Job to Resource

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

            Agent.Send(BasicInstruction.FinalBucket.Create(jobConfirmation.ToImmutable(), jobConfirmation.JobAgentRef));

        }
#endregion
    }
}
