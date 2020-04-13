using Akka.TestKit.Xunit;
using Akka.Util.Internal;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static FProposals;
using static FQueueingPositions;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class ProposalManagement : TestKit
    {
        // private List<ProposalForCapabilityProvider> proposalSets = new List<ProposalForCapabilityProvider>();
        private static Guid jobKey = Guid.NewGuid();
        private static M_Resource OperatorResource = new M_Resource { Name = "Operator", IResourceRef = "Operator" };
        private static M_Resource MachineResource = new M_Resource { Name = "Machine", IResourceRef = "Machine" };
        private static M_Resource WorkerResource = new M_Resource { Name = "Worker", IResourceRef = "Worker" };

        private static readonly ProposalForCapabilityProvider _proposalForCapabilityProvider 
            =  new ProposalForCapabilityProvider(
                new M_ResourceCapabilityProvider
                    {
                        Name = "TestCapability",
                        ResourceSetups = new List<M_ResourceSetup>
                        {
                            new M_ResourceSetup {Name = "Operator", UsedInSetup = true, UsedInProcess = false, Resource = OperatorResource },
                            new M_ResourceSetup {Name = "Machine", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource},
                            new M_ResourceSetup {Name = "Worker", UsedInSetup = false, UsedInProcess = true, Resource = WorkerResource },
                        }
                    });

        public ProposalManagement()
        {

        }

        public static IEnumerable<object[]> GetProposalTestData()
        {
            yield return new object[] { // Test One with Setup
                    new List<FProposal>() { 
                        new FProposal(new List<FQueueingPosition> { // operator
                                new FQueueingPosition(true, false, 2, 6, 1),
                                new FQueueingPosition(true, false, 9, long.MaxValue, 1) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // machine
                                new FQueueingPosition(true, true, 3, 7, 3),
                                new FQueueingPosition(true, true, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // worker
                                new FQueueingPosition(true, false, 1, 1, 3),
                                new FQueueingPosition(true, false, 4, 6, 3),
                                new FQueueingPosition(true, false, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef, jobKey)
            }, /* Operator Start = */ 3L, /* Machine Start = */ 3L,/* Worker Start = */ 4L,"#1 Fit with setup"};
            yield return new object[] { // Test Two without setup
                    new List<FProposal>() {
                        new FProposal(new List<FQueueingPosition> { // operator
                                new FQueueingPosition(true, false, 4, 7, 1),
                                new FQueueingPosition(true, false, 10, long.MaxValue, 1) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // machine
                                new FQueueingPosition(true, false, 4, 6, 3),
                                new FQueueingPosition(true, true, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // worker
                                new FQueueingPosition(true, false, 1, 1, 3),
                                new FQueueingPosition(true, false, 4, 6, 3),
                                new FQueueingPosition(true, false, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef, jobKey)
            },/* Operator Start = */ 4L, /* Machine Start = */ 4L,/* Worker Start = */ 0, "#2 fit without setup"};
            yield return new object[] { // Test Three NoFit
                    new List<FProposal>() {
                        new FProposal(new List<FQueueingPosition> { // operator
                                new FQueueingPosition(true, false, 5, 7, 1),
                                new FQueueingPosition(true, false, 10, long.MaxValue, 1) }
                                , new FPostponeds.FPostponed(0)
                                , _proposalForCapabilityProvider.ProviderId
                                , OperatorResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // machine
                                new FQueueingPosition(true, true, 3, 6, 3),
                                new FQueueingPosition(true, true, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef, jobKey),
                        new FProposal(new List<FQueueingPosition> { // worker
                                new FQueueingPosition(true, false, 1, 1, 3),
                                new FQueueingPosition(true, false, 4, 6, 3),
                                new FQueueingPosition(true, false, 10, long.MaxValue, 3) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef, jobKey)
            },/* Operator Start = */ 10L, /* Machine Start = */ 10L,/* Worker Start = */ 11, "#3 No fit"};
            yield return new object[] { // Fits with Setup but setup pushes scope
                new List<FProposal>() {
                    new FProposal(new List<FQueueingPosition> { // operator
                            new FQueueingPosition(true, false, 4, 7, 1),
                            new FQueueingPosition(true, false, 10, long.MaxValue, 1) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // machine
                            new FQueueingPosition(true, true, 3, 8, 3),
                            new FQueueingPosition(true, true, 11, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // worker
                            new FQueueingPosition(true, false, 2, 8, 3),
                            new FQueueingPosition(true, false, 11, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef, jobKey)
                },/* Operator Start = */ 4L, /* Machine Start = */ 4L,/* Worker Start = */ 5, "#4 Fit with Setup but setup pushes scope"};
            yield return new object[] { // Exact Fit    
                new List<FProposal>() {
                    new FProposal(new List<FQueueingPosition> { // operator
                            new FQueueingPosition(true, false, 2, 3, 1),
                            new FQueueingPosition(true, false, 9, long.MaxValue, 1) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // machine
                            new FQueueingPosition(true, true, 3, 7, 3),
                            new FQueueingPosition(true, true, 10, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // worker
                            new FQueueingPosition(true, false, 4, 6, 3),
                            new FQueueingPosition(true, false, 10, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef, jobKey)
                },/* Operator Start = */ 3L, /* Machine Start = */ 3L,/* Worker Start = */ 4, "#5 Exact Fit"};
            yield return new object[] { // Detached Setup
                new List<FProposal>() {
                    new FProposal(new List<FQueueingPosition> { // operator
                            new FQueueingPosition(true, false, 2, 3, 1),
                            new FQueueingPosition(true, false, 10, long.MaxValue, 1) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // machine
                            new FQueueingPosition(true, true, 3, 8, 3),
                            new FQueueingPosition(true, true, 11, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef, jobKey),
                    new FProposal(new List<FQueueingPosition> { // worker
                            new FQueueingPosition(true, false, 5, 7, 3),
                            new FQueueingPosition(true, false, 10, long.MaxValue, 3) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef, jobKey)
                },/* Operator Start = */ 3L, /* Machine Start = */ 3L,/* Worker Start = */ 5, "#6 Detached Setup"};
        }

        [Theory]
        [MemberData(nameof(GetProposalTestData))]
        public void TestMe(List<FProposal> acknowledgedProposals, long estimatedOperatorStart, long estimatedMachineStart, long estimatedWorkerStart, string description)
        {
            Dictionary<string, FQueueingPosition> finalPositions = new Dictionary<string, FQueueingPosition>();

            // Get All Proposals that are InSetup and InProcess
            var mainResources = _proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => x.UsedInSetup && x.UsedInProcess).Select(x => x.Resource);
            Assert.Collection(mainResources, resource => Assert.Equal("Machine", resource.IResourceRef));

            var setupResources = _proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => x.UsedInSetup && !x.UsedInProcess).Select(x => x.Resource);
            Assert.Collection(setupResources, resource => Assert.Equal("Operator", resource.IResourceRef));

            var processingResources = _proposalForCapabilityProvider.GetCapabilityProvider.ResourceSetups
                .Where(x => !x.UsedInSetup && x.UsedInProcess).Select(x => x.Resource);
            Assert.Collection(processingResources, resource => Assert.Equal("Worker", resource.IResourceRef));



            // Get Proposals for all main resources
            List<FProposal> mainResourceProposals = new List<FProposal>();
            mainResources.ForEach(x => mainResourceProposals.AddRange(acknowledgedProposals.Where(y => y.ResourceAgent.Equals(x.IResourceRef))));

            List<FProposal> setupResourceProposals = new List<FProposal>();
            setupResources.ForEach(x => setupResourceProposals.AddRange(acknowledgedProposals.Where(y => y.ResourceAgent.Equals(x.IResourceRef))));

            List<FProposal> processingResourceProposals = new List<FProposal>();
            processingResources.ForEach(x => processingResourceProposals.AddRange(acknowledgedProposals.Where(y => y.ResourceAgent.Equals(x.IResourceRef))));

            // Check for possible Slots

            // For Each Slot
            // Check Required Worker + Possible Working Slot -> Return reduced Scope
            // Check Required Setup + Possible SetupSlot


            var proposalEnumerator = mainResourceProposals.GetEnumerator();
            while (proposalEnumerator.MoveNext())
            {
                var currentMainProposal = proposalEnumerator.Current;
                var possibleSchedules = currentMainProposal.PossibleSchedule as List<FQueueingPosition>;
                // check for worker here to reduce search scope
                
                // check for setup requirement and slots.
                foreach (var queueSlot in possibleSchedules)
                {
                    if (queueSlot.IsRequieringSetup)
                    {
                        foreach (var setupSlot in setupResourceProposals)
                        {
                            var setupQueueingPositions = setupSlot.PossibleSchedule as List<FQueueingPosition>;
                            // Machine includes Setup time in the Queuing Position if required.
                            var setupIsPossible = setupQueueingPositions.FirstOrDefault(setup 
                                                                                    => SlotComparer(queueSlot, setup, setup.EstimatedWork));
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
                            // create empty worker pos.
                            FQueueingPosition workerPosition = new FQueueingPosition(false, false, 0, 0, 0);


                            // seek for worker to process operation
                            foreach (var processingProposal in processingResourceProposals)
                            {
                                var processingPositions = processingProposal.PossibleSchedule as List<FQueueingPosition>;
                                var processingIsPossible = processingPositions.FirstOrDefault(processing =>
                                    SlotComparer(workingQueueSlot, processing, 0));

                                if(processingIsPossible == null)
                                    continue;

                                earliestProcessingStart = (new[] { processingIsPossible.Start, workingQueueSlot.Start }).Max();
                                
                                workerPosition = new FQueueingPosition(true, false, earliestProcessingStart,
                                    earliestProcessingStart + workingQueueSlot.EstimatedWork - 1,
                                    setupIsPossible.EstimatedWork);

                                // posible schedule found;
                                break;
                            }

                            // no worker found;
                            if (!workerPosition.IsQueueAble)
                                continue;

                            finalPositions.Add("Operator", operatorSlot);
                            finalPositions.Add("Worker", workerPosition);
                            workingQueueSlot = new FQueueingPosition(true, true,
                                operatorSlot.Start,
                                workerPosition.End, queueSlot.EstimatedWork + operatorSlot.EstimatedWork);
                            finalPositions.Add("Machine", workingQueueSlot);
                            break;
                        }
                    }
                    break;
                }
            }
            proposalEnumerator.Dispose();

            // assert for all
            finalPositions.ForEach(pair =>
            {
                switch (pair.Key)
                {
                    case "Operator":
                        Assert.Equal(estimatedOperatorStart, pair.Value.Start);
                        break;
                    case "Worker":
                        Assert.Equal(estimatedWorkerStart, pair.Value.Start);
                        break;
                    default:
                        Assert.Equal(estimatedMachineStart, pair.Value.Start);
                        break;
                }
            });
        }




        private bool SlotComparer(FQueueingPosition mainResourcePos, FQueueingPosition toCompare, long setupOffset)
        {
            // calculate posible earliest start
            long earliestStart = (new[] { toCompare.Start, mainResourcePos.Start }).Max();
            // check setup scope 
            var setupFit = (earliestStart + setupOffset - 1 <= toCompare.End);
            // check Queue scope
            var queueFit = (earliestStart + setupOffset + mainResourcePos.EstimatedWork - 1 <= mainResourcePos.End);

            return (setupFit && queueFit);
        }
    }
}

