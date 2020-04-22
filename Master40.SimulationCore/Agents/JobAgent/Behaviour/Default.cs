using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.JobAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        private FJobConfirmation _jobConfirmation { get; set; }
        private Guid GetJobKey() => _jobConfirmation.Job.Key;
        private Dictionary<IActorRef, StateHandle> _resourceProcessingStates { get; set; }
        private Dictionary<IActorRef, StateHandle> _resourceSetupStates { get; set; }
        private IEnumerator<FOperation> _currentOperation { get; set;}
        private long _processingStart { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Job.Instruction.TerminateJob msg: Terminate(); break;
                case Job.Instruction.AcknowledgeJob msg: AcknowledgeJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.RequestSetupStart msg: RequestSetupStart(); break;
                case Job.Instruction.RequestProcessingStart msg: RequestProcessingStart(); break;
                case Job.Instruction.FinishSetup msg: FinishSetup(); break;
                case Job.Instruction.FinishProcessing msg: FinishProcessing(); break;
                case Job.Instruction.FinalBucket msg: FinalizeBucket(msg.GetObjectFromMessage); break;
                // case Job.Instruction.StartProcessing msg: StartProcessing(); break;
                // case Job.Instruction.StartRequeue msg: StartRequeue(); break;
                default: return false;
            }
            return true;
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

            Agent.DebugMessage($"Acknowledge Proposal {GetJobKey()}");
            foreach (var resourceScope in fJobResourceConfirmation.ScopeConfirmations)
            {
                var resourceRef = resourceScope.Key;
                Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} on resource {resourceRef.Path.Name}" +
                                        $" with scope confirmation for {resourceScope.Value.GetScopeStart()} to {resourceScope.Value.GetScopeEnd()}"
                    , CustomLogger.PROPOSAL, LogLevel.Warn);
                Agent.Send(instruction: Resource.Instruction.Default.AcceptedProposals.Create(_jobConfirmation, target: resourceRef));


                foreach (var scope in resourceScope.Value.Scopes)
                {
                    switch (scope)
                    {
                        case FSetupSlot fs: _resourceSetupStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue)); break;
                        case FProcessingSlot ps: _resourceSetupStates.Add(resourceScope.Key, new StateHandle(JobState.InQueue)); break;
                            default: throw new Exception("Wrong state implementation send.");
                    }
                }
            }
        }

        /// <summary>
        /// Resource send message when resource want to start setup for a job - only setup resources are required for this step
        /// </summary>
        private void RequestSetupStart()
        {
            // FOr Setup or for Processing ? 

            var requestingResource = _resourceSetupStates.Single(x => x.Key == Agent.Sender);
            requestingResource.Value.CurrentState = JobState.Ready;

            if (_resourceSetupStates.Values.All(x => x.CurrentState == JobState.Ready)) 
            // indicates that all Items are in Processing Slot on each resource and therefore no message for requeue from resource can occour
            {
                Agent.Send(Hub.Instruction.BucketScope.SetBucketFix.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
            }
        }

        private void BucketIsFixed()
        {
            _resourceSetupStates.ForEach(x =>
            {
                Resource.Instruction.BucketScope.DoSetup.Create(_jobConfirmation.Key, x.Key);
                x.Value.CurrentState = JobState.InProcess;
            });
            CreateSetupKpi();
        }

        /// <summary>
        /// Called from every setup resource, when setup was finished
        /// </summary>
        private void FinishSetup()
        {

            var requestingResource = _resourceSetupStates.Single(x => x.Key.Equals(Agent.Sender));
            requestingResource.Value.CurrentState = JobState.Finish;
            TryToSetBucketFix();
        }

        /// <summary>
        /// Resource sent message as soon as the resource want to start with processing - only processing actors are required for this step
        /// </summary>
        private void RequestProcessingStart()
        {
            // trigger with JobState ?! --> parameter JobState entscheidet darüber, für welche Phasen angefragt werden soll? -- Konsistenz? 
            var requestingResource = _resourceProcessingStates.Single(x => x.Key.Equals(Agent.Sender));
            requestingResource.Value.CurrentState = JobState.Ready;

            TryToSetBucketFix();
        }

        private void FinishProcessing()
        {
            var requestingResource = _resourceProcessingStates.Single(x => x.Key == Agent.Sender);
            requestingResource.Value.CurrentState = JobState.Ready;

            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                CreateOperationResults();
                UpdateJobKpi();

                DoWork();

            }


        }

        private void CreateOperationResults()
        {
            var fOperationResult = new FOperationResult(key: _currentOperation.Current.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + _currentOperation.Current.Operation.RandomizedDuration
                , originalDuration: _currentOperation.Current.Operation.Duration
                , productionAgent: ActorRefs.Nobody
                , capabilityProvider: _jobConfirmation.CapabilityProvider.Name);

            Agent.Send(BasicInstruction.FinishJob.Create(fOperationResult, fOperationResult.ProductionAgent));
        }



        private void TryToSetBucketFix()
        {
            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.Send(Hub.Instruction.BucketScope.RequestFinalBucket.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
            }

        }

        private void FinalizeBucket(FJobConfirmation jobConfirmation)
        {

            _jobConfirmation = jobConfirmation;

            _processingStart = Agent.CurrentTime;
            DoWork();

        }

        /// <summary>
        /// For Work
        /// </summary>
        /// <param name="jobConfirmation"></param>
        private void DoWork()
        {

            _currentOperation = GetNextOperation();

            if (_currentOperation.Current == null)
            {
                // TODO Test if this works

                CreateBucketKpi();

                _resourceProcessingStates.ForEach(x =>
                {
                    Agent.Send(Resource.Instruction.BucketScope.FinishBucket.Create(x.Key));
                    x.Value.CurrentState = JobState.Finish;

                });
                
                Terminate();

                return;
            }

            Agent.DebugMessage(msg: $"Start withdraw for article {_currentOperation.Current.Operation.Name} {_currentOperation.Current.Key}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: _currentOperation.Current.Key, target: _currentOperation.Current.HubAgent));

            Agent.DebugMessage(msg: $"Starting Job {_currentOperation.Current.Operation.Name}  Key: {_currentOperation.Current.Key} new Duration is {_currentOperation.Current.Operation.RandomizedDuration} " +
                                    $"from bucket {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} with {((FBucket)_jobConfirmation.Job).Operations.Count} operations " +
                                    $"at resource {Agent.Context.Self.Path.Name}");

            _resourceProcessingStates.ForEach(x =>
            {
                Agent.Send(Resource.Instruction.Default.DoWork.Create( message: _currentOperation.Current
                                                                                , target: x.Key));
                x.Value.CurrentState = JobState.InProcess;

            });
        }

        private IEnumerator<FOperation> GetNextOperation ()
        {
            var operationEnumerator = ((FBucket)_jobConfirmation.Job).Operations.OrderByDescending(prio => prio.DueTime).GetEnumerator();
            while (operationEnumerator.MoveNext())
            {
                yield return operationEnumerator.Current;
            }
            operationEnumerator.Dispose();
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
                                    resource: Agent.Name,
                                    capabilityName: _jobConfirmation.Job.RequiredCapability.Name,
                                    setupId: _jobConfirmation.Job.RequiredCapability.Id);
            Agent.Context.System.EventStream.Publish(@event: pubSetup);
        }


        private void UpdateJobKpi()
        {

            var pub = new FUpdateSimulationJob(job: _currentOperation.Current
                , jobType: JobType.OPERATION
                , duration: _currentOperation.Current.Operation.RandomizedDuration
                , start: Agent.CurrentTime
                , resource: _jobConfirmation.CapabilityProvider.Name
                , bucket: _jobConfirmation.Job.Name
                , setupId: _jobConfirmation.CapabilityProvider.Id);
            Agent.Context.System.EventStream.Publish(@event: pub);

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

            Agent.Context.System.EventStream.Publish(@event: pub);
        }


    }
}
