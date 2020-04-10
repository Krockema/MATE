using System;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Resource = Master40.SimulationCore.Agents.ResourceAgent.Resource;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Internal;
using Akka.TestKit.Xunit;
using Akka.Util.Internal;
using Master40.SimulationCore.Agents.HubAgent;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            }, /* Estimated Start = */ 3L};
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
            }, /* Estimated Start = */ 4L};
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
            }, /* Estimated Start = */ 10};
        }

        [Theory]
        [MemberData(nameof(GetProposalTestData))]
        public void TestMe(List<FProposal> acknowledgedProposals, long estimatedStart)
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
                
                
                // ToDo
                
                
                // check for setup requirement and slots.
                foreach (var queueSlot in possibleSchedules)
                {
                    if (queueSlot.IsRequieringSetup)
                    {
                        var setupQueueingPositions = setupResourceProposals.First().PossibleSchedule as List<FQueueingPosition>;
                        // Machine includes Setup time in the Queuing Position if required.
                        var setupIsPossible = setupQueueingPositions
                            .FirstOrDefault(setup => (setup.Start >= queueSlot.Start
                                                       || setup.Start <= queueSlot.End - queueSlot.EstimatedWork + 1)
                                                       && setup.End >= queueSlot.End - queueSlot.EstimatedWork + setup.EstimatedWork + 1
                                                       && setup.Start <= queueSlot.End - queueSlot.EstimatedWork + 1);
                        if (setupIsPossible == null)
                            continue;

                        long earliestStart = (new [] {setupIsPossible.Start, queueSlot.Start}).Max();

                        finalPositions.Add("Operator" ,new FQueueingPosition(true, true, earliestStart, earliestStart + setupIsPossible.EstimatedWork - 1, setupIsPossible.EstimatedWork ));
                        finalPositions.Add("Machine", new FQueueingPosition(true, true, earliestStart, earliestStart + queueSlot.EstimatedWork -1, queueSlot.EstimatedWork ));

                        break;
                    } else { 
                        finalPositions.Add("Machine", new FQueueingPosition(true, true, queueSlot.Start, queueSlot.Start + queueSlot.EstimatedWork - 1, queueSlot.EstimatedWork));
                        break;
                    }
                }
            }
            proposalEnumerator.Dispose();

            // assert for all
            finalPositions.ForEach(pair => Assert.Equal(estimatedStart, pair.Value.Start));
        }

    }
}

