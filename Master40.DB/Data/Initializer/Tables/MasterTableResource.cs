using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.VisualBasic.CompilerServices;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResource
    {

        internal Dictionary<string, List<M_Resource>> CapabilityToResourceDict = new Dictionary<string, List<M_Resource>>();
        internal Dictionary<string, List<M_ResourceSetup>> CapabilityToSetupDict = new Dictionary<string, List<M_ResourceSetup>>();
        private readonly MasterTableResourceCapability _capability;

        public MasterTableResource(MasterTableResourceCapability capability)
        {
            _capability = capability;
        }

        internal void CreateModel(int sawResource, int drillResource, int assemblyResource, bool createTestModel = false)
        {
           CreateResourceGroup(sawResource, _capability.CUTTING);
           CreateResourceGroup(drillResource, _capability.DRILLING);
           CreateResourceGroup(assemblyResource, _capability.ASSEMBLING);
           // For Testing Purpose
           if (createTestModel)
           {
               WaterJet();
               Operators();
           }
        }

        private void Operators()
        {
            var capability = _capability.CUTTING;
            var m_operator = CreateNewResource("Operator", 1);
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            // foreach resource of a given type
            foreach (var resource in CapabilityToResourceDict.Single(x => x.Key == capability.Name).Value)
            {
                // erstelle die
                foreach (var subCapability in _capability.Capabilities
                                                         .Single(x => x.Name == capability.Name)
                                                         .ChildResourceCapabilities)
                {
                    setups.Add(new M_ResourceSetup
                    {
                        ChildResourceId = m_operator.Id,
                        ParentResourceId = resource.Id,
                        Name = "Setup " + m_operator.Name,
                        UsedInProcess = false,
                        UsedInSetup = true,
                        SetupTime = 0,
                        ResourceCapabilityId = subCapability.Id
                    });
                }
                CapabilityToSetupDict.Add($"{resource.Name} Operator", setups);
            }
            CapabilityToResourceDict.Add($"{m_operator.Name} Operator", new List<M_Resource> { m_operator });
        }

        private void WaterJet()
        {
            var waterJet = CreateNewResource("WaterJetCutter", 1);
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            foreach (var subCapability in _capability.Capabilities
                .Single(x => x.Name == _capability.CUTTING.Name).ChildResourceCapabilities)
            {
                setups.Add(new M_ResourceSetup
                {
                    ParentResourceId = waterJet.Id,
                    Name = "Setup " + waterJet.Name,
                    UsedInProcess = true,
                    UsedInSetup = true,
                    SetupTime = 5,
                    ResourceCapabilityId = subCapability.Id
                });
            }

            foreach (var subCapability in _capability.Capabilities
                .Single(x => x.Name == _capability.DRILLING.Name).ChildResourceCapabilities)
            {
                setups.Add(new M_ResourceSetup
                {
                    ParentResourceId = waterJet.Id,
                    Name = "Setup " + waterJet.Name,
                    UsedInProcess = true,
                    UsedInSetup = true,
                    SetupTime = 5,
                    ResourceCapabilityId = subCapability.Id
                });
            }

            CapabilityToSetupDict.Add($"{waterJet.Name} Tooling", setups);
            CapabilityToResourceDict.Add($"{waterJet.Name} Tooling", new List<M_Resource> {waterJet});
        }

        internal void CreateResourceTools(int setupTimeCutting, int setupTimeDrilling, int setupTimeAssembling)
        {
            CreateTools(_capability.CUTTING, setupTimeCutting);
            CreateTools(_capability.DRILLING, setupTimeDrilling);
            CreateTools(_capability.ASSEMBLING, setupTimeAssembling);
        }

        private void CreateTools(M_ResourceCapability capability, long setupTime)
        {
            List<M_Resource> tools = new List<M_Resource>();
            List<M_ResourceSetup> setups = new List<M_ResourceSetup>();
            // foreach resource of a given type
            foreach (var resource in CapabilityToResourceDict.Single(x => x.Key == capability.Name).Value)
            {
                // erstelle die
                foreach (var subCapability in _capability.Capabilities
                                                         .Single(x => x.Name == capability.Name)
                                                         .ChildResourceCapabilities)
                {
                    var tool = CreateNewResource($"{resource.Name} {subCapability.Name}", 0);
                    tools.Add(tool);
                    setups.Add(new M_ResourceSetup {ChildResourceId = tool.Id
                                                  , ParentResourceId = resource.Id
                                                  , Name = "Setup " + tool.Name
                                                  , UsedInProcess = true
                                                  , UsedInSetup = true
                                                  , SetupTime = setupTime
                                                  , ResourceCapabilityId = subCapability.Id
                    });
                }
                CapabilityToSetupDict.Add($"{resource.Name} Tooling", setups);
                CapabilityToResourceDict.Add($"{resource.Name} Tooling", tools);
            }
        }

        private void CreateResourceGroup(int numberOfResources, M_ResourceCapability capability)
        {
            List<M_Resource> resourceGroup = new List<M_Resource>();
            List<M_ResourceSetup> resoruceSetups = new List<M_ResourceSetup>();
            for (int i = 1; i <= numberOfResources; i++)
            {
                var resource = CreateNewResource(capability.Name, 1, i);
                resourceGroup.Add(resource);
                resoruceSetups.Add(CreateNewSetup(resource, capability));
            }
            CapabilityToSetupDict.Add(capability.Name, resoruceSetups);
            CapabilityToResourceDict.Add(capability.Name, resourceGroup);
        }

        private M_ResourceSetup CreateNewSetup(M_Resource resource, M_ResourceCapability capability)
        {
            return new M_ResourceSetup {ChildResourceId = resource.Id
                                      , ParentResourceId = null
                                      , UsedInProcess = true
                                      , UsedInSetup = true
                                      , Name = "Setup " + resource.Name
                                      , ResourceCapabilityId = capability.Id
            };
        }

        private M_Resource CreateNewResource(string resourceName,int isLimited , int? number = null)
        {
            return new M_Resource() { Name = "Resource " + resourceName + " " + number?.ToString(), Capacity = 1, Count = isLimited};
        }
        
        internal void InitSmall(MasterDBContext context)
        {
            CreateModel(1, 1, 1);
            //SaveToDB(context);
        }
        internal void InitMedium(MasterDBContext context)
        {
            CreateModel(2, 1, 2);
            //SaveToDB(context);
        }

        internal void InitLarge(MasterDBContext context)
        {
            CreateModel(4, 2, 4);
            //SaveToDB(context);
        }

        internal void InitXLarge(MasterDBContext context)
        {
            CreateModel(5, 3, 5);
            //SaveToDB(context);
        }

        internal void InitMediumTest(MasterDBContext context)
        {
            CreateModel(2, 1, 2, true);
            //SaveToDB(context);
        }

        internal void SaveToDB(MasterDBContext context)
        {
            foreach (var item in CapabilityToResourceDict)
            {
                context.Resources.AddRange(entities: item.Value);
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