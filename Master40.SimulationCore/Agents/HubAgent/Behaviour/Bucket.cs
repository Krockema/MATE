using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using System;
using System.Linq;
using static FBuckets;
using static FBucketToRequeues;
using static FOperations;
using static FProposals;
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

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueJob msg: EnqueueJob(fOperation: msg.GetObjectFromMessage as FOperation); break;
                case Hub.Instruction.RequeueBucket msg: RequeueBucket(msg.GetObjectFromMessage); break;
                case Hub.Instruction.EnqueueBucket msg: EnqueueBucket(msg.GetObjectFromMessage as FBucket); break;
                case Hub.Instruction.SetJobFix msg: SetJobFix(msg.GetObjectFromMessage as FBucket); break;
                case Hub.Instruction.ProposalFromResource msg: ProposalFromResource(fProposal: msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                case Hub.Instruction.AddResourceToHub msg: AddResourceToHub(hubInformation: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(breakDown: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Cant be changed anymore
        /// </summary>
        /// <param name="fBucket"></param>
        private void SetJobFix(FBucket fBucket)
        {
            var bucket = _bucketManager.GetBucketById(fBucket.Key);
            if(bucket.IsFixPlanned)
                return;

            bucket = bucket.SetFixPlanned;
            var operationsNotSatisfied = bucket.Operations.Where(x => x.StartConditions.Satisfied != true);
            foreach (var operation in operationsNotSatisfied)
            {
                _bucketManager.Remove(operation.Key);
                EnqueueJob(operation);
            }
            _bucketManager.Replace(bucket);

            System.Diagnostics.Debug.WriteLine($"{operationsNotSatisfied.Count()} operations form {fBucket.Name} have been requeued");

            //Feedback to ResourceAgent with bucket only ready operations
            Agent.Send(Resource.Instruction.EnqueueProcessingQueue.Create(message: bucket, target: bucket.ResourceAgent));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fOperation"></param>
        private void EnqueueJob(FOperation fOperation)
        {
            System.Diagnostics.Debug.WriteLine($"Enqueue Job {fOperation.Operation.Name} {fOperation.Key} ");
            Agent.DebugMessage(msg: $"Got New Item to Enqueue: {fOperation.Operation.Name} | with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}");

            //Operation already exits
            if (_bucketManager.GetOperationByKey(fOperation.Key) != null)
            {
                _bucketManager.Remove(fOperation.Key);
            }

            fOperation.UpdateHubAgent(hub: Agent.Context.Self);

            var bucket = _bucketManager.FindBucket(fOperation, Agent.CurrentTime);

            if (bucket == null)
            {
                bucket = _bucketManager.CreateBucket(fOperation, Agent.CurrentTime);
                bucket.UpdateHubAgent(hub: Agent.Context.Self);

                Agent.DebugMessage(msg: $"Create bucket {bucket.Name} for operation {fOperation.Key}");
                System.Diagnostics.Debug.WriteLine($"Create bucket {bucket.Name} for operation {fOperation.Operation.Name} {fOperation.Key} ");

                EnqueueBucket(bucket);
                return;
            }

            //if bucket not fix and bucket is on any ResourceQueue - ask to requeue with operation
            var bucketToRequeue = new FBucketToRequeue(bucket, fOperation);
            
            if (bucket.ResourceAgent == null)
            {
                RequeueBucket(bucketToRequeue);
                System.Diagnostics.Debug.WriteLine($"Start requeue bucket {bucket.Name} for operation {fOperation.Operation.Name} {fOperation.Key} ");

                Agent.DebugMessage(msg: $"Start requeue bucket {bucket.Name} for operation {fOperation.Key}");
                return;
            }
            //else
            Agent.DebugMessage(msg: $"Ask at resource {bucket.ResourceAgent} to requeue {bucket.Name}");
            Agent.Send(Resource.Instruction.AskRequeueBucket.Create(message: bucketToRequeue, target: bucket.ResourceAgent));

        }

        private void RequeueBucket(FBucketToRequeue bucketToRequeue)
        {
            var bucket = _bucketManager.GetBucketById(bucketToRequeue.Bucket.Key);

            bucket = bucket.UpdateResourceAgent(r: ActorRefs.NoSender);
            bucket = bucket.AddOperation(bucketToRequeue.Operation);
            _bucketManager.Replace(bucket);
            System.Diagnostics.Debug.WriteLine($"Start requeue bucket {bucket.Name} has {bucket.Operations.Count} operations with operation {bucketToRequeue.Operation.Operation.Name} {bucketToRequeue.Operation.Key}  ");

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
                Agent.Send(instruction: Resource.Instruction.RequestProposal.Create(message: bucket, target: actorRef));
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

                // item Postponed by All Machines ? -> requeue after given amount of time.
                if (bucket.Proposals.TrueForAll(match: x => x.Postponed.IsPostponed))
                {
                    var postPonedFor = bucket.Proposals.Min(x => x.Postponed.Offset);
                    Agent.DebugMessage(msg: $"{bucket.Name} {bucket.Key} postponed to {postPonedFor}");
                    // Call Hub Agent to Requeue
                    bucket = bucket.UpdateResourceAgent(r: ActorRefs.NoSender);
                    _bucketManager.Replace(bucket);
                    Agent.Send(instruction: Hub.Instruction.EnqueueJob.Create(message: bucket, target: Agent.Context.Self), waitFor: postPonedFor);
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
                Agent.Send(instruction: Resource.Instruction.AcknowledgeProposal.Create(message: bucket, target: acknowledgement.ResourceAgent));
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

            Agent.Send(instruction: Resource.Instruction.BucketReady.Create(message: bucket.Key, target: bucket.ResourceAgent));
            System.Diagnostics.Debug.WriteLine($"UpdateStartConditions for bucket {bucket.Key} was sent");
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
