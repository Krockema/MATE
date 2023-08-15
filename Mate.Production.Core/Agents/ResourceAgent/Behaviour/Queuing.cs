using System;
using System.Collections.Generic;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Agents.DirectoryAgent;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Agents.ResourceAgent.Types;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Queueing;
using Mate.Production.Core.Environment.Records.Reporting;

namespace Mate.Production.Core.Agents.ResourceAgent.Behaviour
{
    public class Queuing : Core.Types.Behaviour
    {
        internal string _resourceName { get; }
        internal int _resourceId { get; }
        internal ResourceType _resourceType { get; }
        internal QueueingJobRecord _currentJob { get; set; }
        internal CapabilityProviderManager _capabilityProviderManager { get; }
        public Queuing(int resourceId, ResourceType resourceType, List<M_ResourceCapabilityProvider> capabilityProvider, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType)
        {
            _resourceId = resourceId;
            _resourceType = resourceType;
            _capabilityProviderManager = new CapabilityProviderManager(capabilityProvider);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.Queuing.DoJob msg: StartJob(msg.GetObjectFromMessage); break;
                case Resource.Instruction.Queuing.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;
                
                default: return false;

            }
            return true;
        }
        public override bool AfterInit()
        {
            var resourceAgent = Agent as Resource;
            var capabilityProviders = _capabilityProviderManager.GetAllCapabilityProvider();
            Agent.Send(instruction: Directory.Instruction.Default.ForwardRegistrationToHub.Create(
                new ResourceInformationRecord(resourceAgent._resource.Id, Agent.Name, capabilityProviders, _resourceType, WorkTimeGenerator: null, String.Empty, Agent.Context.Self)
                , target: Agent.VirtualParent));
            return true;
        }

        public void StartJob(IQueueingJob fQueuingJob)
        {
            Agent.DebugMessage($"Start Setup for {fQueuingJob.JobName} to capability {fQueuingJob.CapabilityProvider.ResourceCapability.Name}");
            
            CreateTask(fQueuingJob);

            Agent.Send(Resource.Instruction.Queuing.FinishJob.Create(fQueuingJob, Agent.Context.Self), fQueuingJob.Duration);
        }
        public void FinishJob(IQueueingJob fQueuingJob)
        {
            Agent.DebugMessage($"Start {fQueuingJob.JobName} with Duration: {fQueuingJob.Duration}");
            
            //find first job to work on -- should be a queue 
            switch (fQueuingJob.JobType)
            {
                case JobType.SETUP: 
                    Agent.Send(Hub.Instruction.Queuing.FinishSetup.Create(fQueuingJob, fQueuingJob.Hub));
                    break;
                case JobType.OPERATION: 
                    Agent.Send(Hub.Instruction.Queuing.FinishWork.Create(fQueuingJob,fQueuingJob.Hub));
                    break;
                default:
                    throw new Exception($"Wrong JobType!");
            }
        }

        #region Reporting
        void CreateTask(IQueueingJob job)
        {
            var pub = new CreateTaskItemRecord(
                Type: job.JobType
                , Resource: Agent.Name.Replace("Resource(", "").Replace(")", "")
                , ResourceId: _resourceId
                , Start: Agent.CurrentTime
                , End: Agent.CurrentTime + job.Duration
                , Capability: job.CapabilityProvider.ResourceCapability.Name
                , Operation: job.JobType == JobType.SETUP ? "Setup for " + job.JobName : job.JobName
                , GroupId: Math.Abs(job.Key.GetHashCode()));

            //TODO NO tracking
            Agent.Context.System.EventStream.Publish(@event: pub);
        }

        #endregion

    }
}
