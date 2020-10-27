using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.JobAgent.Types;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FJobConfirmations;
using static FJobResourceConfirmations;
using static FMeasurementInformations;
using static FOperationResults;
using static FOperations;
using static FProcessingSlots;
using static FSetupSlots;
using static FUpdateSimulationJobs;
using static IConfirmations;
using static IJobs;
using LogLevel = NLog.LogLevel;
using Resource = Master40.SimulationCore.Agents.ResourceAgent.Resource;

namespace Master40.SimulationCore.Agents.JobAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(IConfirmation jobConfirmation, SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {
            _jobConfirmation = jobConfirmation;
            _resourceDistinctResourceStates = new Dictionary<IActorRef, StateHandle>();
            _dissolveRequested = false;
        }

        private IConfirmation _jobConfirmation { get; set; }
        private Dictionary<IActorRef, StateHandle> _resourceProcessingStates =>_resourceDistinctResourceStates.Where(x => x.Value.Processing).ToDictionary(x => x.Key, x => x.Value);
        private Dictionary<IActorRef, StateHandle> _resourceSetupStates => _resourceDistinctResourceStates.Where(x => x.Value.Setup).ToDictionary(x => x.Key, x => x.Value);
        private Dictionary<IActorRef, StateHandle> _resourceDistinctResourceStates { get; } // Dissolve und Requeue // 

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
                case BasicInstruction.FinalBucket msg: FinalizedBucket(msg.GetObjectFromMessage); break;
                case Job.Instruction.AcknowledgeRevoke msg: AcknowledgeRevoke(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateJob msg: UpdateJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.TerminateJob msg: Terminate(); break;
                case Job.Instruction.ResourceWillBeReady msg: ResourceWillBeReady(); break;
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

        private void ResourceWillBeReady()
        {
            if(_resourceDistinctResourceStates.Any(x => x.Value.CurrentState <= JobState.RevokeStarted)) return;

            _resourceDistinctResourceStates.TryGetValue(Agent.Sender, out var resourceStateHandle);
            resourceStateHandle.CurrentState = JobState.WillBeReady;

            TryToStart();

            Agent.DebugMessage($"ResourceWillBeReady: Change _resourceDistinctResourceStates for {Agent.Sender.Path.Name} to {resourceStateHandle.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);
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

        private void StartRequeue()
        {
            _resourceDistinctResourceStates.TryGetValue(Agent.Sender, out var state);
            state.CurrentState = JobState.Revoked;
            Agent.DebugMessage($"StartRequeue: Change _resourceDistinctResourceStates for {Agent.Sender.Path.Name} to {state.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);
            if (_resourceDistinctResourceStates.Count == 1 
                || _resourceDistinctResourceStates.All(x => x.Value.CurrentState.Equals(JobState.Revoked)))
            {
                Agent.DebugMessage($"Start Requeue for {_jobConfirmation.Job.Name} has been initiated and send to Hub", CustomLogger.JOB, LogLevel.Warn);
                RequestRequeueFromHub();
                return;
            }
            StartRevoke();
        }
        private void StartDissolve()
        {
            var allSetupReady = _resourceSetupStates.Values.All(x => x.CurrentState >= JobState.SetupReady);
            var allProcessingReady = _resourceProcessingStates.Values.All(x => x.CurrentState >= JobState.WillBeReady);

            if (allSetupReady && allProcessingReady)
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
            var allSetupReady = _resourceSetupStates.Values.All(x => x.CurrentState >= JobState.SetupReady);
            var allProcessingReady = _resourceProcessingStates.Values.All(x => x.CurrentState >= JobState.WillBeReady);

            if (allSetupReady && allProcessingReady)
            {
                Agent.DebugMessage($"RequestRevoke interrupted for {_jobConfirmation.Job.Name} setups ready: {allSetupReady} setup count: {_resourceSetupStates.Count}" +
                                   $" processing ready {allProcessingReady} processing count: {_resourceProcessingStates.Count}", CustomLogger.JOB, LogLevel.Warn);
                return;
            }

            var states = "";
            _resourceDistinctResourceStates.ForEach(x => states += x.Key.Path.Name + " " +  x.Value.CurrentState.ToString() + ";");
            Agent.DebugMessage($"RequestRevoke: successful for {_jobConfirmation.Job.Name} [{states}]", CustomLogger.JOB, LogLevel.Warn);

            StartRevoke();
        }

        private void StartRevoke()
        {
            Agent.DebugMessage($"Start Revoke for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} has been initiated from {Agent.Sender.Path.Name}", CustomLogger.JOB, LogLevel.Warn);
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
                Agent.Send(Resource.Instruction.Default.DoSetup.Create(_jobConfirmation.Key, x.Key));
                x.Value.CurrentState = JobState.SetupInProcess;
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

        private void AcknowledgeJob(FJobResourceConfirmation fJobResourceConfirmation)
        {
            _jobConfirmation = fJobResourceConfirmation.JobConfirmation;
            
            //Clear all previous states
            _resourceDistinctResourceStates.Clear();
            Agent.DebugMessage($"AcknowledgeJob: Clear _resourceSetupStates _resourceProcessingStates _resourceDistinctResourceStates", CustomLogger.JOBSTATE, LogLevel.Warn);
            Agent.DebugMessage($"Acknowledge Proposal {_jobConfirmation.Job.Name} received", CustomLogger.PROPOSAL, LogLevel.Warn);
            foreach (var resourceScope in fJobResourceConfirmation.ScopeConfirmations)
            {
                var resourceRef = resourceScope.Key;
                var _jobConfirmationForResource = ((FJobConfirmation)_jobConfirmation).UpdateScopeConfirmation(resourceScope.Value);
                
                Agent.Send(instruction: Resource.Instruction.Default.AcceptedProposals.Create(_jobConfirmationForResource, target: resourceRef));

                if (resourceScope.Value.Scopes.Length > 1)
                {
                    _resourceDistinctResourceStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue, true, true));
                }
                else
                {
                    switch (resourceScope.Value.Scopes.First())
                    {
                        case FSetupSlot fs:_resourceDistinctResourceStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue, true, false)); break;
                        case FProcessingSlot ps: _resourceDistinctResourceStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue, false, true)); break;
                        default: throw new Exception("Wrong state implementation send.");
                    }
                }
            }
        }

        private void RequestSetupStart(IActorRef sender)
        {
            if (_resourceSetupStates.Values.All(x => x.CurrentState <= JobState.RevokeStarted))
            {
                Agent.DebugMessage($"RequestSetupStart canceled", CustomLogger.JOBSTATE, LogLevel.Warn);
                return;
            }

            var requestingResource = _resourceSetupStates.Single(x => x.Key.Equals(sender));
            requestingResource.Value.CurrentState = JobState.SetupReady;
            Agent.DebugMessage($"RequestSetupStart: Change _resourceSetupStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState}", CustomLogger.JOBSTATE, LogLevel.Warn);

            TryToStart();
        }

        private void TryToStart()
        {
            Agent.DebugMessage(
                $"Try to Start {_jobConfirmation.Job.Name}", CustomLogger.JOB, LogLevel.Warn);

            if (_resourceDistinctResourceStates.Values.All(x => x.CurrentState >= JobState.WillBeReady)
                && _resourceSetupStates.Values.All(x => x.CurrentState.Equals(JobState.SetupReady)))
                
            {
                Agent.DebugMessage(
                    $"All request for setup received for {_jobConfirmation.Job.Name}. Start fix Bucket sent to Hub!",
                    CustomLogger.JOB, LogLevel.Warn);

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
            requestingResource.Value.CurrentState = JobState.SetupFinished;

            Agent.DebugMessage($"FinishSetup: Change _resourceSetupStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);


            if (_resourceSetupStates.Values.All(x => x.CurrentState == JobState.SetupFinished))
            {
                Agent.DebugMessage($"All setup resources received for {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);

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
            if (_resourceProcessingStates.Values.All(x => x.CurrentState <= JobState.RevokeStarted))
            {
                Agent.DebugMessage($"RequestProcessingStart cancled", CustomLogger.JOBSTATE, LogLevel.Warn);
                return;
            }

            //TODO trigger with JobState ?! --> parameter JobState entscheidet darüber, für welche Phasen angefragt werden soll? -- Konsistenz? 
            var (key, value) = _resourceProcessingStates.SingleOrDefault(x => x.Key.Equals(sender));
            if(value == null) return;
            value.CurrentState = JobState.Ready;
            Agent.DebugMessage($"RequestProcessingStart: Change _resourceProcessingStates for {key.Path.Name} to {value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);
            RequestFinalBucketFromHub();
        }

        private void FinishProcessing(IActorRef sender)
        {
            Agent.DebugMessage($"ONE of {_resourceProcessingStates.Count() } Finisih processing received for {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);
            var requestingResource = _resourceProcessingStates.Single(x => x.Key.Equals(sender));
            requestingResource.Value.CurrentState = JobState.Ready;
            Agent.DebugMessage($"FinishProcessing: Change _resourceProcessingStates for {requestingResource.Key.Path.Name} to {requestingResource.Value.CurrentState.ToString()}", CustomLogger.JOBSTATE, LogLevel.Warn);

            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.DebugMessage($"All finish processing received for {_currentOperation.Operation.Name} {_currentOperation.Key} of {_jobConfirmation.Job.Name}.", CustomLogger.JOB, LogLevel.Warn);
                CreateOperationResults();
                DoWork();
            }


        }


        private void RequestFinalBucketFromHub()
        {
            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.DebugMessage($"All request for processing received for {_jobConfirmation.Job.Name}. Start finalize Bucket sent to Hub!", CustomLogger.JOB, LogLevel.Warn);

                Agent.Send(Hub.Instruction.BucketScope.RequestFinalBucket.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
            }
        }

        private void FinalizedBucket(IConfirmation jobConfirmation)
        {
            Agent.DebugMessage($"Finalized {_jobConfirmation.Job.Name} received.", CustomLogger.JOB, LogLevel.Warn);
            _jobConfirmation = jobConfirmation;

            foreach (var resourceRef in _resourceProcessingStates.Keys)
            {
                Agent.Send(instruction: BasicInstruction.FinalBucket.Create(jobConfirmation, target: resourceRef));
            }
            _finalOperations = new Queue<FOperation>(((FBucket)_jobConfirmation.Job).Operations.OrderByDescending(prio => prio.Priority.Invoke(Agent.CurrentTime)));
            _processingStart = Agent.CurrentTime;
            foreach (var resource in _resourceProcessingStates)
            {
                Agent.DebugMessage($"{resource.Key.Path.Name} in state {resource.Value.CurrentState.ToString()}");
            }
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
                    Agent.Send(Resource.Instruction.Default.FinishBucket.Create(x.Key));
                    x.Value.CurrentState = JobState.Finish;

                });
                Terminate();
                return;
            }

            _currentOperation = _finalOperations.Dequeue();

            UpdateOperationKpi();

            // Create Measurement and set it to MeasurementAgent
            CreateMeasurement();

            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: _currentOperation.Key, target: _currentOperation.ProductionAgent));

            Agent.DebugMessage(msg: $"Starting Job {_currentOperation.Operation.Name}  Key: {_currentOperation.Key} new duration is {_currentOperation.Operation.RandomizedDuration} " +
                                    $"from bucket {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} with {((FBucket)_jobConfirmation.Job).Operations.Count} operations " +
                                    $"at resource {Agent.Context.Self.Path.Name}", CustomLogger.JOB, LogLevel.Warn);

            _resourceProcessingStates.ForEach(x =>
            {
                Agent.Send(Resource.Instruction.Default.DoWork.Create( message: _currentOperation
                                                                                , target: x.Key));
                x.Value.CurrentState = JobState.InProcess;

            });
        }

        public override bool AfterInit()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Name} is created!", CustomLogger.JOB, LogLevel.Warn);
            return base.AfterInit();
        }


        #region Reporting
        private void CreateSetupKpi()
        {
            var setupDuration = _jobConfirmation.Job.RequiredCapability
                .ResourceCapabilityProvider.Sum(s =>
                    s.ResourceSetups.Sum(d =>
                        d.SetupTime));

            ResultStreamFactory.PublishResourceSetup(Agent, setupDuration, _jobConfirmation.CapabilityProvider);
        }

        private void UpdateOperationKpi()
        {
            ResultStreamFactory.PublishJob(Agent, _currentOperation, _currentOperation.Operation.RandomizedDuration, _jobConfirmation.CapabilityProvider, _jobConfirmation.Job.Bucket);
        }


        private void CreateBucketKpi()
        {
            ResultStreamFactory.PublishBucketResult(Agent, _jobConfirmation, _processingStart);
        }

        private void CreateOperationResults()
        {
            ResultStreamFactory.PublishJob(agent: Agent
                , job: _currentOperation
                , duration: _currentOperation.Operation.RandomizedDuration
                , capabilityProvider: _jobConfirmation.CapabilityProvider
                , bucketName: _jobConfirmation.Job.Bucket);

            var fOperationResult = new FOperationResult(key: _currentOperation.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + _currentOperation.Operation.RandomizedDuration
                , originalDuration: _currentOperation.Operation.Duration
                , productionAgent: _currentOperation.ProductionAgent
                , capabilityProvider: _jobConfirmation.CapabilityProvider.Name);

            Agent.Send(BasicInstruction.FinishJob.Create(fOperationResult, _currentOperation.ProductionAgent));
        }
        internal void CreateMeasurement()
        {
            var msg = Resource.Instruction.Default.
                CreateMeasurements.Create(message: new FMeasurementInformation(
                        job: _currentOperation
                        , resource: _jobConfirmation.CapabilityProvider.Name
                        , quantile: _jobConfirmation.CapabilityProvider.ResourceSetups.First().Resource.Quantile
                        , tool: _currentOperation.Operation.ResourceCapability.Name // TODO: Create a posibility to get tool from CapabilityProvider
                        , capabilityProviderId: _jobConfirmation.CapabilityProvider.Id
                        , _currentOperation.Bucket),
                    target: Agent.ActorPaths.MeasurementAgent.Ref);
            Agent.Send(msg);
        }
        #endregion

    }
}
