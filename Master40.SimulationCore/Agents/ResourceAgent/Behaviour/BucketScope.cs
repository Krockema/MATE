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
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBuckets;
using static FOperations;
using static FPostponeds;
using static FRequestProposalForCapabilityProviders;
using static FUpdateStartConditions;
using static IConfirmations;
using static IJobResults;

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
            _scopeQueue = new TimeConstraintQueue(limit: planingJobQueueLength);
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
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.AcceptedProposals msg: AcceptProposals(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(msg.GetObjectFromMessage); break;

                case Resource.Instruction.Default.RevokeJob msg: RevokeJob(msg.GetObjectFromMessage); break;
                
                case Resource.Instruction.BucketScope.DoSetup msg: DoSetup(); break;
                case Resource.Instruction.Default.DoWork msg: DoWork(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.FinishBucket msg: FinishBucket(); break;

                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void RevokeJob(Guid jobKey)
        {
            // ToDo:  revoke job and Setup !!
            var jobConfirmation = _scopeQueue.GetConfirmation(jobKey);

            if (jobConfirmation != null)
            { 
                _scopeQueue.RemoveJob(jobConfirmation);
                UpdateAndRequeuePlanedJobs(jobConfirmation);
            }
        }

#region Proporsal

        /// <summary>
        /// Is Called from Hub Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="jobItem"></param>
        internal void RequestProposal(FRequestProposalForCapabilityProvider requestProposal)
        {
            var jobConfirmation = _scopeQueue.GetConfirmation(requestProposal.Job.Key);

            if (jobConfirmation != null)
                _scopeQueue.RemoveJob(jobConfirmation);

            Agent.DebugMessage(msg: $"Asked by Hub for Proposal: " + requestProposal.Job.Name + " with Id: " + requestProposal.Job.Key + " for setup " + requestProposal.CapabilityProviderId);
            SendProposalTo(requestProposal);
        }

        internal void SendProposalTo(FRequestProposalForCapabilityProvider requestProposal)
        {
            var queuePositions = _scopeQueue.GetQueueAbleTime(requestProposal
                                                                , currentTime: Agent.CurrentTime
                                                                , cpm: _capabilityProviderManager
                                                                , resourceBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                                                , Agent.Context.Self );

            //TODO Sets Postponed to calculated Duration of Bucket
            var fPostponed = new FPostponed(offset: queuePositions.First().IsQueueAble ? 0 : Convert.ToInt32(_scopeQueue.Workload * 0.8));
            var jobPrio = requestProposal.Job.Priority(Agent.CurrentTime);
            Agent.DebugMessage(msg: queuePositions.First().IsQueueAble
                ? $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} IsQueueAble: {queuePositions.First().IsQueueAble} with EstimatedStart: {queuePositions.First().Scope.Start} and Prio: {jobPrio}"
                : $"Bucket: {requestProposal.Job.Name} {requestProposal.Job.Key} Postponed: {fPostponed.IsPostponed} with Offset: {fPostponed.Offset} and Prio: {jobPrio} ", CustomLogger.PROPOSAL, LogLevel.Warn);

            // calculate proposal
            var proposal = new FProposals.FProposal(possibleSchedule: queuePositions
                , postponed: fPostponed
                , requestProposal.CapabilityProviderId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal void AcceptProposals(IConfirmation jobConfirmation)
        {
            var isQueueAble = _scopeQueue.CheckScope(jobConfirmation, Agent.CurrentTime);

            // If is not queueable 
            if (!isQueueAble)
            {
                Agent.DebugMessage(msg: $"Reject proposal for: {jobConfirmation.Job.Name} {jobConfirmation.Key} with jobPrio: { jobConfirmation.Job.Priority(Agent.CurrentTime) } and send reject job to job agent", CustomLogger.PROPOSAL, LogLevel.Warn);
                Agent.Send(instruction: Job.Instruction.RejectAcknowledgeResponseFromResource.Create(target: jobConfirmation.JobAgentRef));
                return;
            }

            this.UpdateAndRequeuePlanedJobs(jobConfirmation);
            _scopeQueue.Enqueue(jobConfirmation);

            Agent.DebugMessage(msg: $"Accepted proposal on resource {Agent.Context.Self.Path.Name} and " +
                                    $"start enqueue {jobConfirmation.Job.Name} {jobConfirmation.Key} queueCount: {_scopeQueue.Count}" +
                                    $"", CustomLogger.PROPOSAL, LogLevel.Warn);

            UpdateProcessingItem();
        }

#endregion

#region Processing

        internal void UpdateProcessingItem()
        {
            // take the next scope and make it fix 
            if(!_jobInProgress.IsSet && _scopeQueue.HasQueueAbleJobs())
            {
                var job = _scopeQueue.DequeueFirstIfSatisfied(currentTime: Agent.CurrentTime, _capabilityProviderManager.GetCurrentUsedCapability());
                _jobInProgress.Set(job, Agent.CurrentTime);
                
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Job.Name} {job.Job.Key} with satisfied: {job.Job.StartConditions.Satisfied} Try to start processing.");
                

                // ToDo : test behaviour of this method.
                if (job.ScopeConfirmation.GetSetup() != null)
                {
                    Agent.Send(instruction: Job.Instruction.RequestSetupStart.Create(target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Ask for setupStart {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}");
                }
                else
                {
                    Agent.Send(instruction: Job.Instruction.RequestProcessingStart.Create(target: job.JobAgentRef));
                    Agent.DebugMessage(msg: $"Ask for Processing {job.Job.Name} {job.Job.Key} at {Agent.Context.Self.Path.Name}");
                }

            }
        }

        internal void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            var buckets = _scopeQueue.GetJobsAs<FBucket>();
            var bucket = buckets?.SingleOrDefault(x => x.Operations.Any(x => x.Key == startCondition.OperationKey));
            if (bucket != null)
            {
                var operation = bucket.Operations.Single(x => x.Key == startCondition.OperationKey);
                operation.SetStartConditions(startCondition: startCondition);
                Agent.DebugMessage($"Operation {operation.Operation.Name} {operation.Key} in {bucket.Name} has been startCondition set to: {operation.StartConditions.Satisfied} with preCondition: {operation.StartConditions.PreCondition} and articlesProvided {operation.StartConditions.ArticlesProvided}");
            }
            else
            {
                Agent.DebugMessage($"Bucket is not in Queue anymore");
            }

            UpdateProcessingItem();
        }

        /// <summary>
        ///  DoSetup();
        /// </summary>
        internal void DoSetup()
        {
            var setupDuration = _capabilityProviderManager.GetSetupDurationByCapabilityProvider(_jobInProgress.Current.CapabilityProvider.Id);
            //Start setup 
            Agent.DebugMessage(msg:
                $"Start with Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} " +
                $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
            
            _capabilityProviderManager.Mount(_jobInProgress.Current.Job.RequiredCapability.Id);
            _jobInProgress.SetStartTime(Agent.CurrentTime);
            Agent.Send(instruction: Job.Instruction.FinishSetup.Create(target: _jobInProgress.Current.JobAgentRef), waitFor: setupDuration);
            Agent.Send(instruction: Job.Instruction.RequestProcessingStart.Create(target: _jobInProgress.Current.JobAgentRef), waitFor: setupDuration);
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork(FOperation operation)
        {
            Agent.DebugMessage($"Call start of {operation.Operation.Name} {operation.Key} to process for {operation.Operation.RandomizedDuration}");

            _jobInProgress.IsWorking = true;

            Agent.Send(instruction: Job.Instruction.FinishProcessing.Create(target: Agent.Sender),waitFor: operation.Operation.RandomizedDuration);
            
        }

        internal void FinishBucket()
        {
            Agent.DebugMessage(msg: $"Bucket finished work with {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} take next...");
            
            _jobInProgress.Reset();

            UpdateProcessingItem();
            
            //TODO  Maybe only if expected != acutal duration
            RequeueAllRemainingJobs();
        }

#endregion

        internal void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage(msg: "Start to Requeue all remaining Jobs");
            var item = _scopeQueue.FirstOrNull();
            if (item != null)
            {
                UpdateAndRequeuePlanedJobs(item);
            }
        }

        #region Requeuing

        internal void UpdateAndRequeuePlanedJobs(IConfirmation jobConfirmation)
        {
            Agent.DebugMessage(msg: $"Old scope queue length on {Agent.Context.Self.Path.Name}: " + _scopeQueue.Count);
            var toRequeue = _scopeQueue.GetTail(currentTime: Agent.CurrentTime, jobConfirmation);
            RequeueJobs(toRequeue);
            Agent.DebugMessage(msg: $"New scope queue length on {Agent.Context.Self.Path.Name}: " + _scopeQueue.Count);
        }
        private void RequeueJobs(HashSet<IConfirmation> toRequeue)
        {
            foreach (var job in toRequeue)
            {
                _scopeQueue.RemoveJob(job);
                Agent.Send(instruction: Job.Instruction.StartRequeue.Create(target: job.JobAgentRef));
            }
        }
        #endregion

    }
}
