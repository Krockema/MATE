using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.DirectoryAgent.Types;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Master40.SimulationCore.Agents.CollectorAgent.Types;
using static FAgentInformations;
using static FBreakDowns;
using static FCapabilityProviderDefinitions;
using static FCentralResourceHubInformations;
using static FResourceInformations;
using static FResourceTypes;
using static FCentralResourceDefinitions;
using static FCentralStockDefinitions;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public class Central : SimulationCore.Types.Behaviour
    {
        internal Central(SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, simulationType: simulationType)
        {

        }


        internal HubManager storageManager { get; set; } = new HubManager();
 
        public override bool Action(object message)
        {
            switch (message)
            {
                case Directory.Instruction.Central.CreateStorageAgents msg: CreateStorageAgents(stock: msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateMachineAgents msg: CreateMachineAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Central.CreateHubAgent msg: CreateHubAgent(msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void CreateHubAgent(FResourceHubInformation resourceHubInformation)
        { 
           var resourceList = resourceHubInformation.ResourceList as ResourceDictionary;
           
           //Create single HubAgent for Central planning

           var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                        , time: Agent.CurrentTime
                        , simtype: SimulationType
                        , maxBucketSize: 0 // not used currently
                        , dbConnectionStringGanttPlan: resourceHubInformation.DbConnectionString
                        , workTimeGenerator: resourceHubInformation.WorkTimeGenerator as WorkTimeGenerator
                        , debug: Agent.DebugThis
                        , principal: Agent.Context.Self)
                    , name: "CentralHub");

           System.Diagnostics.Debug.WriteLine($"Created Central Hub !");
        }

        public void CreateStorageAgents(FCentralStockDefinition stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , time: Agent.CurrentTime
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.StockName + ")").ToActorName());

            storageManager.AddOrCreateRelation(storage, stock.StockName);
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

    }
}
