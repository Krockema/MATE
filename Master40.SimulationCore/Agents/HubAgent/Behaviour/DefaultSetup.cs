using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent;
using System;
using static FOperations;
using static FProposals;
using static FRequestProposalForSetups;
using static FResourceInformations;
using static FUpdateStartConditions;
using static IJobResults;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class DefaultSetup : SimulationCore.Types.Behaviour
    {
        internal DefaultSetup(SimulationType simulationType = SimulationType.DefaultSetup)
                        : base(childMaker: null, simulationType: simulationType) { }


        internal OperationManager _operations { get; set; } = new OperationManager();
        internal CapabilityManager _capabilityManager { get; set; } = new CapabilityManager();
        internal ProposalManager _proposalManager { get; set; } = new ProposalManager();

        public override bool Action(object message)
        {
            switch (message)
            {
                case Hub.Instruction.Default.EnqueueJob msg: EnqueueJob(fOperation: msg.GetObjectFromMessage as FOperation); break;
                case Hub.Instruction.Default.ProposalFromResource msg: ProposalFromResource(fProposal: msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;
                case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                case Hub.Instruction.Default.AddResourceToHub msg: AddResourceToHub(resourceInformation: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(breakDown: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal virtual void EnqueueJob(FOperation fOperation)
        {
            var jobConfirmation = _operations.GetJobConfirmation(fOperation.Key);
            // If item is not Already in Queue Add item to Queue
            // // happens i.e. Machine calls to Requeue item.
            if (jobConfirmation == null)
            {
                fOperation.HubAgent = Agent.Context.Self;
                jobConfirmation = new JobConfirmation(fOperation);
                _operations.Add(jobConfirmation);
                _proposalManager.Add(fOperation.Key, _capabilityManager.GetAllSetupDefinitions(fOperation.RequiredCapability, Agent));
                Agent.DebugMessage(msg: $"Got New Item to Enqueue: {fOperation.Operation.Name} " +
                                        $"| with start condition: {fOperation.StartConditions.Satisfied} with Id: {fOperation.Key}");

            }
            else
            {
                // reset Item.
                fOperation = jobConfirmation.Job as FOperation;
                Agent.DebugMessage(msg: $"Got Item to Requeue: { fOperation.Operation.Name} | with start condition: {fOperation.StartConditions.Satisfied } with Id: { fOperation.Key }");
                _proposalManager.Add(fOperation.Key, _capabilityManager.GetAllSetupDefinitions(fOperation.RequiredCapability, Agent));
                jobConfirmation.ResetConfirmation();
            }

            var capabilityDefinition = _capabilityManager.GetResourcesByCapability(fOperation.RequiredCapability);
            
            foreach (var setupDefinition in capabilityDefinition.GetAllSetupDefinitions)
            {
                foreach (var actorRef in setupDefinition.RequiredResources)
                {
                    Agent.DebugMessage(msg: $"Ask for proposal at resource {actorRef.Path.Name} | for {jobConfirmation.Job.Name } with { setupDefinition.SetupKey }");
                    Agent.Send(instruction: Resource.Instruction.Default.RequestProposal
                                            .Create(message: new FRequestProposalForSetup(jobConfirmation.Job, setupDefinition.SetupKey)
                                                   , target: actorRef));

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proposal"></param>
        internal virtual void ProposalFromResource(FProposal fProposal)
        {
            // get related operation and add proposal.
            var fOperation = _operations.GetJobBy(fProposal.JobKey) as FOperation;

            var propSet = _proposalManager.AddProposal(fProposal);

            Agent.DebugMessage(msg: $"Proposal({propSet.ReceivedProposals}of{propSet.RequiredProposals}) for {fOperation.Key} with Schedule: {fProposal.PossibleSchedule} " +
                                   $"JobKey: {fProposal.JobKey} from: {fProposal.ResourceAgent.Path.Name}!");

            // if all resources answered
            if (propSet.AllProposalsReceived)
            {
                // item Postponed by All Machines ? -> requeue after given amount of time.
                var proposalForSetupDefinition = propSet.GetValidProposal();
                if (proposalForSetupDefinition == null)
                {
                    var postponedFor = propSet.PostponedUntil;
                    _proposalManager.RemoveAllProposalsFor(fOperation.Key);

                    Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: fOperation, target: Agent.Context.Self), waitFor: postponedFor);
                    return;
                }

                // acknowledge resources -> therefore get Machine -> send acknowledgement
                
                var jobConfirmation = _operations.GetJobConfirmation(fOperation.Key);
                jobConfirmation.Schedule = proposalForSetupDefinition.EarliestStart();
                jobConfirmation.SetupDefinition = proposalForSetupDefinition.GetFSetupDefinition;

                foreach (IActorRef resource in proposalForSetupDefinition.GetFSetupDefinition.RequiredResources) {

                    Agent.DebugMessage(msg: $"Start AcknowledgeProposal for {fOperation.Operation.Name} {fOperation.Key} on resource {resource}");

                    Agent.Send(instruction: Resource.Instruction.Default.AcknowledgeProposal
                        .Create(jobConfirmation.ToImmutable()
                            , target: resource));
                }

                _proposalManager.Remove(fOperation.Key);

            }
        }

        internal virtual void UpdateAndForwardStartConditions(FUpdateStartCondition startCondition)
        {

            var jobConfirmation = _operations.UpdateOperationStartConfirmation(startCondition);
            var operation = jobConfirmation.Job as FOperation;
            // if Agent has no ResourceAgent the operation is not queued so here is nothing to do
            if (jobConfirmation.SetupDefinition.SetupKey == -1)
                return;


            foreach (var resource in jobConfirmation.SetupDefinition.RequiredResources)
            {
                Agent.DebugMessage(msg: $"Update and forward start condition: {operation.Operation.Name} {operation.Key}" +
                                        $"| ArticleProvided: {operation.StartConditions.ArticlesProvided} " +
                                        $"| PreCondition: {operation.StartConditions.PreCondition} " +
                                        $"to resource {resource}");

                Agent.Send(instruction: BasicInstruction.UpdateStartConditions.Create(message: startCondition, target: resource));
            }
            
        }

        /// <summary>
        /// Source: ResourceAgent put operation onto processingQueue and will work on it soon
        /// Target: Method should forward message to the associated production agent
        /// </summary>
        /// <param name="key"></param>
        internal virtual void WithdrawRequiredArticles(Guid operationKey)
        {
            var operation = _operations.GetJobBy(operationKey) as FOperation;

            Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles
                                                    .Create(message: operation.Key
                                                           , target: operation.ProductionAgent));
        }

        internal virtual void FinishJob(IJobResult jobResult)
        {
            var operation = _operations.GetJobBy(jobResult.Key) as FOperation;

            Agent.DebugMessage(msg: $"Resource called Item {operation.Operation.Name} {jobResult.Key} finished.");
            Agent.Send(instruction: BasicInstruction.FinishJob.Create(message: jobResult, target: operation.ProductionAgent));
            _operations.RemoveWhere(x => x.Job.Key == operation.Key);
        }

        internal virtual void AddResourceToHub(FResourceInformation resourceInformation)
        {
            foreach (var setup in resourceInformation.ResourceSetups)
            {
                var capabilityDefinition = _capabilityManager.GetCapabilityDefinition(setup.ResourceCapabilityProvider.ResourceCapability);
               
                var setupDefinition = capabilityDefinition.GetSetupDefinitionBy(setup);
                setupDefinition.RequiredResources.Add(resourceInformation.Ref);

                System.Diagnostics.Debug.WriteLine($"Create Capability Definition at {Agent.Name}" +
                                                   $" with setup {setup.Name} " +
                                                   $" from {Agent.Context.Sender.Path.Name}" +
                                                   $" with capability {capabilityDefinition.ResourceCapability.Name}");

            }
            // Added Machine Agent To Machine Pool
            Agent.DebugMessage(msg: "Added Machine Agent " + resourceInformation.Ref.Path.Name + " to Machine Pool: " + resourceInformation.RequiredFor);
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
