using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Master40.SimulationCore.Agents.Guardian;
using Xunit;
using Master40.XUnitTest.Moc;
using System.Collections.Generic;

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
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            MasterDBInitializerSimple.DbInitialize(_ctx);
            ResultDBInitializerBasic.DbInitialize(_resultContext);
        }

        [Fact]
        public void SimulationMocUp()
        {
            var contextProbe = this.CreateTestProbe("Context");
            var targetProbe = this.CreateTestProbe("Target");
            var sourceProbe = this.CreateTestProbe("Source");

            var simContext = Sys.ActorOf(Moc.SimulationContext.Props(contextProbe));

            sourceProbe.Send(simContext, new TestMessage(targetProbe, true));

            AwaitCondition(() => targetProbe.HasMessages);
            contextProbe.ExpectMsg<TestMessage>(a => (bool)a.Message == true);
        }


        //[Fact]
        public void ContractTestCreateDispoAgent()
        {
            // Prepare the System Mocup
            var testProbe = this.CreateTestProbe("ContextProbe");
            var systemProbe = this.CreateTestProbe("SystemProbe");
            var directoryProbe = this.CreateTestProbe("StorageDirectoryProbe");
            var guardianProbe = this.CreateTestProbe("ContractGuardian");
            var inbox = Inbox.Create(Sys);
            var simContext = Sys.ActorOf(Moc.SimulationContext.Props(testProbe));
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSupervisorAgent(systemProbe);
            agentPaths.SetStorageDirectory(directoryProbe);
            var contractGuard = Sys.ActorOf(Guardian.Props(agentPaths, 0, true), "ContractGuard");
                simContext.Tell(BasicInstruction.Initialize.Create(contractGuard, GuardianBehaviour.Get(CreatorOptions.ContractCreator)));
                agentPaths.Guardians.Add(GuardianType.Dispo, contractGuard);

            // create Contract Agent and inject behaviour
            var contract = Sys.ActorOf(Contract.Props(actorPaths: agentPaths
                                                       , time: 1
                                                       , debug: true), "Contract-Test-Agent");
            simContext.Tell(BasicInstruction.Initialize.Create(contract, ContractBehaviour.Get()));


            // Create a Order with custom Article
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            // tell teh Contract agent
            simContext.Tell(Contract.Instruction.StartOrder.Create(orderPart, contract));

            // check if Child is Created correctly and sending its fist Request to the StorageProbe
            AwaitCondition(() => directoryProbe.HasMessages);
            dynamic item = directoryProbe.LastMessage;
            Assert.Equal(item.Message, "Bear");
        }

        [Fact]
        public void CreateStockagent()
        {
            // init System
            var simContext = this.CreateTestProbe();
            var simSystem = this.CreateTestProbe();
            var inbox = Inbox.Create(Sys);
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSupervisorAgent(simSystem);
            agentPaths.SetHubDirectoryAgent(this.TestActor);

            // init Stock 
            var stock = new M_Stock { Article = new M_Article { Name = "Bär" } };
            var storageAgent = Sys.ActorOf(Storage.Props(agentPaths, 0, true, simSystem));
            storageAgent.Tell(BasicInstruction.Initialize.Create(this.TestActor, StorageBehaviour.Get(stock)));

            Within(TimeSpan.FromSeconds(5), () => simContext.HasMessages);
           
        }

        [Fact]
        public void CreateDispoAgent()
        {
            // init System
            var simContext = this.CreateTestProbe();
            var simSystem = this.CreateTestProbe();
            var contractGuard = this.CreateTestProbe();
            var inbox = Inbox.Create(Sys);
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSupervisorAgent(simSystem);
            agentPaths.SetHubDirectoryAgent(this.TestActor);
            agentPaths.AddGuardian(GuardianType.Contract, contractGuard);
            // init Contract
            var contract = Sys.ActorOf(Contract.Props(actorPaths: agentPaths
                                                                   , time: 1
                                                                   , debug: true), "Contract-Test-Agent");
            simContext.Tell(BasicInstruction.Initialize.Create(contract, ContractBehaviour.Get()));


            // Create a Order with custom Article
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            // tell teh Contract agent
            simContext.Tell(Contract.Instruction.StartOrder.Create(orderPart, contract));

            Within(TimeSpan.FromSeconds(5), () => contractGuard.HasMessages);

        }
    }
}