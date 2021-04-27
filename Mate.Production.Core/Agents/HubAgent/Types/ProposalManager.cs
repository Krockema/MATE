using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Microsoft.FSharp.Collections;
using static FBuckets;
using static FProcessingSlots;
using static FProposals;
using static FQueueingScopes;
using static FScopeConfirmations;
using static FScopes;
using static FSetupSlots;
using static IJobs;
using static ITimeRanges;

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

        public ProposalForCapabilityProviderSet AddProposal(FProposal fProposal, IActorRef sender)
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
                var suitableQueuingPosition = GetSuitableQueueingPositionFromProposals(proposal, ((FBucket)job).MaxBucketSize);
                if (suitableQueuingPosition._queuingDictionary.Count == 0)
                    continue;
                possibleProcessingPositions.Add(suitableQueuingPosition);
            }

            return possibleProcessingPositions;
        }

        public PossibleProcessingPosition GetSuitableQueueingPositionFromProposals(ProposalForCapabilityProvider proposalForCapabilityProvider, long jobDuration)
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
                    long earliestProcessingStart = mainScope.Scope.Start + setupDuration;
                    long earliestSetupStart = mainScope.Scope.Start;
                    setupSlot = new FSetupSlot(earliestSetupStart, earliestProcessingStart);
                    processingSlot = new FProcessingSlot(start: earliestProcessingStart, end: earliestProcessingStart + jobDuration);
                    var mainProcessingSlots = new List<ITimeRange>();
                    if (setupResources.Count != 0) // Setup RESOURCE Is Required.
                    {
                        // Machine includes Setup time in the Queuing Position if required.
                        var setupIsPossible = possibleSetupQueuingPositions.FirstOrDefault(setup
                                                            => SlotComparerSetup(mainScope.Scope, setup.Scope, jobDuration, setupDuration));
                        if (setupIsPossible == null)
                            continue;

                        earliestSetupStart = (new[] { setupIsPossible.Scope.Start, mainScope.Scope.Start }).Max();
                        earliestProcessingStart = earliestSetupStart + setupDuration;
                        setupSlot = new FSetupSlot(start: earliestSetupStart, end: earliestProcessingStart);
                        processingSlot = new FProcessingSlot(start: earliestProcessingStart, end: earliestProcessingStart + jobDuration);

                        /*
                         * CASE 1 ONLY MAIN RESOURCE AND SETUP RESSOURCE // WITH SETUP
                         */

                        if (processingResources.Count == 0)
                        {
                            mainProcessingSlots = new List<ITimeRange> { setupSlot, processingSlot };
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
                        mainProcessingSlots = new List<ITimeRange> { setupSlot, processingSlot };
                        mainResources.ForEach(resourceRef => possibleProcessingPosition.Add(resourceRef, CreateScopeConfirmation(mainProcessingSlots, earliestSetupStart), earliestProcessingStart));
                        break;
                    }
                    
                    var processingScope = new FScope(start: earliestProcessingStart, end: mainScope.Scope.End);
                    // seek for worker to process operation
                    var processingResourceSlot = FindProcessingSlot(possibleProcessingQueuingPositions.Where(x => x.IsQueueAble)
                                                                    .Select(x => x.Scope).ToList(), processingScope, jobDuration);
                    // set if a new is found 
                    if (processingResourceSlot == null)
                        continue;

                    processingSlot = new FProcessingSlot(start: processingResourceSlot.Start, end: processingResourceSlot.End);

                    mainProcessingSlots = new List<ITimeRange> { setupSlot, processingSlot };
                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainProcessingSlots, earliestSetupStart)));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(processingSlot, earliestSetupStart), processingSlot.Start));
                    if (setupResources.Count > 0)
                    {
                        setupResources.ForEach(x =>
                            possibleProcessingPosition.Add(x, CreateScopeConfirmation(new List<ITimeRange> { setupSlot }, earliestSetupStart )));
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
                        mainSlot = new FProcessingSlot(start: mainScope.Scope.Start, end: mainScope.Scope.Start + jobDuration);
                        mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start), mainSlot.Start));
                        break;
                    }
                    processingSlot = FindProcessingSlot(possibleProcessingQueuingPositions.Where(x => x.IsQueueAble)
                                                                                          .Select(x => x.Scope).ToList(), mainScope.Scope, jobDuration);
                    // set if a new is found 
                    if (processingSlot == null)
                        continue;

                    mainSlot = new FProcessingSlot(start: processingSlot.Start, end: processingSlot.Start + jobDuration);

                    mainResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start)));
                    processingResources.ForEach(x => possibleProcessingPosition.Add(x, CreateScopeConfirmation(mainSlot, mainSlot.Start), mainSlot.Start));
                    break;
                }

            }

            return possibleProcessingPosition;
        }

        private FScopeConfirmation CreateScopeConfirmation(List<ITimeRange> timeRanges, long setStartAt)
        {
            return new FScopeConfirmation(ListModule.OfSeq(timeRanges), setStartAt);
        }

        private FScopeConfirmation CreateScopeConfirmation(ITimeRange timeRange, long setStartAt)
        {
            return new FScopeConfirmation(ListModule.OfSeq(new List<ITimeRange>() { timeRange }), setStartAt);
        }


        private List<FQueueingScope> GetQueueingPositions(ProposalForCapabilityProvider proposalForCapabilityProvider, List<IActorRef> resources, long requiredDuration)
        {
            List<FProposal> setupResourceProposals = proposalForCapabilityProvider.GetProposalsFor(resources);
            var possibleSetupQueuingPositions = new List<FQueueingScope>();
            if (setupResourceProposals.Count > 0)
            {
                possibleSetupQueuingPositions.AddRange(
                    ProposalReducer(setupResourceProposals.ToArray()
                        , setupResourceProposals[0].PossibleSchedule as List<FQueueingScope>
                        , 1
                        , requiredDuration));
            }

            return possibleSetupQueuingPositions;
        }

        private List<FQueueingScope> ProposalReducer(FProposal[] proposalArray, List<FQueueingScope> possibleFQueueingPositions, int stage, long requiredDuration)
        {
            var reducedSlots = new List<FQueueingScope>();
            if (stage == proposalArray.Length)
                return possibleFQueueingPositions;


            foreach (var position in possibleFQueueingPositions)
            {
                var positionsToCompare = (proposalArray[stage].PossibleSchedule as List<FQueueingScope>);
                var pos = positionsToCompare.FirstOrDefault(x => SlotComparerBasic(position.Scope, x.Scope, requiredDuration));
                
                if (pos == null || pos.IsQueueAble is false)
                    continue;
                var min = (new[] { position.Scope.Start, pos.Scope.Start }).Max();
                var max = (new[] { position.Scope.End, pos.Scope.End }).Min();

                reducedSlots.Add(new FQueueingScope(isQueueAble: pos.IsQueueAble && position.IsQueueAble
                                                        , isRequieringSetup: pos.IsRequieringSetup && position.IsRequieringSetup
                                                        , new FScope(start: min, end: max)));
            }
            if (proposalArray.Length >= stage) return reducedSlots;

            stage++;
            reducedSlots = ProposalReducer(proposalArray, reducedSlots, stage, requiredDuration);
            return reducedSlots;
        }

        private FProcessingSlot FindProcessingSlot(List<ITimeRange> processingPositions, ITimeRange workingQueueSlot, long jobDuration)
        {
            var processingIsPossible = processingPositions.FirstOrDefault(processing =>
                                                    SlotComparerBasic(workingQueueSlot, processing, jobDuration));

            if (processingIsPossible == null) return null;

            var earliestProcessingStart = (new[] { processingIsPossible.Start, workingQueueSlot.Start }).Max();

            return new FProcessingSlot( earliestProcessingStart, earliestProcessingStart + jobDuration);
        }


        private bool SlotComparerSetup(ITimeRange mainResourcePos, ITimeRange toCompare, long processingTime, long setupTime) 
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var setupFit = (earliestStart + setupTime <= toCompare.End);
            // check Queue scope
            var queueFit = (earliestStart + setupTime + processingTime <= mainResourcePos.End);

            return (setupFit && queueFit);
        }


        private bool SlotComparerBasic(ITimeRange mainResourcePos, ITimeRange toCompare, long duration) 
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var fitToCompare = (earliestStart + duration <= toCompare.End);
            // check Queue scope
            var fitMainResource = (earliestStart + duration <= mainResourcePos.End);

            return (fitToCompare && fitMainResource);
        }
    }
}
