using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FProposals;
using static FQueueingPositions;

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

            var possbileProcessingPosition = new PossibleProcessingPosition(proposalForCapabilityProvider.GetCapabilityProvider);

            var mainResources = proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => x.UsedInSetup && x.UsedInProcess).Select(x => x.Resource.IResourceRef).Cast<IActorRef>().ToList();

            var setupResources = proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => x.UsedInSetup && !x.UsedInProcess).Select(x => x.Resource.IResourceRef).Cast<IActorRef>().ToList();

            var processingResources = proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => !x.UsedInSetup && x.UsedInProcess).Select(x => x.Resource.IResourceRef).Cast<IActorRef>().ToList();

            // Get Proposals for all main resources grouped by Resource 
            // Todo: maybe Dictionary<resource, Proposal>
            List<FProposal> mainResourceProposals = proposalForCapabilityProvider.GetProposalsFor(mainResources);
            // Find overlapping timeSlots 
            var possibleMainQueuingPositions = ProposalReducer(mainResourceProposals.ToArray()
                                                                 , mainResourceProposals[0].PossibleSchedule as List<FQueueingPosition>
                                                                 , 1);

            // ToDo: mainResourceTimeslots List<FProposal>
            // ToDo: for setup
            // ToDo: for processing

            List<FProposal> setupResourceProposals = proposalForCapabilityProvider.GetProposalsFor(setupResources);
            var possibleSetupQueuingPositions = setupResourceProposals.First().PossibleSchedule as List<FQueueingPosition>;
            // ToDo: do the same as for main resource when more then one resource is required for setup only.

            List<FProposal> processingResourceProposals = proposalForCapabilityProvider.GetProposalsFor(processingResources);
            var possibleProcessingQueuingPositions = setupResourceProposals.First().PossibleSchedule as List<FQueueingPosition>;
            // ToDo: do the same as for main resource when more then one resource is required for processing only.


            foreach (var queueSlot in possibleMainQueuingPositions)
            {
                if (queueSlot.IsRequieringSetup) // determine implicit ? setupResoruce.Count ?
                {
                    // Machine includes Setup time in the Queuing Position if required.
                    var setupIsPossible = possibleSetupQueuingPositions.FirstOrDefault(setup
                                                                            => SlotComparerSetup(queueSlot, setup));
                    if (setupIsPossible == null)
                        continue;

                    long earliestSetupStart = (new[] { setupIsPossible.Start, queueSlot.Start }).Max();
                    long earliestProcessingStart = earliestSetupStart + setupIsPossible.EstimatedWork;

                    // create possible operation slot
                    var operatorSlot = new FQueueingPosition(isQueueAble: true, isRequieringSetup: true,
                                                            start: earliestSetupStart, end: earliestProcessingStart - 1,
                                                            estimatedWork: setupIsPossible.EstimatedWork);

                    // Reduced Position for processing comparison
                    var workingQueueSlot = new FQueueingPosition(isQueueAble: true, isRequieringSetup: true,
                                                                start: earliestProcessingStart, end: queueSlot.End,
                                                                estimatedWork: queueSlot.EstimatedWork);

                    // seek for worker to process operation
                    var workerPosition = FindProcessingPosition(processingResourceProposals, workingQueueSlot);
                    // set if a new is found 
                    if (!workerPosition.IsQueueAble)
                        continue;

                    workingQueueSlot = new FQueueingPosition(true, true,
                        operatorSlot.Start,
                        workerPosition.End, queueSlot.EstimatedWork + operatorSlot.EstimatedWork);

                    mainResources.ForEach(x => possbileProcessingPosition.Add(x, workingQueueSlot));
                    setupResources.ForEach(x => possbileProcessingPosition.Add(x, operatorSlot));
                    processingResources.ForEach(x => possbileProcessingPosition.Add(x, workerPosition, workerPosition.Start));
                    break;

                }
                else
                {// if no Setup is Required

                    var processingPosition = FindProcessingPosition(processingResourceProposals, queueSlot);
                    // set if a new is found 
                    if (!processingPosition.IsQueueAble)
                        continue;

                    var mainQueueingPosition = new FQueueingPosition(true, true,
                        processingPosition.Start,
                        processingPosition.End, queueSlot.EstimatedWork);

                    mainResources.ForEach(x => possbileProcessingPosition.Add(x, mainQueueingPosition));
                    processingResources.ForEach(x => possbileProcessingPosition.Add(x, processingPosition, processingPosition.Start));
                    break;
                }

            }

            return possbileProcessingPosition;
        }

        private List<FQueueingPosition> ProposalReducer(FProposal[] proposalArray, List<FQueueingPosition> possibleFQueueingPositions, int stage)
        {
            if (stage == proposalArray.Length)
                return possibleFQueueingPositions;

            var reducedSlots = new List<FQueueingPosition>();
            foreach (var position in possibleFQueueingPositions)
            {
                var positionsToCompare = proposalArray[stage].PossibleSchedule as List<FQueueingPosition>;
                var found = positionsToCompare.FirstOrDefault(x => SlotComparerBasic(position, x));
                if (null == found)
                    continue;
                var min = (new[] { position.Start, found.Start }).Max();
                var max = (new[] { position.End, found.End }).Min();

                reducedSlots.Add(new FQueueingPosition(isQueueAble: position.IsQueueAble && found.IsQueueAble,
                                                           isRequieringSetup: position.IsRequieringSetup || found.IsRequieringSetup,
                                                           start: min, end: max,
                                                           estimatedWork: max - min));
            }

            if (proposalArray.Length >= stage) return reducedSlots;

            stage++;
            reducedSlots = ProposalReducer(proposalArray, reducedSlots, stage);
            return reducedSlots;
        }

        private FQueueingPosition FindProcessingPosition(List<FProposal> processingResourceProposals, FQueueingPosition workingQueueSlot)
        {
            FQueueingPosition workerPosition = new FQueueingPosition(false, false, long.MaxValue, 0, 0);
            foreach (var processingProposal in processingResourceProposals)
            {
                var processingPositions = processingProposal.PossibleSchedule as List<FQueueingPosition>;
                var processingIsPossible = processingPositions.FirstOrDefault(processing =>
                    SlotComparerBasic(workingQueueSlot, processing));

                if (processingIsPossible == null)
                    continue; // can this happen ? should return at least one value
                              // maybe in postponed case ? 

                var earliestProcessingStart = (new[] { processingIsPossible.Start, workingQueueSlot.Start }).Max();

                if (earliestProcessingStart < workerPosition.Start)
                {
                    workerPosition = new FQueueingPosition(true, false, earliestProcessingStart,
                                                        earliestProcessingStart + workingQueueSlot.EstimatedWork - 1,
                                                        workingQueueSlot.EstimatedWork);
                }
            }

            return workerPosition;
        }


        private bool SlotComparerSetup(FQueueingPosition mainResourcePos, FQueueingPosition toCompare) // doesnt work with worker
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var setupFit = (earliestStart + toCompare.EstimatedWork - 1 <= toCompare.End);
            // check Queue scope
            var queueFit = (earliestStart + toCompare.EstimatedWork + mainResourcePos.EstimatedWork - 1 <= mainResourcePos.End);

            return (setupFit && queueFit);
        }


        private bool SlotComparerBasic(FQueueingPosition mainResourcePos, FQueueingPosition toCompare) // doesnt work with worker
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
