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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Akka.Util.Internal;
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
                Agent.DebugMessage(msg: $"Revoking Job from Processing {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key}", CustomLogger.JOB, LogLevel.Warn);
                Agent.Send(instruction: Job.Instruction.AcknowledgeRevoke.Create(message: Agent.Context.Self, target: _jobInProgress.Current.JobAgentRef));
                _jobInProgress.Reset();
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
            Agent.DebugMessage($"Start Acknowledge Job {jobConfirmation.Key} | scope : [{jobConfirmation.ScopeConfirmation.GetScopeStart()} to {jobConfirmation.ScopeConfirmation.GetScopeEnd()}]" +
                               $" with Priority {jobConfirmation.Job.Priority(Agent.CurrentTime)}" +
                               $" | scopeLimit: {_scopeQueue.Limit} | scope workload : {_scopeQueue.Workload} | Capacity left {_scopeQueue.Limit - _scopeQueue.Workload} " +
                               $" {((FBucket)jobConfirmation.Job).MaxBucketSize} ", CustomLogger.PRIORITY, LogLevel.Warn);
            
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

        public override bool PostAdvance()
        {
            if (Agent.CurrentTime % 50 == 0 || Agent.CurrentTime == 2)
            {
                CreateGanttChartForRessource();
            }


            if (_jobInProgress.IsSet 
                && !_jobInProgress.IsWorking 
                && _jobInProgress.Current.ScopeConfirmation.GetScopeStart() < Agent.CurrentTime) 
            {
                Agent.Send(Job.Instruction.DelayedStartNotification.Create(_jobInProgress.Current.JobAgentRef));
            }
            UpdateProcessingItem();
            return true;
        }



        private void CreateGanttChartForRessource()
        {
            List<GanttChartItem> ganttData = new List<GanttChartItem>();
            if (_jobInProgress.IsSet)
                ganttData.AddRange(CreateGanttProcessingQueueLog(new[] { _jobInProgress.Current }, true));
            // add from scope

            ganttData.AddRange(CreateGanttProcessingQueueLog(_scopeQueue.GetAllJobs().OrderBy(x => x.Job.Priority(Agent.CurrentTime)).ToArray(), false));
            CustomFileWriter.WriteToFile($"Logs//ResourceScheduleAt-{Agent.CurrentTime}.log",
                JsonConvert.SerializeObject(ganttData).Replace("[", "").Replace("]", ","));
        }
        

        private List<GanttChartItem> CreateGanttProcessingQueueLog(IConfirmation[] jobArray, bool inProcessing)
        {
            var ganttTransformation = new List<GanttChartItem>();
            foreach (var bucket in jobArray)
            {
                long operationStart = 0;
                if (bucket.ScopeConfirmation.GetSetup() != null)
                {
                    operationStart = bucket.ScopeConfirmation.GetSetup().Start;
                    ganttTransformation.Add(new GanttChartItem
                    {
                        article = bucket.Job.Name,
                        articleId = bucket.Key.ToString(),
                        start = operationStart.ToString(),
                        end = bucket.ScopeConfirmation.GetSetup()?.End.ToString(), // bucket.ScopeConfirmation.GetScopeEnd().ToString(),
                        groupId = bucket.Key.ToString(),
                        operation = "Setup for " + bucket.Job.Name,
                        operationId = bucket.Key.ToString(),
                        resource = Agent.Name, 
                        priority = bucket.Job.Priority(Agent.CurrentTime).ToString(),
                        IsProcessing = inProcessing.ToString(),
                        IsReady = ((FBucket)bucket.Job).HasSatisfiedJob.ToString()
                    });
                    operationStart = bucket.ScopeConfirmation.GetSetup().End;
                }
                else
                {
                    operationStart = bucket.ScopeConfirmation.GetScopeStart();
                }

                if (Agent.Name.Contains("Operator")) continue;

                var ops = ((FBucket) bucket.Job).Operations.OrderBy(x => x.Priority.Invoke(Agent.CurrentTime)).ToArray();
                for (int j = 0; j < ops.Length; j++)
                {
                    var operation = ops[j];
                    var operationEnd = operationStart + operation.Operation.Duration;
                    ganttTransformation.Add(new GanttChartItem
                    {
                        article = operation.Bucket,
                        articleId = operation.Key.ToString(),
                        start = operationStart.ToString(),
                        end = operationEnd.ToString(),
                        groupId = bucket.Key.ToString(),
                        operation = operation.Operation.Name,
                        operationId = operation.Operation.Id.ToString(),
                        resource = Agent.Name,
                        priority = operation.Priority.Invoke(Agent.CurrentTime).ToString(),
                        IsProcessing = inProcessing.ToString(),
                        IsReady = operation.StartConditions.Satisfied.ToString()

                    });
                    operationStart = operationEnd;

                    if ((ops.Length - 1) == j && !inProcessing)
                    {
                        ganttTransformation.Add(new GanttChartItem
                        {
                            article = bucket.Job.Name,
                            articleId = bucket.Key.ToString(),
                            start = operationStart.ToString(),
                            end = bucket.ScopeConfirmation.GetScopeEnd().ToString(), // bucket.ScopeConfirmation.GetScopeEnd().ToString(),
                            groupId = bucket.Key.ToString(),
                            operation = "Empty Bucket Space from " + bucket.Job.Name,
                            operationId = bucket.Key.ToString(),
                            resource = Agent.Name,
                            priority = bucket.Job.Priority(Agent.CurrentTime).ToString(),
                            IsProcessing = inProcessing.ToString(),
                            IsReady = "false"
                        });
                    }
                }
            }

            return ganttTransformation;
        }
        #endregion

        #region Processing

        internal void UpdateProcessingItem()
        {
            var job = _scopeQueue.GetFirstIfSatisfied(currentTime: Agent.CurrentTime, _capabilityProviderManager.GetCurrentUsedCapability());


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

            var insideJob = _jobInProgress.IsSet ? $" | Job in Progress {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} " +
                                                         $"at work {_jobInProgress.IsWorking}  " +
                                                         $"scope start {_jobInProgress.Current.ScopeConfirmation.GetScopeStart()} " +
                                                         $"prio {_jobInProgress.Current.Job.Priority(Agent.CurrentTime)} | " 
                                                       : $" | No job in Progress set |";
            var allPrios = "";
            _scopeQueue.GetAllJobs().ForEach(x => allPrios += $"  | Bucket {x.Job.Name} | Prio: {x.Job.Priority(Agent.CurrentTime)} " +
                                                              $"| Is Ready: {((FBucket)x.Job).HasSatisfiedJob } " +
                                                              $"| FirstOperation {((FBucket)x.Job).Operations.First().Operation.Name} | ScopeStart {x.ScopeConfirmation.GetScopeStart()}");

            if (_jobInProgress.IsSet)
            {
                Agent.DebugMessage(msg: $"Invalid start conditions. To Start processing | " + insideJob + " | Remaining in Queue " + allPrios, CustomLogger.JOB, LogLevel.Warn);
                return;
            }
            var isQueueAble = (job != null) ? $" To Progress {job.Job.Name} {job.Job.Key} scope start {job.ScopeConfirmation.GetScopeStart()} prio {job.Job.Priority(Agent.CurrentTime)}" : " not satisfied";
            

            Agent.DebugMessage(msg: $"Try to update processing item from scope queue with {_scopeQueue.Count} bucket. " + insideJob + isQueueAble +
                                    $" | Job is in progress: {_jobInProgress.IsSet}" + allPrios, CustomLogger.JOB, LogLevel.Warn);

            // take the next scope and make it fix 
            if (!_jobInProgress.IsSet && job.IsNotNull())
            {   
                _scopeQueue.RemoveJob(job);
                var setupDuration = 0L;
                if (job.ScopeConfirmation.GetSetup().IsNotNull())
                {
                    setupDuration = job.ScopeConfirmation.GetSetup().End - job.ScopeConfirmation.GetSetup().Start;
                }

                if (Agent.Name.Contains("Operator"))
                {
                    _jobInProgress.Set(job, Agent.CurrentTime + setupDuration);
                } else {
                    _jobInProgress.Set(job, Agent.CurrentTime + setupDuration + job.Duration);
                }


                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Job.Name} {job.Job.Key} with satisfied:" +
                                        $" {((FBucket)job.Job).HasSatisfiedJob} Try to start processing."
                                    , CustomLogger.JOB, LogLevel.Warn);
                
                // ToDo : test behaviour of this method.
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
            var setupDuration = _capabilityProviderManager.GetSetupDurationByCapability(_jobInProgress.Current.CapabilityProvider.ResourceCapabilityId);
            //Start setup 
            Agent.DebugMessage(msg:
                $"Call start Setup for Job {_jobInProgress.Current.Job.Name}  Key: {_jobInProgress.Current.Job.Key} " +
                $"Duration is {setupDuration} and start with Job at {Agent.CurrentTime + setupDuration}", CustomLogger.JOB, LogLevel.Warn);
            
            _capabilityProviderManager.Mount(_jobInProgress.Current.CapabilityProvider.Id);
            _jobInProgress.Start();

            Agent.Send(Resource.Instruction.BucketScope.FinishTask.Create("SETUP", Agent.Context.Self), waitFor: setupDuration);
        }

        internal void FinishSetup() // only in case it is not required for processing.
        {
            Agent.DebugMessage($"Finished Setup for {_jobInProgress.Current.Job.Name}, Resource is released to do next Task;", CustomLogger.JOB, LogLevel.Warn);
            NextTask(); // should only start setup if operations are fine 
            UpdateProcessingItem();
        }

        /// <summary>
        /// Starts the next Job
        /// </summary>
        internal void DoWork(FOperation operation)
        {
            Agent.DebugMessage($"Call start Work for {_jobInProgress.Current.Job.Name} {operation.Operation.Name} {operation.Key} to process for {operation.Operation.RandomizedDuration}", CustomLogger.JOB, LogLevel.Warn);

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
            Agent.DebugMessage(msg: $"Call finished work with {_jobInProgress.Current.Job.Name} {_jobInProgress.Current.Job.Key} take next...", CustomLogger.JOB, LogLevel.Warn);
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
                    || (_scopeQueue.HasQueueAbleJobs() && !isReady)
                    || ( next.ScopeConfirmation.GetScopeStart() > blockUntil && isReady) )
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
                RequeueJobs(_scopeQueue.GetAllJobs());
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
