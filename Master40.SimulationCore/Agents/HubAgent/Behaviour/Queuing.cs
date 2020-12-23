using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Text;
using Akka.Actor;
using Akka.Util.Internal;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.HubAgent.Types.Queuing;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using NLog;
using static FOperations;
using static FQueuingJobs;
using static FQueuingSetups;
using static FResourceInformations;
using static FUpdateStartConditions;
using static IJobs;
using static IQueueingJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Queuing : SimulationCore.Types.Behaviour
    {

        internal ResourceManager _resourceManager { get; } = new ResourceManager();
        internal CapabilityManager _capabilityManager { get; set; } = new CapabilityManager();
        private JobManager _jobManager { get; } = new JobManager();
        private WorkTimeGenerator _workTimeGenerator { get; }

        public Queuing(long maxBucketSize, WorkTimeGenerator workTimeGenerator, SimulationType simulationType = SimulationType.Queuing) : base(childMaker: null, simulationType: simulationType)
        {
            _workTimeGenerator = workTimeGenerator;
        }

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                //Initialize
                case Hub.Instruction.Default.AddResourceToHub msg: AddResourceToHub(resourceInformation: msg.GetObjectFromMessage); break;

                //Jobs
                case Hub.Instruction.Default.EnqueueJob msg: EnqueueJob(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;
                case Hub.Instruction.Queuing.FinishSetup msg: FinishSetup(msg.GetObjectFromMessage); break;
                case Hub.Instruction.Queuing.FinishWork msg: FinishWork(msg.GetObjectFromMessage); break;


                default: return false;
            }
            return success;
        }

        internal void AddResourceToHub(FResourceInformation resourceInformation)
        {
            _resourceManager.Add(
                resourceId: resourceInformation.ResourceId,
                resourceName: resourceInformation.ResourceName,
                resourceRef: resourceInformation.Ref,
                resourceType: resourceInformation.ResourceType,
                resourceCapability: resourceInformation.ResourceCapabilityProvider.FirstOrDefault().ResourceCapability.ParentResourceCapabilityId.Value,
                resourceCapabilityName: resourceInformation.RequiredFor,
                resourceCapabilities: resourceInformation.ResourceCapabilityProvider.Select(x => x.ResourceCapabilityId).ToList());
            
            foreach (var capabilityProvider in resourceInformation.ResourceCapabilityProvider)
            {
                var capabilityDefinition = _capabilityManager.GetCapabilityDefinition(capabilityProvider.ResourceCapability);

                capabilityDefinition.AddResourceRef(resourceId: resourceInformation.ResourceId, resourceRef: resourceInformation.Ref);

                System.Diagnostics.Debug.WriteLine($"Create capability provider at {Agent.Name}" +
                                                   $" with capability provider {capabilityProvider.Name} " +
                                                   $" from {Agent.Context.Sender.Path.Name}" +
                                                   $" with capability {capabilityDefinition.ResourceCapability.Name}", CustomLogger.INITIALIZE, LogLevel.Warn);

            }

            Agent.DebugMessage(msg: "Added Resource Agent " + resourceInformation.Ref.Path.Name + " to Resource Pool: " + resourceInformation.RequiredFor, CustomLogger.INITIALIZE, LogLevel.Warn);
        }

        private void EnqueueJob(IJob job)
        {
            var operation = (FOperation)job;

            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} {operation.Key}" +
                                    $"| with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}" +
                                    $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {operation.StartConditions.PreCondition}", CustomLogger.JOB, LogLevel.Warn);

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            _jobManager.SetJob(job, Agent.CurrentTime);

            StartNext();
        }

        private void StartNext()
        {
            var availableCapabilities = _resourceManager.GetAvailableCapabilities();

            if (availableCapabilities == null || availableCapabilities.Count == 0)
                return;

            foreach (var queue in _jobManager.GetAllJobQueues(Agent.CurrentTime, availableCapabilities))
            {
                TryDoWork(queue);
            }

        }

        private void TryDoWork(JobQueue jobQueue)
        {
            var capability = jobQueue.PeekNext(Agent.CurrentTime).RequiredCapability;

            var capabilityProviders = _capabilityManager.GetAllCapabilityProvider(capability);

            foreach (var capabilityProvider in capabilityProviders)
            {
                var requiredResources = capabilityProvider.ResourceSetups.Where(x=> x.Resource.IsPhysical).Select(x => x.Resource).ToList();
                if (_resourceManager.ResouresAreWorking(requiredResources)) 
                    continue;

                var key = _jobManager.AddActiveJob(jobQueue, capabilityProvider);
                _jobManager.Remove(jobQueue);
                StartWork(key);
                return;
            }

        }

        private void StartWork(Guid queueKey)
        {
            var jobQueue = _jobManager.GetActiveJob(queueKey);

            Agent.DebugMessage($"JobQueue {jobQueue.QueueId} with {jobQueue.JobQueue.Count} jobs require {jobQueue.ResourceCapabilityProvider.Name} will be started on capability provider: {jobQueue.ResourceCapabilityProvider.Name}", CustomLogger.JOB, LogLevel.Debug);
            //Set JobQueue to each resource to block them

            var mainResources = jobQueue.GetOnlyMainResources.Select(x => x.Id).ToList();
            var resourceStatesMainResources = _resourceManager.GetResourceStates(mainResources);

            var nextJob = jobQueue.Peek(Agent.CurrentTime);
            //Check if Setup
            if (resourceStatesMainResources.Any(x => x._currentResourceCapability == null || !resourceStatesMainResources.TrueForAll(x => x._currentResourceCapability.Id.Equals(jobQueue.ResourceCapabilityProvider.ResourceCapabilityId))))
            {
                _resourceManager.SetJobQueue(nextJob, jobQueue.GetAllResources);
                DoSetup(queueKey);
                return;
            }

            _resourceManager.SetJobQueue(nextJob, jobQueue.GetProcessingResources);
            Agent.DebugMessage($"JobQueue {jobQueue.QueueId} skip setup, as all resource are already set up to {jobQueue.ResourceCapabilityProvider.ResourceCapability.Name}", CustomLogger.JOB, LogLevel.Debug);
            //Else start production
            DoWork(queueKey, nextJob);

        }

        private void DoSetup(Guid queueKey)
        {
            var jobQueue = _jobManager.GetActiveJob(queueKey);

            var nextJob = jobQueue.Peek(Agent.CurrentTime);

            Agent.DebugMessage($"Start setup for {jobQueue.QueueId} require capability {jobQueue.ResourceCapabilityProvider.Name}", CustomLogger.JOB, LogLevel.Debug);
            var setupResources = jobQueue.GetSetupResources;
            var setupTime = jobQueue.GetSetupTime();

            foreach (var resource in setupResources)
            {
                jobQueue.AddSetup(resource.Id);

                var fQueuingSetup = new FQueuingSetup(
                    key: jobQueue.QueueId,
                    resourceId: resource.Id,
                    job: nextJob,
                    jobKey: nextJob.Key,
                    jobName: nextJob.Name,
                    duration: setupTime,
                    hub: Agent.Context.Self,
                    capabilityProvider: jobQueue.ResourceCapabilityProvider,
                    jobType: JobType.SETUP
                );

                Agent.DebugMessage($"Request {resource.Name} to setup {jobQueue.ResourceCapabilityProvider.ResourceCapability.Name}", CustomLogger.JOB, LogLevel.Debug);

                Agent.Send(Resource.Instruction.Queuing.DoJob.Create(fQueuingSetup, (IActorRef)resource.IResourceRef));

            }

        }

        private void FinishSetup(IQueueingJob queuingSetup)
        {
            var jobQueue = _jobManager.GetActiveJob(queuingSetup.Key);
            var resourceId = queuingSetup.ResourceId;

            jobQueue.FinishSetup(resourceId);

            //if its a main resource setup tool
            var mainResources = jobQueue.GetOnlyMainResources.Select(x => x.Id).ToList();
            if (mainResources.Contains(resourceId))
            {
                _resourceManager.SetSetup(resourceId, queuingSetup.CapabilityProvider.ResourceCapability);
            }

            if(!jobQueue.IsSetupFinish)
            {
                return;
            }
            
            Agent.DebugMessage($"All resources for jobQueue {jobQueue.QueueId} finished setup");
            //Free all not processing resources 
            foreach (var resource in jobQueue.GetOnlySetupResources)
            {
                _resourceManager.ResetJob(resource.Id);
            }
            StartNext();

            var nextJob = jobQueue.Peek(Agent.CurrentTime);
            DoWork(jobQueue.QueueId, nextJob);

        }


        private void DoWork(Guid queueKey, IJob job)
        {
            var jobQueue = _jobManager.GetActiveJob(queueKey);

            var nextJob = jobQueue.Dequeue(Agent.CurrentTime);
            
            var randomizedDuration = _workTimeGenerator.GetRandomWorkTime(nextJob.Duration);

            Agent.DebugMessage($"Start DoWork for {jobQueue.QueueId} with job {nextJob.Name} and randomized duration of {randomizedDuration}");

            foreach (var resource in jobQueue.GetProcessingResources)
            {
                _resourceManager.SetJob(resourceId: resource.Id, job: nextJob);

                jobQueue.AddProcessing(resource.Id);

                var fQueuingJob = new FQueuingJob(
                    key: jobQueue.QueueId,
                    resourceId: resource.Id,
                    job: nextJob,
                    jobKey: nextJob.Key,
                    jobName: nextJob.Name,
                    duration: randomizedDuration,
                    hub: Agent.Context.Self,
                    capabilityProvider: jobQueue.ResourceCapabilityProvider,
                    jobType: JobType.OPERATION
                    );

                Agent.Send(Resource.Instruction.Queuing.DoJob.Create(fQueuingJob, (IActorRef)resource.IResourceRef));
            }
        }

        private void FinishWork(IQueueingJob queuingJob)
        {
            var jobQueue = _jobManager.GetActiveJob(queuingJob.Key);

            var res = _resourceManager._resourceList.Single(x => x._resourceId.Equals(queuingJob.ResourceId));
            Agent.DebugMessage($"Resource {res._resourceName} finished work with {queuingJob.JobName}");

            jobQueue.FinishProcessing(queuingJob.ResourceId);

            if (!jobQueue.IsProcessingFinish)
            {
                return;
            }

            CreateOperationResults(queuingJob);

            jobQueue.ClearProcessing();

            if(jobQueue.JobQueue.Count > 0)
            {
                var nextJob = jobQueue.JobQueue.PeekNext(Agent.CurrentTime);
                Agent.DebugMessage($"Queue {jobQueue.QueueId} with {jobQueue.JobQueue.Count} jobs left will start with next job {nextJob.Name}");
                DoWork(jobQueue.QueueId, nextJob);
                return;
            }

            foreach (var resource in jobQueue.GetProcessingResources)
            {
                _resourceManager.ResetJob(resource.Id);

            }

            _jobManager.RemoveActiveJob(jobQueue.QueueId);

            StartNext();

        }

        internal void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            Agent.DebugMessage(msg: $"Received: Update and forward start condition for {startCondition.OperationKey}" +
                                    $"| ArticleProvided: {startCondition.ArticlesProvided} " +
                                    $"| PreCondition: {startCondition.PreCondition} ");

            var job = _jobManager.GetJob(startCondition.OperationKey) as FOperation;

            if (job == null)
            {
                Agent.DebugMessage($"Job does not exit anymore");
                return;
            }

            job.SetStartConditions(startCondition.PreCondition, startCondition.ArticlesProvided);

            _jobManager.SetJob(job, Agent.CurrentTime);
            if (job.StartConditions.Satisfied)
            {
                StartNext();
            }
        }

        private void CreateOperationResults(IQueueingJob fQueuingJob)
        {
            var job = fQueuingJob.Job as FOperation;

            ResultStreamFactory.PublishJob(agent: Agent
                , job: job
                , duration: fQueuingJob.Duration
                , capabilityProvider: fQueuingJob.CapabilityProvider);

            var fOperationResult = new FOperationResults.FOperationResult(key: job.Key
                , creationTime: 0
                , start: Agent.CurrentTime
                , end: Agent.CurrentTime + fQueuingJob.Duration
                , originalDuration: job.Operation.Duration
                , productionAgent: job.ProductionAgent
                , capabilityProvider: fQueuingJob.CapabilityProvider.Name);

            Agent.Send(BasicInstruction.FinishJob.Create(fOperationResult, job.ProductionAgent));
        }
    }
    
}
