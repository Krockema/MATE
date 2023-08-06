using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.TestKit.Xunit;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Agents.HubAgent.Types;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Scopes;
using Mate.Test.Online.Preparations;
using Xunit;

/// <summary>
/// Test objectiv
/// Hub.Types.CreatePossibleProcessingPositions
/// </summary>

namespace Mate.Test.Online.Agents.Types
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
                        new M_ResourceSetup {Name = "Operator", UsedInSetup = true, UsedInProcess = false, Resource = OperatorResource, SetupTime = TimeSpan.FromMinutes(2)},
                        new M_ResourceSetup {Name = "Machine", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource},
                        new M_ResourceSetup {Name = "Worker", UsedInSetup = false, UsedInProcess = true, Resource = WorkerResource },
                        new M_ResourceSetup {Name = "Machine2", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource2},
                    }
                });

            yield return new object[] { // Test One with Setup
                    new List<ProposalRecord>() {
                        new ProposalRecord(new List<QueueingScopeRecord> { // operator
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(2), Time.ZERO.Value + TimeSpan.FromMinutes(6))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(9), Time.ZERO.Value + TimeSpan.FromMinutes(100)))}
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef as IActorRef
                            , jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // machine
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3),Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100)))}
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef
                            , jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // worker
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(1), Time.ZERO.Value + TimeSpan.FromMinutes(1))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4),Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100)))}
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // machine2
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4), Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100)))}
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource2.IResourceRef as IActorRef, jobKey),
            }, /* OperatorResource Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(4), /* MachineResource = */ Time.ZERO.Value + TimeSpan.FromMinutes(4),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(6),"#1 Fit with setup"};
            yield return new object[] { // Test Two without setup
                    new List<ProposalRecord>() {
                        new ProposalRecord(new List<QueueingScopeRecord> { // operator
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4),Time.ZERO.Value + TimeSpan.FromMinutes(6))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100)))}
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , OperatorResource.IResourceRef as IActorRef, jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // machine
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4), Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef, jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // worker
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(1), Time.ZERO.Value + TimeSpan.FromMinutes(1))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4), Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey)
            },/* Operator Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(0), /* Machine Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(4),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(4), "#2 fit without setup"};
            yield return new object[] { // Test Three NoFit
                    new List<ProposalRecord>() {
                        new ProposalRecord(new List<QueueingScopeRecord> { // operator
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(5),Time.ZERO.Value + TimeSpan.FromMinutes(7))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                                , new PostponedRecord(TimeSpan.FromMinutes(0))
                                , _proposalForCapabilityProvider.ProviderId
                                , OperatorResource.IResourceRef as IActorRef
                                , jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // machine
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3), Time.ZERO.Value + TimeSpan.FromMinutes(6))),
                                new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , MachineResource.IResourceRef as IActorRef, jobKey),
                        new ProposalRecord(new List<QueueingScopeRecord> { // worker
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(1), Time.ZERO.Value + TimeSpan.FromMinutes(1))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4), Time.ZERO.Value + TimeSpan.FromMinutes(6))),
                                new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                            , new PostponedRecord(TimeSpan.FromMinutes(0))
                            , _proposalForCapabilityProvider.ProviderId
                            , WorkerResource.IResourceRef as IActorRef, jobKey)
            },/* Operator Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(10), /* Machine Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(10),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(12), "#3 No fit"};
            yield return new object[] { // Fits with Setup but setup pushes scope
                new List<ProposalRecord>() {
                    new ProposalRecord(new List<QueueingScopeRecord> { // operator
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(4), Time.ZERO.Value + TimeSpan.FromMinutes(7))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // machine
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3), Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(11), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // worker
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(2),Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(11), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(4), /* Machine Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(4),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(6), "#4 Fit with Setup but setup pushes scope"};
            yield return new object[] { // Exact Fit    
                new List<ProposalRecord>() {
                    new ProposalRecord(new List<QueueingScopeRecord> { // operator
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3),Time.ZERO.Value + TimeSpan.FromMinutes(5))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(9), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // machine
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3), Time.ZERO.Value + TimeSpan.FromMinutes(7))),
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // worker
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(5),Time.ZERO.Value + TimeSpan.FromMinutes(7))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(3), /* Machine Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(3),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(5), "#5 Exact Fit"};
            yield return new object[] { // Detached Setup
                new List<ProposalRecord>() {
                    new ProposalRecord(new List<QueueingScopeRecord> { // operator
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(2),Time.ZERO.Value + TimeSpan.FromMinutes(3))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , OperatorResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // machine
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(3), Time.ZERO.Value + TimeSpan.FromMinutes(8))),
                            new QueueingScopeRecord(true, true, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(11), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , MachineResource.IResourceRef as IActorRef, jobKey),
                    new ProposalRecord(new List<QueueingScopeRecord> { // worker
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(5), Time.ZERO.Value + TimeSpan.FromMinutes(7))),
                            new QueueingScopeRecord(true, false, new ScopeRecord(Time.ZERO.Value + TimeSpan.FromMinutes(10), Time.ZERO.Value + TimeSpan.FromMinutes(100))) }
                        , new PostponedRecord(TimeSpan.FromMinutes(0))
                        , _proposalForCapabilityProvider.ProviderId
                        , WorkerResource.IResourceRef as IActorRef, jobKey)
                },/* Operator Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(11L), /* Machine Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(11L),/* Worker Start = */ Time.ZERO.Value + TimeSpan.FromMinutes(13), "#6 Detached Setup"};
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
        public void CreatePossibleProcessingPositionsFromProposalsOnHub(List<ProposalRecord> acknowledgedProposals,
            DateTime estimatedOperatorStart, DateTime estimatedMachineStart, DateTime estimatedWorkerStart, string description)
        {
            var proposalManager = new ProposalManager();

            var jobCinConfirmation = TypeFactory.CreateDummyJobConfirmations(Time.ZERO.Value, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2L));

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
            var operatorPath = ((IActorRef) OperatorResource.IResourceRef).Path.Name;
            var workerPath = ((IActorRef)WorkerResource.IResourceRef).Path.Name;


            foreach (var pair in toAssert._queuingDictionary)
            {
                if (operatorPath == pair.Key.Path.Name)
                {
                    Assert.Equal(estimatedOperatorStart, pair.Value.GetScopeStart());
                    continue;
                }
                if (workerPath == pair.Key.Path.Name)
                {
                    Assert.Equal(estimatedWorkerStart, pair.Value.GetScopeStart());
                    continue;
                }
                Assert.Equal(estimatedMachineStart, pair.Value.GetScopeStart());
            }
        }
    }
}

