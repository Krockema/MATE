using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using static FAgentInformations;
using static FBreakDowns;
using static FOperations;
using static FProposals;
using static FResourceInformations;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;
using ResourceManager = Master40.SimulationCore.Agents.HubAgent.Types.ResourceManager;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class DefaultSetup : SimulationCore.Types.Behaviour
    {
        internal DefaultSetup(SimulationType simulationType = SimulationType.DefaultSetup)
                        : base(childMaker: null, simulationType: simulationType) { }


        internal List<FOperation> _operationList { get; set; } = new List<FOperation>();
        internal ResourceManager _resourceManager { get; set; } = new ResourceManager();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.EnqueueJob msg: EnqueueJob(fOperation: msg.GetObjectFromMessage as FOperation); break;
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

        
        private void EnqueueJob(FOperation fOperation)
        {
            var localItem = _operationList.FirstOrDefault(x => x.Key == fOperation.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (localItem == null)
            {
                localItem = fOperation;
                localItem.UpdateHubAgent(hub: Agent.Context.Self);
                _operationList.Add(item: localItem);

                Agent.DebugMessage(msg: $"Got New Item to Enqueue: {fOperation.Operation.Name} | with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}");
            }
            else
            {
                // reset Item.
                Agent.DebugMessage(msg: $"Got Item to Requeue: {fOperation.Operation.Name} | with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}");
                fOperation.Proposals.Clear();
                localItem = fOperation;
                //localItem = fOperation.UpdateHubAgent(hub: Agent.Context.Self);
                _operationList.Replace(val: localItem);
            }

            var resourceToRequest = _resourceManager.GetResourceByTool(fOperation.Operation.ResourceTool);
            
            foreach (var actorRef in resourceToRequest)
            {
                Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name}");
                Agent.Send(instruction: Resource.Instruction.RequestProposal.Create(message: localItem, target: actorRef));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proposal"></param>
        private void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var fOperation = _operationList.Single(predicate: x => x.Key == fProposal.JobKey);
            fOperation.Proposals.RemoveAll(x => x.ResourceAgent.Equals(fProposal.ResourceAgent));
            // add New Proposal
            fOperation.Proposals.Add(item: fProposal);

            Agent.DebugMessage(msg: $"Proposal for {fOperation.Operation.Name} with Schedule: {fProposal.PossibleSchedule} Id: {fProposal.JobKey} from: {fProposal.ResourceAgent}!");


            // if all Machines Answered
            if (fOperation.Proposals.Count == _resourceManager.GetResourceByTool(fOperation.Operation.ResourceTool).Count)
            {

                // item Postponed by All Machines ? -> requeue after given amount of time.
                if (fOperation.Proposals.TrueForAll(match: x => x.Postponed.IsPostponed))
                {
                    var postPonedFor = fOperation.Proposals.Min(x => x.Postponed.Offset);
                    Agent.DebugMessage(msg: $"{fOperation.Operation.Name} {fOperation.Key} postponed to {postPonedFor}");
                    // Call Hub Agent to Requeue
                    fOperation = fOperation.UpdateResourceAgent(r: ActorRefs.NoSender);
                    _operationList.Replace(val: fOperation);
                    Agent.Send(instruction: Hub.Instruction.EnqueueJob.Create(message: fOperation, target: Agent.Context.Self), waitFor: postPonedFor);
                    return;
                }


                // acknowledge Machine -> therefore get Machine -> send acknowledgement
                var earliestPossibleStart = fOperation.Proposals.Where(predicate: y => y.Postponed.IsPostponed == false)
                                                               .Min(selector: p => p.PossibleSchedule);

                var acknowledgement = fOperation.Proposals.First(predicate: x => x.PossibleSchedule == earliestPossibleStart
                                                                        && x.Postponed.IsPostponed == false);

                fOperation = ((IJob)fOperation).UpdateEstimations(acknowledgement.PossibleSchedule, acknowledgement.ResourceAgent) as FOperation;

                Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {fOperation.Operation.Name} {fOperation.Key} on resource {acknowledgement.ResourceAgent}");

                // set Proposal Start for Machine to Requeue if time slot is closed.
                _operationList.Replace(fOperation);
                Agent.Send(instruction: Resource.Instruction.AcknowledgeProposal.Create(message: fOperation, target: acknowledgement.ResourceAgent));
            }
        }

        private void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {
            var operation = _operationList.Single(predicate: x => x.Key == startCondition.OperationKey);
            operation.SetStartConditions(startCondition: startCondition);
            // if Agent has no ResourceAgent the operation is not queued so here is nothing to do
            if (operation.ResourceAgent.IsNobody())
                return;

            Agent.DebugMessage(msg: $"Update and forward start condition: {operation.Operation.Name} {operation.Key}" +
                                    $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                    $"| PreCondition: {operation.StartConditions.PreCondition} " +
                                    $"to resource {operation.ResourceAgent}");

            Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: operation.ResourceAgent));
        }

        /// <summary>
        /// Source: ResourceAgent put operation onto processingQueue and will work on it soon
        /// Target: Method should forward message to the associated production agent
        /// </summary>
        /// <param name="key"></param>
        public void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _operationList.Single(predicate: x => x.Key == operationKey);

            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
                                                    .Create(message: operation.Key
                                                           , target: operation.ProductionAgent));
        }

        public void FinishJob(IJobResult jobResult)
        {
            var operation = _operationList.Find(match: x => x.Key == jobResult.Key);

            Agent.DebugMessage(msg: $"Resource called Item {operation.Operation.Name} {jobResult.Key} finished.");
            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: operation.ProductionAgent));
            _operationList.Remove(item: operation);
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
