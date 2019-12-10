using System;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using AkkaSim;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Nominal;
using Master40.SimulationCore.Helper;
using Master40.SimulationMrp.Simulation.Agents.JobDistributor.Skills;
using Master40.SimulationMrp.Simulation.Agents.JobDistributor.Types;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Mrp.MachineManagement;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor
{
    partial class JobDistributor : SimulationElement
    {
        private ResourceManager ResourceManager { get; } = new ResourceManager();

        private OperationManager OperationManager { get; set; }

        public static Props Props(IActorRef simulationContext, long time)
        {
            return Akka.Actor.Props.Create(() => new JobDistributor(simulationContext, time));
        }
        public JobDistributor(IActorRef simulationContext, long time)
            : base(simulationContext, time)
        {
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case AddResources m: CreateMachines(m.GetMachines, TimePeriod); break;
                case OperationsToDistribute m: InitializeDistribution(m.GetOperations); break;
                case ProductionOrderFinished m: ProvideMaterial(m.GetOperation); break;
                case WithDrawMaterialsFor m: WithDrawMaterials(m.GetOperation); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        private void InitializeDistribution(OperationManager operationManager)
        {
            // ResourceManager.AddOperationQueue(operations);
            // TODO Check is Item is in Stock ? 

            OperationManager = operationManager;
            PushWork();
        }

        private void PushWork()
        {
            var operationLeafs = OperationManager.GetLeafs();
            if (operationLeafs == null)
            {
                Debug.WriteLine("No more leafs in OperationManager");
                return;
            }

            // split into machineGroups 
            var operationGroupedBySkill = operationLeafs.GetAll()
                .GroupBy(x => x.GetValue().ResourceId, (resourceId, operations) => new { resourceId, operations });

            // get next item for each resource.
            foreach (var machine in operationGroupedBySkill)
            {
                // TODO:  no idea what performs the best
                // var first = resource.operations.Select(p => (p.GetValue().Start, p)).Min().p.GetValue();
                var first = machine.operations.OrderBy(x => x.GetValue().Start).FirstOrDefault();
                if (first == null) continue;

                // should never happen. but who knows...
                Debug.Assert(machine.resourceId != null, "resource.machineId is null");
                if (ScheduleOperation(productionOrderOperation: first, machineId: machine.resourceId.Value))
                {
                    OperationManager.RemoveOperation(first);
                }
            }
        }

        private void WithDrawMaterials(ProductionOrderOperation productionOrderOperation)
        {
            OperationManager.WithdrawMaterialsFromStock(operation: productionOrderOperation, TimePeriod);
        }

        private bool ScheduleOperation(ProductionOrderOperation productionOrderOperation, int machineId)
        {
            ResourceDetails machine = ResourceManager.GetResourceRefById(new Id(machineId));
            if (machine.IsWorking)
            {
                Debug.WriteLine("Resource is still Working.");
                return false;
            }

            machine.IsWorking = true;
            Resource.Skills.Work msg = Resource.Skills.Work.Create(productionOrderOperation, machine.ResourceRef);

            if (productionOrderOperation.GetValue().Start <= TimePeriod)
            {
                _SimulationContext.Tell(message: msg, sender: Self);
                return true;
            } // else operation starts in the future and has to wait.

            var delay = productionOrderOperation.GetValue().Start - TimePeriod;
            Schedule(delay, msg);
            return true;
        }

        private void CreateMachines(ResourceDictionary machineGroup, long time)
        {
            foreach (var machines in machineGroup)
            {
                foreach (var machine in machines.Value)
                {
                    var machineNumber = ResourceManager.Count + 1;
                    var agentName = $"{machine.GetValue().Name}({machineNumber})".ToActorName();
                    var resourceRef = Context.ActorOf(Resource.Resource.Props(_SimulationContext, time)
                        , agentName);
                    var resource = new ResourceDetails(machine, resourceRef);
                    ResourceManager.AddResource(resource);
                }
            }
        }

        private void ProvideMaterial(ProductionOrderOperation operation)
        {
            // TODO Check for Preconditions (Previous job is finished and Material is Provided.)
            var rawOperation = operation.GetValue();
            rawOperation.ProducingState = ProducingState.Finished;
            var resourceId = rawOperation.ResourceId;
            if (resourceId == null)
                throw new Exception("Resource not found.");

            OperationManager.InsertMaterialsIntoStock(operation, TimePeriod);

            var resource = ResourceManager.GetResourceRefById(new Id(resourceId.Value));
            resource.IsWorking = false;

            PushWork();
        }

        protected override void Finish()
        {
            Debug.WriteLine(Sender.Path + " has been Killed");
        }
    }
}
