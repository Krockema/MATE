using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using static FCreateSimulationResourceSetups;
using static FJobConfirmations;
using static FOperationResults;
using static FPostponeds;
using static FProposals;
using static FRequestProposalForCapabilityProviders;
using static FResourceInformations;
using static FUpdateSimulationJobs;
using static FUpdateStartConditions;
using static IConfirmations;
using static IJobResults;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class DefaultSetup : SimulationCore.Types.Behaviour
    {
        public DefaultSetup(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, List<M_ResourceCapabilityProvider> capabilityProvider, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
        {
            this._processingQueue = new JobQueueItemLimited(limit: fixedJobQueueSize);
            this._planingQueue = new JobQueueTimeLimited(limit: planingJobQueueLength);
            this._agentDictionary = new AgentDictionary();
            _workTimeGenerator = workTimeGenerator;
            _capabilityProviderManager = new CapabilityProviderManager(capabilityProvider);
        }
        // TODO Implement a JobManager
        internal JobQueueTimeLimited _planingQueue { get; set; }
        internal JobQueueItemLimited _processingQueue { get; set; }
        internal JobInProgress _jobInProgress { get; set; } = new JobInProgress();
        internal CapabilityProviderManager _capabilityProviderManager { get; }
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal AgentDictionary _agentDictionary { get; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.Default.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                case Resource.Instruction.Default.RequestProposal msg: RequestProposal(requestProposal: msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.AcknowledgeProposal msg: AcknowledgeProposal(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(startCondition: msg.GetObjectFromMessage); break;
                case Resource.Instruction.Default.DoWork msg: DoWork(); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        public override bool AfterInit()
        {
            var resourceAgent = Agent as Resource;
            var capabilityProviders = _capabilityProviderManager.GetAllCapabilityProvider();
            Agent.Send(instruction: Directory.Instruction.ForwardRegistrationToHub.Create(
                new FResourceInformation(resourceAgent._resource.Id, capabilityProviders , String.Empty, Agent.Context.Self)
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

        /// <summary>
        /// Is Called from Hub Agent to get an Proposal when the item with a given priority can be scheduled.
        /// </summary>
        /// <param name="jobItem"></param>
        internal virtual void RequestProposal(FRequestProposalForCapability requestProposal)
        {
            Agent.DebugMessage(msg: $"Asked by Hub for Proposal: " + requestProposal.Job.Name + " with Id: " + requestProposal.Job.Key + " for setup " + requestProposal.CapabilityId);

            if (_processingQueue.Contains(requestProposal.Job.Key))
                return;

            SendProposalTo(requestProposal);
        }

        /// <summary>
        /// Send Proposal to Hub Client
        /// </summary>
        /// <param name="jobItem"></param>
        internal virtual void SendProposalTo(FRequestProposalForCapability requestProposal)
        {
            var setupDuration = GetSetupTime(requestProposal.CapabilityId);

            var queuePosition = _planingQueue.GetQueueAbleTime(job: requestProposal.Job
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                           , processingQueueLength: _processingQueue.SumDurations
                                                   , setupDuration: setupDuration);

            if (queuePosition.IsQueueAble) { 
                Agent.DebugMessage(msg: $"IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}");
            }
            var fPostponed = new FPostponed(offset: queuePosition.IsQueueAble ? 0 : _planingQueue.Limit);

            if (fPostponed.IsPostponed) { 
                Agent.DebugMessage(msg: $"Postponed: { fPostponed.IsPostponed } with Offset: { fPostponed.Offset} ");
            }
            // calculate proposal
            var proposal = new FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , capabilityId: requestProposal.CapabilityId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

       
        /// <summary>
        /// Is called after RequestProposal if the proposal is accepted by HubAgent
        /// </summary>
        internal virtual void AcknowledgeProposal(IConfirmation acknowledgeProposal)
        {
            Agent.DebugMessage(msg: $"Start Acknowledge proposal for: {acknowledgeProposal.Job.Name} {acknowledgeProposal.Job.Key}");

            var setupDuration = GetSetupTime(acknowledgeProposal.CapabilityProvider.Id);

            var queuePosition = _planingQueue.GetQueueAbleTime(job: acknowledgeProposal.Job
                                                     , currentTime: Agent.CurrentTime
                                          , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                                           , processingQueueLength: _processingQueue.SumDurations
                                                   , setupDuration: setupDuration);
            // if not QueueAble
            if (!queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"Stop Acknowledge proposal for: {acknowledgeProposal.Job.Name} {acknowledgeProposal.Job.Key} and start requeue");
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: acknowledgeProposal.Job, target: acknowledgeProposal.Job.HubAgent));
                return;
            }

            _planingQueue.Enqueue(acknowledgeProposal);

            Agent.DebugMessage(msg: "AcknowledgeProposal Accepted Item: " + acknowledgeProposal.Job.Name + " with Id: " + acknowledgeProposal.Job.Key);
            UpdateAndRequeuePlanedJobs(acknowledgeProposal);
            UpdateProcessingQueue();
            TryToWork();
        }

        internal virtual void RequeueAllRemainingJobs()
        {
            Agent.DebugMessage(msg: "Start to Requeue all remaining Jobs");
            var jobConfirmation = _planingQueue.FirstOrNull();
            if (jobConfirmation != null)
            {
                UpdateAndRequeuePlanedJobs(jobConfirmation);
            }
        }


        internal virtual void UpdateAndRequeuePlanedJobs(IConfirmation jobConfirmation)
        {
            Agent.DebugMessage(msg: "Old planning queue length = " + _planingQueue.Count);
            var toRequeue = _planingQueue.CutTail(currentTime: Agent.CurrentTime, jobConfirmation);
            foreach (var job in toRequeue)
            {
                _planingQueue.RemoveJob(job);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(job.Job, target: job.Job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }


        internal virtual void UpdateProcessingQueue()
        {
            while (_processingQueue.CapacitiesLeft() && _planingQueue.HasQueueAbleJobs())
            {
                var jobConfirmation = _planingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {jobConfirmation.Job.Key} {jobConfirmation.Job.Name} Try to start processing.");
                var ok = _processingQueue.Enqueue(jobConfirmation);
                if (!ok)
                {
                    throw new Exception(message: "Something wen wrong with Queueing!");
                }
                Agent.DebugMessage(msg: $"Start withdraw for article {jobConfirmation.Job.Name} {jobConfirmation.Job.Key}");
                Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: jobConfirmation.Job.Key, target: jobConfirmation.Job.HubAgent));
            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }

        internal virtual void UpdateStartCondition(FUpdateStartCondition startCondition)
        {
            Agent.DebugMessage(msg: $"UpdateArticleProvided for article: {startCondition.OperationKey} ArticleProvided: {startCondition.ArticlesProvided} && PreCondition {startCondition.PreCondition}");

            if (_planingQueue.UpdatePreCondition(startCondition: startCondition))
            {
                UpdateProcessingQueue();
            }

        }

        internal virtual void TryToWork()
        {
            if (_jobInProgress.IsSet)
            {
                Agent.DebugMessage(msg: "Im still working....");
                return; // Resource Agent is still working
            }

            var nextJobInProgress = _processingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);

            // Wait if nothing more to do
            if (nextJobInProgress == null)
            {
                // No more work 
                Agent.DebugMessage(msg: "Nothing more Ready in Queue!");
                return;
            }

            UpdateProcessingQueue();

            _jobInProgress.Set(nextJobInProgress, nextJobInProgress.Duration);

            DoSetup();
        }

        internal virtual void DoSetup()
        {
            //Start setup if necessary 
            var setupDuration = GetSetupTime(_jobInProgress.Current.CapabilityProvider.Id);

            if (setupDuration > 0)
            {
                Agent.DebugMessage(
                    msg:
                    $"Start with Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}");
                _capabilityProviderManager.Mount(_jobInProgress.Current.Job.RequiredCapability.Id);
                var pubSetup = new FCreateSimulationResourceSetup(expectedDuration: setupDuration
                                                                        , duration: setupDuration
                                                                           , start: Agent.CurrentTime
                                                              , capabilityProvider: _jobInProgress.Current.CapabilityProvider.Name
                                                                  , capabilityName: _jobInProgress.RequiredCapabilityName
                                                                         , setupId: _jobInProgress.SetupId);
                Agent.Context.System.EventStream.Publish(@event: pubSetup);
            }

            Agent.Send(instruction: Resource.Instruction.Default.DoWork.Create(message: null, target: Agent.Context.Self),
                waitFor: setupDuration);
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal virtual void DoWork()
        {
            var randomizedWorkDuration = _workTimeGenerator.GetRandomWorkTime(duration: _jobInProgress.Current.Job.Duration);
            Agent.DebugMessage(msg: $"Starting Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} new Duration is {randomizedWorkDuration}");

            var pub = new FUpdateSimulationJob(job: _jobInProgress.Current.Job
                                         , jobType: JobType.OPERATION
                                        , duration: randomizedWorkDuration
                                           , start: Agent.CurrentTime
                              , capabilityProvider: Agent.Name
                                          , bucket: _jobInProgress.Current.Job.Bucket
                                         , setupId: _jobInProgress.SetupId);
            Agent.Context.System.EventStream.Publish(@event: pub);

            var fOperationResult = new FOperationResult(key: _jobInProgress.Current.Job.Key
                                             , creationTime: 0
                                                    , start: Agent.CurrentTime
                                                      , end: Agent.CurrentTime + randomizedWorkDuration
                                         , originalDuration: _jobInProgress.Current.Job.Duration
                                          , productionAgent: ActorRefs.Nobody
                                       , capabilityProvider: _jobInProgress.Current.CapabilityProvider.Name);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: fOperationResult, target: Agent.Context.Self), waitFor: randomizedWorkDuration);

        }

        internal virtual void FinishJob(IJobResult jobResult)
        {
            Agent.DebugMessage(msg: $"Finished Work with {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} take next...");
            jobResult = jobResult.FinishedAt(Agent.CurrentTime);

            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: _jobInProgress.Current.Job.HubAgent));
            _jobInProgress.Reset();

            // then requeue processing queue if the item was delayed 
            if (jobResult.OriginalDuration != Agent.CurrentTime - jobResult.Start)
                RequeueAllRemainingJobs();

            // Do Work
            TryToWork();
        }

        internal long GetSetupTime(int capabilityProviderId)
        {
            var setupTime = 0L;
            if (!_capabilityProviderManager.AlreadyEquipped(capabilityProviderId))
            {
                setupTime = _capabilityProviderManager.GetSetupDurationBy(capabilityProviderId);
            }

            Agent.DebugMessage(
                msg: $"Has Tool: {_capabilityProviderManager.GetCapabilityProviderName()} | require Tool: {capabilityProviderId} with setupDuration {setupTime}");
            return setupTime;
        }

        /*
        private void BreakDown(Resource agent, FBreakDown breakDwon)
        {
            if (breakDwon.IsBroken)
            {
                Break(agent, breakDwon);
            }
            else
            {
                RecoverFromBreakDown(agent);
            }

        }

        private void Break(Resource agent, FBreakDown breakdown)
        {
            agent.Set(Resource.Properties.BROKEN, breakdown.IsBroken);
            // requeue all
            var queue = agent.Get<List<FWorkItem>>(Resource.Properties.QUEUE);
            var Processing = agent.Get<LimitedQueue<FWorkItem>>(Resource.Properties.PROCESSING_QUEUE);
            agent.CallToReQueue(Processing, new List<FWorkItem>(Processing));
            agent.CallToReQueue(queue, new List<FWorkItem>(queue));
            // set Self Recovery
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakdown.SetIsBroken(false), agent.Context.Self), 1440);
        }

        private void RecoverFromBreakDown(Resource agent)
        {
            agent.Set(Resource.Properties.BROKEN, false);
            agent.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, agent.Name, agent.Context.Self), agent.VirtualParent, true));
        }

        */
    }
}
