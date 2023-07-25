using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal.Model;

namespace Mate.DataCore.Data.DynamicInitializer.Tables
{
    public class MasterTableResource
    {

        internal Dictionary<string, List<M_Resource>> CapabilityToResourceDict = new();
        internal Dictionary<string, List<M_ResourceSetup>> CapabilityToSetupDict = new ();
        internal Dictionary<string, List<M_ResourceCapabilityProvider>> CapabilityProviderDict = new();
        private readonly MasterTableResourceCapability _capability;

        public MasterTableResource(MasterTableResourceCapability capability)
        {
            _capability = capability;
        }

        internal void CreateModel(List<ResourceProperty> resourceProperties)
        {
            for (var i = 0; i < resourceProperties.Count; i++)
            {
                CreateResourceGroup(resourceProperties[i].ResourceCount, _capability.Capabilities[i]);
            }
        }

        private void CreateResourceGroup(int numberOfResources, M_ResourceCapability capability)
        {
            List<M_Resource> resourceGroup = new List<M_Resource>();
            for (int i = 1; i <= numberOfResources; i++)
            {
                var resource = CreateNewResource("Resource " + capability.Name, true, false, null, i);
                resourceGroup.Add(resource);
            }
            CapabilityToResourceDict.Add(capability.Name, resourceGroup);
        }

        private M_Resource CreateNewResource(string resourceName, bool isPhysical, bool isBiological, string groupName, int? number = null)
        {
            return new M_Resource() { Name = resourceName + " " + number?.ToString(), Capacity = 1, IsPhysical = isPhysical , IsBiological = isBiological, GroupName = groupName };
        }

        internal void CreateResourceTools(List<ResourceProperty> resourceProperties)
        {
            for (int i = 0; i < resourceProperties.Count; i++)
            {
                CreateTools(_capability.Capabilities[i], resourceProperties[i].SetupTime,
                    resourceProperties[i].OperatorCount, limitedTools: false, numberOfWorker: 3);
            }
        }

        //was sind operators? -> Personen, die Maschinen umrüsten
        private void CreateTools(M_ResourceCapability capability, long setupTime, int numberOfOperators, bool limitedTools = false, int numberOfWorker = 0)
        {
            List<M_Resource> tools = new List<M_Resource>();
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            List<M_ResourceCapabilityProvider> capabilityProviders = new List<M_ResourceCapabilityProvider>();
            List<M_Resource> operators = new List<M_Resource>();
            List<M_Resource> workers = new List<M_Resource>();

            for (int i = 1; i < 1 + numberOfOperators; i++)
            {
                operators.Add(CreateNewResource(capability.Name + " Operator " + i, true, true, capability.Name));
            }

            for (int i = 1; i < 1 + numberOfWorker; i++)
            {
                workers.Add(CreateNewResource(capability.Name + " Worker " + i, true, true, capability.Name));
            }

            var numberOfTools = _capability.Capabilities
                            .Single(x => x.Name == capability.Name)
                            .ChildResourceCapabilities.Count();

            for (int i = 0; i < numberOfTools; i++)
            {
                var tool = CreateNewResource($"Tool {capability.Name} {i}", false, false, null);
                tools.Add(tool);
            }

            foreach (var resource in CapabilityToResourceDict.Single(x => x.Key == capability.Name).Value)
            {
                if (operators.Count > 0)
                {
                    var nTool = 0;
                    foreach (var subCapability in _capability.Capabilities
                        .Single(x => x.Name == capability.Name)
                        .ChildResourceCapabilities)
                    {

                        foreach (var op in operators)
                        {
                            foreach(var worker in workers) { 
                            var capabilityProvider = new M_ResourceCapabilityProvider()
                            {
                                Name = $"Provides {subCapability.Name} {resource.Name}",
                                ResourceCapabilityId = subCapability.Id,
                            };
                          //  var tool = CreateNewResource($"Tool {resource.Name} {subCapability.Name}", false, false, null);
                          //  tools.Add(tool);

                                setups.Add(CreateNewSetup(op, capabilityProvider, false, true, 0));
                                setups.Add(CreateNewSetup(tools[nTool], capabilityProvider, true, true, setupTime));
                                setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                                setups.Add(CreateNewSetup(worker, capabilityProvider, true, false, 0));
                                capabilityProviders.Add(capabilityProvider);
                            }
                        }
                        nTool++;
                    }
                }
                else
                {
                    var nTool = 0;
                    foreach (var subCapability in _capability.Capabilities
                        .Single(x => x.Name == capability.Name)
                        .ChildResourceCapabilities)
                    {
                        var capabilityProvider = new M_ResourceCapabilityProvider()
                        {
                            Name = $"Provides {subCapability.Name} {resource.Name}",
                            ResourceCapabilityId = subCapability.Id,
                        };
                        //var tool = CreateNewResource($"Tool {resource.Name} {subCapability.Name}", false, false, null);
                        //tools.Add(tool);

                        setups.Add(CreateNewSetup(tools[nTool], capabilityProvider, true, true, setupTime));
                        setups.Add(CreateNewSetup(resource, capabilityProvider, true, true, 0));
                        capabilityProviders.Add(capabilityProvider);
                        nTool++;
                    }
                }
                CapabilityProviderDict.Add($"{resource.Name} Tooling", capabilityProviders);
                CapabilityToSetupDict.Add($"{resource.Name} Tooling", setups);
                CapabilityToResourceDict.Add($"{resource.Name} Tooling", tools);
                CapabilityToResourceDict.Add($"{resource.Name} Operators", operators);
                CapabilityToResourceDict.Add($"{resource.Name} Worker", workers);
            }
        }

        private M_ResourceSetup CreateNewSetup(M_Resource resource, M_ResourceCapabilityProvider capabilityProvider, bool usedInProcessing, bool usedInSetup, long setupTime)
        {
            return new M_ResourceSetup
            {
                ResourceCapabilityProviderId = capabilityProvider.Id,
                ResourceId = resource.Id,
                Name = $"Setup {capabilityProvider.Name} {resource.Name}",
                UsedInProcess = usedInProcessing,
                UsedInSetup = usedInSetup,
                SetupTime = setupTime
            };
        }

        internal void SaveToDB(MateDb context)
        {
            foreach (var item in CapabilityToResourceDict)
            {
                context.Resources.AddRange(entities: item.Value);
            }
            context.SaveChanges();

            foreach (var item in CapabilityProviderDict)
            {
                context.ResourceCapabilityProviders.AddRange(entities: item.Value);
            }
            context.SaveChanges();

            foreach (var item in CapabilityToSetupDict)
            {
                context.ResourceSetups.AddRange(entities: item.Value);
            }
            context.SaveChanges();
        }

    }
}