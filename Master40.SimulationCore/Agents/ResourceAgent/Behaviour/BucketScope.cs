using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.DirectoryAgent;
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
using Akka.Event;
using Akka.Util.Internal;
using static FBuckets;
using static FOperations;
using static FPostponeds;
using static FRequestProposalForCapabilityProviders;
using static FResourceInformations;
using static FUpdateStartConditions;
using static IConfirmations;
using static IJobResults;
using LogLevel = NLog.LogLevel;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class BucketScope : SimulationCore.Types.Behaviour
    {
        public BucketScope(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
            _capabilityProviderManager = new CapabilityProviderManager(capabilityProvider);
            _agentDictionary = new AgentDictionary();
            // SCOPELIMIT something like 960
            _scopeQueue = new TimeConstraintQueue(limit: 960);
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

                case Resource.Instruction.BucketScope.FinishBucket msg: FinishBucket(); break;
                case Resource.Instruction.BucketScope.FinishTask msg: FinishTask(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.TryToWork msg: UpdateProcessingItem(); break;
                case Resource.Instruction.BucketScope.DoSetup msg: DoSetup(); break;
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
            if (_jobInProgress.IsSet && _jobInProgress.Current.Job.Key.Equals(jobKey))
            {
                Agent.Send(instruction: Job.Instruction.AcknowledgeRevoke.Create(message: Agent.Context.Self, target: _jobInProgress.Current.JobAgentRef));
                _jobInProgress.Reset();
                UpdateProcessingItem();
                return;
            }

            var jobConfirmation = _scopeQueue.GetConfirmation(jobKey);

            if (jobConfirmation != null)
            { 
                _scopeQueue.RemoveJob(jobConfirmation);
                UpdateAndRequeuePlanedJobs(jobConfirmation);
                Agent.Send(instruction: Job.Instruction.AcknowledgeRevoke.Create(message: Agent.Context.Self, target: jobConfirmation.JobAgentRef));
            }
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
            var fPostponed = new FPostponed(offset: queuePositions.First().IsQueueAble ? 0 : Convert.ToInt32(_scopeQueue.Workload * 0.8));
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
            Agent.DebugMessage($"Start Acknowledge Job {jobConfirmation.Key} | scope : [{jobConfirmation.ScopeConfirmation.GetScopeStart()} to {jobConfirmation.ScopeConfirmation.GetScopeEnd()}]" +
                               $" with Priority {jobConfirmation.Job.Priority(Agent.CurrentTime)}" +
                               $" | scopeLimit: {_scopeQueue.Limit} | scope workload : {_scopeQueue.Workload} | Capacity left {_scopeQueue.Limit - _scopeQueue.Workload} " +
                               $" {((FBucket)jobConfirmation.Job).MaxBucketSize} ", CustomLogger.PRIORITY, LogLevel.Warn);
            
            PrintScopeQueueBecauseWeAreVerzweifelt();


            var isQueueAble = _scopeQueue.CheckScope(jobConfirmation, Agent.CurrentTime);

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
            var job = _scopeQueue.GetFirstIfSatisfied(currentTime: Agent.CurrentTime, _capabilityProviderManager.GetCurrentUsedCapability());

            var isQueueAble = (job != null) ? $"{job.Job.Name} {job.Job.Key} " : " not satisfied";
            var allPrios = "";
            _scopeQueue.GetAllJobs().ForEach(x => allPrios += $"  | Bucket {x.Job.Name} | Prio: {x.Job.Priority(Agent.CurrentTime)} | Is Ready: {((FBucket)x.Job).HasSatisfiedJob } | FirstOperation {((FBucket)x.Job).Operations.First().Operation.Name} | ScopeStart {x.Key}") ;

            PrintScopeQueueBecauseWeAreVerzweifelt();

            Agent.DebugMessage(msg: $"Try to update processing item from scope queue with {_scopeQueue.Count} bucket. " + allPrios +
                                    $" | Job is in progress: {_jobInProgress.IsSet}", CustomLogger.JOB, LogLevel.Warn);

            // check Scope.Start >= Current Time
                // Send Start self with UpdateProcessing Item,
            // else 
                // UpdateProcessingItem

            // take the next scope and make it fix 
            if (!_jobInProgress.IsSet && job != null)
            {   
                _scopeQueue.RemoveJob(job);
                _jobInProgress.Set(job);
                
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Job.Name} {job.Job.Key} with satisfied: {((FBucket)job.Job).HasSatisfiedJob} Try to start processing.");
                
                // ToDo : test behaviour of this method.
                if (job.ScopeConfirmation.GetSetup() != null)
                {
                    Agent.Send(instruction: Job.Instruction.RequestSetupStart.Create(message: Agent.Context.Self, target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Ask for setupStart {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}");
                }
                else
                {
                    Agent.Send(instruction: Job.Instruction.RequestProcessingStart.Create(message: Agent.Context.Self, target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Ask for Processing {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}");
                }

            }
        }

        private void PrintScopeQueueBecauseWeAreVerzweifelt()
        {
            foreach (var scopeJob in _scopeQueue.GetAllJobs())
            {
                Agent.DebugMessage($"JOB from ScopeQueue {scopeJob.Key} | {scopeJob.Job.Name}| Is Ready {((FBucket)scopeJob.Job).HasSatisfiedJob} | scope : [{scopeJob.ScopeConfirmation.GetScopeStart()} to {scopeJob.ScopeConfirmation.GetScopeEnd()}] | with Priority {scopeJob.Job.Priority(Agent.CurrentTime)}", CustomLogger.PRIORITY, LogLevel.Warn);

                foreach (var operation in ((FBucket)scopeJob.Job).Operations)
                {
                    Agent.DebugMessage($"OPERATION from JOB {((FBucket)scopeJob.Job).Name} with Operation {operation.Operation.Name} {operation.Key} Prio: {((IJob)operation).Priority(Agent.CurrentTime)} PreCondition: {operation.StartConditions.PreCondition} ArticleProvided: {operation.StartConditions.ArticlesProvided}");
                }
            }
        }

        internal void UpdateStartCondition(IJob job)
        {
            var jobConfirmation = _scopeQueue.GetAllJobs().SingleOrDefault(x => x.Job.Key == job.Key);
            if (jobConfirmation != null)
            {
                _scopeQueue.UpdateBucket(job);
                Agent.DebugMessage($"Bucket {job.Key} {job.Name} found and updated");
            }
            else
            {
                Agent.DebugMessage($"Bucket {job.Key} {job.Name} is not in Queue anymore");
            }
            RequeueIfNecessary();
            UpdateProcessingItem();
        }

        /// <summary>
        ///  DoSetup();
        /// </summary>
        internal void DoSetup()
        {
            var setupDuration = _capabilityProviderManager.GetSetupDurationByCapability(_jobInProgress.Current.CapabilityProvider.ResourceCapabilityId);
            //Start setup 
            Agent.DebugMessage(msg:
                $"Start with Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} " +
                $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
            
            _capabilityProviderManager.Mount(_jobInProgress.Current.CapabilityProvider.Id);
            _jobInProgress.Start();

            Agent.Send(Resource.Instruction.BucketScope.FinishTask.Create("SETUP", Agent.Context.Self), waitFor: setupDuration);
        }

        internal void FinishSetup() // only in case it is not required for processing.
        {
            Agent.DebugMessage("Finished Setup, Resource is released to do next Task;");
            NextTask(); // should only start setup if operations are fine 
            UpdateProcessingItem();
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork(FOperation operation)
        {
            Agent.DebugMessage($"Call start of {operation.Operation.Name} {operation.Key} to process for {operation.Operation.RandomizedDuration}");

            _jobInProgress.Start();

            Agent.Send(Resource.Instruction.BucketScope.FinishTask.Create("PROCESSING", Agent.Context.Self), waitFor: operation.Operation.RandomizedDuration);
        }

        private void FinishTask(string task)
        {
            if (task.Equals("SETUP"))
            {
                Agent.Send(instruction: BasicInstruction.FinishSetup.Create(message: Agent.Context.Self, target: _jobInProgress.Current.JobAgentRef));
            }
            else { 
                Agent.Send(instruction: Job.Instruction.FinishProcessing.Create(Agent.Context.Self, _jobInProgress.Current.JobAgentRef));
            }
        }


        internal void FinishBucket()
        {
            Agent.DebugMessage(msg: $"Bucket finished work with {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} take next...");
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
            if (next != null)
            {
                var isOverdue = next.ScopeConfirmation.GetScopeStart() < Agent.CurrentTime;
                var isReady = ((FBucket)next.Job).HasSatisfiedJob;

                if (isOverdue && !isReady || (_scopeQueue.HasQueueAbleJobs() && !isReady))
                {

                    Agent.DebugMessage("Requeue because all jobs are overdue", CustomLogger.JOB, LogLevel.Warn);
                    RequeueAllRemainingJobs();
                }
            }
        }

#endregion

        internal void RequeueAllRemainingJobs()
        {
            var item = _scopeQueue.FirstOrNull();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(item);
            }
        }

        #region Requeuing

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

    }
}
