using System;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ContractAgent.Behaviour;
using Master40.SimulationCore.Agents.Guardian;
using Master40.XUnitTest.Online.Preparations;
using Xunit;
using static Master40.SimulationCore.Agents.Guardian.Instruction;

namespace Master40.XUnitTest.Online.Agents.Contract.Behaviour
{
    public class Default : TestKit
    {
        [Fact]
        public void StartOrder()
        {
            var contractAgentRef = CreateTestProbe();
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            var message = SimulationCore.Agents.ContractAgent.Contract.Instruction.StartOrder.Create(message: orderPart, target: contractAgentRef);
            var behave = Factory.Get(simType: DB.Nominal.SimulationType.None);
            var simContext = CreateTestProbe();
            var actorPaths = AgentMoc.CreateActorPaths(testKit: this, simContext: simContext);
            AgentMoc.CreateAgent(actorPaths: actorPaths
                                           , principal: null
                                           , behaviour: behave
                                           , guardianType: GuardianType.Dispo);
            behave.Action(message: message);

            Assert.Equal(expected: "Bear", actual: ((IDefaultProperties)behave)._fArticle.Article.Name);
            var item = simContext.FishForMessage(isMessage: msg => msg is CreateChild
                                                , max: (TimeSpan.FromSeconds(value: 5))) as CreateChild;
            Assert.NotNull(@object: item);
        }


    }
}