using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.JobAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using Microsoft.FSharp.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using Akka.Util.Internal;
using static FJobConfirmations;
using static FJobResourceConfirmations;
using static FProcessingSlots;
using static FScopeConfirmations;
using static FSetupConfirmations;
using static FSetupSlots;
using static IConfirmations;
using static ITimeRanges;
using static IJobs;
using static FCreateSimulationResourceSetups;

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


        public override bool Action(object message)
        {
            switch (message)
            {
                case Job.Instruction.TerminateJob msg: Terminate(); break;
                case Job.Instruction.AcknowledgeJob msg: AcknowledgeJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.RequestSetupStart msg: RequestSetupStart(); break;
                case Job.Instruction.RequestProcessingStart msg: RequestProcessingStart(); break;
                case Job.Instruction.FinishSetup msg: FinishSetup(); break;
                case Job.Instruction.FinalBucket msg: DoWork(msg.GetObjectFromMessage); break;
                // case Job.Instruction.StartProcessing msg: StartProcessing(); break;
                // case Job.Instruction.AcceptAcknowledgeResponseFromResource msg: AcceptAcknowledgeResponseFromResource(); break;
                // case Job.Instruction.RejectAcknowledgeResponseFromResource msg: RejectAcknowledgeResponseFromResource(); break;
                // case Job.Instruction.StartRequeue msg: StartRequeue(); break;
                default: return false;
            }
            return true;
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
            {
                Agent.Send(Hub.Instruction.BucketScope.SetBucketFix.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
                _resourceSetupStates.ForEach(x =>
                {
                    Resource.Instruction.BucketScope.DoSetup.Create(_jobConfirmation.Key, x.Key);
                    x.Value.CurrentState = JobState.InProcess;
                });
                CreateSetupKpi();
            }
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

        private void FinishSetup()
        {
            
            var requestingResource = _resourceSetupStates.Single(x => x.Key == Agent.Sender);
            requestingResource.Value.CurrentState = JobState.Finish;
            TryToSetBucketFix();
        }
        private void TryToSetBucketFix()
        {
            if (_resourceProcessingStates.Values.All(x => x.CurrentState == JobState.Ready))
            {
                Agent.Send(Hub.Instruction.BucketScope.RequestFinalBucket.Create(_jobConfirmation.Job.Key, _jobConfirmation.Job.HubAgent));
            }
        }

        private void DoWork(FJobConfirmation jobConfirmation)
        {
            _jobConfirmation = jobConfirmation;
            _resourceProcessingStates.ForEach(x =>
            {
                Agent.Send(Resource.Instruction.Default.DoWork.Create(jobConfirmation.Job, x.Key));
                x.Value.CurrentState = JobState.InProcess;
            });
        }


        private void AcknowledgeJob(FJobResourceConfirmation fJobResourceConfirmation)
        {
            _jobConfirmation = fJobResourceConfirmation.JobConfirmation;
            _resourceProcessingStates.Clear();

            Agent.DebugMessage($"Acknowledge Proposal {GetJobKey()}");
            foreach (var resourceScope in fJobResourceConfirmation.ScopeConfirmations)
            {
                var resourceRef = resourceScope.Key;
                Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} on resource {resourceRef.Path.Name}" +
                                        $" with scope confirmation for {resourceScope.Value.GetScopeStart()} to {resourceScope.Value.GetScopeEnd()}"
                                            , CustomLogger.PROPOSAL, LogLevel.Warn);
                    Agent.Send(instruction: Resource.Instruction.Default.AcceptedProposals.Create(_jobConfirmation, target: resourceRef));
            }
        }

        public override bool AfterInit()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Name} is created!", CustomLogger.JOB, LogLevel.Warn);
            return base.AfterInit();
        }

        private void Terminate()
        {
            Agent.DebugMessage($"Bucket {_jobConfirmation.Job.Name} is terminated!", CustomLogger.JOB, LogLevel.Warn);
            this.Agent.TryToFinish();
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
    }
}
