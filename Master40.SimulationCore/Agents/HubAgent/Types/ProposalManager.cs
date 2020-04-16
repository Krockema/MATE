using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FProposals;
using static FQueueingScopes;
using static FScopeConfirmations;
using static IScopes;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalManager
    {
        private Dictionary<Guid, ProposalForCapabilityProviderSet> _proposalDictionary { get; set; } 

        /// <summary>
        /// Contains a Dictionary with a JobKey and a ProposalForCapabilityProviderSet
        /// </summary>
        public ProposalManager()
        {
            _proposalDictionary = new Dictionary<Guid, ProposalForCapabilityProviderSet>();
        }

        public bool Add(Guid jobKey, List<M_ResourceCapabilityProvider> resourceCapabilityProvider)
        {
            var defs = new ProposalForCapabilityProviderSet();
            resourceCapabilityProvider.ForEach(x => defs.Add(new ProposalForCapabilityProvider(x)));
            return _proposalDictionary.TryAdd(jobKey, defs);
        }

        public ProposalForCapabilityProviderSet AddProposal(FProposal fProposal)
        {
            if (!_proposalDictionary.TryGetValue(fProposal.JobKey, out var proposalForSetupDefinitionSet))
                return null;

            var proposalForCapabilityProvider = proposalForSetupDefinitionSet.Single(x => x.ProviderId == fProposal.CapabilityProviderId);

            proposalForCapabilityProvider.Add(fProposal);
                return proposalForSetupDefinitionSet;
        }

        public ProposalForCapabilityProviderSet GetProposalForSetupDefinitionSet(Guid jobKey)
        {
            return _proposalDictionary.SingleOrDefault(x => x.Key == jobKey).Value;
        }

        internal bool RemoveAllProposalsFor(Guid job)
        {
            if (!_proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet))
                return false;

            proposalForSetupDefinitionSet.ForEach(x => x.RemoveAll());
            return true;
        }

        public bool Remove(Guid jobKey)
        {
            return _proposalDictionary.Remove(jobKey);
        }

        internal List<PossibleProcessingPosition> CreatePossibleProcessingPositions(List<ProposalForCapabilityProvider> proposalForCapabilityProviders)
        {
            var possibleProcessingPositions = new List<PossibleProcessingPosition>();

            foreach (var proposal in proposalForCapabilityProviders)
            {
                var suitableQueuingPosition = GetSuitableQueueingPositionFromProposals(proposal);

                possibleProcessingPositions.Add(suitableQueuingPosition);
            }

            return possibleProcessingPositions;
        }

        public PossibleProcessingPosition GetSuitableQueueingPositionFromProposals(ProposalForCapabilityProvider proposalForCapabilityProvider)
        {

            var possibleProcessingPosition = new PossibleProcessingPosition(proposalForCapabilityProvider.GetCapabilityProvider);

            var mainResources = proposalForCapabilityProvider.GetResources(usedInSetup:true, usedInProcess: true);
            var possibleMainQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, mainResources);

            var setupResources = proposalForCapabilityProvider.GetResources(usedInSetup: true, usedInProcess: false);
            var possibleSetupQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, setupResources);
            
            var processingResources = proposalForCapabilityProvider.GetResources(usedInSetup: false, usedInProcess: true);
            var possibleProcessingQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, processingResources);
            

            foreach (var mainScope in possibleMainQueuingPositions)
            {
                if (mainScope.IsRequieringSetup) // determine implicit ? setupResoruce.Count ?
                {
                    long earliestProcessingStart = mainScope.Start;
                    long earliestSetupStart = mainScope.Start;
                    FScopeConfirmation setupSlot = null;
                    FScopeConfirmation mainSlot = null;
                    if (setupResources.Count != 0)
                    {
                        // Machine includes Setup time in the Queuing Position if required.
                        var setupIsPossible = possibleSetupQueuingPositions.FirstOrDefault(setup
                                                                                       => SlotComparerSetup(mainScope, setup));
                        if (setupIsPossible == null)
                            continue;

                        earliestSetupStart = (new[] { setupIsPossible.Start, mainScope.Start }).Max();
                        earliestProcessingStart = earliestSetupStart + setupIsPossible.EstimatedWork;

                        // create possible operation slot
                        setupSlot = new FScopeConfirmation( start: earliestSetupStart, 
                                                            end: earliestProcessingStart - 1,
                                                            estimatedWork: setupIsPossible.EstimatedWork);
                        
                        // Reduced Position for processing comparison
                        mainSlot = new FScopeConfirmation(  start: earliestProcessingStart, 
                                                            end: earliestProcessingStart + mainScope.EstimatedWork -1,
                                                            estimatedWork: mainScope.EstimatedWork);


                        if (processingResources.Count == 0)
                        {
                            mainResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot, mainSlot.Start));
                            setupResources.ForEach(x => possibleProcessingPosition.Add(x, setupSlot));
                            break;
                        }
                    }

                    if(processingResources.Count == 0)
                    {
                        mainSlot = new FScopeConfirmation(start: /* ToDo : SetupTime + */ earliestProcessingStart,
                                                                    end: earliestProcessingStart + mainScope.EstimatedWork - 1,
                                                                    estimatedWork: mainScope.EstimatedWork);
                        mainResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot, mainSlot.Start));
                        break;
                    }


                    var processingScope = new FScopeConfirmation(start: /* ToDo : SetupTime + */ earliestProcessingStart,
                                                                    end: mainScope.End,
                                                                    estimatedWork: mainScope.EstimatedWork);
                    // seek for worker to process operation
                    var processingPosition = FindProcessingPosition(possibleProcessingQueuingPositions.Cast<IScope>().ToList(), processingScope);
                    // set if a new is found 
                    if (processingPosition == null)
                        continue;

                    var processingSlot = new FScopeConfirmation(start: processingPosition.Start,
                                                                end: processingPosition.Start + mainScope.EstimatedWork - 1,
                                                                estimatedWork: processingPosition.EstimatedWork);

                    mainSlot = new FScopeConfirmation(start: mainSlot.Start, end: processingSlot.End - 1,
                                                            estimatedWork: processingSlot.EstimatedWork);

                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, processingSlot, processingSlot.Start));
                    if (setupResources.Count > 0)
                    {
                        setupResources.ForEach(x => possibleProcessingPosition.Add(x, setupSlot));
                    }

                    break;

                }
                else
                {// if no Setup is Required
                    FScopeConfirmation mainSlot = null;
                    if (processingResources.Count == 0)
                    {
                        mainSlot = new FScopeConfirmation(
                            start: mainScope.Start, end: mainScope.Start + mainScope.EstimatedWork -1,
                            estimatedWork: mainScope.EstimatedWork);
                        mainResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot, mainSlot.Start));
                        break;
                    }
                    var processingPosition = FindProcessingPosition(possibleProcessingQueuingPositions.Cast<IScope>().ToList(), mainScope);
                    // set if a new is found 
                    if (processingPosition == null)
                        continue;

                    mainSlot = new FScopeConfirmation(start: mainScope.Start, 
                                                        end: mainScope.Start + mainScope.EstimatedWork - 1,
                                                        estimatedWork: mainScope.EstimatedWork);
                    
                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, mainSlot, mainSlot.Start));
                    break;
                }

            }

            return possibleProcessingPosition;
        }

        private List<FQueueingScope> GetQueueingPositions(ProposalForCapabilityProvider proposalForCapabilityProvider, List<IActorRef> resources)
        {
            List<FProposal> setupResourceProposals = proposalForCapabilityProvider.GetProposalsFor(resources);
            var possibleSetupQueuingPositions = new List<FQueueingScope>();
            if (setupResourceProposals.Count > 0)
            {
                possibleSetupQueuingPositions.AddRange(
                    ProposalReducer(setupResourceProposals.ToArray()
                        , setupResourceProposals[0].PossibleSchedule as List<FQueueingScope>
                        , 1));
            }

            return possibleSetupQueuingPositions;
        }

        private List<FQueueingScope> ProposalReducer(FProposal[] proposalArray, List<FQueueingScope> possibleFQueueingPositions, int stage)
        {
            var reducedSlots = new List<FQueueingScope>();
            if (stage == proposalArray.Length)
                return possibleFQueueingPositions;


            foreach (var position in possibleFQueueingPositions)
            {
                var positionsToCompare = proposalArray[stage].PossibleSchedule as List<FQueueingScope>;
                var found = positionsToCompare.FirstOrDefault(x => SlotComparerBasic(position, x));
                if (null == found)
                    continue;
                var min = (new[] { position.Start, found.Start }).Max();
                var max = (new[] { position.End, found.End }).Min();

                reducedSlots.Add(new FQueueingScope(isQueueAble: true, isRequieringSetup: found.IsRequieringSetup,
                                                    start: min, 
                                                    end: max,
                                                    estimatedSetup: found.EstimatedSetup,
                                                    estimatedWork: max - min));
            }

            if (proposalArray.Length >= stage) return reducedSlots;

            stage++;
            reducedSlots = ProposalReducer(proposalArray, reducedSlots, stage);
            return reducedSlots;
        }

        private FScopeConfirmation FindProcessingPosition(List<IScope> processingPositions, IScope workingQueueSlot)
        {
            var processingIsPossible = processingPositions.FirstOrDefault(processing =>
                SlotComparerBasic(workingQueueSlot, processing));

            if (processingIsPossible == null) return null;

            var earliestProcessingStart = (new[] { processingIsPossible.Start, workingQueueSlot.Start }).Max();

            return new FScopeConfirmation( earliestProcessingStart,
                                           earliestProcessingStart + workingQueueSlot.EstimatedWork - 1,
                                           workingQueueSlot.EstimatedWork);
        }


        private bool SlotComparerSetup(IScope mainResourcePos, IScope toCompare) // doesnt work with worker
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var setupFit = (earliestStart + toCompare.EstimatedWork - 1 <= toCompare.End);
            // check Queue scope
            var queueFit = (earliestStart + toCompare.EstimatedWork + mainResourcePos.EstimatedWork - 1 <= mainResourcePos.End);

            return (setupFit && queueFit);
        }


        private bool SlotComparerBasic(IScope mainResourcePos, IScope toCompare) // doesnt work with worker
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var fitToCompare = (earliestStart + mainResourcePos.EstimatedWork - 1 <= toCompare.End);
            // check Queue scope
            var fitMainResource = (earliestStart + mainResourcePos.EstimatedWork - 1 <= mainResourcePos.End);

            return (fitToCompare && fitMainResource);
        }
    }
}
