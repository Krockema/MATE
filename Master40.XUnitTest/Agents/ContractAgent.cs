using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSim.Interfaces;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Models;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AkkaSim.Definitions;

namespace Master40.XUnitTest.Agents
{
    public class ContractAgent : TestKit
    {
        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                                                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                                    .Options);
        public ContractAgent()
        {


            //_ctx.Database.EnsureDeleted();
            MasterDBInitializerSmall.DbInitialize(_ctx);
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            //_productionDomainContext.Database.EnsureDeleted();
            //_productionDomainContext.Database.EnsureCreated();
            //MasterDBInitializerLarge.DbInitialize(_productionDomainContext);

        }

        [Fact]
        public void MocUpTest()
        {
            var contextProbe = this.CreateTestProbe("Context");
            var targetProbe = this.CreateTestProbe("Target");
            var sourceProbe = this.CreateTestProbe("Source");

            var simContext = Sys.ActorOf(SimulationContextMoc.Props(contextProbe));

            sourceProbe.Send(simContext, new TestMessage(targetProbe, true));

            AwaitCondition(() => targetProbe.HasMessages);
            contextProbe.ExpectMsg<TestMessage>(a => (bool)a.Message == true);
        }


        [Fact]
        public async Task PriorityRule()
        {
            await Task.Run(() =>
            {
                var probe = this.CreateTestProbe();

                var wi = MessageFactory.ToWorkItem(new WorkSchedule() { Duration = 5 }, 15, ElementStatus.Created, probe, 0);

                var w1 = wi.Priority(10);
                var w2 = wi.GetPriority(10);
                var w3 = wi.ItemPriority;
                Debug.WriteLine(w1);
                Debug.WriteLine(w2);

                Assert.Equal(w1, w2);
                Assert.Equal(w1, w3);
            });
        }


        [Fact]
        public void ContractTestCreateDispoAgent()
        {
            var testProbe = this.CreateTestProbe();
            var storageProbe = this.CreateTestProbe();
            var systemProbe = this.CreateTestProbe();
            var simContext = Sys.ActorOf(SimulationContextMoc.Props(testProbe));

            var agentPaths = new ActorPaths(simContext);
            agentPaths.SetSystemAgent(storageProbe);
            agentPaths.SetStorageDirectory(storageProbe);
            var contract = Sys.ActorOf(SimulationCore.Agents.ContractAgent.Props(actorPaths: agentPaths
                                                               , time: 1
                                                               , debug: true), "Contract-Test-Agent");

            //var contract = Sys.CreateActor(() => new ContractAgent(agentPaths, 1, true));
            // OrderPart Probe
            var order = new Order() { DueTime = 0, Id = 1 };
            var orderPart = new OrderPart() { Article = new Article() { Name = "Bear" }, Quantity = 1, Id = 1, OrderId = 1, Order = order };

            var behave = ContractBehaviour.Default();

            simContext.Tell(BasicInstruction.Initialize.Create(behave, contract));
            simContext.Tell(SimulationCore.Agents.ContractAgent.Instruction.StartOrder.Create(orderPart, contract));

            //.ExpectMsgFrom<RegisterUser>
            // AwaitCondition(() => testProbe.HasMessages);
            // Within(TimeSpan.FromSeconds(5), () =>
            // {
            //testProbe.FishForMessage<DispoAgent.Instruction.RequestArticle>();
            //   Console.WriteLine(((RequestItem)((DispoAgent.Instruction.RequestArticle)testProbe.LastMessage).Message).Article.Name);
            //
            // });



            // ExpectMsg("done", TimeSpan.FromSeconds(1));
            //
            // // the action needs to finish within 3 seconds
            // Within(TimeSpan.FromSeconds(3), () => {
            //     subject.Tell("hello", this.TestActor);
            //
            //     // This is a demo: would normally use expectMsgEquals().
            //     // Wait time is bounded by 3-second deadline above.
            //     AwaitCondition(() => probe.HasMessages);
            //
            //     // response must have been enqueued to us before probe
            //     ExpectMsg("world", TimeSpan.FromSeconds(0));
            //     // check that the probe we injected earlier got the msg
            //     probe.ExpectMsg("hello", TimeSpan.FromSeconds(0));
            //
            //     Assert.Equal(TestActor, probe.Sender);
            //
            //     // Will wait for the rest of the 3 seconds
            //     ExpectNoMsg();
            // });

        }
        [Fact]
        public async void InitialisationAsync()
        {
            var simContext = new SimulationCore.AgentSimulation(true, _ctx, new Moc.MessageHub());
            var simConfig = _ctx.SimulationConfigurations.FirstOrDefault();
            simConfig.OrderQuantity = 0;
            var simulation = await simContext.InitializeSimulation(simConfig);

            // simulation.ActorSystem.EventStream.Subscribe(testProbe, typeof(DirectoryAgent.Instruction.CreateMachineAgents));

            await Task.Delay(5000);
            // 
            var orderParts = _ctx.OrderParts.Single(x => x.Id == 1);

            var contract = SystemAgent.Instruction.CreateContractAgent.Create(orderParts, simContext.ActorPaths.SystemAgent.Ref);
            simulation.SimulationContext.Tell(contract, ActorRefs.NoSender);

            // simContext.Run();
        }


        [Fact]
        public void DirectoryTest()
        {
            var simContext = this.CreateTestProbe();
            var agentPaths = new ActorPaths(simContext);
            var directory = Sys.ActorOf(DirectoryAgent.Props(agentPaths, 0, true), "DirectoryAgent");

        }

        [Fact]
        public async Task DispoArticleRequestFromStorage()
        {
            var simContext = new SimulationCore.AgentSimulation(true, _ctx, new Moc.MessageHub());
            var simConfig = _ctx.SimulationConfigurations.First();
            simContext.InitializeSimulation(simConfig).Wait();
            simContext.Run();
        }

        [Fact]
        public void CreateStockagent()
        {
            // init System
            var simContext = this.CreateTestProbe();
            var simSystem = this.CreateTestProbe();
            var agentPaths = new ActorPaths(simContext);
            agentPaths.SetSystemAgent(simSystem);
            agentPaths.SetHubDirectoryAgent(this.TestActor);

            // init Stock 
            var stock = new Stock { Article = new Article { Name = "Bär" } };
            var directory = Sys.ActorOf(StorageAgent.Props(agentPaths, 0, true));
            directory.Tell(BasicInstruction.Initialize.Create(StorageBehaviour.Default(stock), this.TestActor));

            AwaitCondition(() => simContext.HasMessages);
            Within(TimeSpan.FromSeconds(5), () =>
            {
                simContext.ExpectMsg<DirectoryAgent.Instruction.RequestRessourceAgent>(x => x.Message == "Bär");
            });
        }
    }
}