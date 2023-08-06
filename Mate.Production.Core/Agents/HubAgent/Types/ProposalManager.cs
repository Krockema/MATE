using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Scopes;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.HubAgent.Types
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

        public ProposalForCapabilityProviderSet AddProposal(ProposalRecord fProposal, IActorRef sender)
        {
            if (!_proposalDictionary.TryGetValue(fProposal.JobKey, out var proposalForSetupDefinitionSet))
                return null;

            foreach (var proposalForCapabilityProvider in proposalForSetupDefinitionSet)
            {
                foreach (var resource in proposalForCapabilityProvider.GetAllResources())
                {
                    if (resource.Equals(sender))
                    {
                        proposalForCapabilityProvider.Add(fProposal);
                    }   
                }
            }
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

        public List<PossibleProcessingPosition> CreatePossibleProcessingPositions(List<ProposalForCapabilityProvider> proposalForCapabilityProviders, IJob job)
        {
            var possibleProcessingPositions = new List<PossibleProcessingPosition>();

            foreach (var proposal in proposalForCapabilityProviders)
            {
                var suitableQueuingPosition = GetSuitableQueueingPositionFromProposals(proposal, ((BucketRecord)job).MaxBucketSize);
                if (suitableQueuingPosition._queuingDictionary.Count == 0)
                    continue;
                possibleProcessingPositions.Add(suitableQueuingPosition);
            }

            return possibleProcessingPositions;
        }

        public PossibleProcessingPosition GetSuitableQueueingPositionFromProposals(ProposalForCapabilityProvider proposalForCapabilityProvider, TimeSpan jobDuration)
        {

            var possibleProcessingPosition = new PossibleProcessingPosition(proposalForCapabilityProvider.GetCapabilityProvider);
            var setupDuration =
                proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups.Sum(x => x.SetupTime);

            var mainResources = proposalForCapabilityProvider.GetResources(usedInSetup:true, usedInProcess: true);
            var possibleMainQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, mainResources, jobDuration);

            var setupResources = proposalForCapabilityProvider.GetResources(usedInSetup: true, usedInProcess: false);
            var possibleSetupQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, setupResources, jobDuration);
            
            var processingResources = proposalForCapabilityProvider.GetResources(usedInSetup: false, usedInProcess: true);
            var possibleProcessingQueuingPositions = GetQueueingPositions(proposalForCapabilityProvider, processingResources, jobDuration);

            // Apporach 1.
            // 1. Durchlaufe alle MainScopes 

            // S ->S<---->S<-->  <---------
            // R ->S      S|xxx|    <------- ----   ------
            // W ------->  |xxx|   <------- ------------

            // Reduziere den Möglichen Zeitaum der Main Ressourcen 
            // R1 ----  |      |-----  ------
            // R2 ------|      |   -------
            // E1 ------|      |-------------|              inf|

            // Prüfe ob Bearbeitungsressourcen notwendig sind
            // --> Nein --> Continue
            // --> JA --> Reduziere den Möglichen Bearbeitungsraum mit Werker 
            // E1 ------|      |-------------|              inf|
            // W ----------|   |------    ------|           inf|
            // E2 ---------|   |------    ------|           inf|

            // prüfe ob setup notwenig ist
            // --> Nein --> Return 
            // --> Ja --> Passe den notwendigen setup raum an
            // E1 ------|               |-------------|              inf|
            // W ----------|            |------    ------|           inf|
            // E2 ---------         |xxx|------    ------|           inf|
            // E3 ------|           |--------------------|           inf|


            // prüfe auf setup ressourcen.
            // S -|  |---|          |--|    |---------|
            // E3 ------|           |--------------------|           inf|
            // E4 -------|          |--------------------|           inf|
            
            // Von allen möglichen Setups innerhalb des scopes Nimm das Letzte aus E4.

            // Minimiere den Zeitraum zwischen SetupEnde und Bearbeitungsstart 
            
            
            // Approach 2
            
            // 1. Nimm über alle MainResourcen die kleine ProcessingStartzeit
            // 2. Prüfe ob ein setup notwenig ist und ermittle alle start und endwerte --> sind bereits da
            // 3. Wenn Setup notwendig, prüfe ob alle anderen resourcen zu den Setups und Processing Starts frei sind
            //  --> ja Lücke gefunden
            //  --> nein nimm die ProcessingStartzeit und rechne + 1
            //  goto 2. --> flieg raus aus dem Slot und nimm den nächsten slot, wenn Zeit nicht mehr reicht
            // goto 1. mit einer größeren Pr


            foreach (var mainScope in possibleMainQueuingPositions)
            {
                ITimeRange setupSlot = null;
                ITimeRange processingSlot = null;
                /*
                 * CASE WITH SETUP
                 */
                if (mainScope.IsRequieringSetup)
                {
                    possibleProcessingPosition.RequireSetup = true;
                    DateTime earliestProcessingStart = mainScope.Scope.Start + setupDuration;
                    DateTime earliestSetupStart = mainScope.Scope.Start;
                    setupSlot = new SetupSlotRecord(earliestSetupStart, earliestProcessingStart);
                    processingSlot = new ProcessingSlotRecord(Start: earliestProcessingStart, End: earliestProcessingStart + jobDuration);
                    var mainProcessingSlots =  ImmutableHashSet.Create<ITimeRange>();
                    if (setupResources.Count != 0) // Setup RESOURCE Is Required.
                    {
                        // Machine includes Setup time in the Queuing Position if required.
                        var setupIsPossible = possibleSetupQueuingPositions.FirstOrDefault(setup
                                                            => SlotComparerSetup(mainScope.Scope, setup.Scope, jobDuration, setupDuration));
                        if (setupIsPossible == null)
                            continue;

                        earliestSetupStart = (new[] { setupIsPossible.Scope.Start, mainScope.Scope.Start }).Max();
                        earliestProcessingStart = earliestSetupStart + setupDuration;
                        setupSlot = new SetupSlotRecord(Start: earliestSetupStart, End: earliestProcessingStart);
                        processingSlot = new ProcessingSlotRecord(Start: earliestProcessingStart, End: earliestProcessingStart + jobDuration);

                        /*
                         * CASE 1 ONLY MAIN RESOURCE AND SETUP RESSOURCE // WITH SETUP
                         */

                        if (processingResources.Count == 0)
                        {
                            mainProcessingSlots = ImmutableHashSet.Create<ITimeRange>(new [] { setupSlot, processingSlot});
                            mainResources.ForEach(resourceRef => possibleProcessingPosition.Add(resourceRef, CreateScopeConfirmation(mainProcessingSlots, earliestSetupStart), earliestProcessingStart));
                            setupResources.ForEach(resourceRef => possibleProcessingPosition.Add(resourceRef, CreateScopeConfirmation(setupSlot, earliestSetupStart)));
                            break;
                        }
                    }

                    /*
                     * CASE 2 ONLY MAIN RESOURCE // WITH SETUP
                     */

                    if (processingResources.Count == 0) //
                    {
                        mainProcessingSlots = ImmutableHashSet.Create<ITimeRange>(new[] { setupSlot, processingSlot });
                        mainResources.ForEach(resourceRef => possibleProcessingPosition.Add(resourceRef, CreateScopeConfirmation(mainProcessingSlots, earliestSetupStart), earliestProcessingStart));
                        break;
                    }
                    
                    var processingScope = new ScopeRecord(Start: earliestProcessingStart, End: mainScope.Scope.End);
                    // seek for worker to process operation
                    var processingResourceSlot = FindProcessingSlot(possibleProcessingQueuingPositions.Where(x => x.IsQueueAble)
                                                                    .Select(x => x.Scope).ToList(), processingScope, jobDuration);
                    // set if a new is found 
                    if (processingResourceSlot == null)
                        continue;

                    processingSlot = new ProcessingSlotRecord(Start: processingResourceSlot.Start, End: processingResourceSlot.End);

                    mainProcessingSlots = ImmutableHashSet.Create<ITimeRange>(new[] { setupSlot, processingSlot });
                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainProcessingSlots, earliestSetupStart)));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(processingSlot, earliestSetupStart), processingSlot.Start));
                    if (setupResources.Count > 0)
                    {
                        setupResources.ForEach(x =>
                            possibleProcessingPosition.Add(x, CreateScopeConfirmation(ImmutableHashSet.Create(setupSlot), earliestSetupStart )));
                    }
                    break;

                }
                else
                {
                    /*
                     * CASE NoSetup
                     */
                    ITimeRange mainSlot = null;
                    if (processingResources.Count == 0)
                    {
                        mainSlot = new ProcessingSlotRecord(Start: mainScope.Scope.Start, End: mainScope.Scope.Start + jobDuration);
                        mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start), mainSlot.Start));
                        break;
                    }
                    processingSlot = FindProcessingSlot(possibleProcessingQueuingPositions.Where(x => x.IsQueueAble)
                                                                                          .Select(x => x.Scope).ToList(), mainScope.Scope, jobDuration);
                    // set if a new is found 
                    if (processingSlot == null)
                        continue;

                    mainSlot = new ProcessingSlotRecord(Start: processingSlot.Start, End: processingSlot.Start + jobDuration);

                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start)));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start), mainSlot.Start));
                    break;
                }

            }

            return possibleProcessingPosition;
        }

        private ScopeConfirmationRecord CreateScopeConfirmation(ImmutableHashSet<ITimeRange> timeRanges, DateTime setStartAt)
        {
            return new ScopeConfirmationRecord(timeRanges, setStartAt);
        }

        private ScopeConfirmationRecord CreateScopeConfirmation(ITimeRange timeRange, DateTime setStartAt)
        {
            return new ScopeConfirmationRecord(ImmutableHashSet.Create(timeRange), setStartAt);
        }


        private List<QueueingScopeRecord> GetQueueingPositions(ProposalForCapabilityProvider proposalForCapabilityProvider, List<IActorRef> resources, TimeSpan requiredDuration)
        {
            List<ProposalRecord> setupResourceProposals = proposalForCapabilityProvider.GetProposalsFor(resources);
            var possibleSetupQueuingPositions = new List<QueueingScopeRecord>();
            if (setupResourceProposals.Count > 0)
            {
                possibleSetupQueuingPositions.AddRange(
                    ProposalReducer(setupResourceProposals.ToArray()
                        , setupResourceProposals[0].PossibleSchedule as List<QueueingScopeRecord>
                        , 1
                        , requiredDuration));
            }

            return possibleSetupQueuingPositions;
        }

        private List<QueueingScopeRecord> ProposalReducer(ProposalRecord[] proposalArray, List<QueueingScopeRecord> possibleFQueueingPositions, int stage, TimeSpan requiredDuration)
        {
            var reducedSlots = new List<QueueingScopeRecord>();
            if (stage == proposalArray.Length)
                return possibleFQueueingPositions;


            foreach (var position in possibleFQueueingPositions)
            {
                var positionsToCompare = (proposalArray[stage].PossibleSchedule as List<QueueingScopeRecord>);
                var pos = positionsToCompare.FirstOrDefault(x => SlotComparerBasic(position.Scope, x.Scope, requiredDuration));
                
                if (pos == null || pos.IsQueueAble is false)
                    continue;
                var min = (new[] { position.Scope.Start, pos.Scope.Start }).Max();
                var max = (new[] { position.Scope.End, pos.Scope.End }).Min();

                reducedSlots.Add(new QueueingScopeRecord(IsQueueAble: pos.IsQueueAble && position.IsQueueAble
                                                        , IsRequieringSetup: pos.IsRequieringSetup && position.IsRequieringSetup
                                                        , new ScopeRecord(Start: min, End: max)));
            }
            if (proposalArray.Length >= stage) return reducedSlots;

            stage++;
            reducedSlots = ProposalReducer(proposalArray, reducedSlots, stage, requiredDuration);
            return reducedSlots;
        }

        private ProcessingSlotRecord FindProcessingSlot(List<ITimeRange> processingPositions, ITimeRange workingQueueSlot,TimeSpan jobDuration)
        {
            var processingIsPossible = processingPositions.FirstOrDefault(processing =>
                                                    SlotComparerBasic(workingQueueSlot, processing, jobDuration));

            if (processingIsPossible == null) return null;

            var earliestProcessingStart = (new[] { processingIsPossible.Start, workingQueueSlot.Start }).Max();

            return new ProcessingSlotRecord( earliestProcessingStart, earliestProcessingStart + jobDuration);
        }


        private bool SlotComparerSetup(ITimeRange mainResourcePos, ITimeRange toCompare, TimeSpan processingTime, TimeSpan setupTime) 
        {
            // calculate posible earliest start
            DateTime earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var setupFit = (earliestStart + setupTime <= toCompare.End);
            // check Queue scope
            var queueFit = (earliestStart + setupTime + processingTime <= mainResourcePos.End);

            return (setupFit && queueFit);
        }


        private bool SlotComparerBasic(ITimeRange mainResourcePos, ITimeRange toCompare, TimeSpan duration) 
        {
            // calculate posible earliest start
            DateTime earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var fitToCompare = (earliestStart + duration <= toCompare.End);
            // check Queue scope
            var fitMainResource = (earliestStart + duration <= mainResourcePos.End);

            return (fitToCompare && fitMainResource);
        }
    }
}
