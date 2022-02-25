using System;
using Akka.TestKit.Xunit;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Agents.ContractAgent.Behaviour;
using Mate.Production.Core.Agents.Guardian;
using Mate.Test.Online.Preparations;
using Xunit;
using static FStartOrders;

namespace Mate.Test.Online.Agents.Contract.Behaviour
{
    public class Default : TestKit
    {
        [Fact]
        public void StartOrder()
        {
            var contractAgentRef = CreateTestProbe();
            var order = new T_CustomerOrder() { DueTime = 0, Id = 1 };
            var orderPart = new T_CustomerOrderPart() { Article = new M_Article { Name = "Bear" }, Quantity = 1, Id = 1, CustomerOrderId = 1, CustomerOrder = order };
            var message = Mate.Production.Core.Agents.ContractAgent.Contract.Instruction.StartOrder.Create(message: new FStartOrder(orderPart, 0L), target: contractAgentRef);
            var behave = Factory.Get(simType: DataCore.Nominal.SimulationType.None);
            var simContext = CreateTestProbe();
            var actorPaths = AgentMoc.CreateActorPaths(testKit: this, simContext: simContext);
            AgentMoc.CreateAgent(actorPaths: actorPaths
                             ,configuration: null
                                , principal: null
                                , behaviour: behave
                             , guardianType: GuardianType.Dispo);
            behave.Action(message: message);

            Assert.Equal(expected: "Bear", actual: ((IDefaultProperties)behave)._fArticle.Article.Name);
            var item = simContext.FishForMessage(isMessage: msg => msg is Instruction.CreateChild
                                                , max: (TimeSpan.FromSeconds(value: 5))) as Instruction.CreateChild;
            Assert.NotNull(@object: item);
        }


    }
}