using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent.Behaviour;
using Master40.XUnitTest.Preparations;
using System;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.Guardian;
using Xunit;
using static Master40.SimulationCore.Agents.Guardian.Instruction;

namespace Master40.XUnitTest.Agents.Contract.Behaviour
{
    public class Default : TestKit
    {
        public Default()
        {

        }

        [Fact]
        public void StartOrder()
        {
            var contractAgentRef = CreateTestProbe();
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            var message = SimulationCore.Agents.ContractAgent.Contract.Instruction.StartOrder.Create(orderPart, contractAgentRef);
            var behave = Factory.Get(DB.Enums.SimulationType.None);
            var simContext = CreateTestProbe();
            var actorPaths = AgentMoc.CreateActorPaths(this, simContext);
            var agent = AgentMoc.CreateAgent(actorPaths: actorPaths
                                           , principal: null,
                                             behaviour: behave);
            behave.Action(agent, message);

            Assert.Equal("Bear", ((IDefaultProperties)behave)._fArticle.Article.Name);
            var item = simContext.FishForMessage(msg => msg is CreateChild, (TimeSpan.FromSeconds(5))) as CreateChild;
            Assert.NotNull(item);
        }


    }
}