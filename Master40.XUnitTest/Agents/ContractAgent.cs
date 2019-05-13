using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Master40.Simulation.CLI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Agents.SupervisorAegnt;
using Master40.XUnitTest.Moc;

namespace Master40.XUnitTest.Agents
{
    public class ContractAgent : TestKit
    {
        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                                                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                                    .Options);

        ResultContext _resultContext = new ResultContext(new DbContextOptionsBuilder<ResultContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryResults")
            .Options);


        public ContractAgent()
        {


            //_ctx.Database.EnsureDeleted();
            MasterDBInitializerSmall.DbInitialize(_ctx);
            ResultDBInitializerBasic.DbInitialize(_resultContext);
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
        public async Task PruneTest()
        {
            var tree = new Tree();
            var what = tree;
            if (what == null)
            {
                throw new NullReferenceException();
            }

        }

        [Fact]
        public async Task PriorityRule()
        {
            await Task.Run(() =>
            {
                var probe = this.CreateTestProbe();

                var wi = MessageFactory.ToWorkItem(new M_Operation() { Duration = 5 }, 15, ElementStatus.Created, probe, 0);

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
            // Prepare the System Mocup
            var testProbe = this.CreateTestProbe("ContextProbe");
            var systemProbe = this.CreateTestProbe("SystemProbe");
            var directoryProbe = this.CreateTestProbe("StorageDirectoryProbe");
            var inbox = Inbox.Create(Sys);
            var simContext = Sys.ActorOf(SimulationContextMoc.Props(testProbe));
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSystemAgent(systemProbe);
            agentPaths.SetStorageDirectory(directoryProbe);


            // create Contract Agent and inject behaviour
            var contract = Sys.ActorOf(Contract.Props(actorPaths: agentPaths
                                                               , time: 1
                                                               , debug: true), "Contract-Test-Agent");
            var behave = ContractBehaviour.Get();
            simContext.Tell(BasicInstruction.Initialize.Create(contract, behave));


            // Create a Order with custom Article
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article() { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            // tell teh Contract agent
            simContext.Tell(Contract.Instruction.StartOrder.Create(orderPart, contract));

            // check if Child is Created correctly and sending its fist Request to the StorageProbe
            dynamic item = directoryProbe.ReceiveOne(TimeSpan.FromSeconds(50));
            Assert.Equal(item.Message, "Bear");
            
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
            var simContext = new SimulationCore.AgentSimulation(true, _ctx, _resultContext, new ConsoleHub());
            var simConfig = _resultContext.SimulationConfigurations.FirstOrDefault();
            simConfig.OrderQuantity = 0;
            var simulation = await simContext.InitializeSimulation(simConfig, new SimulationConfig(false, 480));

            // simulation.ActorSystem.EventStream.Subscribe(testProbe, typeof(DirectoryAgent.Instruction.CreateMachineAgents));

            await Task.Delay(5000);
            // 
            var orderParts = _ctx.CustomerOrderParts.Single(x => x.Id == 1);

            var contract = Supervisor.Instruction.CreateContractAgent.Create(orderParts, simContext.ActorPaths.SystemAgent.Ref);
            simulation.SimulationContext.Tell(contract, ActorRefs.NoSender);

            // simContext.Run();
        }


        [Fact]
        public void DirectoryTest()
        {
            var simContext = this.CreateTestProbe();
            var inbox = Inbox.Create(Sys);
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            var directory = Sys.ActorOf(Directory.Props(agentPaths, 0, true), "DirectoryAgent");

        }

        [Fact]
        public async Task DispoArticleRequestFromStorage()
        {
            var simContext = new SimulationCore.AgentSimulation(true, _ctx, _resultContext, new ConsoleHub());
            var simConfig = _resultContext.SimulationConfigurations.First();
            simContext.InitializeSimulation(simConfig, new SimulationConfig(false, 480)).Wait();
            simContext.Run();
        }

        [Fact]
        public void CreateStockagent()
        {
            // init System
            var simContext = this.CreateTestProbe();
            var simSystem = this.CreateTestProbe();
            var inbox = Inbox.Create(Sys);
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSystemAgent(simSystem);
            agentPaths.SetHubDirectoryAgent(this.TestActor);

            // init Stock 
            var stock = new M_Stock { Article = new M_Article { Name = "Bär" } };
            var storageAgent = Sys.ActorOf(Storage.Props(agentPaths, 0, true, simSystem));
            storageAgent.Tell(BasicInstruction.Initialize.Create(this.TestActor, StorageBehaviour.Get(stock)));

            Within(TimeSpan.FromSeconds(5), () => simContext.HasMessages);
           
        }
    }
}