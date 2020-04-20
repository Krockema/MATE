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
using System.Linq;
using static FJobConfirmations;
using static FJobResourceConfirmations;
using static FProcessingSlots;
using static FScopeConfirmations;
using static FSetupConfirmations;
using static FSetupSlots;
using static IConfirmations;
using static ITimeRanges;

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
                case Job.Instruction.LockJob msg: LockJob(msg.GetObjectFromMessage); break;
                case Job.Instruction.RequestSetupStart msg: RequestSetupStart(); break;
                case Job.Instruction.RequestJobStart msg: RequestJobStart(); break;
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

            var requestingResource = _resourceStates.Single(x => x.Key == Agent.Sender);
            requestingResource.Value.CurrentState = JobState.InSetup;

            ///
            // Same handling as requestJobStart only processing required
            ///

        }

        /// <summary>
        /// Resource sent message as soon as the resource want to start with processing - only processing actors are required for this step
        /// </summary>
        private void RequestJobStart()
        {
            // trigger with JobState ?! --> parameter JobState entscheidet darüber, für welche Phasen angefragt werden soll? -- Konsistenz? 
            var requestingResource = _resourceStates.Single(x => x.Key == Agent.Sender);
            requestingResource.Value.CurrentState = JobState.InProcess;

            foreach(var setup in _jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.UsedInProcess == true))
            {
                var resourceRef = setup.Resource.IResourceRef as IActorRef;
                //  skip if resource has already called finished
                var resourceState = _resourceStates.Single(x => x.Key == resourceRef);
                if (resourceState.Value.Equals(JobState.InProcess))
                    continue;
                Agent.Send(Resource.Instruction.BucketScope.ForceJob.Create(GetJobKey(), resourceRef));
            }

            //dictionary als type mit methode allin Process --> send to all start --> unterscheidung setup und process?
            //Vielleicht nur doProcess? eigentlich müssen für das setup erstmal nur die setupresourcen zur Verfügung stehen
            // zum do process dann nur die operations resources

            if(_resourceStates.Count(x => x.Value.Equals(JobState.InProcess)) == _jobConfirmation.CapabilityProvider.ResourceSetups.Count(x => x.UsedInProcess == true))
            {
                foreach (var setup in _jobConfirmation.CapabilityProvider.ResourceSetups.Where(x => x.UsedInProcess == true))
                {
                    Agent.Send(Resource.Instruction.BucketScope.DoSetup.Create(GetJobKey(), setup.Resource.IResourceRef as IActorRef));
                }
            }
        }

        private void LockJob(FJobConfirmation jobConfirmation)
        {
            foreach (var setups in _jobConfirmation.CapabilityProvider.ResourceSetups)
            {
                var resourceRef = setups.Resource.IResourceRef as IActorRef;
                Agent.Send(Resource.Instruction.BucketScope.AcknowledgeJob.Create(jobConfirmation, resourceRef));
                var requestingResource = _resourceStates.Single(x => x.Key == Agent.Sender);
                requestingResource.Value.CurrentState = JobState.InProcessingQueue;
            }
            if (jobConfirmation.IsReset)
            {
                Terminate();
            }
        }

        private void AcknowledgeJob(FJobResourceConfirmation fJobResourceConfirmation)
        {
            _jobConfirmation = fJobResourceConfirmation.JobConfirmation;
            _resourceProcessingStates.Clear();

            Agent.DebugMessage($"Acknowledge Proposal {GetJobKey()}");
            var setupKey = Guid.NewGuid();
            foreach (var resourceScope in fJobResourceConfirmation.ScopeConfirmations)
            {
                foreach( var scope in resourceScope.Value.Scopes)
                {
                    var resourceRef = resourceScope.Key;
                    switch (scope)
                    {
                        case FProcessingSlot ps: 
                            _resourceProcessingStates.Add(resourceRef, new StateHandle(JobState.InQueue));
                            var pscopelist = new List<ITimeRange>() { ps };
                            var pscopeConfirmation = new FScopeConfirmation(ListModule.OfSeq(pscopelist));
                            //FJobConfirmation
                            var jobConfirmation= _jobConfirmation.UpdateScopeConfirmation(pscopeConfirmation);
                            Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal
                                        .Create(jobConfirmation // DANGER - TO TEST
                                            , target: resourceRef));
                            break;
                        case FSetupSlot fs: 
                            _resourceSetupStates.Add(resourceRef, new StateHandle(JobState.InQueue));
                            var scopelist = new List<ITimeRange>() { fs };
                            var scopeConfirmation = new FScopeConfirmation(ListModule.OfSeq(scopelist));
                            var setupConfirmation = Convert.ChangeType(value: _jobConfirmation.UpdateScopeConfirmationAndKey(scopeConfirmation, setupKey)
                                                                       ,conversionType: typeof(FSetupConfirmation));
                            Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal
                                        .Create((IConfirmation)setupConfirmation // DANGER - TO TEST
                                            , target: resourceRef));
                            break; 
                        default:
                            throw new Exception("Typemismatch on FScopeSlot");
                    }
                    Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {_jobConfirmation.Job.Name} {_jobConfirmation.Job.Key} on resource {resourceRef.Path.Name}" +
                                            $" with scope confirmation for {resourceScope.Value.GetScopeStart()} to {resourceScope.Value.GetScopeEnd()}"
                                            , CustomLogger.PROPOSAL, LogLevel.Warn);
                }
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
    }
}
