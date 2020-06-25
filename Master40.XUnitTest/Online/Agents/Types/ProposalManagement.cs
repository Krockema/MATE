using Akka.Actor;
using Akka.Actor.Internal;
using Akka.TestKit;
using Akka.TestKit.Xunit;
using Akka.Util.Internal;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.XUnitTest.Online.Preparations;
using System;
using System.Collections.Generic;
using System.Linq;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using Xunit;
using static FProposals;
using static FQueueingScopes;
using static FScopes;

/// <summary>
/// Test objectiv
/// Hub.Types.CreatePossibleProcessingPositions
/// </summary>

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class ProposalManagement
    {
        private List<ProposalForCapabilityProvider> proposalSets = new List<ProposalForCapabilityProvider>();
        private static Guid jobKey = Guid.NewGuid();

        private static M_Resource OperatorResource { get; set; }
        private static M_Resource MachineResource { get; set; }
        private static M_Resource WorkerResource { get; set; }
        private static M_Resource MachineResource2 { get; set; }


        private static ProposalForCapabilityProvider _proposalForCapabilityProvider { get; set; }


        public static IEnumerable<object[]> GetProposalTestData()
        {
            var testKit = new TestKit();
            var operatorTestProbe = testKit.CreateTestProbe();
            var machineTestProbe = testKit.CreateTestProbe();
            var workerTestProbe = testKit.CreateTestProbe();
            var machine2TestProbe = testKit.CreateTestProbe();

            OperatorResource = new M_Resource { Name = "Operator", IResourceRef = operatorTestProbe.Ref };
            MachineResource = new M_Resource { Name = "Machine", IResourceRef = machineTestProbe.Ref };
            WorkerResource = new M_Resource { Name = "Worker", IResourceRef = workerTestProbe.Ref };
            MachineResource2 = new M_Resource { Name = "Machine2", IResourceRef = machine2TestProbe.Ref };

            _proposalForCapabilityProvider = new ProposalForCapabilityProvider(
                new M_ResourceCapabilityProvider
                {
                    Name = "TestCapability",
                    ResourceSetups = new List<M_ResourceSetup>
                    {
                        new M_ResourceSetup {Name = "Operator", UsedInSetup = true, UsedInProcess = false, Resource = OperatorResource, SetupTime = 2},
                        new M_ResourceSetup {Name = "Machine", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource},
                        new M_ResourceSetup {Name = "Worker", UsedInSetup = false, UsedInProcess = true, Resource = WorkerResource },
                        new M_ResourceSetup {Name = "Machine2", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource2},
                    }
                });

            yield return new object[] { // Test One with Setup
                    new List<FProposal>() {
                        new FProposal(new List<FQueueingScope> { // operator
                                new FQueueingScope(true, false, new FScope(2, 6)),
                                new FQueueingScope(true, false, new FScope(9, long.MaxValue))}
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef as IActorRef
                            , jobKey),
                        new FProposal(new List<FQueueingScope> { // machine
                                new FQueueingScope(true, true, new FScope(3, 8)),
                                new FQueueingScope(true, true, new FScope(10, long.MaxValue))}
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef
                            , jobKey),
                        new FProposal(new List<FQueueingScope> { // worker
                                new FQueueingScope(true, false, new FScope(1, 1)),
                                new FQueueingScope(true, false, new FScope(4, 8)),
                                new FQueueingScope(true, false, new FScope(10, long.MaxValue))}
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey),
                        new FProposal(new List<FQueueingScope> { // machine2
                                new FQueueingScope(true, true, new FScope(4, 8)),
                                new FQueueingScope(true, true, new FScope(10, long.MaxValue))}
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource2.IResourceRef as IActorRef, jobKey),
            }, /* OperatorResource Start = */ 4L, /* MachineResource = */ 4L,/* Worker Start = */ 6L,"#1 Fit with setup"};
            yield return new object[] { // Test Two without setup
                    new List<FProposal>() {
                        new FProposal(new List<FQueueingScope> { // operator
                                new FQueueingScope(true, false, new FScope(4, 6)),
                                new FQueueingScope(true, false, new FScope(10, long.MaxValue))}
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef as IActorRef, jobKey),
                        new FProposal(new List<FQueueingScope> { // machine
                                new FQueueingScope(true, false, new FScope(4, 8)),
                                new FQueueingScope(true, true, new FScope(10, long.MaxValue)) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef, jobKey),
                        new FProposal(new List<FQueueingScope> { // worker
                                new FQueueingScope(true, false, new FScope(1, 1)),
                                new FQueueingScope(true, false, new FScope(4, 8)),
                                new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey)
            },/* Operator Start = */ 0L, /* Machine Start = */ 4L,/* Worker Start = */ 4, "#2 fit without setup"};
            yield return new object[] { // Test Three NoFit
                    new List<FProposal>() {
                        new FProposal(new List<FQueueingScope> { // operator
                                new FQueueingScope(true, false, new FScope(5, 7)),
                                new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                                , new FPostponeds.FPostponed(0)
                                , _proposalForCapabilityProvider.ProviderId
                                , OperatorResource.IResourceRef as IActorRef
                                , jobKey),
                        new FProposal(new List<FQueueingScope> { // machine
                                new FQueueingScope(true, true, new FScope(3, 6)),
                                new FQueueingScope(true, true, new FScope(10, long.MaxValue)) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef, jobKey),
                        new FProposal(new List<FQueueingScope> { // worker
                                new FQueueingScope(true, false, new FScope(1, 1)),
                                new FQueueingScope(true, false, new FScope(4, 6)),
                                new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                            , new FPostponeds.FPostponed(0)
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey)
            },/* Operator Start = */ 10L, /* Machine Start = */ 10L,/* Worker Start = */ 12, "#3 No fit"};
            yield return new object[] { // Fits with Setup but setup pushes scope
                new List<FProposal>() {
                    new FProposal(new List<FQueueingScope> { // operator
                            new FQueueingScope(true, false, new FScope(4, 7)),
                            new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // machine
                            new FQueueingScope(true, true, new FScope(3, 8)),
                            new FQueueingScope(true, true, new FScope(11, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // worker
                            new FQueueingScope(true, false, new FScope(2, 8)),
                            new FQueueingScope(true, false, new FScope(11, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ 4L, /* Machine Start = */ 4L,/* Worker Start = */ 6, "#4 Fit with Setup but setup pushes scope"};
            yield return new object[] { // Exact Fit    
                new List<FProposal>() {
                    new FProposal(new List<FQueueingScope> { // operator
                            new FQueueingScope(true, false, new FScope(3, 5)),
                            new FQueueingScope(true, false, new FScope(9, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // machine
                            new FQueueingScope(true, true, new FScope(3, 7)),
                            new FQueueingScope(true, true, new FScope(10, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // worker
                            new FQueueingScope(true, false, new FScope(5, 7)),
                            new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ 3L, /* Machine Start = */ 3L,/* Worker Start = */ 5L, "#5 Exact Fit"};
            yield return new object[] { // Detached Setup
                new List<FProposal>() {
                    new FProposal(new List<FQueueingScope> { // operator
                            new FQueueingScope(true, false, new FScope(2, 3)),
                            new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // machine
                            new FQueueingScope(true, true, new FScope(3, 8)),
                            new FQueueingScope(true, true, new FScope(11, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new FProposal(new List<FQueueingScope> { // worker
                            new FQueueingScope(true, false, new FScope(5, 7)),
                            new FQueueingScope(true, false, new FScope(10, long.MaxValue)) }
                        , new FPostponeds.FPostponed(0)
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ 11L, /* Machine Start = */ 11L,/* Worker Start = */ 13L, "#6 Detached Setup"};
        }

        /// <summary>
        /// test series
        /// </summary>
        /// <param name="acknowledgedProposals"></param>
        /// <param name="estimatedOperatorStart"></param>
        /// <param name="estimatedMachineStart"></param>
        /// <param name="estimatedWorkerStart"></param>
        /// <param name="description"></param>

        [Theory]
        [MemberData(nameof(GetProposalTestData))]
        public void CreatePossibleProcessingPositionsFromProposalsOnHub(List<FProposal> acknowledgedProposals,
            long estimatedOperatorStart, long estimatedMachineStart, long estimatedWorkerStart, string description)
        {
            var proposalManager = new ProposalManager();

            var jobCinConfirmation = TypeFactory.CreateDummyFJobConfirmations(0, 2, 2L);

            foreach (var proposal in acknowledgedProposals)
            {
                _proposalForCapabilityProvider.Add(proposal);
            }

            proposalSets.Add(_proposalForCapabilityProvider);

            var possibleProcessingPositions =
                proposalManager.CreatePossibleProcessingPositions(proposalSets, jobCinConfirmation.Job);

            _proposalForCapabilityProvider.RemoveAll();

            // assert for all
            System.Diagnostics.Debug.WriteLine("Evaluating: " + description);
            var toAssert = possibleProcessingPositions.OrderBy(x => x._queuingDictionary).First();
            toAssert._queuingDictionary.ForEach(pair =>
            {
                switch (pair.Key.ToString())
                {
                    case string a when a.Contains("testActor2"): //Operator
                        Assert.Equal(estimatedOperatorStart, pair.Value.GetScopeStart());
                        break;
                    case string b when b.Contains("testActor4"): //Worker
                        Assert.Equal(estimatedWorkerStart, pair.Value.GetScopeStart());
                        break;
                    default: // Machine
                        Assert.Equal(estimatedMachineStart, pair.Value.GetScopeStart());
                        break;
                }
            });
        }
    }
}

