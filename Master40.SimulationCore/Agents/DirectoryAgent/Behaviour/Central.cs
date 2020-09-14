using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using Master40.SimulationCore.Agents.DirectoryAgent.Types;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using static FCentralResourceDefinitions;
using static FCentralResourceHubInformations;
using static FCentralResourceRegistrations;
using static FCentralStockDefinitions;
using static FCentralStockPostings;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public class Central : SimulationCore.Types.Behaviour
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
                case Directory.Instruction.Central.InsertMaterial msg: InsertMaterial(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.WithdrawMaterial msg: WithdrawMaterial(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateMachineAgents msg: CreateMachineAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateHubAgent msg: CreateHubAgent(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.ForwardRegistrationToHub msg: ForwardRegistrationToHub(msg.GetResourceRegistration); break;
                default: return false;
            }
            return true;
        }

        private void InsertMaterial(FCentralStockPosting stockPosting)
        {
            var actorRef = StorageManager.GetHubActorRefBy(stockPosting.MaterialId);
            Agent.Send(Storage.Instruction.Central.InsertMaterial.Create(stockPosting, actorRef));
        }

        private void WithdrawMaterial(FCentralStockPosting stockPosting)
        {
            var actorRef = StorageManager.GetHubActorRefBy(stockPosting.MaterialId);
            Agent.Send(Storage.Instruction.Central.WithdrawMaterial.Create(stockPosting, actorRef));
        }
        
        private void CreateHubAgent(FResourceHubInformation resourceHubInformation)
        {
            var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
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

        public void CreateStorageAgents(FCentralStockDefinition stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , time: Agent.CurrentTime
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.StockId + " " + stock.StockName +")").ToActorName());

            StorageManager.AddOrCreateRelation(storage, stock.StockId.ToString());
            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: storage, message: StorageAgent.Behaviour.Factory.Central(stockDefinition: stock, simType: SimulationType)));
        }


        public void CreateMachineAgents(FCentralResourceDefinition resourceDefinition)
        {
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: ResourceAgent.Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    , principal: Agent.Context.Self)
                                                    , name: ("Resource(" + resourceDefinition.ResourceName + ")").ToActorName());

            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Central(resourceDefinition)));
        }

        private void ForwardRegistrationToHub(FCentralResourceRegistration resourceRegistration)
        {
            Agent.Send(Hub.Instruction.Central.AddResourceToHub.Create(resourceRegistration, HubAgentActorRef));
        }

    }
}
