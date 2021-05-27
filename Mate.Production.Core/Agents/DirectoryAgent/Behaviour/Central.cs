using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.DirectoryAgent.Types;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Agents.StorageAgent;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;

namespace Mate.Production.Core.Agents.DirectoryAgent.Behaviour
{
    public class Central : Core.Types.Behaviour
    {
        internal Central(SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {

        }

        internal HubManager StorageManager { get; set; } = new HubManager();
        internal IActorRef HubAgentActorRef { get; set; }
        public override bool Action(object message)
        {
            switch (message)
            {
                case Directory.Instruction.Central.CreateStorageAgents msg: CreateStorageAgents(stock: msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardAddOrder msg: ForwardAddOrder(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardProvideOrder msg: ForwardProvideOrder(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardInsertMaterial msg: ForwardInsertMaterial(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardWithdrawMaterial msg: ForwardWithdrawMaterial(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateMachineAgents msg: CreateMachineAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateHubAgent msg: CreateHubAgent(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardRegistrationToHub msg: ForwardRegistrationToHub(msg.GetResourceRegistration); break;
                default: return false;
            }
            return true;
        }

        private void ForwardAddOrder(FArticles.FArticle fArticle)
        {
            var actorRef = StorageManager.GetHubActorRefBy(fArticle.Article.Id.ToString());
            Agent.Send(Storage.Instruction.Central.AddOrder.Create(fArticle, actorRef));
        }

        private void ForwardProvideOrder(FCentralProvideOrders.FCentralProvideOrder order)
        {
            var actorRef = StorageManager.GetHubActorRefBy(order.MaterialId);
            Agent.Send(Storage.Instruction.Central.ProvideOrderAtDue.Create(order, actorRef));
        }

        private void ForwardInsertMaterial(FCentralStockPostings.FCentralStockPosting stockPosting)
        {
            var actorRef = StorageManager.GetHubActorRefBy(stockPosting.MaterialId);
            Agent.Send(Storage.Instruction.Central.InsertMaterial.Create(stockPosting, actorRef));
        }

        private void ForwardWithdrawMaterial(FCentralStockPostings.FCentralStockPosting stockPosting)
        {
            var actorRef = StorageManager.GetHubActorRefBy(stockPosting.MaterialId);
            Agent.Send(Storage.Instruction.Central.WithdrawMaterial.Create(stockPosting, actorRef));
        }
        
        private void CreateHubAgent(FCentralResourceHubInformations.FResourceHubInformation resourceHubInformation)
        {
            var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                        , configuration: Agent.Configuration
                        , time: Agent.CurrentTime
                        , simtype: SimulationType
                        , maxBucketSize: 0 // not used currently
                        , dbConnectionStringGanttPlan: resourceHubInformation.DbConnectionString
                        , dbConnectionStringMaster: resourceHubInformation.MasterDbConnectionString
                        , workTimeGenerator: resourceHubInformation.WorkTimeGenerator as WorkTimeGenerator
                        , debug: Agent.DebugThis
                        , principal: Agent.Context.Self)
                    , name: "CentralHub");

           HubAgentActorRef = hubAgent;

           System.Diagnostics.Debug.WriteLine($"Created Central Hub !");
        }

        public void CreateStorageAgents(FCentralStockDefinitions.FCentralStockDefinition stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , configuration: Agent.Configuration
                                            , time: Agent.CurrentTime
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.StockId + " " + stock.MaterialName +")").ToActorName());

            StorageManager.AddOrCreateRelation(storage, stock.StockId.ToString());
            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: storage, message: StorageAgent.Behaviour.Factory.Central(stockDefinition: stock, simType: SimulationType)));
        }


        public void CreateMachineAgents(FCentralResourceDefinitions.FCentralResourceDefinition resourceDefinition)
        {
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: ResourceAgent.Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , configuration: Agent.Configuration
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    , principal: Agent.Context.Self)
                                                    , name: ("Resource(" + resourceDefinition.ResourceName + ")").ToActorName());

            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Central(resourceDefinition)));
        }

        private void ForwardRegistrationToHub(FCentralResourceRegistrations.FCentralResourceRegistration resourceRegistration)
        {
            Agent.Send(Hub.Instruction.Central.AddResourceToHub.Create(resourceRegistration, HubAgentActorRef));
        }

    }
}
