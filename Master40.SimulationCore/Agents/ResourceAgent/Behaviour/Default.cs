using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.JobAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Reporting;
using Master40.Tools.ExtensionMethods;
using Master40.Tools.Messages;
using Newtonsoft.Json;
using static FBuckets;
using static FOperations;
using static FPostponeds;
using static FRequestProposalForCapabilityProviders;
using static FResourceInformations;
using static IConfirmations;
using LogLevel = NLog.LogLevel;
using static IJobs;
using Directory = Master40.SimulationCore.Agents.DirectoryAgent.Directory;
using static FCreateTaskItems;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _capabilityProviderManager = new CapabilityProviderManager(capabilityProvider);
            _agentDictionary = new AgentDictionary();
            // SCOPELIMIT something like 960
            _scopeQueue = new TimeConstraintQueue(limit: 480);
        }

        internal JobInProgress _jobInProgress { get; set; } = new JobInProgress();
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal CapabilityProviderManager _capabilityProviderManager { get; }
        internal AgentDictionary _agentDictionary { get; }

        private readonly IJobQueue _scopeQueue;
        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Resource.Instruction.Default.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;

                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.AcceptedProposals msg: AcceptProposals(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateJob msg: UpdateStartCondition(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.RevokeJob msg: RevokeJob(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.DoWork msg: DoWork(msg.GetObjectFromMessage); break;
                case BasicInstruction.FinalBucket msg: FinalizedBucket(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.FinishBucket msg: FinishBucket(); break;
                case Resource.Instruction.Default.FinishTask msg: FinishTask(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.TryToWork msg: UpdateProcessingItem(); break;
                case Resource.Instruction.Default.DoSetup msg: DoSetup(); break;
                case BasicInstruction.FinishSetup msg: FinishSetup(); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        public override bool AfterInit()
        {
            var resourceAgent = Agent as Resource;
            var capabilityProviders = _capabilityProviderManager.GetAllCapabilityProvider();
            Agent.Send(instruction: Directory.Instruction.ForwardRegistrationToHub.Create(
                new FResourceInformation(resourceAgent._resource.Id, capabilityProviders, String.Empty, Agent.Context.Self)
                , target: Agent.VirtualParent));
            return true;
        }

        public override bool PostAdvance()
        {
            //TODO only debugging reasons
            
            if (Agent.CurrentTime % 50 == 0 || Agent.CurrentTime == 2)
            { 
               // GanttStatistics.CreateGanttChartForRessource(_jobInProgress, _scopeQueue, Agent);
            }
            
            //TODO _JobInProgress.ContainsJobWithKey(Key)
            if (_jobInProgress.IsSet
                && !_jobInProgress.IsWorking
                && _jobInProgress.IsCurrentDelayed(Agent.CurrentTime))
            {
                Agent.Send(Job.Instruction.DelayedStartNotification.Create(_jobInProgress.JobAgentRef));
            }
            UpdateProcessingItem();
            RequeueIfNecessary();
            return true;
        }

        /// <summary>
        /// Register the Resource in the System on Startup and Save the Hub agent.
        /// </summary>
        internal void SetHubAgent(IActorRef hubAgent)
        {
            // Save to Value Store
            _agentDictionary.Add(key: "Default", value: hubAgent);
            // Debug Message
            Agent.DebugMessage(msg: "Successfully registered resource at : " + hubAgent.Path.Name);
        }

        private void RevokeJob(Guid jobKey)
        {
            var revokedJob = _jobInProgress.RevokeJob(jobKey);
            if (revokedJob != null)
            {
                Agent.DebugMessage(msg: $"Revoking Job from Processing {revokedJob.Job.Name} {revokedJob.Job.Key}", CustomLogger.JOB, LogLevel.Warn);
                Agent.Send(instruction: Job.Instruction.AcknowledgeRevoke.Create(message: Agent.Context.Self, target: revokedJob.JobAgentRef));
                UpdateProcessingItem();
                return;
            }

            var jobConfirmation = _scopeQueue.GetConfirmation(jobKey);

            if (jobConfirmation != null)
            {
                Agent.DebugMessage(msg: $"Revoking Job from ScopeQueue {jobConfirmation.Job.Name} {jobConfirmation.Job.Key}", CustomLogger.JOB, LogLevel.Warn);
                _scopeQueue.RemoveJob(jobConfirmation);
                UpdateAndRequeuePlanedJobs(jobConfirmation);
                Agent.Send(instruction: Job.Instruction.AcknowledgeRevoke.Create(message: Agent.Context.Self, target: jobConfirmation.JobAgentRef));
                return;
            }
            Agent.DebugMessage(msg: $"Job could not be Revoked {jobKey} its already gone.", CustomLogger.JOB, LogLevel.Warn);
        }

        #region Proporsal

        /// <summary>
        /// Is Called from Hub Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="jobItem"></param>
        internal void RequestProposal(FRequestProposalForCapability requestProposal)
        {
            var jobConfirmation = _scopeQueue.GetConfirmation(requestProposal.Job.Key);

            if (jobConfirmation != null)
                _scopeQueue.RemoveJob(jobConfirmation);

            Agent.DebugMessage(msg: $"Asked by Hub for Proposal: " + requestProposal.Job.Name + " with Id: " + requestProposal.Job.Key + " for setup " + requestProposal.CapabilityId);
            SendProposalTo(requestProposal);
        }

        internal void SendProposalTo(FRequestProposalForCapability requestProposal)
        {
            Agent.DebugMessage($"Send Proposal for Job {requestProposal.Job.Key} with Priority {requestProposal.Job.Priority(Agent.CurrentTime)}", CustomLogger.PRIORITY, LogLevel.Warn);

            foreach (var job in _scopeQueue.GetAllJobs())
            {
                Agent.DebugMessage($"{job.Key} with Priority {job.Job.Priority(Agent.CurrentTime)}", CustomLogger.PRIORITY, LogLevel.Warn);
            }

            var queuePositions = _scopeQueue.GetQueueAbleTime(requestProposal
                                                                , currentTime: Agent.CurrentTime
                                                                , cpm: _capabilityProviderManager
                                                                , resourceBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                                                , Agent.Context.Self );

            //TODO Sets Postponed to calculated Duration of Bucket
            var fPostponed = new FPostponed(offset: queuePositions.Any(x => x.IsQueueAble) ? 0 : Convert.ToInt32(_scopeQueue.Workload * 0.8));
            var jobPrio = requestProposal.Job.Priority(Agent.CurrentTime);
            Agent.DebugMessage(msg: queuePositions.First().IsQueueAble
                ? $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} IsQueueAble: {queuePositions.First().IsQueueAble} and has satisfiedJobs {((FBucket)requestProposal.Job).HasSatisfiedJob } with EstimatedStart: {queuePositions.First().Scope.Start} and Prio: {jobPrio}"
                : $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} Postponed: {fPostponed.IsPostponed} with Offset: {fPostponed.Offset} and Prio: {jobPrio} ", CustomLogger.PRIORITY, LogLevel.Warn);

            // calculate proposal
            var proposal = new FProposals.FProposal(possibleSchedule: queuePositions
                , postponed: fPostponed
                , requestProposal.CapabilityId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal void AcceptProposals(IConfirmation jobConfirmation)
        {
            Agent.DebugMessage($"Start Acknowledge Job {jobConfirmation.Key}  {jobConfirmation.Job.Name}  | scope : [{jobConfirmation.ScopeConfirmation.GetScopeStart()} to {jobConfirmation.ScopeConfirmation.GetScopeEnd()}]" +
                               $" with Priority {jobConfirmation.Job.Priority(Agent.CurrentTime)}" +
                               $" | scopeLimit: {_scopeQueue.Limit} | scope workload : {_scopeQueue.Workload} | Capacity left {_scopeQueue.Limit - _scopeQueue.Workload} " +
                               $" {((FBucket)jobConfirmation.Job).MaxBucketSize} ", CustomLogger.PRIORITY, LogLevel.Warn);

            var setup = jobConfirmation.CapabilityProvider.ResourceSetups.Single(x =>
                x.Resource.IResourceRef != null &&
                ((IActorRef) x.Resource.IResourceRef).Path.Name == Agent.Context.Self.Path.Name);
            var isQueueAble = _scopeQueue.CheckScope(jobConfirmation, Agent.CurrentTime, _jobInProgress.ResourceIsBusyUntil, _capabilityProviderManager.GetCurrentUsedCapabilityId(), setup.UsedInSetup);


            if (isQueueAble && _jobInProgress.IsSet
                && _jobInProgress.ResourceIsBusyUntil > jobConfirmation.ScopeConfirmation.GetScopeStart())
            {
                Agent.DebugMessage(msg: $"Seems to be wrong #3", CustomLogger.JOB, LogLevel.Warn);
            }




            // If is not queueable 
            if (!isQueueAble)
            {
                Agent.DebugMessage(msg: $"Reject proposal for: {jobConfirmation.Job.Name} {jobConfirmation.Key} with jobPrio: { jobConfirmation.Job.Priority(Agent.CurrentTime) } and send reject job to job agent", CustomLogger.PRIORITY, LogLevel.Warn);
                Agent.Send(instruction: Job.Instruction.StartRequeue.Create(target: jobConfirmation.JobAgentRef));
                return;
            }

            this.UpdateAndRequeuePlanedJobs(jobConfirmation);
            _scopeQueue.Enqueue(jobConfirmation);

            Agent.DebugMessage(msg: $"Accepted proposal on resource {Agent.Context.Self.Path.Name} and " +
                                    $"start enqueue {jobConfirmation.Job.Name} {jobConfirmation.Key} queueCount: {_scopeQueue.Count}" +
                                    $"", CustomLogger.PRIORITY, LogLevel.Warn);

            UpdateProcessingItem();
        }

#endregion

#region Processing

        internal void UpdateProcessingItem()
        {
            // Fälle
            // 1. _jobInProgress ist leer
            // 2. _jobInProgress hat etwas in progress aber nichts Ready Gelistet
            // 3. _jobInProgress hat etwas in progress und Ready Gelistete elemente
            var foundItem = true;
            var jobInProgressHasChanged = false;
            while (foundItem)
            {
                var item = _scopeQueue.GetFirstIfSatisfiedAndSetReadyAtIsSmallerOrEqualThan(currentTime: Agent.CurrentTime,
                                                                                          _capabilityProviderManager.GetCurrentUsedCapability());
                if (item == null)
                {
                    foundItem = false; 
                    continue;
                }

                if (_jobInProgress.IsSet
                    && _jobInProgress.ResourceIsBusyUntil > item.ScopeConfirmation.GetScopeStart())
                {
                    Agent.DebugMessage(msg: $"Seems to be wrong #2", CustomLogger.JOB, LogLevel.Warn);
                }



                jobInProgressHasChanged = true;
                Agent.Send(instruction: Job.Instruction.ResourceWillBeReady.Create(target: item.JobAgentRef));
                _jobInProgress.Add(item);
                Agent.DebugMessage(msg: $"Add to jobInProgress { item.Job.Name } { item.Job.Key } scope start { item.ScopeConfirmation.GetScopeStart() } setReadyAt { item.ScopeConfirmation.SetReadyAt } Has Satisfied Jobs { ((FBucket)item.Job).HasSatisfiedJob }", CustomLogger.JOB, LogLevel.Warn);
                _scopeQueue.RemoveJob(item);
            }

            var job = _jobInProgress.ReadyItemToProcessingItem();
                //_jobInProgress.has any ready items
            if (!_jobInProgress.IsSet && job.IsNull())
            {
                Agent.DebugMessage(msg: $"Start Queue Health check.", CustomLogger.JOB, LogLevel.Warn);
                if (_scopeQueue.QueueHealthCheck(Agent.CurrentTime))
                {
                    Agent.DebugMessage(msg: $"Queue seems unhealthy, try to requeue!", CustomLogger.JOB, LogLevel.Warn);
                    RequeueAllRemainingJobs();
                    return;
                };
            }

            if (job.IsNotNull() || jobInProgressHasChanged)
            {
                RequeueIfNecessary();
            }

            // take the next scope and make it fix 
            if (job.IsNotNull())
            {   
                if (job.ScopeConfirmation.GetSetup() != null)
                {
                    Agent.Send(instruction: Job.Instruction.RequestSetupStart.Create(message: Agent.Context.Self, target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Asking for SetupStart {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}", CustomLogger.JOB, LogLevel.Warn);
                }
                else
                {
                    Agent.Send(instruction: Job.Instruction.RequestProcessingStart.Create(message: Agent.Context.Self, target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Asking for Processing {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}", CustomLogger.JOB, LogLevel.Warn);
                }
            }
        }

        private void FinalizedBucket(IConfirmation fJobConfirmation)
        {
            if (_jobInProgress.UpdateJob(fJobConfirmation)) 
            {
                _jobInProgress.DissolveBucketToQueue(Agent.CurrentTime);
                RequeueIfNecessary();
            }
        }

        internal void UpdateStartCondition(IJob job)
        {
            var jobConfirmation = _scopeQueue.GetAllJobs().SingleOrDefault(x => x.Job.Key == job.Key);
            if (jobConfirmation == null)
            {
                Agent.DebugMessage($"Bucket {job.Key} {job.Name} is not in Queue anymore", CustomLogger.JOB, LogLevel.Warn);
                return;   
            }
            _scopeQueue.UpdateBucket(job);
            Agent.DebugMessage($"Bucket {job.Key} {job.Name} found and updated", CustomLogger.JOB, LogLevel.Warn);
            RequeueIfNecessary();
            UpdateProcessingItem();
        }

        /// <summary>
        ///  DoSetup();
        /// </summary>
        internal void DoSetup()
        {
            _jobInProgress.SetupIsOngoing = true;
            var setupDuration = _capabilityProviderManager.GetSetupDurationBy(_jobInProgress.ResourceCapabilityId);
            var duration = Agent.Name.Contains("Operator")
                ? setupDuration
                : setupDuration + _jobInProgress.JobMaxDuration;

            //Start setup 
            Agent.DebugMessage(msg:
                $"Call start Setup for Job {_jobInProgress.JobName}  Key: {_jobInProgress.JobKey} " +
                $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}", CustomLogger.JOB, LogLevel.Warn);
            
            _capabilityProviderManager.Mount(_jobInProgress.CapabilityProviderId);
            _jobInProgress.StartSetup(Agent.CurrentTime, duration);
            if (_scopeQueue.FirstOrNull() != null && _jobInProgress.ResourceIsBusyUntil > _scopeQueue.FirstOrNull().ScopeConfirmation.GetScopeStart())
            {
                RequeueAllRemainingJobs();
                Agent.DebugMessage(msg: $"Queue seems unhealthy, try to requeue!", CustomLogger.JOB, LogLevel.Warn);
            }



            CreateSetupTask(setupDuration, JobType.SETUP);

            Agent.Send(Resource.Instruction.Default.FinishTask.Create("SETUP", Agent.Context.Self), waitFor: setupDuration);
        }

        internal void FinishSetup() // only in case it is not required for processing.
        {
            Agent.DebugMessage($"Finished Setup for {_jobInProgress.JobName }, Resource is released to do next Task;", CustomLogger.JOB, LogLevel.Warn);
            NextTask(); // should only start setup if operations are fine 
            _jobInProgress.SetupIsOngoing = false;
            UpdateProcessingItem();
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork(FOperation operation)
        {
            Agent.DebugMessage($"Call start Work for {_jobInProgress.JobName} {operation.Operation.Name} {operation.Key} to process for {operation.Operation.RandomizedDuration}", CustomLogger.JOB, LogLevel.Warn);
            _jobInProgress.SetupIsOngoing = false;
            _jobInProgress.StartProcessing(Agent.CurrentTime, _jobInProgress.JobDuration);

            _jobInProgress.CurrentOperation.Set(operation, Agent.CurrentTime);
            CreateProcessingTask(operation);

            Agent.Send(Resource.Instruction.Default.FinishTask.Create("PROCESSING", Agent.Context.Self), waitFor: operation.Operation.RandomizedDuration);
        }

        private void FinishTask(string task)
        {
            // TODO Scheduling /  has to be at the end of the time -> flag log for next timestamp -> Agent.Send(Instruction.LogAtNextAdvanceTime, Directory)
            
            if (task.Equals("SETUP"))
            {
                Agent.Send(instruction: BasicInstruction.FinishSetup.Create(message: Agent.Context.Self, target: _jobInProgress.JobAgentRef));
                _jobInProgress.ResetIsWorking();
            }
            else { 
                Agent.Send(instruction: Job.Instruction.FinishProcessing.Create(Agent.Context.Self, _jobInProgress.JobAgentRef));
                _jobInProgress.DequeueNextOperation();
                _jobInProgress.CurrentOperation.Reset();
            }
        }


        internal void FinishBucket()
        {
            Agent.DebugMessage(msg: $"Call finished work with {_jobInProgress.JobName} {_jobInProgress.JobKey} take next...", CustomLogger.JOB, LogLevel.Warn);
            NextTask();
            UpdateProcessingItem();
        }

        private void NextTask()
        {
            _jobInProgress.Reset();
            RequeueIfNecessary();
        }

        private void RequeueIfNecessary()
        {
            var next = _scopeQueue.FirstOrNull();
            if (next.IsNotNull())
            {
                var isOverdue = next.ScopeConfirmation.GetScopeStart() < Agent.CurrentTime;
                var isReady = ((FBucket)next.Job).HasSatisfiedJob;
                var blockUntil = _jobInProgress.ResourceIsBusyUntil == 0 ? Agent.CurrentTime
                                                                         : _jobInProgress.ResourceIsBusyUntil;
                if ((isOverdue && !isReady) 
                  ||(_scopeQueue.HasQueueAbleJobs() && !isReady) // to switch places
                  ||( next.ScopeConfirmation.GetScopeStart() > blockUntil && isReady ) // Pull Close
                  ||( next.ScopeConfirmation.GetScopeStart() < blockUntil ) ) // Push because BucketSize on Resource has changed
                { 
                    Agent.DebugMessage("Requeue because all jobs are overdue", CustomLogger.JOB, LogLevel.Warn);
                    RequeueAllRemainingJobs();
                }
            }
        }

#endregion
        
#region Requeuing
        internal void RequeueAllRemainingJobs()
        {
            var item = _scopeQueue.FirstOrNull();
            if (item != null)
            {
                RequeueJobs(_scopeQueue.GetAllJobs());
            }
        }
        internal void UpdateAndRequeuePlanedJobs(IConfirmation jobConfirmation)
        {
            
            var toRequeue = _scopeQueue.GetTail(currentTime: Agent.CurrentTime, jobConfirmation);
            RequeueJobs(toRequeue);
        }

        private void RequeueJobs(HashSet<IConfirmation> toRequeue)
        {
            Agent.DebugMessage(msg: $"Remove {toRequeue.Count} from {Agent.Context.Self.Path.Name}", CustomLogger.JOB, LogLevel.Warn);

            foreach (var job in toRequeue)
            {
                Agent.DebugMessage(msg: $"Remove for requeue {job.Job.Name} {job.Key} from {Agent.Context.Self.Path.Name}", CustomLogger.JOB, LogLevel.Warn);
                _scopeQueue.RemoveJob(job);
                Agent.Send(instruction: Job.Instruction.StartRequeue.Create(target: job.JobAgentRef));
            }
        }
#endregion

#region Reporting

        void CreateProcessingTask(FOperation item)
        {
            var pub = new FCreateTaskItem(
                type: JobType.OPERATION
                , resource: Agent.Name.Replace("Resource(", "").Replace(")","")
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + item.Operation.RandomizedDuration
                , capability: _jobInProgress.RequiredCapabilityName
                , operation: item.Operation.Name
                , groupId: _jobInProgress.JobName );

            //TODO NO tracking
            Agent.Context.System.EventStream.Publish(@event: pub);
        }

        void CreateSetupTask(long gap, string type)
        {
            var pub = new FCreateTaskItem(
                type
                , resource: Agent.Name.Replace("Resource(", "").Replace(")", "")
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + gap
                , capability: _capabilityProviderManager.GetCurrentUsedCapability().Name
                , operation: $"{type} for {_jobInProgress.JobName}"
                , groupId: _jobInProgress.JobName );

            //TODO NO tracking
            Agent.Context.System.EventStream.Publish(@event: pub);
        }

#endregion

    }
}
