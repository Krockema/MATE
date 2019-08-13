using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent.Behaviour;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using Master40.XUnitTest.Moc;
using System;
using Xunit;
using static Master40.SimulationCore.Agents.ContractAgent.Contract;

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
            var message = Instruction.StartOrder.Create(orderPart, contractAgentRef);
            var behave = Factory.Get(DB.Enums.SimulationType.None);
            var simContext = CreateTestProbe();
            var actorPaths = AgentMoc.Create(this, simContext);
            var agent = AgentMoc.Create(actorPaths: actorPaths
                                       , principal: null);
            behave.Action(agent, message);

            Assert.Equal("Bear", ((IDefaultProperties)behave).fArticle.Article.Name);
            var item = simContext.ReceiveOne(TimeSpan.FromSeconds(5)) as CreateChild;
            Assert.NotNull(item);
        }


    }
}