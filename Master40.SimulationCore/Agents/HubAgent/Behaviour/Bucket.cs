using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using System;
using System.Linq;
using static FBuckets;
using static FOperations;
using static FProposals;
using static FRequestToRequeues;
using static FResourceInformations;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;
using ResourceManager = Master40.SimulationCore.Agents.HubAgent.Types.ResourceManager;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Bucket : SimulationCore.Types.Behaviour
    {
        internal Bucket(SimulationType simulationType = SimulationType.DefaultSetup)
                        : base(childMaker: null, simulationType: simulationType) { }


        internal ResourceManager _resourceManager { get; set; } = new ResourceManager();
        internal BucketManager _bucketManager { get; } = new BucketManager();
        internal PendingOperationDictionary _pendingOperationDictionary { get; } = new PendingOperationDictionary();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.Default.AddResourceToHub msg: AddResourceToHub(hubInformation: msg.GetObjectFromMessage); break;
                case Hub.Instruction.Default.EnqueueJob msg: AssignJob(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.ResponseRequeueBucket msg: ResponseRequeueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage as FBucket); break;
                case Hub.Instruction.Default.ProposalFromResource msg: ProposalFromResource(fProposal: msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;
                case Hub.Instruction.BucketScope.SetJobFix msg: SetBucketFix(msg.GetObjectFromMessage as FBucket); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(breakDown: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// source: production agent
        /// target: 
        /// </summary>
        /// <param name="fOperation"></param>
        private void AssignJob(IJob job)
        {
            var operation = (FOperation) job;

            System.Diagnostics.Debug.WriteLine($"Enqueue Operation {operation.Operation.Name} {operation.Key} ");
            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {operation.Operation.Name} | with start condition: {operation.StartConditions.Satisfied} with Id: {operation.Key}");

            operation.UpdateHubAgent(hub: Agent.Context.Self);

            FindOrCreateBucket(fOperation: operation);
        }

        private void FindOrCreateBucket(FOperation fOperation)
        {
            var bucket = _bucketManager.AddOrCreateBucket(fOperation, Agent.Context.Self, Agent.CurrentTime);

            //create new bucket
            if (bucket == null)
            {
                bucket = _bucketManager.CreateBucket(fOperation, Agent.Context.Self, Agent.CurrentTime);
                bucket.UpdateHubAgent(hub: Agent.Context.Self);

                Agent.DebugMessage(msg: $"Create bucket {bucket.Name} for operation {fOperation.Key}");
                System.Diagnostics.Debug.WriteLine(
                    $"Create bucket {bucket.Name} for operation {fOperation.Operation.Name} {fOperation.Key} ");

                EnqueueBucket(bucket);
                return;
            }

            //if bucket not fix and bucket is on any ResourceQueue - ask to requeue with operation
            var bucketToQueue = new FRequestToRequeue(jobKey: bucket.Key, approved: false);

            if (bucket.ResourceAgent != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Request requeue bucket {bucket.Name} for operation {fOperation.Operation.Name} {fOperation.Key} ");

                _pendingOperationDictionary.AddOperation(bucketKey: bucket.Key, fOperation: fOperation);

                Agent.Send(Resource.Instruction.Default.RequestToRequeue.Create(message: bucketToQueue, target: bucket.ResourceAgent));
                return;
            }

            bucket = bucket.AddOperation(op: fOperation);
            _bucketManager.Replace(bucket: bucket);
            EnqueueBucket(fBucket: bucket);
        }

        private void ResponseRequeueBucket(FRequestToRequeue fRequestToRequeue)
        {
            var bucket = _bucketManager.GetBucketById(fRequestToRequeue.JobKey);
            var operationsToBucket = _pendingOperationDictionary.GetAllOperationsForBucket(fRequestToRequeue.JobKey);

            if (fRequestToRequeue.Approved)
            {
                foreach (var job in operationsToBucket)
                {
                    bucket.AddOperation(job.fOperation);
                    _pendingOperationDictionary.Remove(job);
                }
                _bucketManager.Replace(bucket);
                EnqueueBucket(bucket);
                return;
            }

            foreach (var job in operationsToBucket)
            {
                //FindBucket

                //createNewBucket
            }

            //bucket = _bucketManager.CreateBucket(fOperation, Agent.CurrentTime);
            bucket.UpdateHubAgent(hub: Agent.Context.Self);
            EnqueueBucket(bucket);
            
        }

        private void EnqueueBucket(FBucket fBucket)
        {
            var bucket = _bucketManager.GetBucketById(fBucket.Key);
            System.Diagnostics.Debug.WriteLine($"Enqueue {bucket.Name} {bucket.Key} with {bucket.Operations.Count}"); 
            Agent.DebugMessage(msg: $"Got Bucket to Requeue: {bucket.Name} {bucket.Key} with {bucket.Operations.Count} operations | with start condition: {bucket.StartConditions.Satisfied} with Id: {bucket.Key}");
            bucket.Proposals.Clear();

            var resourceToRequest = _resourceManager.GetResourceByTool(bucket.Tool);

            foreach (var actorRef in resourceToRequest)
            {
                System.Diagnostics.Debug.WriteLine($"Ask for proposal {bucket.Name} {bucket.Key} at resource {actorRef.Path.Name}");
                Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name}");
                Agent.Send(instruction: Resource.Instruction.Default.RequestProposal.Create(message: bucket, target: actorRef));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proposal"></param>
        private void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var bucket = _bucketManager.GetBucketById(fProposal.JobKey);
            bucket.Proposals.RemoveAll(x => x.ResourceAgent.Equals(fProposal.ResourceAgent));
            // add New Proposal
            bucket.Proposals.Add(item: fProposal);

            System.Diagnostics.Debug.WriteLine($"Proposal for {bucket.Name} with Schedule: {fProposal.PossibleSchedule} Id: {fProposal.JobKey} from: {fProposal.ResourceAgent.Path.Name}!");
            Agent.DebugMessage(msg: $"Proposal for {bucket.Name} with Schedule: {fProposal.PossibleSchedule} Id: {fProposal.JobKey} from: {fProposal.ResourceAgent}!");


            // if all Machines Answered
            if (bucket.Proposals.Count == _resourceManager.GetResourceByTool(bucket.Tool).Count)
            {
                System.Diagnostics.Debug.WriteLine($"All Proposals for {bucket.Name} {bucket.Key} at {Agent.Context.Self.Path.Name} returned");
                // item Postponed by All Machines ? -> requeue after given amount of time.
                if (bucket.Proposals.TrueForAll(match: x => x.Postponed.IsPostponed))
                {
                    var postPonedFor = bucket.Proposals.Min(x => x.Postponed.Offset);
                    System.Diagnostics.Debug.WriteLine($"{bucket.Name} {bucket.Key} postponed to {postPonedFor}");
                    Agent.DebugMessage(msg: $"{bucket.Name} {bucket.Key} postponed to {postPonedFor}");
                    // Call Hub Agent to Requeue
                    bucket = bucket.UpdateResourceAgent(r: ActorRefs.NoSender);
                    _bucketManager.Replace(bucket);
                    Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: bucket, target: Agent.Context.Self), waitFor: postPonedFor);
                    return;
                }


                // acknowledge Machine -> therefore get Machine -> send acknowledgement
                var earliestPossibleStart = bucket.Proposals.Where(predicate: y => y.Postponed.IsPostponed == false)
                                                               .Min(selector: p => p.PossibleSchedule);

                var acknowledgement = bucket.Proposals.First(predicate: x => x.PossibleSchedule == earliestPossibleStart
                                                                        && x.Postponed.IsPostponed == false);

                bucket = ((IJob)bucket).UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent) as FBucket;

                System.Diagnostics.Debug.WriteLine($"Start AcknowledgeProposal for {bucket.Name} {bucket.Key} at {bucket.ResourceAgent.Path.Name}");
                Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {bucket.Name} {bucket.Key} on resource {acknowledgement.ResourceAgent}");

                // set Proposal Start for Machine to Requeue if time slot is closed.
                _bucketManager.Replace(bucket);
                Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal.Create(message: bucket, target: acknowledgement.ResourceAgent));
            }
        }

        private void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            System.Diagnostics.Debug.WriteLine($"Try to update operation {startCondition.OperationKey} | ArticleProvided: {startCondition.ArticlesProvided} | PreCondition: {startCondition.PreCondition}");
            var operation = _bucketManager.GetOperationByKey(startCondition.OperationKey);
            operation.SetStartConditions(startCondition: startCondition);

            var bucket = _bucketManager.GetBucketByOperationKey(startCondition.OperationKey);
            
            // if any Operation is ready in bucket, set bucket ready
            if (!bucket.Operations.Any(x => x.StartConditions.Satisfied) || bucket.StartConditions.Satisfied)
                return;

            var bucketStartCondition = new FUpdateStartCondition(bucket.Key, true, true);
            bucket.SetStartConditions(bucketStartCondition);
            System.Diagnostics.Debug.WriteLine($"Bucket startConditions true");

            // if Agent has no ResourceAgent the operation is not queued so here is nothing to do
            if (bucket.ResourceAgent.IsNobody())
            {
                EnqueueBucket(bucket);
                return;
            }

            Agent.DebugMessage(msg: $"Update and forward start condition: {operation.Operation.Name} {operation.Key}" +
                                    $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {operation.StartConditions.PreCondition} " +
                                    $"to resource {operation.ResourceAgent}");

            Agent.Send(instruction: Resource.Instruction.BucketScope.BucketReady.Create(message: bucket.Key, target: bucket.ResourceAgent));
            System.Diagnostics.Debug.WriteLine($"UpdateStartConditions for bucket {bucket.Key} was sent");
        }

        /// <summary>
        /// source: resource want to set a bucket fix, because its going to be added to processingQueue
        /// do: remove and requeue unsatisfied jobs
        /// target: send updatedBucket back to resource
        /// </summary>
        /// <param name="fBucket"></param>
        private void SetBucketFix(FBucket fBucket)
        {
            var bucket = _bucketManager.GetBucketById(fBucket.Key);

            if (bucket.IsFixPlanned)
                throw new Exception($"Bucket was set fix twice");

            bucket = bucket.SetFixPlanned;
            var operationsNotSatisfied = bucket.Operations.Where(x => x.StartConditions.Satisfied != true);
            foreach (var operation in operationsNotSatisfied)
            {
                _bucketManager.Remove(operation.Key);
                AssignJob(operation);
            }
            _bucketManager.Replace(bucket);

            System.Diagnostics.Debug.WriteLine($"{operationsNotSatisfied.Count()} operations form {fBucket.Name} have been requeued");

            //Feedback to ResourceAgent with bucket only ready operations
            Agent.Send(Resource.Instruction.BucketScope.EnqueueProcessingQueue.Create(message: bucket, target: bucket.ResourceAgent));
        }

        /// <summary>
        /// Source: ResourceAgent put operation onto processingQueue and will work on it soon
        /// Target: Method should forward message to the associated production agent
        /// </summary>
        /// <param name="key"></param>
        public void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _bucketManager.GetOperationByKey(operationKey);
            System.Diagnostics.Debug.WriteLine($"WithdrawRequiredArticles for operation {operationKey} was sent from {Agent.Sender.Path.Name}");
            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
                                                    .Create(message: operation.Key
                                                           , target: operation.ProductionAgent));
        }

        public void FinishJob(IJobResult jobResult)
        {
            var bucket = _bucketManager.GetBucketById(jobResult.Key);
            System.Diagnostics.Debug.WriteLine($"Resource called Item  {bucket.Name}  {jobResult.Key} finished");
            Agent.DebugMessage(msg: $"Resource called Item {bucket.Name} {jobResult.Key} finished.");
            foreach (var op in bucket.Operations)
            {
                Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: op.ProductionAgent));
                _bucketManager.Remove(op.Key);
            }
            
        }

        private void AddResourceToHub(FResourceInformation hubInformation)
        {
            var resourceSetup = new ResourceSetup(hubInformation.ResourceSetups, hubInformation.Ref, hubInformation.RequiredFor);
            _resourceManager.Add(resourceSetup);
            // Added Machine Agent To Machine Pool
            Agent.DebugMessage(msg: "Added Machine Agent " + hubInformation.Ref.Path.Name + " to Machine Pool: " + hubInformation.RequiredFor);
        }

        /*
         //TODO not working at the moment - implement and change to _resourceManager
        private void ResourceBreakDown(FBreakDown breakDown)
        {
            var brockenMachine = _resourceAgents.Single(predicate: x => breakDown.Resource == x.Value).Key;
            _resourceAgents.Remove(key: brockenMachine);
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: brockenMachine, logThis: true), waitFor: 45);

            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Hub");
        }
        */
    }
}
