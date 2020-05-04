﻿using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.JobAgent.Types;
using Master40.SimulationCore.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static FBucketResults;
using static FBuckets;
using static FCreateSimulationResourceSetups;
using static FJobConfirmations;
using static FJobResourceConfirmations;
using static FOperationResults;
using static FOperations;
using static FProcessingSlots;
using static FSetupSlots;
using static FUpdateSimulationJobs;
using static FUpdateStartConditions;
using static IConfirmations;
using static IJobs;
using Resource = Master40.SimulationCore.Agents.ResourceAgent.Resource;

namespace Master40.SimulationCore.Agents.JobAgent.Behaviour
{

    public class Default : SimulationCore.Types.Behaviour
    {
        // Tasks of Job Agent
        // Handle Re-queuing
        // Handle Job State - Initial - Waiting - Processing - Finish
        // Sync Resources - Wait for all to send Ready state.
        // Job: Start Operation, Withdraw Material, Bucket
        // Job: Finish Operation, Add Material, Finish Bucket
        // Job: Send / Create Measurements
        // Job: - Dispose

        internal Default(FJobConfirmation jobConfirmation, SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {
            _jobConfirmation = jobConfirmation;
            _resourceProcessingStates = new Dictionary<IActorRef, StateHandle>();
            _resourceSetupStates = new Dictionary<IActorRef, StateHandle>();
            _resourceDistinctResourceStates = new Dictionary<IActorRef, StateHandle>();
            _dissolveRequested = false;
        }

        private IConfirmation _jobConfirmation { get; set; }
        private Guid GetJobKey() => _jobConfirmation.Job.Key;
        private Dictionary<IActorRef, StateHandle> _resourceProcessingStates { get; set; } // processing
        private Dictionary<IActorRef, StateHandle> _resourceSetupStates { get; set; } // setup
        private Dictionary<IActorRef, StateHandle> _resourceDistinctResourceStates { get; set; } // Dissolve und Requeue // 

        private FOperation _currentOperation { get; set;}
        private long _processingStart { get; set; }
        private bool _dissolveRequested { get; set; }
        private bool _bucketIsReadyToBeTerminated { get; set; } = false;
        private Queue<FOperation> _finalOperations { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Job.Instruction.RequestDissolve msg: StartDissolve(); break;
                case Job.Instruction.AcknowledgeJob msg: AcknowledgeJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.RequestSetupStart msg: RequestSetupStart(msg.GetObjectFromMessage); break;
                case Job.Instruction.RequestProcessingStart msg: RequestProcessingStart(msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishSetup msg: FinishSetup(msg.GetObjectFromMessage); break;
                case Job.Instruction.FinishProcessing msg: FinishProcessing(msg.GetObjectFromMessage); break;
                case Job.Instruction.BucketIsFixed msg: BucketIsFixed(); break;
                case Job.Instruction.FinalBucket msg: FinalizedBucket(msg.GetObjectFromMessage); break;
                case Job.Instruction.AcknowledgeRevoke msg: AcknowledgeRevoke(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateJob msg: UpdateJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.TerminateJob msg: Terminate(); break;
                // case Job.Instruction.StartProcessing msg: StartProcessing(); break;
                case Job.Instruction.StartRequeue msg: StartRequeue(); break;
                case Job.Instruction.DelayedStartNotification msg: RequestRevoke(); break;
                default: return false;
            }
            return true;
        }

        private void UpdateJob(IJob job)
        {
            if (_bucketIsReadyToBeTerminated)
            {
                Agent.DebugMessage($" with {job.Key}  is ready to be terminated and will not be updated.");
                return;
            }
                

            _jobConfirmation = _jobConfirmation.UpdateJob(job);
            foreach (var resourceRef in _resourceDistinctResourceStates.Keys)
            { 
                Agent.Send(instruction: BasicInstruction.UpdateJob.Create(message: job, target: resourceRef));
            }
        }

        private void AcknowledgeRevoke(IActorRef sender)
        {
            _resourceDistinctResourceStates.TryGetValue(sender, out var resourceStateHandle);
            resourceStateHandle.CurrentState = JobState.Revoked;
            Agent.DebugMessage($"AcknowledgeRevoke: Change _resourceDistinctResourceStates for {sender.Path.Name} to {resourceStateHandle.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);

            RequestRequeueFromHub();
        }

        private void RequestRequeueFromHub()
        {
            if (_resourceDistinctResourceStates.All(x => x.Value.CurrentState.Equals(JobState.Revoked)))
            {
                if (_dissolveRequested)
                {
                    Agent.DebugMessage($"Acknowledge Dissolve {_jobConfirmation.Job.Name} and send to Hub", CustomLogger.JOB, LogLevel.Warn);
                    Agent.Send(Hub.Instruction.BucketScope.DissolveBucket.Create(_jobConfirmation.Key, _jobConfirmation.Job.HubAgent));
                    _bucketIsReadyToBeTerminated = true;
                    return;
                }

                Agent.DebugMessage($"Acknowledge Requeue {_jobConfirmation.Job.Name} and send to Hub", CustomLogger.JOB, LogLevel.Warn);
                Agent.Send(Hub.Instruction.BucketScope.EnqueueBucket.Create(_jobConfirmation.Key, _jobConfirmation.Job.HubAgent));

            }
        }

        /// <summary>
        /// FROM RESOURCE
        /// </summary>
        private void StartRequeue()
        {
            _resourceDistinctResourceStates.TryGetValue(Agent.Sender, out var state);
            state.CurrentState = JobState.Revoked;
            Agent.DebugMessage($"StartRequeue: Change _resourceDistinctResourceStates for {Agent.Sender.Path.Name} to {state.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);
            if (_resourceDistinctResourceStates.Count == 1 
                || _resourceDistinctResourceStates.All(x => x.Value.CurrentState.Equals(JobState.Revoked)))
            {
                Agent.DebugMessage($"Start Requeue for {_jobConfirmation.Job.Name} has been initiated", CustomLogger.JOB, LogLevel.Warn);
                RequestRequeueFromHub();
                return;

            }
            StartRevoke();
        }

        /// <summary>
        /// FROM HUB
        /// </summary>
        private void StartDissolve()
        {
            var allSetupReady = _resourceSetupStates.Values.All(x => x.CurrentState == JobState.Ready || x.CurrentState == JobState.Finish);
            var allProcessingReady = _resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready);
            if (allSetupReady || allProcessingReady)
            {
                Agent.DebugMessage($"StartDissolve interrupted setups ready: {allSetupReady} setup count: {_resourceSetupStates.Count}" +
                                   $" processing ready {allProcessingReady} processing count: {_resourceProcessingStates.Count}", CustomLogger.JOBSTATE, LogLevel.Warn);
                return;
            }
            _dissolveRequested = true;
            Agent.DebugMessage($"StartDissolve: successful", CustomLogger.JOBSTATE, LogLevel.Warn);

            StartRevoke();
        }

        private void RequestRevoke()
        {
            var allSetupReady = _resourceSetupStates.Values.All(x => x.CurrentState == JobState.Ready || x.CurrentState == JobState.Finish);
            var allProcessingReady = _resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready);
            if (allSetupReady || allProcessingReady)
            {
                Agent.DebugMessage($"RequestRevoke interrupted for {_jobConfirmation.Job.Name} setups ready: {allSetupReady} setup count: {_resourceSetupStates.Count}" +
                                   $" processing ready {allProcessingReady} processing count: {_resourceProcessingStates.Count}", CustomLogger.JOB, LogLevel.Warn);
                return;
            }
            Agent.DebugMessage($"RequestRevoke: successful for {_jobConfirmation.Job.Name}", CustomLogger.JOB, LogLevel.Warn);

            StartRevoke();
        }

        private void StartRevoke()
        {
            Agent.DebugMessage($"Start Revoke for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} has been initiated", CustomLogger.JOB, LogLevel.Warn);
            foreach (var resource in _resourceDistinctResourceStates)
            {
                if (resource.Value.CurrentState == JobState.Revoked || resource.Value.CurrentState == JobState.RevokeStarted)
                    continue;

                resource.Value.CurrentState = JobState.RevokeStarted;
                Agent.Send(Resource.Instruction.Default.RevokeJob
                        .Create(_jobConfirmation.Key, target: resource.Key));
                Agent.DebugMessage($"StartRevoke from : Change _resourceDistinctResourceStates for {resource.Key.Path.Name} to {resource.Value.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);
            }
        }

        private void BucketIsFixed()
        {
            Agent.DebugMessage($"{_jobConfirmation.Job.Name} has been set fix", CustomLogger.JOB, LogLevel.Warn);
            _resourceSetupStates.ForEach(x =>
            {
                Agent.Send(Resource.Instruction.BucketScope.DoSetup.Create(_jobConfirmation.Key, x.Key));
                x.Value.CurrentState = JobState.InProcess;
                Agent.DebugMessage($"BucketIsFixed: Change _resourceSetupStates for {x.Key.Path.Name} to {x.Value.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);

            });
            CreateSetupKpi();
        }

        /// <summary>
        /// Call Terminate if bucket was dissolved or all operations have been processed
        /// </summary>
        private void Terminate()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Name} is terminated!", CustomLogger.JOB, LogLevel.Warn);
            this.Agent.TryToFinish();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fJobResourceConfirmation"></param>
        private void AcknowledgeJob(FJobResourceConfirmation fJobResourceConfirmation)
        {
            _jobConfirmation = fJobResourceConfirmation.JobConfirmation;
            
            //Clear all previous states
            _resourceSetupStates.Clear();
            _resourceProcessingStates.Clear();
            _resourceDistinctResourceStates.Clear();
            Agent.DebugMessage($"AcknowledgeJob: Clear _resourceSetupStates _resourceProcessingStates _resourceDistinctResourceStates", CustomLogger.JOBSTATE, LogLevel.Warn);
            Agent.DebugMessage($"Acknowledge Proposal {_jobConfirmation.Job.Name} received", CustomLogger.PROPOSAL, LogLevel.Warn);
            var counter = 0;
            foreach (var resourceScope in fJobResourceConfirmation.ScopeConfirmations)
            {
                var resourceRef = resourceScope.Key;
                counter++;
                var _jobConfirmationForResource = ((FJobConfirmation)_jobConfirmation).UpdateScopeConfirmation(resourceScope.Value);

                Agent.DebugMessage(msg: $"Start round {counter} AcknowledgeProposal for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} on resource {resourceRef.Path.Name}" +
                                        $" with scope confirmation for {_jobConfirmationForResource.ScopeConfirmation.GetScopeStart()} to {_jobConfirmationForResource.ScopeConfirmation.GetScopeEnd()}"
                    , CustomLogger.PROPOSAL, LogLevel.Warn);

                Agent.Send(instruction: Resource.Instruction.Default.AcceptedProposals.Create(_jobConfirmationForResource, target: resourceRef));
                var scopeCounter=0;

                foreach (var scope in resourceScope.Value.Scopes)
                {
                    scopeCounter++;
                    switch (scope)
                    {
                        case FSetupSlot fs: 
                            _resourceSetupStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue));
                            Agent.DebugMessage($"AcknowledgeJob: round {scopeCounter}  Change _resourceSetupStates for {resourceScope.Key.Path.Name}" +
                                               $" ThreadId: { Thread.CurrentThread.ManagedThreadId }  TaskId: { Task.CurrentId }" +
                                               $"to {_resourceSetupStates.Single(x => x.Key.Equals(resourceScope.Key)).Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);
                            break;
                        case FProcessingSlot ps: _resourceProcessingStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue));
                            Agent.DebugMessage($"AcknowledgeJob: round {scopeCounter} Change _resourceProcessingStates for {resourceScope.Key.Path.Name} " +
                                               $"ThreadId: { Thread.CurrentThread.ManagedThreadId }  TaskId: { Task.CurrentId }" +
                                               $"to {_resourceProcessingStates.Single(x => x.Key.Equals(resourceScope.Key)).Value.CurrentState.ToString()}"
                                , CustomLogger.JOBSTATE, LogLevel.Warn); break;
                            default: throw new Exception("Wrong state implementation send.");
                    }
                }
            }

            var resourcesDistinct = _resourceProcessingStates.Select(x => x.Key)
                                                                               .Union(_resourceSetupStates.Select(s => s.Key)).Distinct();

            foreach (var resource in resourcesDistinct)
            {
                _resourceDistinctResourceStates.Add(resource, new StateHandle(JobState.Created));
                Agent.DebugMessage($"AcknowledgeJob: Change _resourceDistinctResourceStates for {resource.Path.Name} to {_resourceDistinctResourceStates.Single(x => x.Key.Equals(resource)).Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);
            }
        }

        /// <summary>
        /// Resource send message when resource want to start setup for a job - only setup resources are required for this step
        /// </summary>
        private void RequestSetupStart(IActorRef sender)
        {
            // FOr Setup or for Processing ? 

            var requestingResource = _resourceSetupStates.Single(x => x.Key.Equals(sender));
            requestingResource.Value.CurrentState = JobState.Ready;
            Agent.DebugMessage($"RequestSetupStart: Change _resourceSetupStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);

            if (_resourceSetupStates.Values.All(x => x.CurrentState == JobState.Ready)
                && !_resourceDistinctResourceStates.Values.Any(x => x.CurrentState == JobState.RevokeStarted || x.CurrentState == JobState.Revoked)) 
            // indicates that all Items are in Processing Slot on each resource and therefore no message for requeue from resource can occour
            {
                Agent.DebugMessage($"All request for setup received for {_jobConfirmation.Job.Name}. Start fix Bucket sent to Hub!", CustomLogger.JOB, LogLevel.Warn);

                RequestToFixBucketOnHub();
            }
        }

        private void RequestToFixBucketOnHub()
        {
            Agent.Send(Hub.Instruction.BucketScope.SetBucketFix.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
        }

        /// <summary>
        /// Called from every setup resource, when setup was finished
        /// </summary>
        private void FinishSetup(IActorRef sender)
        {
            var requestingResource = _resourceSetupStates.Single(x => x.Key.Equals(sender));
            requestingResource.Value.CurrentState = JobState.Finish;

            Agent.DebugMessage($"FinishSetup: Change _resourceSetupStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);


            if (_resourceSetupStates.Values.All(x => x.CurrentState == JobState.Finish))
            {
                Agent.DebugMessage($"All setup resources finished setup for {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);

                foreach (var resource in _resourceSetupStates.Keys)
                {
                    RequestProcessingStart(resource);

                    // send finish setup to all setup resources that are not required for processing
                    if (!_resourceProcessingStates.ContainsKey(resource))
                    {
                        Agent.Send(BasicInstruction.FinishSetup.Create(Agent.Context.Self, resource));
                    }
                }
            }

        }

        /// <summary>
        /// Resource sent message as soon as the resource want to start with processing - only processing actors are required for this step
        /// </summary>
        private void RequestProcessingStart(IActorRef sender)
        {
            // trigger with JobState ?! --> parameter JobState entscheidet darüber, für welche Phasen angefragt werden soll? -- Konsistenz? 
            var (key, value) = _resourceProcessingStates.SingleOrDefault(x => x.Key.Equals(sender));
            if(value == null) return;
            value.CurrentState = JobState.Ready;
            Agent.DebugMessage($"RequestProcessingStart: Change _resourceProcessingStates for {key.Path.Name} to {value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);


            RequestFinalBucketFromHub();
        }

        private void FinishProcessing(IActorRef sender)
        {
            Agent.DebugMessage($"ONE of {_resourceProcessingStates.Count() } Finisih processing received for {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);
            var requestingResource = _resourceProcessingStates.Single(x => x.Key == sender);
            requestingResource.Value.CurrentState = JobState.Ready;
            Agent.DebugMessage($"FinishProcessing: Change _resourceProcessingStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);

            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.DebugMessage($"All finisih processing received for {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);
                CreateOperationResults();
                UpdateJobKpi();
                DoWork();
            }


        }

        private void CreateOperationResults()
        {
            var fOperationResult = new FOperationResult(key: _currentOperation.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + _currentOperation.Operation.RandomizedDuration
                , originalDuration: _currentOperation.Operation.Duration
                , productionAgent: _currentOperation.ProductionAgent
                , capabilityProvider: _jobConfirmation.CapabilityProvider.Name);

            Agent.Send(BasicInstruction.FinishJob.Create(fOperationResult, _currentOperation.ProductionAgent));
        }



        private void RequestFinalBucketFromHub()
        {
            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.DebugMessage($"All request for processing received for {_jobConfirmation.Job.Name}. Start finalize Bucket sent to Hub!", CustomLogger.JOB, LogLevel.Warn);

                Agent.Send(Hub.Instruction.BucketScope.RequestFinalBucket.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
            }
        }

        private void FinalizedBucket(FJobConfirmation jobConfirmation)
        {
            Agent.DebugMessage($"Finalized {_jobConfirmation.Job.Name} received.", CustomLogger.JOB, LogLevel.Warn);
            _jobConfirmation = jobConfirmation;
            _finalOperations = new Queue<FOperation>(((FBucket)_jobConfirmation.Job).Operations.OrderByDescending(prio => prio.DueTime));
            _processingStart = Agent.CurrentTime;
            DoWork();

        }

        /// <summary>
        /// For Work
        /// </summary>
        /// <param name="jobConfirmation"></param>
        private void DoWork()
        {
            if (_finalOperations.Count == 0)
            {
                CreateBucketKpi();
                _resourceProcessingStates.ForEach(x =>
                {
                    Agent.Send(Resource.Instruction.BucketScope.FinishBucket.Create(x.Key));
                    x.Value.CurrentState = JobState.Finish;
                    Agent.DebugMessage($"DoWork: Change _resourceProcessingStates for {x.Key.Path.Name} to {x.Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);

                });
                Terminate();
                return;
            }

            _currentOperation = _finalOperations.Dequeue();

            Agent.DebugMessage($"Start working with {_currentOperation.Operation.Name} {_currentOperation.Key} in {_jobConfirmation.Job.Name} with {_finalOperations.Count} operations left", CustomLogger.JOB, LogLevel.Warn);

            Agent.DebugMessage(msg: $"Start withdraw for article {_currentOperation.Operation.Name} {_currentOperation.Key}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: _currentOperation.Key, target: _currentOperation.ProductionAgent));

            Agent.DebugMessage(msg: $"Starting Job {_currentOperation.Operation.Name}  Key: {_currentOperation.Key} new Duration is {_currentOperation.Operation.RandomizedDuration} " +
                                    $"from bucket {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} with {((FBucket)_jobConfirmation.Job).Operations.Count} operations " +
                                    $"at resource {Agent.Context.Self.Path.Name}");

            _resourceProcessingStates.ForEach(x =>
            {
                Agent.Send(Resource.Instruction.Default.DoWork.Create( message: _currentOperation
                                                                                , target: x.Key));
                x.Value.CurrentState = JobState.InProcess;
                Agent.DebugMessage($"DoWork: Change _resourceProcessingStates for {x.Key.Path.Name} to {x.Value.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);

            });
        }

        public override bool AfterInit()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Name} is created!", CustomLogger.JOB, LogLevel.Warn);
            return base.AfterInit();
        }

        private void CreateSetupKpi()
        {
            var setupDuration = _jobConfirmation.Job.RequiredCapability
                .ResourceCapabilityProvider.Sum(s =>
                    s.ResourceSetups.Sum(d =>
                        d.SetupTime));
            var pubSetup = new FCreateSimulationResourceSetup(
                                    expectedDuration: setupDuration,
                                    duration: setupDuration,
                                    start: Agent.CurrentTime,
                                    capabilityProvider: _jobConfirmation.CapabilityProvider.Name,
                                    capabilityName: _jobConfirmation.Job.RequiredCapability.Name,
                                    setupId: _jobConfirmation.Job.RequiredCapability.Id);

            //TODO NO tracking
            //Agent.Context.System.EventStream.Publish(@event: pubSetup);
        }


        private void UpdateJobKpi()
        {

            var pub = new FUpdateSimulationJob(job: _currentOperation
                , jobType: JobType.OPERATION
                , duration: _currentOperation.Operation.RandomizedDuration
                , start: Agent.CurrentTime
                , resource: _jobConfirmation.CapabilityProvider.Name
                , bucket: _jobConfirmation.Job.Name
                , setupId: _jobConfirmation.CapabilityProvider.Id);
            //TODO NO tracking
            //Agent.Context.System.EventStream.Publish(@event: pub);

        }


        private void CreateBucketKpi()
        {
            var pub = new FBucketResult(key: _jobConfirmation.Job.Key
                , creationTime: 0
                , start: _processingStart
                , end: Agent.CurrentTime
                , originalDuration: _jobConfirmation.Job.Duration
                , productionAgent: ActorRefs.Nobody
                , capabilityProvider: _jobConfirmation.CapabilityProvider.Name);

            //TODO NO tracking
            //Agent.Context.System.EventStream.Publish(@event: pub);
        }


    }
}
